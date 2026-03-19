using DevExpress.Blazor;

namespace CursilloWeb.Services;

public class ThemeState
{
    public ITheme SelectedTheme { get; set; } = DevExpress.Blazor.Themes.BlazingBerry;
}
