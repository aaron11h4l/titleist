using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptSDK;
using System.Text.RegularExpressions;
using System.IO;
using ScriptSDK;
using ScriptSDK.Data;
using ScriptSDK.Engines;
using ScriptSDK.Gumps;
using ScriptSDK.Items;
using ScriptSDK.Utils;

namespace RailRunner
{
    [QuerySearch(new ushort[] {
       0x22C5 })]


    public class Runebook : Item
    {
 
 public Runebook(Serial serial)
            : base(serial)
        {
        }
    }
}
