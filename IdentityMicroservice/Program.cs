using IdentityMicroservice.DI;
using IdentityMicroservice.Middlewares;
using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMvc()
             .AddJsonOptions(options =>
             {
                 options.JsonSerializerOptions.DefaultIgnoreCondition
                       = JsonIgnoreCondition.WhenWritingNull;
             });

DependencyResolver.AddSwagger(builder.Services);
DependencyResolver.RegisterServices(builder.Services);
DependencyResolver.AddAuthentication(builder.Services, builder.Configuration);
DependencyResolver.AddLogging(builder.Services);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(config =>
{
    config.WithOrigins(builder.Configuration.GetSection("CORS:Allow-Origins").Get<string[]>())
       .AllowAnyHeader().AllowAnyMethod();
});

//if (!app.Environment.IsDevelopment())
//{
//    app.UseHttpsRedirection();
//}

app.HandleExceptionsAsync();

app.UseAuthorization();

app.MapControllers();

app.Run();
