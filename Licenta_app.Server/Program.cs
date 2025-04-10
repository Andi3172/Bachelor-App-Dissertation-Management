using Licenta_app.Server.Data;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;

var builder = WebApplication.CreateBuilder(args);



var jwtSettings = builder.Configuration.GetSection("JWT");

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Licenta_app", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid JWT token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddSingleton<CryptoProviderFactory>(new CryptoProviderFactory()
{
    CacheSignatureProviders = false
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var key = jwtSettings["Key"];
        if (string.IsNullOrEmpty(key))
        {
            throw new InvalidOperationException("JWT Key is not configured or missing.");
        }
        

        options.TokenValidationParameters = new TokenValidationParameters()
        {
            LogValidationExceptions = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            NameClaimType = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Sub,
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key.Trim()))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
                Console.WriteLine($"Token from Authorization header: {token}");
                Console.WriteLine("JWT Key from appsettings: " + jwtSettings["Key"]);
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                if (context.Exception is SecurityTokenInvalidIssuerException)
                {
                    Console.WriteLine("Invalid Issuer");
                }
                if (context.Exception is SecurityTokenInvalidAudienceException)
                {
                    Console.WriteLine("Invalid Audience");
                }
                if (context.Exception is SecurityTokenExpiredException)
                {
                    Console.WriteLine("Token Expired");
                }
                if (context.Exception is SecurityTokenInvalidSignatureException)
                {
                    Console.WriteLine("Invalid Signature");
                }
                if (context.Exception is SecurityTokenInvalidIssuerException)
                {
                    Console.WriteLine($"Invalid Issuer. Expected: {jwtSettings["Issuer"]}");
                }
                if (context.Exception is SecurityTokenInvalidAudienceException)
                {
                    Console.WriteLine($"Invalid Audience. Expected: {jwtSettings["Audience"]}");
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var claims = context.Principal?.Claims;
                if (claims != null)
                {
                    foreach (var claim in claims)
                    {
                        Console.WriteLine($"{claim.Type}: {claim.Value}");
                    }
                }
                return Task.CompletedTask;
            }
        };
    });


var app = builder.Build();


app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();
