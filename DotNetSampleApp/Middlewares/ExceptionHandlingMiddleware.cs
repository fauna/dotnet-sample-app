using Fauna.Exceptions;

namespace DotNetSampleApp.Middlewares;

/// <summary>
/// Exception handling middleware
/// </summary>
/// <param name="next"></param>
/// <param name="logger"></param>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    /// <summary>
    /// Invoke middleware
    /// </summary>
    /// <param name="context"></param>
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AbortException ex) when (ex.Message.Contains("does not exist"))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new { ex.Message });
        }
        catch (FaunaException ex)
        {
            logger.LogError(ex, "Fauna exception occurred.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { Message = "An unexpected error occurred." });
        }
    }
}
