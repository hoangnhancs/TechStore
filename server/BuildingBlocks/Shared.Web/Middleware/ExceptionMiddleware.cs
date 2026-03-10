using System;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Core.EF.Application;

namespace SharedWeb.Middleware
{
    public class ExceptionMiddleware(ILogger<ExceptionMiddleware> logger, IHostEnvironment env) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try 
            {
                await next(context);
            }
            catch (ValidationException ex)
            {
                await HandleValidationException(context, ex);
            }
            catch (Exception ex)
            {
                await HandleException(context, ex);
            }
        }

        private async Task HandleException(HttpContext context, Exception ex)
        {
            logger.LogError(ex, ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var response =  env.IsDevelopment()
            ? new AddException(context.Response.StatusCode, ex.Message, ex.StackTrace)
            : new AddException(context.Response.StatusCode, ex.Message, null);

            var options = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);         
        }

        private static async Task HandleValidationException(HttpContext context, ValidationException ex)
        {
            // var validationErrors = new Dictionary<string, string[]>();
            var validationErrors = new ModelStateDictionary();
            if (ex.Errors is not null)
            {
                foreach(var err in ex.Errors)
                {
                    validationErrors.AddModelError(err.PropertyName, err.ErrorMessage);   
                }
            }
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var validationProblemDetails = new ValidationProblemDetails(validationErrors)
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "ValidationFailure",
                Title = "Validation error",
                Detail = "One or more validation errors has occurred"
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(validationProblemDetails, options);
            
            await context.Response.WriteAsync(json);
        }
    }
}
