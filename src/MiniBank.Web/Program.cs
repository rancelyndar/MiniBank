using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using MiniBank.Core;
using MiniBank.Core.CurrencyConverterServices;
using MiniBank.Data.CurrencyCourseServices;
using MiniBank.Web.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

ConfigurationManager configuration = builder.Configuration;
builder.Services.AddCore().AddData(configuration);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Audience = "api";
        options.Authority = "https://demo.duendesoftware.com";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateLifetime = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                ClientCredentials = new OpenApiOAuthFlow
                {
                    TokenUrl = new Uri("https://demo.duendesoftware.com/connect/token"),
                    Scopes = new Dictionary<string, string>()
                }
            }
        });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = SecuritySchemeType.OAuth2.GetDisplayName(),
                }
            },
            new List<string>()
        }
    });
});


builder.Services.AddControllers().AddJsonOptions(j =>
{
    var currencyConverterOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    currencyConverterOptions.Converters.Add(new JsonStringEnumConverter());
    var currencyConverter = new JsonCurrencyConverter(currencyConverterOptions);

    j.JsonSerializerOptions.Converters.Add(currencyConverter);
});


var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseMiddleware<ValidationExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseMiddleware<CustomAuthenticationMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.MapControllers();

app.Run();