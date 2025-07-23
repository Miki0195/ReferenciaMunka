using ELTE.Cinema.DataAccess.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ELTE.Cinema.WebAPI.Infrastructure;

/// <summary>
/// ExceptionToProblemDetailsHandler
/// </summary>
public class ExceptionToProblemDetailsHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="problemDetailsService"></param>
    public ExceptionToProblemDetailsHandler(IProblemDetailsService problemDetailsService)
    {
        _problemDetailsService = problemDetailsService;
    }

    /// <summary>
    /// Map the exceptions to status codes and create problemDetails
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="exception"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        return exception switch
        {
            EntityNotFoundException => await CreateProblemDetails(httpContext, exception,
                StatusCodes.Status404NotFound),
            AccessViolationException => await CreateProblemDetails(httpContext, exception,
                StatusCodes.Status403Forbidden),
            ArgumentOutOfRangeException => await CreateProblemDetails(httpContext, exception,
                StatusCodes.Status400BadRequest),
            ArgumentNullException => await CreateProblemDetails(httpContext, exception,
                StatusCodes.Status400BadRequest),
            ArgumentException => await CreateProblemDetails(httpContext, exception,
                StatusCodes.Status409Conflict),
            InvalidDataException => await CreateProblemDetails(httpContext, exception,
                StatusCodes.Status409Conflict),
            InvalidOperationException => await CreateProblemDetails(httpContext, exception,
                StatusCodes.Status409Conflict),
            _ => false
        };
    }

    private async Task<bool> CreateProblemDetails(HttpContext httpContext, Exception exception, int statusCode)
    {
        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Title = "An error occurred",
            Type = exception.GetType().Name,
            Detail = exception.Message,
        };

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            Exception = exception,
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });
    }
}
