using AsynchronousBackgroundProcessing.Configurations;
using AsynchronousBackgroundProcessing.Consts;
using AsynchronousBackgroundProcessing.Extensions;
using AsynchronousBackgroundProcessing.HostedServices;
using AsynchronousBackgroundProcessing.Services;
using AsynchronousBackgroundProcessing.Services.Jobs;
using Hangfire;
using Hangfire.SqlServer;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IMySingletonService, MySingletonService>();
builder.Services.AddScoped<IMyScopedService, MyScopedService>();
builder.Services.AddTransient<IMyTransientService, MyTransientService>();
builder.Services.AddSingleton<IMyJobs, MyJobs>();
builder.Services.AddScoped<IBaseJob, FirstJob>(JobType.FirstJob);
builder.Services.AddScoped<IBaseJob, SecondJob>(JobType.SecondJob);
builder.Services.AddScoped<IBaseJob, ThirdJob>(JobType.ThirdJob);

//builder.Services.AddHostedService<MyBackgroundService>();
builder.Services.AddHostedService<MyHostedService>();

// Load configurations file
var configsFileTemplate =
    Environment.GetEnvironmentVariable("CONFIGS_FILE_PATH") ??
    Environment.GetEnvironmentVariable("CONFIGS_FILE_PATH", EnvironmentVariableTarget.Machine) ??
    "/etc/configs/configs.{ENVIRONMENT}.yml";
var configsFile = configsFileTemplate.Replace("{ENVIRONMENT}.", string.Empty);
var configsFileOverride = configsFileTemplate.Replace("{ENVIRONMENT}.", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
builder.Configuration.AddYamlFile(configsFile, optional: false, reloadOnChange: false);
builder.Configuration.AddYamlFile(configsFileOverride, optional: true, reloadOnChange: true);

var secretsFileTemplate =
    Environment.GetEnvironmentVariable("SECRETS_FILE_PATH") ??
    Environment.GetEnvironmentVariable("SECRETS_FILE_PATH", EnvironmentVariableTarget.Machine) ??
    "/etc/secrets/secrets.{ENVIRONMENT}.yml";
var secretsFile = secretsFileTemplate.Replace("{ENVIRONMENT}.", string.Empty);
var secretsFileOverride = secretsFileTemplate.Replace("{ENVIRONMENT}.", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
builder.Configuration.AddYamlFile(secretsFile, optional: false, reloadOnChange: false);
builder.Configuration.AddYamlFile(secretsFileOverride, optional: true, reloadOnChange: true);

builder.Services.Configure<List<MyJobConfiguration>>(builder.Configuration.GetSection(ConfigurationKeys.MyJobs));

// Hangfire
var connectionString = builder.Configuration.GetConnectionString(ConnectionStrings.Default);
builder.Services.AddHangfire(x => x.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
{
    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
    QueuePollInterval = TimeSpan.Zero,
    UseRecommendedIsolationLevel = true
}));
builder.Services.AddScoped(_ => JobStorage.Current.GetConnection());
builder.Services.AddHangfireServer();

// Serilog
const string format =
    "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}";
builder.Logging.AddSerilog();
builder.Host.UseSerilog((hostBuilderContext, serviceProvider, configuration) =>
{
    configuration
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: format, theme: AnsiConsoleTheme.Literate);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHangfireDashboard("/mydashboard");
app.UseSerilogRequestLogging();

app.Run();