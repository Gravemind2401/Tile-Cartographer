﻿using System;
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
        /// <summary>
        /// Allows the user to select multiple tiles by clicking and dragging.
        /// </summary>
        public bool AllowSelectionDrag { get; set; }

        public TilePicker()
        {
            InitializeComponent();
        }

        public void LoadTileset(TCProject Project, Image Tileset)
        {
            this.Enabled = true;
            tProj = Project;
            Tilemap = Tileset;

            //set scrollbar to align with tiles
            this.VerticalScroll.Maximum = Tilemap.Height / tSize * 32;
            this.VerticalScroll.SmallChange = 32;
            this.VerticalScroll.LargeChange = 32 * 8;

            Redraw();
        }

        public void CloseTileset()
        {
            this.VerticalScroll.Value = 0;
            imgViewport.Image = null;
            if (Tilemap != null) Tilemap.Dispose();
            this.Enabled = false;
        }

        public TileClip GetClip()
        {
            var r = Selection;
            var rScale = new Rectangle(r.X * tSize, r.Y * tSize, r.Width * tSize, r.Height * tSize);
            var clip = new TileClip() { Origin = ClipOrigin.Tileset, Section = r };
            clip.SectionImg = ((Bitmap)Tilemap).Clone(rScale, PixelFormat.Format32bppArgb);
            clip.Data = new BytePoint2D[r.Width, r.Height];

            for (int i = 0; i < r.Width; i++)
                for (int j = 0; j < r.Height; j++)
                    clip.Data[i, j] = new BytePoint2D(r.X + i, r.Y + j);

            return clip;
        }

        #region Event Handlers
        private void imgViewport_MouseEnter(object sender, EventArgs e)
        {
            if (imgViewport.Image == null) return;

            isMouseOver = true;
        }

        private void imgViewport_MouseLeave(object sender, EventArgs e)
        {
            if (imgViewport.Image == null) return;

            isMouseOver = false;
            Redraw();
        }

        private void imgViewport_MouseDown(object sender, MouseEventArgs e)
        {
            if (imgViewport.Image == null) return;

            isMouseDown = true;
            var prevPoint = new BytePoint2D(selPoint.X, selPoint.Y);
            SetPoint(ref selPoint, e.X, e.Y);
            SetPoint(ref drgPoint, e.X, e.Y);
            if (PointsChanged != null) PointsChanged(this);
            Redraw();
        }

        private void imgViewport_MouseUp(object sender, MouseEventArgs e)
        {
            if (imgViewport.Image == null) return;

            isMouseDown = false;
            if (PointsChanged != null) PointsChanged(this);
        }

        private void imgViewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (imgViewport.Image == null) return;

            var prevPoint = new BytePoint2D(drgPoint.X, drgPoint.Y);
            if (AllowSelectionDrag && isMouseDown)
            {
                SetPoint(ref drgPoint, e.X, e.Y);
                if (prevPoint.Equals(drgPoint)) return;
            }

            prevPoint = new BytePoint2D(hovPoint.X, hovPoint.Y);
            SetPoint(ref hovPoint, e.X, e.Y);

            if (isMouseDown || HoverHighlighting && !prevPoint.Equals(hovPoint)) Redraw();
        }
        #endregion

        public event PointsChangedEventHandler PointsChanged;
    }
}