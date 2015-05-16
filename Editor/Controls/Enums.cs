using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TileCartographer.Controls
{
    public enum LayerViewMode : byte
    {
        AllLayers = 0,
        CurrentAndBelow = 1,
    }

    public enum LayerEditMode : byte
    {
        Layer1 = 0,
        Layer2 = 1,
        Layer3 = 2,
        Collision = 3
    }

    public enum LayerPaintMode : byte
    {
        Single,
        Rectangle,
        Fill,
        Select,
    }
}
