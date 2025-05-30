using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.License;
using Codeer.LowCode.Blazor.SystemSettings;
using LowCodeApp.Server.Services;
using LowCodeApp.Server.Services.DataChangeHistory;
using LowCodeApp.Server.Services.FileManagement;
using PdfSharp.Fonts;
using System.Globalization;
using System.Text.Json.Serialization;
using ApexCharts;
using Codeer.LowCode.Bindings.ApexCharts;

ApexChartsServerInitializer.Initialize();

var builder = WebApplication.CreateBuilder(args);

GlobalFontSettings.FontResolver = new CustomFontResolver();

LicenseManager.DomainLicense = builder.Configuration.GetSection("DomainLicense").Get<string>() ?? string.Empty;
LicenseManager.IsAutoUpdate = builder.Configuration.GetSection("IsLicenseAutoUpdate").Get<bool>();
SystemConfig.Instance.UseHotReload = builder.Configuration.GetSection("UseHotReload").Get<bool>();
SystemConfig.Instance.DataSources = builder.Configuration.GetSection("DataSources").Get<DataSource[]>() ?? [];
SystemConfig.Instance.FileStorages = builder.Configuration.GetSection("FileStorages").Get<FileStorage[]>() ?? [];
SystemConfig.Instance.DataChangeHistoryTableInfo = builder.Configuration.GetSection("DataChangeHistoryTableInfo").Get<DataChangeHistoryTableInfo[]>() ?? [];
SystemConfig.Instance.TemporaryFileTableInfo = builder.Configuration.GetSection("TemporaryFileTableInfo").Get<TemporaryFileTableInfo[]>() ?? [];
SystemConfig.Instance.DesignFileDirectory = builder.Configuration["DesignFileDirectory"] ?? string.Empty;
SystemConfig.Instance.FontFileDirectory = builder.Configuration["FontFileDirectory"] ?? string.Empty;
SystemConfig.Instance.MailSettings = builder.Configuration.GetSection("MailSettings").Get<MailSettings>() ?? new();
SystemConfig.Instance.DataSources.ToList().ForEach(e => e.ConnectionString = builder.Configuration.GetConnectionString(e.Name) ?? string.Empty);
SystemConfig.Instance.FileStorages.ToList().ForEach(e => e.ConnectionString = builder.Configuration.GetConnectionString(e.Name) ?? string.Empty);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddApexCharts();

builder.Services.AddControllers()
      .AddJsonOptions(options =>
      {
          options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
          options.JsonSerializerOptions.Converters.AddJsonConverters();
      });

if (SystemConfig.Instance.UseHotReload)
{
    builder.Services.AddSignalR();
    builder.Services.AddHostedService<FileWatcherService>();
}

//Localize
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("ja-JP")
    };
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

app.UseRequestLocalization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

if (SystemConfig.Instance.UseHotReload)
{
    app.MapHub<HotReloadHub>("/hot_reload_hub");
}

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

// Exception handling.
app.UseExceptionHandlerSendToFront();
app.Run();
