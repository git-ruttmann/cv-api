namespace ruttmann.vita.api
{
  using System;
  using System.Linq;
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.AspNetCore.Mvc.Filters;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Primitives;

  /// <summary>
  /// an attribute to inject authentication test
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
  public class AuthorizeAttribute : Attribute, IAsyncActionFilter
  {
    const String AuthCookieHeaderName = "VitaApiAuth";

    /// <summary>
    /// callback before the method is called
    /// </summary>
    /// <param name="context">the execution context</param>
    /// <param name="next">the next action (actually the method)</param>
    /// <returns>A task that executes 'next()'</returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
      if (!context.HttpContext.Request.Cookies.TryGetValue(AuthCookieHeaderName, out var authCookie)) 
      {
        context.Result = new UnauthorizedResult();
        return;
      }

      var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
      if (!authService.IsValidCookie(authCookie, out var code))
      {
        context.Result = new UnauthorizedResult();
        return;
      }

      context.HttpContext.Request.Headers.Add("Code", new StringValues(code));
      await next();
    }
  }
}
