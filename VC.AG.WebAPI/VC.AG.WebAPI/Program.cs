using System.Net;
using VC.AG.WebAPI.Extensions;
using VC.AG.WebAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);
IConfiguration configuration = builder.Configuration;

builder.Services.AddRequirements(builder, configuration);
builder.Services.AddHttpSecurityHeaders();
var app = builder.Build();
app.UseResponseCompression();
app.UseMiddleware<CustomSecurityHeaderMiddleware>();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseCors();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
await app.RunAsync();
