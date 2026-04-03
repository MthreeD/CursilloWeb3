namespace CursilloWeb.Data;

public class ApplicationSettings
{
    public Guid Id { get; set; }

    // Global Settings
    public bool ShowDebuggingPages { get; set; } = false;

    // Page Visibility Settings
    public bool ShowHomePage { get; set; } = true;
    public bool ShowArticleDetailsPage { get; set; } = true;
    public bool ShowCounterPage { get; set; } = true;
    public bool ShowWeatherPage { get; set; } = true;
    public bool ShowTestPage { get; set; } = false;
    public bool ShowTest2Page { get; set; } = false;
    public bool ShowDebugPage { get; set; } = false;
    public bool ShowAdminCleanupTestPage { get; set; } = false;
    public bool ShowFileUploadPage { get; set; } = true;
    public bool ShowWebmasterSettingsPage { get; set; } = true;

    // Admin Pages
    public bool ShowDashboardPage { get; set; } = true;
    public bool ShowManageArticlePage { get; set; } = true;
    public bool ShowManageContentPage { get; set; } = true;
    public bool ShowManageFooterPage { get; set; } = true;
    public bool ShowNewFooterEditPage { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}
