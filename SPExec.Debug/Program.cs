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
                { "full", options=>
                    {
                        var l = "";
                    }
                },
                { "divarts", options=>
                    {
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
                { "divlists", options=>
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

            SharePoint.RunCSOM("--configPath='./configs/private.prod.json' --forcePrompts=true", fun);

            var t = "";
        }
    }
}
