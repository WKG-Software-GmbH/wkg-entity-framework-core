using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wkg.EntityFrameworkCore.ProcedureMapping.Builder.ThrowHelpers.Internals;

namespace Wkg.EntityFrameworkCore.Tests.ProcedureMapping.Builder.ThrowHelpers.Internals;

[TestClass]
public class ExceptionHelperTests
{
    private const string ORIGINAL_MESSAGE = "Original message";
    private const string ORIGINAL_PARAM = "originalParam";
    private const string NEW_MESSAGE = "New message";
    private const string NEW_PARAM = "newParam";

    [TestMethod]
    public void SetExceptionMessage_ShouldSetMessage1()
    {
        // Arrange
        Exception exception = new(ORIGINAL_MESSAGE);
        // Act
        ExceptionHelper.SetExceptionMessage(exception, NEW_MESSAGE);
        // Assert
        Assert.AreEqual(NEW_MESSAGE, exception.Message);
    }

    [TestMethod]
    public void SetExceptionMessage_ShouldSetMessage2()
    {
        // Arrange
        Exception argumentException = new();
        // Act
        ExceptionHelper.SetExceptionMessage(argumentException, NEW_MESSAGE);
        // Assert
        Assert.AreEqual(NEW_MESSAGE, argumentException.Message);
    }

    [TestMethod]
    public void SetExceptionMessage_ShouldSetMessageForDerivedException1()
    {
        // Arrange
        InvalidOperationException argumentException = new(ORIGINAL_MESSAGE);
        // Act
        ExceptionHelper.SetExceptionMessage(argumentException, NEW_MESSAGE);
        // Assert
        Assert.AreEqual(NEW_MESSAGE, argumentException.Message);
    }

    [TestMethod]
    public void SetExceptionMessage_ShouldSetMessageForDerivedException2()
    {
        // Arrange
        InvalidOperationException argumentException = new();
        // Act
        ExceptionHelper.SetExceptionMessage(argumentException, NEW_MESSAGE);
        // Assert
        Assert.AreEqual(NEW_MESSAGE, argumentException.Message);
    }

    [TestMethod]
    public void SetArgumentName_ShouldSetParameterName1()
    {
        // Arrange
        ArgumentException argumentException = new(ORIGINAL_MESSAGE, ORIGINAL_PARAM);
        // Act
        ExceptionHelper.SetArgumentName(argumentException, NEW_PARAM);
        // Assert
        Assert.AreEqual(NEW_PARAM, argumentException.ParamName);
    }

    [TestMethod]
    public void SetArgumentName_ShouldSetParameterName2()
    {
        // Arrange
        ArgumentException argumentException = new();
        // Act
        ExceptionHelper.SetExceptionMessage(argumentException, NEW_MESSAGE);
        ExceptionHelper.SetArgumentName(argumentException, NEW_PARAM);
        // Assert
        Assert.IsTrue(argumentException.Message.StartsWith(NEW_MESSAGE), $"Message should start with '{NEW_MESSAGE}'");
        Assert.AreEqual(NEW_PARAM, argumentException.ParamName);
    }

    [TestMethod]
    public void SetArgumentName_ShouldSetParameterNameForDerivedException1()
    {
        // Arrange
        ArgumentNullException argumentException = new(ORIGINAL_MESSAGE, ORIGINAL_PARAM);
        // Act
        ExceptionHelper.SetArgumentName(argumentException, NEW_PARAM);
        // Assert
        Assert.AreEqual(NEW_PARAM, argumentException.ParamName);
    }

    [TestMethod]
    public void SetArgumentName_ShouldSetParameterNameForDerivedException2()
    {
        // Arrange
        ArgumentNullException argumentException = new();
        // Act
        ExceptionHelper.SetExceptionMessage(argumentException, NEW_MESSAGE);
        ExceptionHelper.SetArgumentName(argumentException, NEW_PARAM);
        // Assert
        Assert.IsTrue(argumentException.Message.StartsWith(NEW_MESSAGE), $"Message should start with '{NEW_MESSAGE}'");
        Assert.AreEqual(NEW_PARAM, argumentException.ParamName);
    }
}