using System.Net.Mime;
using Microsoft.AspNetCore.Diagnostics;

namespace BankingPoc;

public static class ExceptionHandler
{
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                // using static System.Net.Mime.MediaTypeNames;
                context.Response.ContentType = MediaTypeNames.Text.Plain;
                
                var exceptionHandlerPathFeature =
                    context.Features.Get<IExceptionHandlerPathFeature>();

                if (exceptionHandlerPathFeature?.Error 
                    is InvalidOperationException
                    or ArgumentNullException
                    or ArgumentOutOfRangeException)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync(exceptionHandlerPathFeature.Error.Message);
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("An exception was thrown.");
                }
            });
        });
        return app;
    }
}