using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CityInfo.API;
using CityInfo.API.DbContexts;
using CityInfo.API.Services;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();
builder.Host.UseSerilog(
        (context, loggerConfiguration) => loggerConfiguration
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/cityinfo.txt", rollingInterval: RollingInterval.Day)
            .WriteTo.ApplicationInsights(new TelemetryConfiguration
            {
                InstrumentationKey = builder.Configuration["ApplicationInsightsInstrumentationKey"]
            }, TelemetryConverter.Traces)); 
builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson()
.AddXmlDataContractSerializerFormatters();
builder.Services.AddProblemDetails();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();
builder.Services.AddSingleton<CitiesDataStore>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidAudience = builder.Configuration["Authentication:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
               Convert.FromBase64String(builder.Configuration["Authentication:SecretForKey"]))
        };
    }
    );
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBeFromHyd", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("city", "Hyderabad");
    });
});
builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.ReportApiVersions = true;
    setupAction.AssumeDefaultVersionWhenUnspecified = true;
    setupAction.DefaultApiVersion = new ApiVersion(1, 0);
}).AddMvc().AddApiExplorer(setupAction =>
{
    setupAction.SubstituteApiVersionInUrl = true;
}); 
builder.Services.AddDbContext<CityInfoContext>(dbContextOptions => dbContextOptions.UseSqlite(builder.Configuration["ConnectionStrings:DBConnection"]));
builder.Services.AddScoped<ICityInfoRepository, CityInfoRepository>();
#if DEBUG
builder.Services.AddTransient<IMailService,MailService>();
#else
builder.Services.AddTransient<IMailService,CloudMailService>();
#endif
builder.Services.AddEndpointsApiExplorer();
var apiVersionDescriptionProvider = builder.Services.BuildServiceProvider()
  .GetRequiredService<IApiVersionDescriptionProvider>();
builder.Services.AddSwaggerGen(setupAction =>
{
    foreach (var description in
         apiVersionDescriptionProvider.ApiVersionDescriptions)
    {
        setupAction.SwaggerDoc(
            $"{description.GroupName}",
            new()
            {
                Title = "City Info API",
                Version = description.ApiVersion.ToString(),
                Description = "Through this API you can access cities and their points of interest."
            });
    }

    var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

    setupAction.IncludeXmlComments(xmlCommentsFullPath);
    setupAction.AddSecurityDefinition("CityInfoApiBearerAuth", new()
    {
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        Description = "Input a valid token to access this API"
    });

    setupAction.AddSecurityRequirement(new()
    {
        {
            new ()
            {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "CityInfoApiBearerAuth" }
            },
            new List<string>()
        }
    });
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
    | ForwardedHeaders.XForwardedProto;
});
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI(setupAction =>
    {
        var descriptions = app.DescribeApiVersions();
        foreach (var description in descriptions)
        {
            setupAction.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });
//}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
//app.MapControllers();

app.Run();
