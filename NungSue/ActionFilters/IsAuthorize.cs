using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NungSue.Constants;

namespace NungSue.ActionFilters;

public class IsAuthorize : Attribute, IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
        var user = context.HttpContext.User.Identity;
        var IsAuthenticated = user.IsAuthenticated && user.AuthenticationType == AuthSchemes.CustomerAuth;
        if (IsAuthenticated)
            context.Result = new RedirectResult("/");
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {

    }
}
