using Presentation.Configurations;
using Presentation.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using System.Text;
using Application.Extensions;
using Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

builder.Configuration.AddEnvironmentVariables();

builder.Host.UseSerilog(LoggerConfigurator.ConfigureLogger(builder.Configuration));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder
    .Services
    .AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition(
            "oauth2",
            new OpenApiSecurityScheme
            {
                Description =
                    "Standard Authorization Header Using The Bearer Scheme (\"bearer {token}\")",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            }
        );
        ;
        options.OperationFilter<SecurityRequirementsOperationFilter>();
    });

// JWT Token
builder
    .Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!)
            ),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

    //.AddJwtBearer(options =>
    // {
    //     options.Authority = "https://localhost:5000"; // My SSO
    //     options.Audience = "demo_client";
    //     options.RequireHttpsMetadata = false;
    // });

// Cross-Origin Resource Sharing
var allowSpecificOrigin = "_myAllowSpecificOrigins";

builder
    .Services
    .AddCors(options =>
    {
        options.AddPolicy(
            allowSpecificOrigin,
            builder =>
            {
                builder
                    .WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            }
        );
    });

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    //options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddProblemDetails();

builder.Services.AddCustomRateLimiters();
builder.Services.AddExceptionHandlers();


builder.Services.AddApplicationServices();
builder.Services.AddMessagingServices();
builder.Services.AddSecurityServices();

builder.AddCachingServices();
builder.AddPersistenceServices();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseForwardedHeaders();

//app.UseRateLimiter();
app.UseExceptionHandler();
app.UseCors(allowSpecificOrigin);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/test-ip", (HttpContext context) =>
{
    return context.Connection.RemoteIpAddress?.ToString() ?? "no ip";
});
app.Run();
