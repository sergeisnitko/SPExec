using Microsoft.SharePoint.Client;
using SPAuthN;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPExec
{
    public class ExtendedOptions
    {
        public string configPath { get; set; }

        public bool forcePrompts { get; set; }

        public ClientContext Context { get; set; }

        public Stream Stream { get; set; }

        public Options Options { get; set; }

        public dynamic LoadedSettings { get; set; }

        Dictionary<string, object> CmdArgs { get; set; }

        public ExtendedOptions()
        {

        }
        public ExtendedOptions(Dictionary<string, object> Args)
        {
            CmdArgs = Args;

            Args.Where(arg => arg.Key == "configPath").ToList().ForEach(arg =>
            {
                if (!String.IsNullOrEmpty((string)arg.Value))
                {
                    configPath = (string)arg.Value;
                };
            });

            Args.Where(arg => arg.Key == "forcePrompts").ToList().ForEach(arg =>
            {
                bool tryForce = false;
                Boolean.TryParse(arg.Value.ToString(), out tryForce);
                forcePrompts = tryForce;

            });
        }
        public string GetCmdValue(string key)
        {
            return (string)CmdArgs.Where(arg => arg.Key == key).FirstOrDefault().Value;
        }

    }
}
