using CommandLine;
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

        public static void RunCSOM(string args, SPFunctions Functions)
        {
            GetParams(args, ExtOptions =>
            {
                string ExecuteParamsString = ExtOptions.LoadedSettings.custom.executeParams.ToString();

                List<string> FunctionsToExecute = ExecuteParamsString.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).ToList();

                ExtOptions.SharePointCSOM(ctx =>
                {
                    ExtOptions.Context = ctx;

                    FunctionsToExecute.ForEach(FunctionName =>
                    {
                        var Function = Functions.Where(k => k.Key.ToLower() == FunctionName.ToLower()).FirstOrDefault();
                        if (Function.Value != null)
                        {
                            Function.Value(ExtOptions);
                        }
                    });
                });
            });
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

        public static void SharePointCSOM(this ExtendedOptions ExtendedOptions, Action<ClientContext> OnSuccess)
        {
            using (var clientContext = new ClientContext(ExtendedOptions.Options.SiteUrl))
            {
                Request.ApplyAuth<WebRequestEventArgs>(clientContext, ExtendedOptions.Options);

                OnSuccess(clientContext);
            }
        }

        public static void GetParams(string args, Action<ExtendedOptions> OnSuccess)
        {
            var ConnectionOptions = SPAuth.GetAuth(args);

            var argsArr = args.ModParams().Split(' ');
            var extoptions = new ExtendedOptions();

            Parser.Default.ParseArguments(argsArr, extoptions);

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
    }
}
