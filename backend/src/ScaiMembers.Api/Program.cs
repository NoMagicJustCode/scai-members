using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using ScaiMembers.Api.Configuration;
using ScaiMembers.Api.Services;
using ScaiMembers.Api.Services.Email;

var builder = WebApplication.CreateBuilder(args);

// Bind configuration sections
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection(AppSettings.SectionName));
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection(MongoDbSettings.SectionName));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<CorsSettings>(builder.Configuration.GetSection(CorsSettings.SectionName));
builder.Services.Configure<UploadSettings>(builder.Configuration.GetSection(UploadSettings.SectionName));
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection(SmtpSettings.SectionName));

// MongoDB
builder.Services.AddSingleton<MongoDbContext>();

// Domain services
builder.Services.AddScoped<ConfigService>();
builder.Services.AddScoped<ApplicationService>();

// Email: SMTP when configured, otherwise the dev log sender (which refuses outside Development)
var smtpSettings = builder.Configuration.GetSection(SmtpSettings.SectionName).Get<SmtpSettings>()
    ?? new SmtpSettings();
if (smtpSettings.IsConfigured)
    builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();
else
    builder.Services.AddSingleton<IEmailSender, LogEmailSender>();

// Behind the VPS reverse proxy: take the client IP from X-Forwarded-For for rate limiting.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddRateLimiter(RateLimits.Configure);

// JWT Authentication (wired now; login endpoints arrive in Phase 4)
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? new JwtSettings();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                string.IsNullOrEmpty(jwtSettings.Secret)
                    ? "phase0-placeholder-secret-set-JWT__SECRET-in-env!!"
                    : jwtSettings.Secret))
    };
});

// Authorization policies
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAdmin", policy =>
        policy.RequireAssertion(context =>
            context.User.FindFirst("is_admin")?.Value == "true"))
    .AddPolicy("RequireActiveMember", policy =>
        policy.RequireAssertion(context =>
            context.User.FindFirst("member_status")?.Value == "Active"));

// CORS
var disableCors = builder.Configuration.GetValue<bool>("DISABLE_CORS");
var corsSettings = builder.Configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>()
    ?? new CorsSettings();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (disableCors)
        {
            policy.SetIsOriginAllowed(_ => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(corsSettings.AllowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "SCAI Members API");
    });
}

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
