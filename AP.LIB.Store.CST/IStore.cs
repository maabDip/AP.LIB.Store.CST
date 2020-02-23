using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AP.LIB.Store.CST
{
    public interface IStore
    {
        void Import(string catalogAsJson);

        int Quantity(string name);

        double Buy(params string[] basketByNames);
    }
}
