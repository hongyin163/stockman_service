using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StockMan.EntityModel;
namespace StockMan.EntityModel
{
    public enum IndexState
    {
        Up = 1,
        Down = -1,
        Warn = 0
    }

    public enum IndexEnableState
    {
     
        Disable=0,
        Running = 1
    }

}
