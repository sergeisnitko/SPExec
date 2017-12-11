using Microsoft.SharePoint.Client;
using Newtonsoft.Json.Linq;
using SPAuthN;
using System;
using System.Collections.Generic;
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
            var extoptions = new ExtendedOptions(Extentions.CommandLineParse(argsArr));
            extoptions.configPath = ConnectionOptions.Settings.configPath;
            dynamic LoadedSettings = Extentions.LoadSettings(extoptions.configPath);
            extoptions.Options = ConnectionOptions;

            if (LoadedSettings != null)
            {
                LoadedSettings.custom = LoadedSettings.custom != null ? LoadedSettings.custom : JObject.Parse("{'executeParams': null}");
                var CustomProperties = LoadedSettings.custom;

                var ExecuteParams = CustomProperties.executeParams;
                extoptions.LoadedSettings = LoadedSettings;

                var forcePrompts = extoptions.forcePrompts || ExecuteParams == null;
                if (forcePrompts)
                {
                    CustomProperties.executeParams = Extentions.InlineParam(Extentions.ExecuteParamsDescription, ExecuteParams.ToString());
                    Extentions.SaveSettings(LoadedSettings, extoptions.configPath);
                }
                else
                {
                    Extentions.EchoParams(extoptions);
                }

                OnSuccess(extoptions);
            }
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
