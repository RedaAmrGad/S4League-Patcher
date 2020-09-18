using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S4Launcher
{

    public class Texts
    {
        public enum Keys
        {
            UNKNOWNERROR,
            MISSINGBINARY,
            MODIFIEDBINARY,
            WRONGFOLDER,
            SUPDATE
        }

        public static Dictionary<Keys, string> Text = new Dictionary<Keys, string>
        {
            {
                Keys.UNKNOWNERROR,
                "A critical error occured...error message which can help to solve the problem: \n{0}"
            },
            {
                Keys.MISSINGBINARY,
                "The game cannot be started, because the {0} is missing."
            },
                {
                Keys.MODIFIEDBINARY,
                "The game cannot be started, because the {0} is Modified."
            },
            {
                Keys.WRONGFOLDER,
                "Patcher is not in s4 folder. Download S4P Official Client!"
       
            },
            {
            Keys.SUPDATE,
            "S4Patcher SelfUpdate is {0}"
            }
        };



    }
}
    
