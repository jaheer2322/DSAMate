using System.Net;
using System.Text.Json;
using DSAMate.API.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace DSAMate.API.Tests
{
    [TestClass]
    public class ExceptionHandlerMiddlewareTests
    {
        private Mock<ILogger<ExceptionHandlerMiddleware>> _mockLogger;
        private DefaultHttpContext _httpContext;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockLogger = new Mock<ILogger<ExceptionHandlerMiddleware>>();
            _httpContext = new DefaultHttpContext();
            _httpContext.Response.Body = new MemoryStream(); // Capture response body
        }

        private ExceptionHandlerMiddleware CreateMiddleware(RequestDelegate next)
        {
            return new ExceptionHandlerMiddleware(next, _mockLogger.Object);
        }

        // --- Test 1: General Exception (500 Internal Server Error) ---

        [TestMethod]
        public async Task InvokeAsync_HandlesGenericException_Returns500InternalServerError()
        {
            // Arrange
            var genericException = new Exception("Something went wrong! Please check the logs with the help of provided Id");

            // This delegate simulates the next middleware/controller throwing an exception
            RequestDelegate next = (HttpContext context) => throw genericException;
            var middleware = CreateMiddleware(next);

            // Act
            await middleware.InvokeAsync(_httpContext);

            // Assert

            // 1. Check HTTP Status Code (Must be 500)
            Assert.AreEqual((int)HttpStatusCode.InternalServerError, _httpContext.Response.StatusCode, "Status code should be 500.");

            // 2. Check Content Type (Must be application/json)
            Assert.AreEqual("application/json; charset=utf-8", _httpContext.Response.ContentType, "Content type should be application/json.");

            // 3. Check Response Body (Must contain the generic error message)
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseJson = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var errorResponse = JsonDocument.Parse(responseJson).RootElement;

            Assert.IsTrue(errorResponse.TryGetProperty("id", out _), "Response must contain an 'Id'.");
            Assert.AreEqual("Something went wrong! Please check the logs with the help of provided Id",
                            errorResponse.GetProperty("errorMessage").GetString(),
                            "Response message should be the generic 500 message.");

            // 4. Verify Logging (Must log the exception)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(genericException.Message)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
                Times.Once,
                "The generic exception must be logged at Error level."
            );
        }

        // --- Test 2: InvalidOperationException (400 Bad Request) ---

        [TestMethod]
        public async Task InvokeAsync_HandlesInvalidOperationException_Returns400BadRequest()
        {
            // Arrange
            const string specificErrorMessage = "Something went wrong! Please check the logs with the help of provided Id";
            var invalidOperationException = new InvalidOperationException(specificErrorMessage);

            // This delegate simulates a repository throwing a specific exception
            RequestDelegate next = (HttpContext context) => throw invalidOperationException;
            var middleware = CreateMiddleware(next);

            // Act
            await middleware.InvokeAsync(_httpContext);

            // Assert

            // 1. Check HTTP Status Code (Must be 400)
            Assert.AreEqual((int)HttpStatusCode.BadRequest, _httpContext.Response.StatusCode, "Status code should be 400 for InvalidOperationException.");

            // 2. Check Response Body (Must contain the specific error message)
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseJson = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var errorResponse = JsonDocument.Parse(responseJson).RootElement;

            Assert.AreEqual(specificErrorMessage,
                            errorResponse.GetProperty("errorMessage").GetString(),
                            "Response message should be the specific message from the InvalidOperationException.");

            // 3. Verify Logging (Must log the exception)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(specificErrorMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
                Times.Once,
                "The InvalidOperationException must be logged at Error level."
            );
        }

        // --- Test 3: Happy Path (No Exception) ---

        [TestMethod]
        public async Task InvokeAsync_HandlesNoException_CallsNextDelegate()
        {
            // Arrange
            bool nextCalled = false;

            // This delegate simulates the pipeline succeeding
            RequestDelegate next = (HttpContext context) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };
            var middleware = CreateMiddleware(next);

            // Act
            await middleware.InvokeAsync(_httpContext);

            // Assert
            Assert.IsTrue(nextCalled, "The next RequestDelegate should have been called.");
            Assert.AreEqual(StatusCodes.Status200OK, _httpContext.Response.StatusCode, "Status code should be 200 (default successful status).");

            // Verify no logging occurred
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()!),
                Times.Never,
                "No exception logging should occur on the happy path."
            );
        }
    }
}
