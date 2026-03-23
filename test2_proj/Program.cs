using System;
using System.Linq;

namespace Test2Proj
{
    public class TestProgram
    {
        public static void RunTest()
        {
            var obj = Activator.CreateInstance(typeof(DevExpress.Blazor.HtmlEditorToolbarItemNames));
            var fontNameProperty = typeof(DevExpress.Blazor.HtmlEditorToolbarItemNames).GetProperty("FontName");
            if (fontNameProperty != null)
            {
                Console.WriteLine(fontNameProperty.GetValue(obj));
            }
        }
    }
}
