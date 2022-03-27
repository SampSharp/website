using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SampSharp.Documentation;
using SampSharp.Documentation.Configuration;
using SampSharp.Documentation.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<RepositoryOptions>(builder.Configuration.GetSection("Repository"));
builder.Services.Configure<StorageOptions>(builder.Configuration.GetSection("Storage"));
builder.Services.Configure<ImportOptions>(builder.Configuration.GetSection("Import"));

builder.Services.AddTransient<IDataImportService, DataImportService>();
builder.Services.AddSingleton<IVersionBuilder, VersionBuilder>();
builder.Services.AddSingleton<DataImportService>();
builder.Services.AddSingleton<IDataRepository, DataRepository>();
builder.Services.AddTransient<IGithubDataRepository, GithubDataRepository>();

var app = builder.Build();

if (app.Services.GetRequiredService<IDataRepository>().IsEmpty)
{
    var options = app.Services.GetRequiredService<IOptions<RepositoryOptions>>();
    app.Logger.LogInformation($"Collecting initial docs from {options.Value.Owner}/{options.Value.Name} (secret: {options.Value.Secret})");
    app.Services.GetRequiredService<IDataImportService>().ImportAllBranches();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/__internal/error/server_error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "home",
    pattern: "/",
    new
    {
        controller = "Home",
        action = "Index"
    });

app.MapControllerRoute(
    name: "server_error",
    pattern: "/__internal/error/server_error",
    new
    {
        controller = "Error",
        action = "ServerError"
    });

app.MapControllerRoute(
    name: "not_found",
    pattern: "/__internal/error/not_found",
    new
    {
        controller = "Error",
        action = "PageNotFound"
    });

app.MapControllerRoute(
    name: "webhook_manual_all_branches",
    pattern: "/webhook/all",
    new
    {
        controller = "WebHook",
        action = "ImportAllBranches"
    });

app.MapControllerRoute(
    name: "webhook_github",
    pattern: "/webhook/github",
    new
    {
        controller = "WebHook",
        action = "GitHub"
    });

app.MapControllerRoute(
    name: "documentation",
    pattern: "{versionOrPage?}/{*page}",
    new
    {
        controller = "Documentation",
        action = "Index"
    });

app.Run();