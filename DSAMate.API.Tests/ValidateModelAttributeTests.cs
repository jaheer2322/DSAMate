using DSAMate.API.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;

namespace DSAMate.API.Tests
{
    [TestClass]
    public class ValidateModelAttributeTests
    {
        private ActionExecutingContext _context;
        private ValidateModelAttribute _filter;
        private ModelStateDictionary _modelState;

        [TestInitialize]
        public void TestInitialize()
        {
            _filter = new ValidateModelAttribute();
            _modelState = new ModelStateDictionary();
            // 1. Create a fully set up ActionContext, including the ModelState
            var actionContext = new ActionContext(
                httpContext: new DefaultHttpContext(),
                routeData: new Microsoft.AspNetCore.Routing.RouteData(),
                actionDescriptor: new ActionDescriptor(),
                modelState: _modelState // <-- The fix: Pass ModelState into the ActionContext constructor
            );

            // 2. Setup the ActionExecutingContext using the ActionContext
            _context = new ActionExecutingContext(
                actionContext: actionContext, // Use the fully constructed ActionContext
                filters: new List<IFilterMetadata>(),
                actionArguments: new Dictionary<string, object?>(),
                controller: new Mock<Controller>().Object // Controller is required but not directly used
            );
        }

        [TestMethod]
        public void OnActionExecuting_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            // Add a model validation error to simulate an invalid DTO being passed
            _modelState.AddModelError("Title", "The Title field is required.");

            // Act
            _filter.OnActionExecuting(_context);

            // Assert
            // The filter should set the Result property to BadRequestResult
            Assert.IsNotNull(_context.Result, "The filter should set the Result.");
            Assert.IsInstanceOfType(_context.Result, typeof(BadRequestResult), "The result should be a BadRequestResult.");
        }

        [TestMethod]
        public void OnActionExecuting_DoesNotSetResult_WhenModelStateIsValid()
        {
            // Arrange
            // ModelState is clean by default in TestInitialize, representing a valid DTO.

            // Act
            _filter.OnActionExecuting(_context);

            // Assert
            // The filter should NOT set the Result property, allowing the action method to execute
            Assert.IsNull(_context.Result, "The filter should not set the Result when the model state is valid.");
        }
    }
}