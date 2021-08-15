using System;
using System.IO;
using CronoLog.Utils;

namespace CronoLog
{
    class CodeGen
    {
        public static void GenAll()
        {
            GenJSPropFile();
        }
        private static void GenJSPropFile()
        {
            // const TRELLO_TIMER = { API_URL: "https://trello-timer.herokuapp.com" }
            // const CRONO_LOG_CDN = "https://trello-timer.herokuapp.com"
            string[] fileContent = {
                "const TRELLO_TIMER = { API_URL: '" + ApiUtils.API_URL + "' }",
                "const CRONO_LOG_CDN = '" + ApiUtils.API_URL + "'"
            };
            File.WriteAllLines("./wwwroot/tpu/js/gen/gen_global.js", fileContent);
        }
    }
}