using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using Wkg.Logging;
using System.Runtime.CompilerServices;

namespace Wkg.EntityFrameworkCore;

/// <summary>
/// An interface that can be used to throw exceptions.
/// </summary>
public interface IThrowHelper
{
    /// <summary>
    /// Throws an exception of type <typeparamref name="TException"/> with the specified <paramref name="message"/>.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to be thrown.</typeparam>
    /// <param name="message">The message that describes the error.</param>
    [DoesNotReturn]
    [StackTraceHidden]
    public void Throw<TException>(string message) where TException : Exception, new();

    /// <summary>
    /// Throws an exception of type <typeparamref name="TException"/> with the specified <paramref name="message"/> and expected return type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to be thrown.</typeparam>
    /// <typeparam name="T">The expected return type (which will never be returned).</typeparam>
    /// <param name="message">The message that describes the error.</param>
    /// <returns>never</returns>
    [DoesNotReturn]
    [StackTraceHidden]
    public T Throw<TException, T>(string message) where TException : Exception, new();

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> of type <typeparamref name="TArgumentException"/> with the specified <paramref name="message"/> and <paramref name="paramName"/>.
    /// </summary>
    /// <typeparam name="TArgumentException">The concrete type of the <see cref="ArgumentException"/> to be thrown.</typeparam>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="paramName">The name of the parameter that caused the current exception.</param>
    [DoesNotReturn]
    [StackTraceHidden]
    public void Throw<TArgumentException>(string message, string paramName) where TArgumentException : ArgumentException, new();

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> of type <typeparamref name="TArgumentException"/> with the specified <paramref name="message"/>, <paramref name="paramName"/>, and expected return type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="TArgumentException">The concrete type of the <see cref="ArgumentException"/> to be thrown.</typeparam>
    /// <typeparam name="T">The expected return type (which will never be returned).</typeparam>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="paramName">The name of the parameter that caused the current exception.</param>
    /// <returns>never</returns>
    [DoesNotReturn]
    [StackTraceHidden]
    public T Throw<TArgumentException, T>(string message, string paramName) where TArgumentException : ArgumentException, new();

    /// <summary>
    /// Logs a warning <paramref name="message"/> in the current context to the configured <see cref="Log.CurrentLogger"/>.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    public void Warn(string message);

    /// <summary>
    /// Logs a warning <paramref name="message"/> in the current context to the configured <see cref="Log.CurrentLogger"/> with the specified <paramref name="paramName"/>.
    /// </summary>
    /// <param name="message">The message to be logged.</param>
    /// <param name="paramName">The name of the parameter that caused the warning.</param>
    public void Warn(string message, string paramName);
}