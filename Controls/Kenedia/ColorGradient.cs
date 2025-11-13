using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kenedia.Modules.Core.Structs
{
    public struct ColorGradient
    {
        public Color Start { get; set; }

        public Color End { get; set; }

        public ColorGradient(Color start, Color? end = null)
        {
            Start = start;
            End = end ?? Color.Transparent;
        }
    }
}
