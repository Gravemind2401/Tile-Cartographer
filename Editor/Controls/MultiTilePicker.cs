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
    public partial class MultiTilePicker : PickerBase
    {
        //forceIndex is an index override used to override multitiles at render time. 
        //0xFF represents -1, meaning there is no override and the tile must use a
        //dynamic graphic, otherwise it must use the graphic specified by the index.
        private byte forceIndex = 0xFF;

        #region Control Properties
        /// <summary>
        /// Allows the user to select multiple tiles by clicking and dragging.
        /// </summary>
        public bool AllowSelectionDrag { get; set; }
        #endregion

        #region Public Methods
        public MultiTilePicker()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loads and displays the given project's multitiles.
        /// </summary>
        /// <param name="tProj">The project containing the multitiles.</param>
        public void LoadMultiTiles(TCProject Project)
        {
            //reset if theres no tiles to display
            if (Project.MultiTiles.Count == 0)
            {
                CloseMultiTiles();
                return;
            }

            this.Enabled = true;

            tProj = Project;
            forceIndex = 0xFF;

            int rows = (int)Math.Ceiling(tProj.MultiTiles.Count / 8f);
            Tilemap = new Bitmap(8 * tSize, rows * tSize);

            using (var g = Graphics.FromImage(Tilemap))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                var sr = new Rectangle(0, 0, tSize, tSize);
                int c = 0;

                //create a tileset image from the multitiles
                for (int j = 0; j < rows; j++)
                {
                    if (c >= tProj.MultiTiles.Count) break;
                    for (int i = 0; i < 8; i++)
                    {
                        if (c >= tProj.MultiTiles.Count) break;

                        var dr = new Rectangle(i * tSize, j * tSize, tSize, tSize);
                        using (var bitm = tProj.MultiTiles[c++].GetBitmap())
                            g.DrawImage(bitm, dr, sr, GraphicsUnit.Pixel);
                    }
                }
            }

            //set scrollbar to align with tiles
            this.VerticalScroll.Maximum = (int)Math.Ceiling(tProj.MultiTiles.Count / 8f) * 32;
            this.VerticalScroll.SmallChange = 32;
            this.VerticalScroll.LargeChange = 32 * 2;

            Redraw();
        }

        /// <summary>
        /// Disposes of all multitile display resources.
        /// </summary>
        public void CloseMultiTiles()
        {
            this.VerticalScroll.Value = 0;

            if (imgViewport.Image != null) imgViewport.Image.Dispose();
            imgViewport.Image = null;

            if (Tilemap != null) Tilemap.Dispose();
            Tilemap = null;
            
            tProj = null;
            this.Enabled = false;
        }

        /// <summary>
        /// Gets a clipboard instance containing the currently selected multitile.
        /// </summary>
        /// <returns></returns>
        public TileClip GetClip()
        {
            var r = Selection;
            var rScale = new Rectangle(r.X * tSize, r.Y * tSize, r.Width * tSize, r.Height * tSize);
            var clip = new TileClip() { Origin = ClipOrigin.MultiTiles, Section = r };
            clip.SectionImg = ((Bitmap)Tilemap).Clone(rScale, PixelFormat.Format32bppArgb);
            clip.Data = new BytePoint2D[r.Width, r.Height];

            for (int i = 0; i < r.Width; i++)
                for (int j = 0; j < r.Height; j++)
                {
                    int index = (r.Y + j) * 8 + (r.X + i);
                    clip.Data[i, j] = new BytePoint2D((byte)(index + 8), forceIndex);
                }

            forceIndex = 0xFF; //reset the index override here so it only lasts until you click another multitile (can be the same one).
            return clip;
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

            //prevent blank selections
            var r = Selection;
            int index = r.Y * 8 + r.X;
            if (index >= tProj.MultiTiles.Count)
            {
                selPoint = new BytePoint2D(prevPoint.X, prevPoint.Y);
                drgPoint = new BytePoint2D(prevPoint.X, prevPoint.Y);
                return;
            }

            if (PointsChanged != null) PointsChanged(this);

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
            if (AllowSelectionDrag && isMouseDown)
            {
                SetPoint(ref drgPoint, e.X, e.Y);
                if (prevPoint.Equals(drgPoint)) return;
            }

            prevPoint = new BytePoint2D(hovPoint.X, hovPoint.Y);
            SetPoint(ref hovPoint, e.X, e.Y);

            if (isMouseDown || HoverHighlighting && !prevPoint.Equals(hovPoint)) Redraw();
        }

        //gives the user the option to override a multitile by double-clicking on it
        private void imgTileset_DoubleClick(object sender, EventArgs e)
        {
            if (imgViewport.Image == null) return;

            var r = Selection;
            int index = r.Y * 8 + r.X;
            if (index >= tProj.MultiTiles.Count) return; //dont allow blank selections

            using (var bitm = TCRefSheet.GenerateReference(tProj.MultiTiles[index], tProj.TileSize))
                if (!TCImage.IsValid(bitm, TCImageType.Reference, tProj.TileSize)) return;

            var box = new MultiTileDialogue();
            forceIndex = box.ShowDialog(this, tProj, index);
        }
        #endregion

        public event PointsChangedEventHandler PointsChanged;
    }
}
