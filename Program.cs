using apiBukLitoprocess.Clases;
using apiBukLitoprocess.conf;
using apiBukLitoprocess.Data;
using apiBukLitoprocess.repository.implementation;
using apiBukLitoprocess.repository.interfaces;
using apiBukLitoprocess.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Logger(sqlLogger => sqlLogger
            .Filter.ByIncludingOnly(Matching.FromSource("SqlQueries"))
            .WriteTo.File(
                "logs/sql-queries-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHttpClient<RestClientService>();
builder.Services.AddSingleton<DbConnectionFactory>();
builder.Services.AddScoped<ColaboradorService>();
builder.Services.AddScoped<AsistenciaService>();

builder.Services.AddScoped<IColaboradorRepository, ColaboradorRepository>();
builder.Services.AddScoped<IAsistenciaRepository, AsistenciaRepository>();

builder.Services.AddHttpClient(ApiClientNames.Buk, (sp, client) =>
{
    var settings = builder.Configuration.GetSection("BukApiSettings").Get<ApiSettings>()!;
    client.BaseAddress = new Uri(settings.Url_API);
    client.DefaultRequestHeaders.Add("auth_token", settings.Token);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient(ApiClientNames.Asistencia, (sp, client) =>
{
    var settings = builder.Configuration.GetSection("AsistenciaApiSettings").Get<ApiSettings>()!;
    client.BaseAddress = new Uri(settings.Url_API);
    client.DefaultRequestHeaders.Add("token", settings.Token);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Registrar RestClientService que usa IHttpClientFactory
builder.Services.AddScoped<RestClientService>();

//builder.Services.AddHostedService<OutBoxWorker>();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors(
    builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.MapControllers();

app.Run();
