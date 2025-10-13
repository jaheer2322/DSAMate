using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DSAMate.API.Filters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionExecutingContext) {
            if (!actionExecutingContext.ModelState.IsValid)
            {
                actionExecutingContext.Result = new BadRequestResult();
            }
        }
    }
}
