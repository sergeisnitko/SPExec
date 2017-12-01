﻿using Newtonsoft.Json;
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
        public static string ExecuteParamsDescription = "Keys of functions to execute with a space like a delimiter";

        public static void ExecuteMappedFunctions(this ExtendedOptions ExtOptions, SPFunctions Functions)
        {
            string ExecuteParamsString = ExtOptions.LoadedSettings.custom.executeParams.ToString();

            List<string> FunctionsToExecute = ExecuteParamsString.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();

            FunctionsToExecute.ForEach(FunctionName =>
            {
                var Function = Functions.Where(k => k.Key.ToLower() == FunctionName.ToLower()).FirstOrDefault();
                if (Function.Value != null)
                {
                    Function.Value(ExtOptions);
                }
            });
        }
        public static List<T> ConvertToData<T>(this Stream s)
        {
            dynamic ListJsonData = s.ConvertToJSON();

            var text = JsonConvert.SerializeObject(ListJsonData.d.results);

            return JsonConvert.DeserializeObject<List<T>>(text);
        }

        public static dynamic ConvertToJSON(this Stream s)
        {
            var reader = new StreamReader(s);
            dynamic ListJsonData = JsonConvert.DeserializeObject(reader.ReadToEnd());

            return ListJsonData;
        }

        public static string ModParams(this string Params)
        {
            Params = Params
                .Replace("=true", "")
                .Replace("='true'", "")
                .Replace("=True", "")
                .Replace("='True'", "")
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

        public static String GetConsoleValue()
        {
            var Value = "";
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (Value.Length > 0)
                    {
                        Value = Value.Substring(0, Value.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else
                {
                    Value += i.KeyChar;
                    Console.Write(i.KeyChar);
                }
            }

            return Value;
        }

        public static string InlineParam(string Description, string DefaultValue, bool WaitInput=true)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(WaitInput ? "? ": "! ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Description+" ");

            var ConsoleValue = DefaultValue;

            if (WaitInput)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                if (!String.IsNullOrEmpty(DefaultValue.ToString()))
                {
                    Console.Write("(" + DefaultValue.ToString() + ") ");
                }

                ConsoleValue = GetConsoleValue();
                if (ConsoleValue == null)
                {
                    ConsoleValue = "";
                }
                if (!String.IsNullOrEmpty(ConsoleValue))
                {
                    for (int i = 0, ilen = ConsoleValue.Length; i < ilen; i += 1)
                    {
                        Console.Write("\b \b");
                    }
                }
                if (String.IsNullOrEmpty(ConsoleValue) && !String.IsNullOrEmpty(DefaultValue.ToString()))
                {
                    ConsoleValue = DefaultValue.ToString();
                }
            }


            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(ConsoleValue);
            Console.ResetColor();

            return ConsoleValue;
        }

        public static void EchoParams(ExtendedOptions Options)
        {
            Console.Clear();
            InlineParam("SharePoint URL", Options.LoadedSettings.siteUrl.ToString(), false);
            Console.WriteLine();
            InlineParam("Strategy", Options.LoadedSettings.strategy.ToString(), false);
            Console.WriteLine();
            InlineParam("User name", Options.LoadedSettings.username.ToString(), false);
            Console.WriteLine();
            InlineParam(ExecuteParamsDescription, Options.LoadedSettings.custom.executeParams.ToString(), false);
            Console.WriteLine();
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