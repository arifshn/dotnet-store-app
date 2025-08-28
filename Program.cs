using System.Text;
using System.Text.Json.Serialization;
using API.Data;
using API.Entity;
using API.Hubs;
using API.Middlewares;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Seq("http://localhost:5341") 
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<DataContext>(options =>
{
    var config = builder.Configuration;
    var connectionString = config.GetConnectionString("defaultConnection");
    options.UseSqlServer(connectionString);
});

builder.Services.AddCors();
builder.Services.AddIdentity<AppUser, AppRole>().AddEntityFrameworkStores<DataContext>();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;

    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
});

builder.Services.AddSignalR();
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidIssuer = "arifsahin.com",
        ValidateAudience = false,
        ValidAudience = "calmora",
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
            builder.Configuration["JWTSecurity:SecretKey"]!)),
        ValidateLifetime = true
    };

    x.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/chathub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddOpenApi();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<FavoriteService>();

var app = builder.Build();

app.MapGet("/", () =>
{
    Log.Information("Ana sayfa çağrıldı!");
    return "Merhaba, e-ticaret API!";
});
app.MapPost("/login", (string username) =>
{
    Log.Information("Kullanıcı giriş yaptı. Username: {Username}", username);
    return Results.Ok(new { Success = true });
});
app.MapGet("/products", async (DataContext db) =>
{
    Log.Information("Products endpoint çağrıldı");
    var products = await db.Products.ToListAsync();
    Log.Information("Products sayısı: {Count}", products.Count);
    return products;
});


app.UseMiddleware<ExceptionHandling>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Demo API");
    });
    app.MapScalarApiReference();
    app.UseCors(opt =>
        opt.AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials()
           .SetIsOriginAllowed(_ => true)
    );
}
else
{
    app.UseCors(opt =>
        opt.AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials()
           .WithOrigins("http://192.168.1.104:8081")
    );

    app.UseHttpsRedirection();
}
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");

await SeedDatabase.Initialize(app);

app.Run();
