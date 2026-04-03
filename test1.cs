using System;
using System.Linq;
using System.Reflection;

public static class Test1
{
    public static void RunTest()
    {
        var file = @"C:\Users\JoeRomstadt\source\repos\CursilloWeb3\bin\Debug\net10.0\DevExpress.Blazor.v25.2.dll";
        try
        {
            var asm = Assembly.LoadFrom(file);
            var components = asm.GetTypes().Where(t => typeof(Microsoft.AspNetCore.Components.IComponent).IsAssignableFrom(t)).ToList();
            var htmlEditorComponents = components.Where(t => t.Name.Contains("HtmlEditor")).ToList();
            foreach (var t in htmlEditorComponents)
            {
                Console.WriteLine(t.Name);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
