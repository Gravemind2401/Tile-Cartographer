using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using TileCartographer.Library;

namespace TileCartographer.Controls
{
    public delegate void CollisionValueChangedEventHandler(object sender, byte newValue);
    //public delegate void SelectionChangedEventHandler(object sender, Rectangle selection);
    //public delegate void HoverPointChangedEventHandler(object sender, BytePoint2D point);
    public delegate void LayerEditModeChangedEventHandler(object sender, LayerEditMode mode);
    public delegate void PointsChangedEventHandler(object sender);
    public delegate void IndexChangedEventHandler(object sender, byte index);
    public delegate void UndoCountChangedEventHandler(object sender, int count);
    public delegate void RedoCountChangedEventHandler(object sender, int count);
    public delegate void SelectedMapChangedEventHandler(object sender, TCMap map);
    public delegate void MapPropertyChangedEventHandler(object sender, TCMap map, PropertyValueChangedEventArgs e);
}
