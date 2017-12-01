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
                        options.SharePointREST("/_api/web/lists", Stream =>
                        {
                            var k = Stream.ConvertToJSON();
                        });
                        /*var ctx = options.Context;
                        var Web = ctx.Web;
                        ctx.Load(Web);
                        ctx.ExecuteQuery();*/

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

            SharePoint.Run("--configPath='./configs/private.prod3.json' --forcePrompts=false", fun);

            var t = "";
        }
    }
}
