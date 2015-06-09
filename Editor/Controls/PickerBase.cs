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
    public partial class PickerBase : Panel
    {
        #region Declarations
        private int zoomPower = 5;
        private bool showGrid = false;

        private float scale { get { return (float)Math.Pow(2, zoomPower) / tSize; } }
        private Rectangle sourceBounds { get { return new Rectangle(0, 0, Tilemap.Width, Tilemap.Height); } }

        protected bool isMouseOver = false;
        protected bool isMouseDown = false;
        protected bool hideSelection = false;

        protected BytePoint2D selPoint; //which tile the mouse was clicked on
        protected BytePoint2D drgPoint; //which tile the mouse was released on
        protected BytePoint2D hovPoint; //which tile is under the cursor

        protected TCProject tProj;

        protected Image Tilemap; //use Image instead of TCImage to save disk reads (lots of redrawing will be done)

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
        #endregion

        #region Properties
        /// <summary>
        /// Allows highlighting tiles as the mouse hovers over them.
        /// </summary>
        public bool HoverHighlighting { get; set; }
        /// <summary>
        /// Renders a grid over the control image.
        /// </summary>
        public bool ShowGrid { get { return showGrid; } set { showGrid = value; Refresh(); } }
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
        public int ZoomPower { get { return zoomPower; } set { zoomPower = Math.Max(3, Math.Min(6, value)); Refresh(); } }
        #endregion

        #region Methods
        public PickerBase()
        {
            InitializeComponent();
            this.DoubleBuffered = true; //prevents flickering
            
            this.HorizontalScroll.SmallChange = 
            this.VerticalScroll.SmallChange = 32;
            
            this.HorizontalScroll.LargeChange = 
            this.VerticalScroll.LargeChange = 32 * 8;
        }

        public void HideSelection()
        {
            hideSelection = true;
            Refresh();
        }

        public void ShowSelection()
        {
            hideSelection = false;
            Refresh();
        }

        protected void SetPoint(ref BytePoint2D point, int X, int Y)
        {
            if (Tilemap == null || tProj == null) return;
            X -= this.AutoScrollPosition.X; //translate click coordinates
            Y -= this.AutoScrollPosition.Y; //to match scrollbar offsets
            point.X = (byte)Math.Max(0, Math.Min(Tilemap.Width / tSize - 1, X / tScale));
            point.Y = (byte)Math.Max(0, Math.Min(Tilemap.Height / tSize - 1, Y / tScale));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(this.BackColor);
            base.OnPaint(e);

            if (Tilemap == null || tProj == null) return;
            
            var p = new Pen(Color.FromArgb(50, Color.Black), 2);
            var g = e.Graphics;

            g.TranslateTransform(this.AutoScrollPosition.X, this.AutoScrollPosition.Y);
            g.InterpolationMode = InterpolationMode.NearestNeighbor; //prevents fuzziness
            g.PixelOffsetMode = PixelOffsetMode.Half; //required for accurate scaling
            g.DrawImage(Tilemap, new RectangleF(0, 0, Tilemap.Width * scale, Tilemap.Height * scale));

            if (showGrid) //draw grid
            {
                for (int x = 0; x <= Tilemap.Width * scale; x += tScale)
                    g.DrawLine(p, x, 0, x, Tilemap.Height * scale);

                for (int y = 0; y <= Tilemap.Height * scale; y += tScale)
                    g.DrawLine(p, 0, y, Tilemap.Width * scale, y);
            }

            if (!hideSelection)
            {
                p.Color = Color.Red;
                g.DrawRectangle(p, ScaleSelection);
            }

            if (HoverHighlighting && isMouseOver && !isMouseDown)
            {
                p.Color = Color.Blue;
                g.DrawRectangle(p, new Rectangle(hovPoint.X * tScale, hovPoint.Y * tScale, tScale, tScale));
            }
        }

        new public void Refresh()
        {
            this.AutoScrollMinSize = (Tilemap == null || tProj == null) ? 
                new Size() : new Size((int)(Tilemap.Width * scale), (int)(Tilemap.Height * scale));

            base.Refresh();
        }
        #endregion

        #region Event Handlers
        private void PickerBase_MouseEnter(object sender, EventArgs e)
        {
            isMouseOver = true;
        }

        private void PickerBase_MouseLeave(object sender, EventArgs e)
        {
            isMouseOver = false;
            Refresh();
        }

        private void PickerBase_MouseDown(object sender, MouseEventArgs e)
        {
            this.Focus();
        }
        #endregion
    }
}
