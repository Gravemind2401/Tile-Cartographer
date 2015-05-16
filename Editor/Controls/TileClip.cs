using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using TileCartographer.Library;

namespace TileCartographer.Controls
{
    public class TileClip
    {
        public ClipOrigin Origin;
        public Rectangle Section;
        public Image SectionImg; //use this image to draw over the canvas (saves looking up sections of the tileset)
        public BytePoint2D[,] Data;
    }

    public enum ClipOrigin
    {
        Tileset,
        Canvas,
        MultiTiles,
        Collision,
    }
}
