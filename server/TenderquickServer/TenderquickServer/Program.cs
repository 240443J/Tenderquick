using System.Text;
using TenderquickServer.Data;
using TenderquickServer.Services;
using TenderquickServer.Services.Sources;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ───────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("MyConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString!, ServerVersion.AutoDetect(connectionString!)));

// ─── CORS ───────────────────────────────────────────────────
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader());
});

// ─── Authentication / JWT ────────────────────────────────────
var jwtConfig = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtConfig["Issuer"],
        ValidAudience            = jwtConfig["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtConfig["Key"]!))
    };
});
builder.Services.AddAuthorization();

// ─── Application services ───────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<CurrentUser>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITenderService, EfTenderService>();
builder.Services.AddScoped<ITenderSearchService, TenderSearchService>();
builder.Services.AddScoped<ICalendarService, LocalCalendarService>();
builder.Services.AddScoped<IDeadlineService, EfDeadlineService>();
builder.Services.AddScoped<IInventoryService, EfInventoryService>();
builder.Services.AddScoped<IQuotationService, EfQuotationService>();
builder.Services.AddScoped<IDocumentService, EfDocumentService>();
builder.Services.AddScoped<IDiscoveryService, EfDiscoveryService>();

// The only line that names an AI vendor. Swapping providers happens here.
builder.Services.AddScoped<IAiProvider, RuleBasedAiProvider>();

// External tender sources — registered as IEnumerable<ITenderSource> for fan-out search.
builder.Services.AddHttpClient<ITenderSource, GebizSource>();
builder.Services.AddHttpClient<ITenderSource, SesamiSource>();
builder.Services.AddHttpClient<ITenderSource, TenderboardSource>();

// ─── Controllers + Swagger ──────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var scheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Paste the JWT from /api/auth/login (without the 'Bearer ' prefix).",
        Reference = new Microsoft.OpenApi.Models.OpenApiReference
        {
            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
            Id = "Bearer",
        },
    };
    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        [scheme] = Array.Empty<string>(),
    });
});

var app = builder.Build();

// ─── Migrate + seed ─────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await DbInitializer.SeedAsync(db);
    }
    catch (Exception ex)
    {
        logger.LogError(ex,
            "Database migration/seed failed. Check ConnectionStrings:MyConnection and that MySQL is running.");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html"); // SPA fallback

app.Run();
