using System.Text.Json;
using EmployeeService.Models;

namespace EmployeeService.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next=next;
            _logger=logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (NotFoundException ex)
            {
                await WriteErrorResponse(context,404,ex.Message);
            }
            catch (BadRequestException ex)
            {
                await WriteErrorResponse(context,400,ex.Message);
            }
            catch (ConflictException ex)
            {
                await WriteErrorResponse(context,409,ex.Message);
            }
            catch (UnauthorizedException ex)
            {
                await WriteErrorResponse(context,404,ex.Message);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"Unhandled exception occured");
                await WriteErrorResponse(context,500,"An interval server error occurred");
            }
        }

    private static async Task WriteErrorResponse(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode=statusCode;
            context.Response.ContentType="application/json";
            var Response= ApiResponse<object>.Fail(message);
            await context.Response.WriteAsync(JsonSerializer.Serialize(Response));
        }
    }    
}