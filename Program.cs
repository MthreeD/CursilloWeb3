using CursilloWeb.Components;
using CursilloWeb.Data;
using CursilloWeb.Services;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDevExpressBlazor();
builder.Services.AddDevExpressAI();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromSeconds(3);
    });

builder.Services.AddScoped<ArticleService>();
builder.Services.AddScoped<ContentService>();
builder.Services.AddScoped<DatabaseFixService>();
builder.Services.AddScoped<WebmasterSettingsService>();
// Register browser lifecycle services to stop application when browser closes
builder.Services.AddSingleton<BrowserLifecycleService>();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Server.Circuits.CircuitHandler, ShutdownCircuitHandler>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString), ServiceLifetime.Scoped, ServiceLifetime.Singleton);
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();
// use pictures from local folder
app.UseStaticFiles();


using (var scope = app.Services.CreateScope())
{
    // First fix any database data issues
    var dbFixService = scope.ServiceProvider.GetRequiredService<DatabaseFixService>();
    try
    {
        Console.WriteLine("Checking for database issues...");
        var diagnosis = await dbFixService.DiagnoseContentBlocksAsync();
        Console.WriteLine(diagnosis);

        // Automatically fix binary data issues
        await dbFixService.FixContentBlocksDataAsync();
        Console.WriteLine("Database fix completed.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Database fix failed: {ex.Message}");
    }

    await CursilloWeb.Data.DataSeeder.SeedAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
