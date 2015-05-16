using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TileCartographer.Library;

namespace TileCartographer.Controls
{
    public partial class PickerBase : UserControl
    {
        private int zoomPower = 5;
        private bool showGrid = false;

        protected bool isMouseOver = false;
        protected bool isMouseDown = false;
        protected bool hideSelection = false;

        protected BytePoint2D selPoint; //which tile the mouse was clicked on
        protected BytePoint2D drgPoint; //which tile the mouse was released on
        protected BytePoint2D hovPoint; //which tile is under the cursor

        protected TCProject tProj;

        protected Image Tilemap; //use Image instead of TCImage to save disk reads (lots of redrawing will be done)

        private float scale { get { return (float)Math.Pow(2, zoomPower) / tSize; } }
        private Rectangle sourceBounds { get { return new Rectangle(0, 0, Tilemap.Width, Tilemap.Height); } }
        private Rectangle viewPortBounds { get { return new Rectangle(0, 0, imgViewport.Image.Width, imgViewport.Image.Height); } }

        protected int tSize { get { return (int)tProj.TileSize; } }
        protected int tScale { get { return (int)(tSize * scale); } }
        protected Rectangle ScaleSelection
        {
            get
            {
                var r = Selection;
                return new Rectangle(r.X * tScale, r.Y * tScale, r.Width * tScale, r.Height * tScale);
            }
        }

        /// <summary>
        /// Allows highlighting tiles as the mouse hovers over them.
        /// </summary>
        public bool HoverHighlighting { get; set; }
        /// <summary>
        /// Renders a grid over the control image.
        /// </summary>
        public bool ShowGrid { get { return showGrid; } set { showGrid = value; Redraw(); } }
        /// <summary>
        /// Gets the currently selected area as a Rectangle.
        /// </summary>
        public Rectangle Selection
        {
            get
            {
                var x = Math.Min(selPoint.X, drgPoint.X);
                var y = Math.Min(selPoint.Y, drgPoint.Y);
                var w = Math.Abs(selPoint.X - drgPoint.X) + 1;
                var h = Math.Abs(selPoint.Y - drgPoint.Y) + 1;

                return new Rectangle(x, y, w, h);
            }
        }
        /// <summary>
        /// The zoom level of the viewport (2 ^ ZoomPower).
        /// </summary>
        public int ZoomPower { get { return zoomPower; } set { zoomPower = Math.Max(3, Math.Min(5, value)); Redraw(); } }

        public PickerBase()
        {
            InitializeComponent();
        }

        public void HideSelection()
        {
            hideSelection = true;
            Redraw();
        }

        public void ShowSelection()
        {
            hideSelection = false;
            Redraw();
        }

        private void DrawGrid(Graphics g)
        {
            var p = new Pen(Color.FromArgb(50, Color.Black), 2);

            for (int x = 0; x <= imgViewport.Image.Width; x += tScale)
                g.DrawLine(p, x, 0, x, imgViewport.Image.Height);

            for (int y = 0; y <= imgViewport.Image.Height; y += tScale)
                g.DrawLine(p, 0, y, imgViewport.Image.Width, y);
        }

        protected void SetPoint(ref BytePoint2D point, int X, int Y)
        {
            point.X = (byte)Math.Max(0, Math.Min(Tilemap.Width / tSize, X / tScale));
            point.Y = (byte)Math.Max(0, Math.Min(Tilemap.Height / tSize, Y / tScale));
        }

        protected virtual void Redraw(bool refresh = true)
        {
            if (Tilemap == null) return;

            GC.Collect();

            if (imgViewport.Image == null) 
                imgViewport.Image = new Bitmap((int)(Tilemap.Width * scale), (int)(Tilemap.Height * scale));
            if (imgViewport.Image.Width != Tilemap.Width * scale || imgViewport.Image.Height != Tilemap.Height * scale)
            {
                imgViewport.Image.Dispose();
                imgViewport.Image = new Bitmap((int)(Tilemap.Width * scale), (int)(Tilemap.Height * scale));
            }
                
            var p = new Pen(Color.Red, 2);

            using (var g = Graphics.FromImage(imgViewport.Image))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.CompositingMode = CompositingMode.SourceCopy;
                g.PixelOffsetMode = PixelOffsetMode.Half; //required for accurate scaling
                g.DrawImage(Tilemap, viewPortBounds);

                g.CompositingMode = CompositingMode.SourceOver;

                if (showGrid) DrawGrid(g);
                if (!hideSelection) g.DrawRectangle(p, ScaleSelection);

                if (HoverHighlighting && isMouseOver && !isMouseDown)
                {
                    p = new Pen(Color.Blue, 2);
                    g.DrawRectangle(p, new Rectangle(hovPoint.X * tScale, hovPoint.Y * tScale, tScale, tScale));
                }
            }

            if (refresh) imgViewport.Refresh();
        }
    }
}
