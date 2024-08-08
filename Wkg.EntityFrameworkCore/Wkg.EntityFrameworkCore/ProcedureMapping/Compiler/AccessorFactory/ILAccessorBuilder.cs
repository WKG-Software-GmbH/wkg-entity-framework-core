﻿using System.Reflection;
using System.Reflection.Emit;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;
using Wkg.Reflection;
using Wkg.Reflection.Exceptions;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.AccessorFactory;

/// <summary>
/// An <see cref="IAccessorBuilder"/> that uses IL code generation to create parameter accessors.
/// </summary>
internal readonly struct ILAccessorBuilder : IAccessorBuilder, IAccessorBuilderFactory
{
    /// <summary>
    /// the argument types for the parameter setter delegate (<see cref="PropertySetter"/>), cached for performance (read-only, so thread safe)
    /// </summary>
    private static readonly Type[] s_parameterSetterArgumentTypes = TypeArray.Of<object, object>();

    /// <summary>
    /// argument types for the parameter getter delegate (<see cref="PropertyGetter"/>), again cached for performance (again, read-only)
    /// </summary>
    private static readonly Type[] s_parameterGetterArgumentTypes = TypeArray.Of<object>();

    /// <summary>
    /// the property info of the property this accessor builder is for
    /// </summary>
    private readonly PropertyInfo _pInfo;

    /// <summary>
    /// the type of the owner of the property (the type that contains the property)
    /// </summary>
    private readonly Type _ownerType;

    /// <summary>
    /// the throw helper used to throw exceptions
    /// </summary>
    private readonly IThrowHelper _throwHelper;

    [Obsolete("Use factory method instead!", true)]
    public ILAccessorBuilder()
    {
        throw new InvalidOperationException("Use factory method instead!");
    }

    /// <summary>
    /// Creates a new <see cref="ILAccessorBuilder"/> with the specified property info and throw helper.
    /// </summary>
    /// <param name="propertyInfo">The property info of the property this accessor builder is for.</param>
    /// <param name="throwHelper">The throw helper used to throw exceptions.</param>
    private ILAccessorBuilder(PropertyInfo propertyInfo, IThrowHelper throwHelper)
    {
        _pInfo = propertyInfo;
        _ownerType = propertyInfo.DeclaringType!;
        _throwHelper = throwHelper;
    }

    /// <inheritdoc/>
    public PropertyGetter BuildGetter()
    {
        // create a new dynamic method for the getter
        // the method name uses a random GUID, to prevent conflicts.
        // we declare the method on the owner type; this is not strictly necessary, but it keeps things clean.
        // in C# this would be equivalent to:
        // private static object Getter_acb9b0c03b9d4b9e9b0c0b3b9d4b9e9b(object owner) => (object)(((OwnerType)owner).Property);
        DynamicMethod dynamicMethod = new(
            $"Getter_{Guid.NewGuid:N}",
            typeof(object),
            s_parameterGetterArgumentTypes,
            _ownerType,
            true);

        MethodInfo unsafeAsOwnerType = UnsafeReflection.As(_ownerType);

        // get the IL generator for the dynamic method
        ILGenerator generator = dynamicMethod.GetILGenerator();

        // load the first argument (the owner) onto the stack
        generator.Emit(OpCodes.Ldarg_0);

        // cast the owner to the owner type
        generator.Emit(OpCodes.Call, unsafeAsOwnerType);

        // call the getter method of the property, which will leave the value of the property on the stack
        generator.Emit(OpCodes.Callvirt, _pInfo.GetMethod!);

        // if the property type is a value type, we need to box it to an object
        // otherwise, we can just return the value
        if (_pInfo.PropertyType.IsValueType)
        {
            generator.Emit(OpCodes.Box, _pInfo.PropertyType);
        }

        // return the value on the stack
        generator.Emit(OpCodes.Ret);

        // create a delegate for the dynamic method
        return dynamicMethod.CreateDelegate<PropertyGetter>();
    }

    /// <inheritdoc/>
    public PropertySetter BuildSetter()
    {
        // get the backing field for the property
        // this is the field that is automatically generated by the compiler when you use the auto property syntax (int Foo { get; set; })
        // if the property does not have an auto-generated backing field we throw an exception (we do not support manually implemented properties)
        FieldInfo backingField = _pInfo.GetBackingField()
            // if we can't find the backing field, throw an exception
            ?? _throwHelper.Throw<InvalidOperationException, FieldInfo>($"Parameter is an output parameter, but linked property {_pInfo.Name} does not have a valid auto-generated backing field.");

        // in C# this would be equivalent to:
        // private static void Setter_acb9b0c03b9d4b9e9b0c0b3b9d4b9e9b(object owner, object value) => ((OwnerType)owner).Property = (PropertyType)value;
        // except that we use the backing field instead of the property which allows us to set the value even if the property is read-only or init-only (default for records)
        DynamicMethod dynamicMethod = new(
            $"Setter_{Guid.NewGuid:N}",
            typeof(void),
            s_parameterSetterArgumentTypes,
            _ownerType,
            true);

        // we use Unsafe.As<OwnerType>(owner) instead of (OwnerType)owner because Castclass is slooooow
        MethodInfo unsafeAsOwnerType = UnsafeReflection.As(_ownerType);

        // get the IL generator for the dynamic method
        ILGenerator generator = dynamicMethod.GetILGenerator();

        // load the first argument (the owner) onto the stack
        generator.Emit(OpCodes.Ldarg_0);

        // cast the owner to the owner type
        generator.Emit(OpCodes.Call, unsafeAsOwnerType);

        // we can optimize manually here.
        // this is why in the benchmarks we are faster than Roslyn :)
        // Roslyn just always emits unbox.any
        if (_pInfo.PropertyType.IsValueType)
        {
            // push the value onto the stack (second argument, index 1)
            generator.Emit(OpCodes.Ldarg_1);

            // unboxes object to managed byref pointer
            generator.Emit(OpCodes.Unbox, _pInfo.PropertyType);

            // switch on the type of the property to determine the correct Ldind instruction
            // the Ldind family of instructions loads a value from a managed pointer
            Type t = _pInfo.PropertyType;
            OpCode? simpleIndirectLoad = t switch
            {
                _ when t == typeof(nint) || t == typeof(nuint) => OpCodes.Ldind_I,
                _ when t == typeof(sbyte) => OpCodes.Ldind_I1,
                _ when t == typeof(short) => OpCodes.Ldind_I2,
                _ when t == typeof(int) => OpCodes.Ldind_I4,
                _ when t == typeof(long) || t == typeof(ulong) => OpCodes.Ldind_I8,
                _ when t == typeof(byte) || t == typeof(bool) => OpCodes.Ldind_U1,
                _ when t == typeof(ushort) => OpCodes.Ldind_U2,
                _ when t == typeof(uint) => OpCodes.Ldind_U4,
                _ when t == typeof(float) => OpCodes.Ldind_R4,
                _ when t == typeof(double) => OpCodes.Ldind_R8,
                _ => null
            };
            // if the type is not one of the above, we need to use Ldobj (for larger/not primitive types)
            if (simpleIndirectLoad is null)
            {
                // Ldobj also needs some type information, so we need to push the type token onto the stack too here
                generator.Emit(OpCodes.Ldobj, t);
            }
            else
            {
                // this is one of the simple Ldind instructions
                // they are already typed, so we don't need to push the type token
                generator.Emit(simpleIndirectLoad.Value);
            }
        }
        else
        {
            // for reference types, we can cast the reference (no Castclass with checks for custom conversions needed).
            // in theory we could also just jam the new pointer in there without casting, it wouldn't actually matter
            // but the JIT is smart enough to optimize this away anyway.
            // benchmarking shows that this is even faster than just jamming the pointer in there
            // probably because this legal IL is easier to jit than the illegal IL we'd get otherwise :)
            // We use Unsafe.As<object, TargetType>(ref value) here
            MethodInfo unsafeAsPropertyType = UnsafeReflection.As(typeof(object), _pInfo.PropertyType);

            // push argument address (ByRef) of the value onto the stack (second argument, index 1, managed ByRef)
            generator.Emit(OpCodes.Ldarga_S, 1);

            // invoke Unsafe.As<object, TargetType>(ref value)
            generator.Emit(OpCodes.Call, unsafeAsPropertyType);

            // resolve the ByRef pointer to a normal reference (ByRef<object> is just void**)
            generator.Emit(OpCodes.Ldind_Ref);
        }

        // whatever we have on the stack now is the value we want to set
        // we need to store it in the backing field
        generator.Emit(OpCodes.Stfld, backingField);

        // return from the method
        generator.Emit(OpCodes.Ret);

        // create a delegate for the dynamic method
        return dynamicMethod.CreateDelegate<PropertySetter>();
    }

    public PropertySetter<T> BuildSetterDirect<T>()
    {
        if (typeof(T) != _pInfo.PropertyType)
        {
            _throwHelper.Throw<TypeMismatchException>($"Expected type parameter T to be of type {_pInfo.PropertyType}, but got {typeof(T).Name}");
        }

        // get the backing field for the property
        // this is the field that is automatically generated by the compiler when you use the auto property syntax (int Foo { get; set; })
        // if the property does not have an auto-generated backing field we throw an exception (we do not support manually implemented properties)
        FieldInfo backingField = _pInfo.GetBackingField()
            // if we can't find the backing field, throw an exception
            ?? _throwHelper.Throw<InvalidOperationException, FieldInfo>($"Parameter is an output parameter, but linked property {_pInfo.Name} does not have a valid auto-generated backing field.");

        // in C# this would be equivalent to:
        // private static void Setter_acb9b0c03b9d4b9e9b0c0b3b9d4b9e9b(object owner, object value) => ((OwnerType)owner).Property = (PropertyType)value;
        // except that we use the backing field instead of the property which allows us to set the value even if the property is read-only or init-only (default for records)
        DynamicMethod dynamicMethod = new(
            $"Setter_{Guid.NewGuid:N}",
            typeof(void),
            TypeArray.Of<object, T>(),
            _ownerType,
            true);

        // we use Unsafe.As<OwnerType>(owner) instead of (OwnerType)owner because Castclass is slooooow
        MethodInfo unsafeAs1 = UnsafeReflection.As(_ownerType);

        // get the IL generator for the dynamic method
        ILGenerator generator = dynamicMethod.GetILGenerator();

        // load the first argument (the owner) onto the stack
        generator.Emit(OpCodes.Ldarg_0);

        // cast the owner to the owner type
        generator.Emit(OpCodes.Call, unsafeAs1);

        // push the value onto the stack (second argument, index 1)
        // we assume by convention that the types match
        generator.Emit(OpCodes.Ldarg_1);

        // whatever we have on the stack now is the value we want to set
        // we need to store it in the backing field
        generator.Emit(OpCodes.Stfld, backingField);

        // return from the method
        generator.Emit(OpCodes.Ret);

        // create a delegate for the dynamic method
        return dynamicMethod.CreateDelegate<PropertySetter<T>>();
    }

    /// <inheritdoc/>
    public static IAccessorBuilder CreateBuilder(PropertyInfo propertyInfo, IThrowHelper throwHelper) => new ILAccessorBuilder(propertyInfo, throwHelper);
}