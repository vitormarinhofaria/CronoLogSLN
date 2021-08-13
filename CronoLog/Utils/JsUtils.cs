using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace CronoLog.Utils
{
    public static class JsUtils
    {
        public async static void SaveAs(IJSRuntime js, string filename, byte[] data)
        {
            await js.InvokeVoidAsync(
            "saveAsFile",
            filename,
            Convert.ToBase64String(data));
        }
        public async static Task ShowMarkdown(IJSRuntime jS)
        {
            await jS.InvokeVoidAsync("showMarkdown", null);
        }
    }
}