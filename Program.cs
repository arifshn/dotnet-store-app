using API.Data;
using API.Middlewares;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DataContext>(options =>
{
    var config = builder.Configuration;
    var connectionString = config.GetConnectionString("defaultConnection");
    options.UseSqlServer(connectionString);
 });

builder.Services.AddCors();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseMiddleware<ExceptionHandling>();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Demo API");
    });
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors(opt =>
{
    opt.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("http://localhost:3000");
});

app.UseAuthorization();

app.MapControllers();

app.Run();
