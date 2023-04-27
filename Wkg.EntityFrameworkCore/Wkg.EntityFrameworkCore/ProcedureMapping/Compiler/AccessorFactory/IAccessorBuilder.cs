using System.Reflection;
using Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers;
using Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.Output;

namespace Wkg.EntityFrameworkCore.ProcedureMapping.Compiler.AccessorGeneration;

/// <summary>
/// Represents a builder to generate <see cref="PropertyGetter"/> and <see cref="PropertySetter"/> delegates for a given property.
/// </summary>
public interface IAccessorBuilder
{
    /// <summary>
    /// Builds a <see cref="PropertyGetter"/> delegate for the given property.
    /// </summary>
    /// <returns>A <see cref="PropertyGetter"/> delegate.</returns>
    PropertyGetter BuildGetter();

    /// <summary>
    /// Builds a <see cref="PropertySetter"/> delegate for the given property.
    /// </summary>
    /// <returns>A <see cref="PropertySetter"/> delegate.</returns>
    PropertySetter BuildSetter();

    /// <summary>
    /// Builds a <see cref="PropertySetter{T}"/> delegate for the given property, omitting type checks and/or unboxing.
    /// </summary>
    /// <returns>A <see cref="PropertySetter{T}"/> delegate.</returns>
    PropertySetter<T> BuildSetterDirect<T>();
}

/// <summary>
/// Represents a factory to create <see cref="IAccessorBuilder"/> instances.
/// </summary>
internal interface IAccessorBuilderFactory
{
    /// <summary>
    /// Creates a new <see cref="IAccessorBuilder"/> instance for the given property.
    /// </summary>
    /// <param name="propertyInfo">The property to create the builder for.</param>
    /// <param name="throwHelper">The <see cref="IThrowHelper"/> instance to use.</param>
    /// <returns>A new <see cref="IAccessorBuilder"/> instance.</returns>
    static abstract IAccessorBuilder CreateBuilder(PropertyInfo propertyInfo, IThrowHelper throwHelper);
}