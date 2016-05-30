using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailRunner
{
    class Points
    {
        private List<ushort> _xPoints = new List<ushort>();
        private List<ushort> _yPoints = new List<ushort>();

        public List<ushort> xPoints
        {
            get { return this._xPoints; }
            set { this._xPoints = value; }
        }
        public List<ushort> yPoints
        {
            get { return this._yPoints; }
            set { this._yPoints = value; }
        }
      
    }
}
