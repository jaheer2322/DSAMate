namespace DSAMate.API.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;
        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                var errorId = Guid.NewGuid();

                // Log the exception
                _logger.LogError(ex, errorId + " : " + ex.Message);

                // Return a custom error response
                if (ex is InvalidOperationException)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
                else
                {
                    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                }
                httpContext.Response.ContentType = "application/json";
                var error = new
                {
                    Id = errorId,
                    ErrorMessage = "Something went wrong! Please check the logs with the help of provided Id"
                };
                await httpContext.Response.WriteAsJsonAsync(error);
            }
        }
    }
}
