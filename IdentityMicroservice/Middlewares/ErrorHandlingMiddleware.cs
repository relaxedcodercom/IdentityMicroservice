using Microsoft.AspNetCore.Http.Extensions;
using System.Net;
using System.Text;
using FluentValidation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IdentityMicroservice.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ErrorHandlingMiddleware> logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                context.Request.EnableBuffering();
                await next(context);
            }
            catch (Exception exception)
            {
                if (exception is ValidationException || exception is ApplicationException)
                {
                    await HandleBadRequestExceptionsAsync(context, new { message = exception.Message });
                }
                else
                {
                    await HandleExceptionAsync(context, exception);
                }
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            try
            {
                LogError(context, exception).RunSynchronously();
            }
            catch { }

            var result = JsonConvert.SerializeObject(new { message = "Internal Server Error" });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return context.Response.WriteAsync(result);
        }

        private async Task LogError(HttpContext context, Exception exception)
        {
            var errorParams = new Dictionary<object, object>();

            errorParams["user"] = context.User.Identity.Name;
            errorParams["url"] = context.Request.GetEncodedUrl();
            errorParams["method"] = context.Request.Method;

            var headers = string.Empty;
            foreach (var requestHeader in context.Request.GetTypedHeaders().Headers)
            {
                if (requestHeader.Key == "Authorization")
                {
                    headers += $"{requestHeader.Key}: ****** | ";
                    continue;
                }
                headers += $"{requestHeader.Key}: {requestHeader.Value} | ";
            }
            headers = headers.TrimEnd(' ', '|');
            errorParams["headers"] = headers;

            context.Request.Body.Position = 0;
            string body;
            using (var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8))
            {
                body = await reader.ReadToEndAsync();
            }

            body = HideSensitiveData(body);

            errorParams["body"] = body;

            logger.LogError(exception, exception.Message, errorParams);
        }

        private string HideSensitiveData(string body)
        {
            if (body.Contains("password"))
            {
                var jsonObject = JObject.Parse(body);
                if (jsonObject["password"] != null)
                {
                    jsonObject["password"] = "******";
                }

                body = JsonConvert.SerializeObject(jsonObject);
            }

            return body;
        }

        private Task HandleBadRequestExceptionsAsync(HttpContext context, object error)
        {
            var result = JsonConvert.SerializeObject(error);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return context.Response.WriteAsync(result);
        }
    }

    #region ExtensionMethod
    public static class ExceptionExtension
    {
        public static IApplicationBuilder HandleExceptionsAsync(this IApplicationBuilder app)
        {
            app.UseMiddleware(typeof(ErrorHandlingMiddleware));
            return app;
        }
    }
    #endregion
}
