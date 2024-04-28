using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using NzbDrone.Core.Configuration;

namespace Sonarr.Http.REST.Filters;

public class LogDatabaseDisabledActionFilterAttribute<TResult> : IActionFilter
    where TResult : class, new()
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var configFileProvider = context.HttpContext.RequestServices.GetService<IConfigFileProvider>();
        if (!configFileProvider.LogDbEnabled)
        {
            context.Result = new OkObjectResult(new TResult());
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
