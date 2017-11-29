using CommandLine;
using Microsoft.SharePoint.Client;
using SPAuthN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPExec
{
    public class ExtendedOptions
    {
        [Option("configPath")]
        public string configPath { get; set; }

        [Option("forcePrompts")]
        public bool forcePrompts { get; set; }

        public ClientContext Context { get; set; }

        public Options Options { get; set; }

        public dynamic LoadedSettings { get; set; }

    }
}
