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
    public partial class TilePicker : PickerBase
    {
        #region Properties
        /// <summary>
        /// Allows the user to select multiple tiles by clicking and dragging.
        /// </summary>
        public bool MultiSelect { get; set; }
        #endregion

        #region Methods
        public TilePicker()
        {
            InitializeComponent();
        }

        public void LoadTileset(TCProject Project, Image Tileset)
        {
            tProj = Project;
            Tilemap = Tileset;
            Refresh();
        }

        public void CloseTileset()
        {
            if (Tilemap != null) Tilemap.Dispose();
            Tilemap = null;
            tProj = null;
            Refresh();
        }

        public TileClip GetClip()
        {
            var r = Selection;
            var rScale = new Rectangle(r.X * tSize, r.Y * tSize, r.Width * tSize, r.Height * tSize);
            var clip = new TileClip() { Origin = ClipOrigin.Tileset, Section = r };
            if (clip.SectionImg != null) clip.SectionImg.Dispose();
            clip.SectionImg = ((Bitmap)Tilemap).Clone(rScale, PixelFormat.Format32bppArgb);
            clip.Data = new BytePoint2D[r.Width, r.Height];

            for (int i = 0; i < r.Width; i++)
                for (int j = 0; j < r.Height; j++)
                    clip.Data[i, j] = new BytePoint2D(r.X + i, r.Y + j);

            return clip;
        }
        #endregion

        #region Event Handlers
        private void TilePicker_MouseDown(object sender, MouseEventArgs e)
        {
            if (Tilemap == null) return;
            if (e.Button !=  MouseButtons.Left) return;

            isMouseDown = true;
            var prevPoint = new BytePoint2D(selPoint.X, selPoint.Y);
            SetPoint(ref selPoint, e.X, e.Y);
            SetPoint(ref drgPoint, e.X, e.Y);
            if (PointsChanged != null) PointsChanged(this);
            Refresh();
        }

        private void TilePicker_MouseUp(object sender, MouseEventArgs e)
        {
            if (Tilemap == null) return;
            if (e.Button != MouseButtons.Left) return;

            isMouseDown = false;
            if (PointsChanged != null) PointsChanged(this);
        }

        private void TilePicker_MouseMove(object sender, MouseEventArgs e)
        {
            if (Tilemap == null) return;
            if (e.Button != MouseButtons.Left) return;

            var prevPoint = new BytePoint2D(drgPoint.X, drgPoint.Y);
            if (MultiSelect && isMouseDown)
            {
                SetPoint(ref drgPoint, e.X, e.Y);
                if (prevPoint.Equals(drgPoint)) return;
            }

            prevPoint = new BytePoint2D(hovPoint.X, hovPoint.Y);
            SetPoint(ref hovPoint, e.X, e.Y);

            if (isMouseDown || HoverHighlighting && !prevPoint.Equals(hovPoint)) Refresh();
        }
        #endregion

        public event PointsChangedEventHandler PointsChanged;

    }
}
