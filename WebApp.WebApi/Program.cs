using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApp.BusinessLogic;
using WebApp.DataAccess.Data;
using WebApp.DataAccess.Helpers;
using WebApp.DataAccess.Interfaces;
using WebApp.WebApi;

var builder = WebApplication.CreateBuilder(args);

IConfigurationRoot confString = new ConfigurationBuilder()
    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.Development.json").Build();
builder.Services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(confString.GetConnectionString("DefaultConnection")));

builder.Services.AddApplicationServices();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = AuthOptions.ISSUER,
            ValidateAudience = true,
            ValidAudience = AuthOptions.AUDIENCE,
            ValidateLifetime = true,
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
            ValidateIssuerSigningKey = true,
        };
    });

builder.Services.AddAutoMapper(typeof(AutomapperProfile));

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Forum API",
        Description = "An ASP.NET Core Web API for managing forum",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your token in the text input below.\r\n\r\nExample: \"12345abcdef\"",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var users = services.GetRequiredService<IUser>();
        var roles = services.GetRequiredService<IRole>();
        await DbInit.InitializeAsync(users, roles);

        var context = services.GetRequiredService<ApplicationContext>();
        await DbInit.InitializeContentAsync(context);
    }
    catch (InvalidOperationException ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while resolving a service.");
    }
    catch (DbUpdateException ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while updating the database.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");
app.UseAuthorization();
app.MapControllers();
await app.RunAsync();
