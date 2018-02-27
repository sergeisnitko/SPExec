using Microsoft.SharePoint.Client;
using Newtonsoft.Json.Linq;
using SPAuthN;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SPExec
{
    public static class SharePoint
    {
        public static string SystemPath = HttpUtility.UrlDecode(Path.GetDirectoryName((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath));

        public static void Run(string args, SPFunctions Functions)
        {
            CheckShowHelpInformation(Functions, args, () =>
            {
                Run(args, null, Functions);
            });
            
        }
        public static void Run(string args, Options ConnectionOptions, SPFunctions Functions)
        {
            CheckShowHelpInformation(Functions, args, () =>
            {
                GetParams(args, ConnectionOptions, ExtOptions =>
                {
                    ExtOptions.ExecuteMappedFunctions(Functions);

                });
            });
        }
        public static void RunCSOM(string args, SPFunctions Functions)
        {
            CheckShowHelpInformation(Functions, args, () =>
            {
                RunCSOM(args, null, Functions);
            });
        }
        public static void RunCSOM(string args, Options ConnectionOptions, SPFunctions Functions)
        {
            CheckShowHelpInformation(Functions, args, () =>
            {
                GetParams(args, ConnectionOptions, ExtOptions =>
                {
                    ExtOptions.SharePointCSOM(ctx =>
                    {
                        ExtOptions.Context = ctx;

                        ExtOptions.ExecuteMappedFunctions(Functions);
                    });
                });
            });
        }

        public static void SharePointREST(this ExtendedOptions ExtendedOptions, string RequestUrl, Action<Stream> OnSuccess)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ExtendedOptions.Options.SiteUrl + RequestUrl);
            request.Accept = "application/json;odata=verbose";

            Request.ApplyAuth(request, ExtendedOptions.Options);

            using (HttpWebResponse spResponse = (HttpWebResponse)request.GetResponse())
            {
                using (Stream spResponseStream = spResponse.GetResponseStream())
                {
                    OnSuccess(spResponseStream);
                }
            }
        }

        public static void SharePointCSOM(this ExtendedOptions ExtendedOptions, Action<ClientContext> OnSuccess)
        {
            using (var clientContext = new ClientContext(ExtendedOptions.Options.SiteUrl))
            {
                Request.ApplyAuth<WebRequestEventArgs>(clientContext, ExtendedOptions.Options);

                OnSuccess(clientContext);
            }
        }

        public static void GetParams(string args,Action<ExtendedOptions> OnSuccess)
        {
            var ConnectionOptions = SPAuth.GetAuth(args);
            GetParams(args, ConnectionOptions, OnSuccess);
        }

        public static void GetParams(string args, Options ConnectionOptions, Action<ExtendedOptions> OnSuccess)
        {
            if (ConnectionOptions == null)
            {
                ConnectionOptions = SPAuth.GetAuth(args);
            }            

            var argsArr = args.ModParams().Split(' ');
            var ParsedArgs = Extentions.CommandLineParse(argsArr);

            var extoptions = new ExtendedOptions(ParsedArgs);
            extoptions.configPath = ConnectionOptions.Settings.configPath;
            dynamic LoadedSettings = Extentions.LoadSettings(extoptions.configPath);
            extoptions.Options = ConnectionOptions;

            if (LoadedSettings != null)
            {

                LoadedSettings = Extentions.AddExpandoProperty(LoadedSettings, "custom");
                var CustomProperties = Extentions.AddExpandoProperty(LoadedSettings["custom"], "executeParams", false);

                
                var ExecuteParams = CustomProperties["executeParams"];

                foreach (var ParsedArg in ParsedArgs)
                {
                    if (ParsedArg.Key.ToLower().IndexOf("custom.") != -1)
                    {
                        //CustomProperties.Add(ParsedArg.Key.Replace("custom.","").Trim(), ParsedArg.Value);
                        CustomProperties = Extentions.AddExpandoProperty(CustomProperties, ParsedArg.Key.Replace("custom.", "").Trim(), ParsedArg.Value);
                    }
                }
                LoadedSettings["custom"] = CustomProperties;


                extoptions.LoadedSettings = LoadedSettings;

                var forcePrompts = extoptions.forcePrompts || String.IsNullOrEmpty(ExecuteParams);
                if (forcePrompts)
                {
                    List<string> CustomPropertiesKeys = new List<string>(CustomProperties.Keys);

                    foreach (var CustomPropertyKey in CustomPropertiesKeys)
                    {
                        var Description = CustomPropertyKey == "executeParams" ? Extentions.ExecuteParamsDescription : CustomPropertyKey;
                        CustomProperties[CustomPropertyKey] = Extentions.InlineParam(Description, CustomProperties[CustomPropertyKey].ToString());
                    }                                        
                }
                else
                {
                    Extentions.EchoParams(extoptions);
                }
                Extentions.SaveSettings(LoadedSettings, extoptions.configPath);

                OnSuccess(extoptions);
            }
        }

        public static void EnsureCustomParam(this ExtendedOptions ExOptions, string ParamName)
        {
            var ConnectionOptions = ExOptions.Options;

            dynamic LoadedSettings = Extentions.LoadSettings(ConnectionOptions.Settings.configPath);
            dynamic CustomProperties = LoadedSettings["custom"];
            // var forcePrompts = ConnectionOptions.Settings.forcePrompts;


            ParamName = ParamName.Replace("custom.", "");

            var CustomPropertiesDict = Extentions.ConvertExpandoToDict(CustomProperties);
            var CurrentValue = "";

            if (!CustomPropertiesDict.ContainsKey(ParamName))
            {
                CustomProperties[ParamName] = Extentions.InlineParam(ParamName, CurrentValue);
            }

            Extentions.SaveSettings(LoadedSettings, ConnectionOptions.Settings.configPath);
            ExOptions.LoadedSettings = LoadedSettings;
        }
        public static void CheckShowHelpInformation(this SPFunctions Functions, string args, Action Void)
        {
            var argsArr = Extentions.CommandLineParse(args);
            var help = argsArr.Where(k => (k.Key.ToLower() == "help" || k.Key.ToLower() == "?")).FirstOrDefault();
            if (help.Key != null)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("'ExecuteParams' functions:");

                foreach (var Function in Functions)
                {
                    var Name = Function.Key;
                    var Description = Function.Description;
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write(Name);
                    if (!String.IsNullOrEmpty(Description))
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(" - " + Description);
                    }
                    Console.WriteLine();
                }

                Console.ResetColor();
            }
            else
            {
                Void();
            }
        }
    }
}
