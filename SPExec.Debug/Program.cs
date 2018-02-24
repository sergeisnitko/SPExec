using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPExec.Debug
{
    class Program
    {
        static void Main(string[] args)
        {

            var fun = new SPFunctions()
            {
                { "artefacts", options=>
                    {
                        var l = "";
                    }
                },
                { "data", options=>
                    {
                        var l = "";
                    }
                },
                { "full", "KJNKJNKJN", options=>
                    {
                        var l = "";
                    }
                },
                { "divarts", "Description of divarts",options=>
                    {

                        options.EnsureCustomParam("Test4");

                        var MyCustomArg2 = options.LoadedSettings["custom"]["MyCustomArg2"];
                        var MyCustomArg5 = options.LoadedSettings["custom"]["MyCustomArg5"];
                        var l = "";
                    }
                },
                { "divfields", options=>
                    {
                        var l = "";
                    }
                },
                { "divct", options=>
                    {
                        var l = "";
                    }
                },
                { "divlists", "Description of divlists", options=>
                    {
                        var l = "";
                    }
                },
                { "divquicklaunch", options=>
                    {
                        var l = "";
                    }
                },
                { "test_provision", options=>
                    {
                        var l = "";
                    }
                }
            };
            
            //SharePoint.Run("--forcePrompts='true' --custom.MyCustomArg6='dasdasdasdasdasdsa' --siteUrl='http://ssssss'", fun);

            var t = "";
        }
    }
}