using CommandLine;
using Microsoft.SharePoint.Client;
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

        public static void RunCSOM(string args, SPFunctions Functions)
        {
            var ConnectionOptions = SPAuth.GetAuth(args);

            var argsArr = args.ModParams().Split(' ');
            var extoptions = new ExtendedOptions();

            Parser.Default.ParseArguments(argsArr, extoptions);

            dynamic LoadedSettings = Extentions.LoadSettings(extoptions.configPath);
            extoptions.Options = ConnectionOptions;

            if (LoadedSettings != null)
            {
                var ExecuteParams = LoadedSettings.executeParams;

                var forcePrompts = (LoadedSettings.forcePrompts != null && LoadedSettings.forcePrompts) || ExecuteParams == null;
                if (forcePrompts)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write("? ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Enter the keys of functions to execute with a space like a delimiter");
                    if (ExecuteParams != null)
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write(" (" + ExecuteParams.ToString() + ")");                        
                    }
                    Console.ForegroundColor = ConsoleColor.White;

                    var ConsoleValue = Console.ReadLine();
                    if (ConsoleValue == null)
                    {
                        ConsoleValue = "";
                    }
                    LoadedSettings.executeParams = ConsoleValue;

                    Console.ResetColor();
                    Extentions.SaveSettings(LoadedSettings, extoptions.configPath);
                }

                string ExecuteParamsString = LoadedSettings.executeParams.ToString();

                List<string> FunctionsToExecute = ExecuteParamsString.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();

                ConnectionOptions.SharePointCSOM(ctx =>
                {
                    extoptions.Ctx = ctx;

                    FunctionsToExecute.ForEach(FunctionName =>
                    {
                        var Function = Functions.Where(k => k.Key.ToLower() == FunctionName.ToLower()).FirstOrDefault();
                        if (Function.Value != null)
                        {
                            Function.Value(extoptions);
                        }
                    });
                });

            }

        }
        public static void RunCSOM(SPFunctions Functions)
        {
           


            /*var FunctionsToExecute = options.ExecuteParams.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();

            FunctionsToExecute.ForEach(FunctionName =>
            {
                var Function = Functions.Where(k => k.Key.ToLower() == FunctionName.ToLower()).FirstOrDefault();
                if (Function.Value != null)
                {
                    Function.Value(options);
                }
            });*/
        }
        public static void SharePointRest(this SPAuthN.Options options, string RequestUrl, Action<Stream> OnSuccess)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(options.SiteUrl + RequestUrl);
            request.Accept = "application/json;odata=verbose";

            foreach (var key in options.Headers.AllKeys)
            {
                request.Headers[key] = options.Headers[key];
            }

            using (HttpWebResponse spResponse = (HttpWebResponse)request.GetResponse())
            {
                using (Stream spResponseStream = spResponse.GetResponseStream())
                {
                    OnSuccess(spResponseStream);
                }
            }
        }

        public static void SharePointCSOM(this SPAuthN.Options options, Action<ClientContext> OnSuccess)
        {
            using (var clientContext = new ClientContext(options.SiteUrl))
            {
                Request.ApplyAuth<WebRequestEventArgs>(clientContext, options);

                OnSuccess(clientContext);
            }
        }
    }
}
