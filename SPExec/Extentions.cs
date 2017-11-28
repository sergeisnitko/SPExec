using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPExec
{
    public static class Extentions
    {
        public static string ModParams(this string Params)
        {
            Params = Params
                .Replace("=true", "")
                .Replace("=false", "")
                .Replace("='true'", "")
                .Replace("='false'", "")
                .Replace("=True", "")
                .Replace("=False", "")
                .Replace("='True'", "")
                .Replace("='False'", "")
                ;
            return Params;
        }
        public static string ModPath(this string SettingsFileName)
        {
            SettingsFileName = SettingsFileName
                .Replace("./", "")
                .Replace("'", "")
                .Replace("/", "\\")
                ;
            SettingsFileName = Path.Combine(SharePoint.SystemPath, SettingsFileName);
            return SettingsFileName;
        }
        public static dynamic LoadSettings(string SettingsFileName)
        {
            SettingsFileName = SettingsFileName.ModPath();

            if (System.IO.File.Exists(SettingsFileName))
            {
                return JsonConvert.DeserializeObject<dynamic>(LoadDataFromFile(SettingsFileName));
            }
            return null;
        }
        public static void SaveSettings(dynamic RunSettings, string SettingsFileName)
        {
            SettingsFileName = SettingsFileName.ModPath();

            var json = JsonConvert.SerializeObject(RunSettings,Formatting.Indented);
            var SW = File.CreateText(SettingsFileName);
            SW.WriteLine(json);
            SW.Close();
        }

        public static string LoadDataFromFile(string SourcePath)
        {
            if (System.IO.File.Exists(SourcePath))
            {
                var StringData = string.Join("\n\r", File.ReadAllLines(SourcePath));
                return StringData;
            }
            return "{}";
        }
    }
}
