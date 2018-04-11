using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPExec
{
    public static class Extentions
    {
        public static string ExecuteParamsDescription = "Keys of functions to execute with a space like a delimiter";

        public static string StringValueOrEmpty(this object Value)
        {
            return Value != null ? Value.ToString() : "";
        }
        public static Dictionary<string, object> ConvertExpandoToDict(dynamic expando)
        {
            Dictionary<string, object> expandoDict = null;

            if (expando.GetType() == typeof(JObject))
            {
                expandoDict = (expando as JObject).ToObject<Dictionary<string, object>>();
            }
            if (expando.GetType() == typeof(Dictionary<string, object>))
            {
                expandoDict = expando as Dictionary<string, object>;
            }
            return expandoDict;
        }
        public static dynamic AddExpandoProperty(object expando, string propertyName, object propertyValue)
        {
            Dictionary<string, object> expandoDict = ConvertExpandoToDict(expando);

            if (expandoDict.ContainsKey(propertyName))
            {
                expandoDict[propertyName] = propertyValue;
            }
            else
            {
                expandoDict.Add(propertyName, propertyValue);
            }

            return expandoDict;
        }
        public static dynamic AddExpandoProperty(object expando, string propertyName, bool Dictionary = true)
        {
            Dictionary<string, object> expandoDict = ConvertExpandoToDict(expando);

            //var expandoDict = expando as IDictionary<string, object>;
            if (!expandoDict.ContainsKey(propertyName))
            {
                if (Dictionary)
                {
                    expandoDict.Add(propertyName, new Dictionary<string, object>());
                }
                else
                {
                    expandoDict.Add(propertyName, "");
                }
                
            }
            return expandoDict;
        }
        public static void ExecuteMappedFunctions(this ExtendedOptions ExtOptions, SPFunctions Functions)
        {
            string ExecuteParamsString = ExtOptions.LoadedSettings["custom"]["executeParams"].ToString();

            List<string> FunctionsToExecute = ExecuteParamsString.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();

            FunctionsToExecute.ForEach(FunctionName =>
            {
                var Function = Functions.Where(k => k.Key.ToLower() == FunctionName.ToLower()).FirstOrDefault();
                if (Function != null && Function.Void != null)
                {
                    Function.Void(ExtOptions);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("There is no function for key " + FunctionName);
                    Console.ResetColor();
                }
            });
        }

        public static List<T> ConvertToData<T>(this Stream s)
        {
            dynamic ListJsonData = s.ConvertToJSON();

            var text = JsonConvert.SerializeObject(ListJsonData.d.results);

            return JsonConvert.DeserializeObject<List<T>>(text);
        }

        public static Dictionary<string, Object> CommandLineParse(string args)
        {
            args = args.Replace("--", "-");
            return CommandLineParse(args.Split(new string[] { " -" }, StringSplitOptions.None));
        }
        public static Dictionary<string, Object> CommandLineParse(string[] args)
        {
            var argsObject = new Dictionary<string, Object>();
            args.ToList().ForEach(arg =>
            {
                var paramsArr = arg.Split('=');
                var paramName = paramsArr[0].Replace("--", "").Replace("-", "");
                if (paramsArr.Length > 1)
                {
                    argsObject[paramName] = TrimQuotes(paramsArr[1]);
                }
                else
                {
                    argsObject[paramName] = true;
                }
            });
            return argsObject;
        }

        public static dynamic ConvertToJSON(this Stream s)
        {
            var reader = new StreamReader(s);
            dynamic ListJsonData = JsonConvert.DeserializeObject(reader.ReadToEnd());

            return ListJsonData;
        }
        public static string TrimQuotes(this object InputValue)
        {
            return TrimQuotes((string)InputValue);
        }
        public static string TrimQuotes(this string InputValue)
        {
            if (String.IsNullOrEmpty(InputValue))
                return "";

            InputValue = InputValue.StartsWith("'") ? InputValue.Substring(1) : InputValue;
            InputValue = InputValue.StartsWith("\"") ? InputValue.Substring(1) : InputValue;
            InputValue = InputValue.EndsWith("'") ? InputValue.Substring(0, InputValue.Length - 1) : InputValue;
            InputValue = InputValue.EndsWith("\"") ? InputValue.Substring(0, InputValue.Length - 1) : InputValue;

            return InputValue;

        }
        public static string ModParams(this string Params)
        {
            if (!String.IsNullOrEmpty(Params))
            {
                Params = Params
                    .Replace("=true", "")
                    .Replace("='true'", "")
                    .Replace("=True", "")
                    .Replace("='True'", "")
                    ;
            }

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

        public static string InlineMenu(SPFunctions Functions, string Description, string DefaultValue)
        {
            Console.CursorVisible = false;

            var TopPosition = Console.CursorTop;

            var Value = "";
            var SelectedValue = Functions[0].Key;

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write("? ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Description + " ");
            Console.ForegroundColor = ConsoleColor.Gray;

            if (!String.IsNullOrEmpty(DefaultValue.ToString()))
            {
                Console.Write("(" + DefaultValue.ToString() + ") ");
            }
            
            Console.WriteLine();
            var ConsoleValue = DefaultValue.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Where(x=> Functions.FirstOrDefault(y=>y.Key == x) != null).ToList();
            if (ConsoleValue.Count > 0)
            {
                SelectedValue = ConsoleValue[0];
            }

            var StartedPosition = Console.CursorTop;
            while (true)
            {
                ClearCurrentConsoleLine(StartedPosition);
                var index = 0;
                var selectedIndex = 0;
                foreach (var Function in Functions)
                {
                    ClearCurrentConsoleLine();
                    var Name = Function.Key;
                    var InDescription = Function.Description;
                    
                    if (Name == SelectedValue)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write("> ");
                        selectedIndex = index;
                    }
                    else
                    {
                        Console.Write("  ");
                    }
                    Console.Write(" [");
                    if (ConsoleValue.Contains(Name))
                    {
                        Console.Write("X");
                    }
                    else
                    {
                        Console.Write(" ");
                    }
                    Console.Write("] ");
                    Console.Write(Name);
                    if (!String.IsNullOrEmpty(InDescription))
                    {
                        Console.Write(" (" + InDescription + ") ");
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine();

                    index += 1;
                }

                Console.ResetColor();

                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    Value = String.Join(" ", ConsoleValue.ToArray());
                    ClearCurrentConsoleLine(TopPosition);
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write("? ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(Description + " ");
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write(Value);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine();
                    ClearCurrentConsoleLine(StartedPosition);

                    Functions.ForEach(f=>
                    {
                        ClearCurrentConsoleLine();
                        Console.WriteLine();
                    });
                    Console.SetCursorPosition(0, StartedPosition);

                    break;
                }
                if (i.Key == ConsoleKey.DownArrow)
                {
                    selectedIndex += 1;
                    if (selectedIndex >= Functions.Count)
                    {
                        selectedIndex = 0;
                    }

                    SelectedValue = Functions[selectedIndex].Key;
                }
                if (i.Key == ConsoleKey.UpArrow)
                {
                    selectedIndex -= 1;
                    if (selectedIndex < 0)
                    {
                        selectedIndex = Functions.Count - 1;
                    }
                    SelectedValue = Functions[selectedIndex].Key;
                }
                if (i.Key == ConsoleKey.Spacebar)
                {
                    var CurrentKey = Functions[selectedIndex].Key;
                    if (ConsoleValue.Contains(CurrentKey))
                    {
                        ConsoleValue = ConsoleValue.Where(val => val != CurrentKey).ToList();
                    }
                    else
                    {
                        ConsoleValue.Add(CurrentKey);
                    }
                }
            }
            Console.CursorVisible = true;

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
            Console.WriteLine(ConsoleValue);
            Console.ResetColor();

            return ConsoleValue;
        }

        public static void ClearCurrentConsoleLine(int CursorTop = -1)
        {
            int currentLineCursor = CursorTop != -1 ? CursorTop : Console.CursorTop;
            Console.SetCursorPosition(0, currentLineCursor);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
        public static void EchoParams(ExtendedOptions Options)
        {
            Console.Clear();
            InlineParam("SharePoint URL", Options.LoadedSettings["siteUrl"].ToString(), false);
            Console.WriteLine();
            InlineParam("Strategy", Options.LoadedSettings["strategy"].ToString(), false);
            // Console.WriteLine();
            // InlineParam("User name", Options.LoadedSettings["username"].ToString(), false);
            Console.WriteLine();
            InlineParam(ExecuteParamsDescription, Options.LoadedSettings["custom"]["executeParams"].ToString(), false);
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
