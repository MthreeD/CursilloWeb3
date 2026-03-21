$files = Get-ChildItem -Path ~/.nuget/packages -Recurse -Filter DevExpress.Blazor.RichEdit.v25.1.dll
if ($files.Count -gt 0) {
    $dllInfo = [System.Reflection.Assembly]::LoadFrom($files[-1].FullName)
    $dllInfo.GetTypes() | Where-Object { $_.Name -like '*Ribbon*' -or $_.Name -like '*Tab*' } | Select-Object Name
}
