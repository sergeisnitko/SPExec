using SPAuthN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPExec
{
    public class SPFunctions : List<SPFunction>
    {
        public void Add(string Key, string Description, Action<ExtendedOptions> Void)
        {
            this.Add(new SPFunction
            {
                Key = Key,
                Description = Description,
                Void = Void
            });
        }
        public void Add(string Key, Action<ExtendedOptions> Void)
        {
            this.Add(new SPFunction
            {
                Key = Key,
                Description = "",
                Void = Void
            });
        }
    }

    public class SPFunction
    {
        public string Key;
        public string Description;
        public Action<ExtendedOptions> Void;
    }
}
