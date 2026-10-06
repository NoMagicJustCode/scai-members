using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using ScaiMembers.Api.Configuration;
using ScaiMembers.Api.Services;
using ScaiMembers.Api.Services.Auth;
using ScaiMembers.Api.Services.Email;
using ScaiMembers.Api.Tools;

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
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<BoardService>();

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

// Sessions: JWT in an httpOnly cookie (see SessionService)
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? new JwtSettings();
if (string.IsNullOrEmpty(jwtSettings.Secret) && builder.Environment.IsDevelopment())
    jwtSettings.Secret = "development-only-secret-never-used-in-production!";
if (jwtSettings.Secret.Length < 32)
    throw new InvalidOperationException("Jwt__Secret (JWT_SECRET) must be set to at least 32 characters.");
builder.Services.Configure<JwtSettings>(options => options.Secret = jwtSettings.Secret);

builder.Services.AddSingleton<SessionService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false; // keep "sub" as "sub"
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = SessionService.SigningKey(jwtSettings)
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies[SessionService.CookieName];
            return Task.CompletedTask;
        }
    };
});

// Authorization policies
builder.Services.AddScoped<IAuthorizationHandler, AdminRequirementHandler>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAdmin", policy => policy.AddRequirements(new AdminRequirement()))
    .AddPolicy("RequireActiveMember", policy =>
        policy.RequireAssertion(context =>
            context.User.FindFirst("member_status")?.Value == "Active"));

// CORS — the allow-all switch is honoured in Development only: it would void the CSRF protection.
var disableCors = builder.Configuration.GetValue<bool>("DISABLE_CORS") && builder.Environment.IsDevelopment();
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

if (args.FirstOrDefault() == CreateAdminCommand.Name)
    return await CreateAdminCommand.RunAsync(app.Services);

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

// CSRF: a mutating request that carries the session cookie must also carry the custom header.
app.Use(async (context, next) =>
{
    var method = context.Request.Method;
    var mutating = !(HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method));
    if (mutating
        && context.Request.Cookies.ContainsKey(SessionService.CookieName)
        && !context.Request.Headers.ContainsKey(SessionService.CsrfHeader))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }
    await next();
});

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
return 0;
