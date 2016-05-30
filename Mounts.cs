using ScriptSDK;
using ScriptSDK.Data;
using ScriptSDK.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailRunner
{

    [QuerySearch(new ushort[] {
       0x20F6, 0x20DD, 0x2619, 0x2D9C, 0x25CE , 0x2615, 0x2135     })]
    public class Mounts : Item
    {
        public Mounts(Serial serial)
            : base(serial)
        {
        }
    }
}
