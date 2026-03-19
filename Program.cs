using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using CursilloWeb.Components;
using CursilloWeb.Components.Account;
using CursilloWeb.Data;
using CursilloWeb.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDevExpressBlazor();

builder.Services.AddHttpContextAccessor();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<CursilloWeb.Services.ArticleService>();
builder.Services.AddScoped<CursilloWeb.Services.ContentService>();
builder.Services.AddScoped<CursilloWeb.Services.ThemeState>();
builder.Services.AddScoped<CircuitHandler, ShutdownCircuitHandler>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString), ServiceLifetime.Scoped, ServiceLifetime.Singleton);
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 0;
        options.Password.RequiredUniqueChars = 0;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Add XAF Security System
builder.Services.AddXafSecurity(options => {
    options.RoleType = typeof(PermissionPolicyRole);
    options.UserType = typeof(PermissionPolicyUser);
}).AddAuthenticationStandard(options => {
    options.IsSupportChangePassword = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
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

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.MapPost("/Account/PopupLogin", async (
    HttpContext context,
    [Microsoft.AspNetCore.Mvc.FromServices] SignInManager<ApplicationUser> signInManager,
    [Microsoft.AspNetCore.Mvc.FromForm] string username,
    [Microsoft.AspNetCore.Mvc.FromForm] string? password,
    [Microsoft.AspNetCore.Mvc.FromForm] string returnUrl) =>
{
    var result = await signInManager.PasswordSignInAsync(username, password ?? "", false, lockoutOnFailure: false);

    // Ensure returnUrl is local to prevent open redirect attacks and remove leading ~ or / to avoid ~//
    if (string.IsNullOrEmpty(returnUrl) || !returnUrl.StartsWith("/"))
    {
        returnUrl = "/";
    }

    if (result.Succeeded)
    {
        return Microsoft.AspNetCore.Http.TypedResults.LocalRedirect(returnUrl);
    }

    var sep = returnUrl.Contains('?') ? "&" : "?";
    return Microsoft.AspNetCore.Http.TypedResults.LocalRedirect($"{returnUrl}{sep}LoginFailed=true");
});

app.Run();
