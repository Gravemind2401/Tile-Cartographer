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
    public partial class RefIndexPicker : PickerBase
    {
        #region Public Methods
        public RefIndexPicker()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loads a reference sheet from a multitile.
        /// </summary>
        /// <param name="tProj">The project containing the multitile.</param>
        /// <param name="multiTileIndex">The index of the multitile.</param>
        public void LoadRefSheet(TCProject Project, int multiTileIndex)
        {
            this.Enabled = true;
            tProj = Project;

            Tilemap = TCRefSheet.GenerateReference(tProj.MultiTiles[multiTileIndex], tProj.TileSize);

            Redraw();
        }

        /// <summary>
        /// Closes the reference sheet and disposes of drawing resources.
        /// </summary>
        public void CloseRefSheet()
        {
            this.VerticalScroll.Value = 0;
            imgViewport.Image = null;
            if (Tilemap != null) 
                Tilemap.Dispose();
            this.Enabled = false;
        }
        #endregion

        #region Event Handlers
        private void imgTileset_MouseEnter(object sender, EventArgs e)
        {
            if (imgViewport.Image == null) return;

            isMouseOver = true;
        }

        private void imgTileset_MouseLeave(object sender, EventArgs e)
        {
            if (imgViewport.Image == null) return;

            isMouseOver = false;
            Redraw();
        }

        private void imgTileset_MouseDown(object sender, MouseEventArgs e)
        {
            if (imgViewport.Image == null) return;

            isMouseDown = true;
            var prevPoint = new BytePoint2D(selPoint.X, selPoint.Y);
            SetPoint(ref selPoint, e.X, e.Y);
            SetPoint(ref drgPoint, e.X, e.Y);
            Redraw();
        }

        private void imgTileset_MouseUp(object sender, MouseEventArgs e)
        {
            if (imgViewport.Image == null) return;

            isMouseDown = false;
            if (PointsChanged != null) PointsChanged(this);
        }

        private void imgTileset_MouseMove(object sender, MouseEventArgs e)
        {
            if (imgViewport.Image == null) return;

            var prevPoint = new BytePoint2D(drgPoint.X, drgPoint.Y);

            prevPoint = new BytePoint2D(hovPoint.X, hovPoint.Y);
            SetPoint(ref hovPoint, e.X, e.Y);

            if (isMouseDown || HoverHighlighting && !prevPoint.Equals(hovPoint)) Redraw();
        }

        //choose index by double-click instead of single click (less accidental clicks)
        private void imgTileset_DoubleClick(object sender, EventArgs e)
        {
            var r = Selection;
            byte index = (byte)(r.Y * 8 + r.X);
            if (IndexChanged != null) IndexChanged(this, index);      
        }
        #endregion

        public event PointsChangedEventHandler PointsChanged;
        public event IndexChangedEventHandler IndexChanged;
    }
}
