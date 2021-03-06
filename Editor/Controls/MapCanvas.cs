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
    public partial class MapCanvas : PickerBase
    {
        #region Declarations
        private MouseButtons cButton; //which mouse button was clicked
        private Image[] layerImgs;    //these 5 images are used as a cache to speed up rendering
        private Image Lower, Upper;   //each redraw we only work with a small area of one image

        private BytePoint2D[,] currentLayer 
        { get { return tMap.Layers[(int)LayerMode]; } set { tMap.Layers[(int)LayerMode] = value; } } //shortcut to get current layer rather than using a method

        private Image Tileset;

        //queues individual tiles for redraw instead
        //of redrawing the entire map after an edit
        private Queue<KeyValuePair<int, BytePoint2D>> updateQueue;
        private Stack<TCMap> undoQueue;
        private Stack<TCMap> redoQueue;
        private List<Point> fillList;

        private Rectangle selRect;

        public TCMap tMap;
        public TileClip Clipboard;
        #endregion

        #region GetSet Properties
        //these are all set up as get/set properties so that the mapcanvas can
        //change drawing patterns as soon as the properties change with minimal code
        private LayerPaintMode lmp = LayerPaintMode.Single;
        private LayerEditMode lem = LayerEditMode.Layer1;
        private LayerViewMode lvm = LayerViewMode.AllLayers;

        private bool dimLayers;
        private bool unDimOnExit;

        public LayerPaintMode PaintMode
        { get { return lmp; } set { lmp = value; selRect = Rectangle.Empty; Refresh(); } }
        public LayerEditMode LayerMode
        { 
            get { return lem; } 
            set 
            { 
                lem = value; 
                selRect = Rectangle.Empty;
                DrawSandwich();
                Refresh(); 
                if (LayerEditModeChanged != null) LayerEditModeChanged(this, value); 
            } 
        }
        public LayerViewMode ViewMode
        { get { return lvm; } set { lvm = value; Refresh(); } }

        public bool DimLayers
        { get { return dimLayers; } set { dimLayers = value; Refresh(); } }
        public bool UnDimOnExit
        { get { return unDimOnExit; } set { unDimOnExit = value; Refresh(); } }

        /// <summary>
        /// Allows highlighting tiles as the mouse hovers over them.
        /// </summary>
        new public bool HoverHighlighting { get; set; }
        public BytePoint2D HoverPoint { get { return hovPoint; } }
        #endregion

        #region Methods
        public MapCanvas()
        {
            InitializeComponent();
            layerImgs = new Image[4];
            updateQueue = new Queue<KeyValuePair<int, BytePoint2D>>();
            undoQueue = new Stack<TCMap>(20);
            redoQueue = new Stack<TCMap>(20);
            fillList = new List<Point>();
            //20 should be fine, maybe make it user-defined in future?
        }

        /// <summary>
        /// Loads the specified TCMap using resources from the specified TCProject.
        /// </summary>
        /// <param name="tProj">The project to use.</param>
        /// <param name="tMap">The map to open.</param>
        /// <param name="tSet">The tileset (in Image form) to get tile graphics from.</param>
        /// <param name="clip">The initial clipboard to use.</param>
        public void LoadMap(TCProject Project, TCMap Map, Image tSet, TileClip clip)
        {
            undoQueue.Clear();
            redoQueue.Clear();

            tProj = Project;
            tMap = Map;
            Tilemap = new Bitmap(tMap.Width * tSize, tMap.Height * tSize, PixelFormat.Format32bppArgb);
            Tileset = tSet;
            Clipboard = clip;

            selPoint = new BytePoint2D();
            drgPoint = new BytePoint2D();
            hovPoint = new BytePoint2D();
            selRect = Rectangle.Empty;
            hideSelection = true;

            RedrawAll();
        }

        /// <summary>
        /// Closes the currently open map and disposes all drawing resources.
        /// </summary>
        public void CloseMap()
        {
            if (Tilemap != null) Tilemap.Dispose();
            if (Tileset != null) Tileset.Dispose();
            if (Lower != null) Lower.Dispose();
            if (Upper != null) Upper.Dispose();

            for (int i = 0; i < layerImgs.Length; i++)
                if (layerImgs[i] != null) layerImgs[i].Dispose();

            undoQueue.Clear();
            redoQueue.Clear();
            Clipboard = null;
            Tilemap = null;
            tMap = null;
            tProj = null;
            selRect = Rectangle.Empty;
            Refresh();
        }

        /// <summary>
        /// Saves a 1:1 image of the map without gridlines or dimming. Only saves .png files.
        /// </summary>
        /// <param name="Filename">The file path to save to. Assumes .png extension.</param>
        public void SaveToImage(string Filename)
        {
            using(var bitm = (Bitmap)layerImgs[0].Clone())
            using (var g = Graphics.FromImage(bitm))
            {
                g.DrawImage(layerImgs[1], Point.Empty);
                g.DrawImage(layerImgs[2], Point.Empty);
                bitm.Save(Filename, ImageFormat.Png);
            }
        }

        //TODO: add neighbouring autotiles to the queue (without adding the same one more than once)
        private void SetQueue(TCMap oldMap, TCMap newMap)
        {
            for (int x = 0; x < 4; x++)
                for (int i = 0; i < tMap.Width; i++)
                    for (int j = 0; j < tMap.Height; j++)
                    {
                        var oldTile = oldMap.Layers[x][i, j];
                        var newTile = newMap.Layers[x][i, j];

                        if (!oldTile.Equals(newTile))
                            updateQueue.Enqueue(new KeyValuePair<int, BytePoint2D>(x, new BytePoint2D(i, j)));
                    }
        }
        #endregion

        #region Editing Functions
        public void Undo()
        {
            if (undoQueue.Count == 0) return;

            var oldMap = TCMap.DeepClone(tMap);
            var newMap = undoQueue.Pop();
            redoQueue.Push(oldMap);
            tMap.CopyFrom(newMap); //"copy from" so we keep the same ref value

            //full redraw on dimension change, otherwise delta redraw
            if (oldMap.Width != newMap.Width || oldMap.Height != newMap.Height)
                RedrawAll();
            else
            {
                SetQueue(oldMap, newMap);
                Refresh();
            }

            if (UndoCountChanged != null) UndoCountChanged(this, undoQueue.Count);
            if (RedoCountChanged != null) RedoCountChanged(this, redoQueue.Count);
        }

        public void Redo()
        {
            if (redoQueue.Count == 0) return;

            var oldMap = TCMap.DeepClone(tMap);
            var newMap = redoQueue.Pop();
            undoQueue.Push(oldMap);
            tMap.CopyFrom(newMap); //"copy from" so we keep the same ref value

            //full redraw on dimension change, otherwise delta redraw
            if (oldMap.Width != newMap.Width || oldMap.Height != newMap.Height)
                RedrawAll();
            else
            {
                SetQueue(oldMap, newMap);
                Refresh();
            }

            if (UndoCountChanged != null) UndoCountChanged(this, undoQueue.Count);
            if (RedoCountChanged != null) RedoCountChanged(this, redoQueue.Count);
        }

        public void CopySelection()
        {
            SetClip(true, false);
        }

        public void CutSelection()
        {
            SetClip(true, true);
        }

        public void DeleteSelection()
        {
            SetClip(false, true);
        }

        /// <summary>
        /// Performs a recursive flood fill using the current clipboard data.
        /// </summary>
        /// <param name="loc">The current tile coordinate to fill.</param>
        /// <param name="origin">The origin of the flood fill. Required for tiling with clipboards larger than 1x1.</param>
        public void FloodFill(Point loc, Point origin, bool clear)
        {
            if (loc.Equals(origin)) fillList.Clear();

            if (loc.X < 0 || loc.X >= tMap.Width ||
                loc.Y < 0 || loc.Y >= tMap.Height)
                return; //skip out of bounds tiles

            //dont keep doing the same tiles
            if (fillList.Contains(loc)) return;
            else fillList.Add(loc);

            BytePoint2D old = currentLayer[loc.X, loc.Y];
            int x = (loc.X - origin.X) % Clipboard.Data.GetLength(0);
            int y = (loc.Y - origin.Y) % Clipboard.Data.GetLength(1);

            //fix negatives for tiling backwards
            if (x < 0) x = Clipboard.Data.GetLength(0) + x;
            if (y < 0) y = Clipboard.Data.GetLength(1) + y;

            if (currentLayer[loc.X, loc.Y].Equals(Clipboard.Data[x, y])) return;

            if (clear)
                currentLayer[loc.X, loc.Y] = new BytePoint2D(0xFF, 0);
            else
                currentLayer[loc.X, loc.Y] = Clipboard.Data[x, y];

            updateQueue.Enqueue(new KeyValuePair<int, BytePoint2D>((int)LayerMode, new BytePoint2D(loc.X, loc.Y)));
      
            //keep inside bounds         //check for matching tiles                      //recursive fill
            if (loc.Y - 1 >= 0)          if (currentLayer[loc.X, loc.Y - 1].Equals(old)) FloodFill(new Point(loc.X, loc.Y - 1), origin, clear);
            if (loc.X + 1 < tMap.Width)  if (currentLayer[loc.X + 1, loc.Y].Equals(old)) FloodFill(new Point(loc.X + 1, loc.Y), origin, clear);
            if (loc.Y + 1 < tMap.Height) if (currentLayer[loc.X, loc.Y + 1].Equals(old)) FloodFill(new Point(loc.X, loc.Y + 1), origin, clear);
            if (loc.X - 1 >= 0)          if (currentLayer[loc.X - 1, loc.Y].Equals(old)) FloodFill(new Point(loc.X - 1, loc.Y), origin, clear);
        }

        /// <summary>
        /// Shifts the selected area by a given offset.
        /// </summary>
        /// <param name="X">Number of tiles to move across the X axis.</param>
        /// <param name="Y">Number of tiles to move across the Y axis.</param>
        public void ShiftSelection(int X, int Y)
        {
            if (selRect.IsEmpty) return;

            var r = new Rectangle(selRect.X + X, selRect.Y + Y, selRect.Width, selRect.Height);

            var tempClip = Clipboard;
            SetClip(true, true);
            PasteClipData(r);
            selRect = r;
            Clipboard = tempClip;

            Refresh();
        }

        /// <summary>
        /// Performs editing operations on the selected area.
        /// </summary>
        /// <param name="copy">Whether to copy the selected area to the clipboard.</param>
        /// <param name="clear">Whether to clear tile data from the selected area.</param>
        private void SetClip(bool copy, bool clear)
        {
            if (selRect.IsEmpty) return;

            bool flag = false;
            var r = selRect;
            var rScale = new Rectangle(r.X * tSize, r.Y * tSize, r.Width * tSize, r.Height * tSize);
            //TODO: bounds check on r and rScale (must be positive)

            if (clear)
            {
                undoQueue.Push(TCMap.DeepClone(tMap));
                if (UndoCountChanged != null) UndoCountChanged(this, undoQueue.Count);
            }

            if (copy)
            {
                Clipboard = new TileClip() { Origin = ClipOrigin.Canvas, Section = r };
                Clipboard.SectionImg = ((Bitmap)layerImgs[(int)LayerMode]).Clone(rScale, PixelFormat.Format32bppArgb);
                Clipboard.Data = new BytePoint2D[r.Width, r.Height];
            }

            for (int i = 0; i < r.Width; i++)
                for (int j = 0; j < r.Height; j++)
                {
                    if (currentLayer[r.X + i, r.Y + j].X > 7) flag = true;
                    if (copy) Clipboard.Data[i, j] = currentLayer[r.X + i, r.Y + j];
                    if (clear) currentLayer[r.X + i, r.Y + j] = new BytePoint2D(0xFF, 0);
                }

            if (clear)
            {
                //redraw current layer if we are near autotiles; this
                //ensures all existing autotiles will be updated to
                //match any new neighbours they may have after the edit
                //==========================================================
                //[this should really only be a delta update, but I'm lazy;]
                //[each update we should also update neighbouring autotiles]
                if (flag)
                {
                    layerImgs[(int)LayerMode] = DrawLayer((int)LayerMode);
                    return;
                }

                using (var g = Graphics.FromImage(layerImgs[(int)LayerMode]))
                {
                    g.CompositingMode = CompositingMode.SourceCopy;
                    g.FillRectangle(new SolidBrush(Color.Transparent), rScale);
                }

                redoQueue.Clear();
                if (RedoCountChanged != null) RedoCountChanged(this, redoQueue.Count);
            }

            Refresh();
        }

        /// <summary>
        /// Pastes the clipboard over a given area. Clipboards over 1x1 will be tiled.
        /// </summary>
        /// <param name="dest">The area to paste to (in tile coordinates).</param>
        private void PasteClipData(Rectangle dest)
        {
            bool flag = false;

            for (int i = 0; i < dest.Width; i++)
            {
                if (dest.X + i < 0 || dest.X + i >= tMap.Width) continue;
                for (int j = 0; j < dest.Height; j++)
                {
                    if (dest.Y + j < 0 || dest.Y + j >= tMap.Height) continue;

                    if (cButton == MouseButtons.Right) currentLayer[dest.X + i, dest.Y + j] = new BytePoint2D(0xFF, 0);
                    else currentLayer[dest.X + i, dest.Y + j] = Clipboard.Data[i % Clipboard.Section.Width, j % Clipboard.Section.Height];
                }
            }

            //check the clip for autotiles
            for(int i = 0; i < Clipboard.Section.Width; i++)
                for (int j = 0; j < Clipboard.Section.Height; j++)
                    if (Clipboard.Data[i, j].X > 7)
                        flag = true;

            //check the destination and its immediate neighbours for autotiles
            for (int i = -1; i < dest.Width + 2; i++)
            {
                if (dest.X + i < 0 || dest.X + i >= tMap.Width) continue;
                for (int j = -1; j < dest.Height + 2; j++)
                {
                    if (dest.Y + j < 0 || dest.Y + j >= tMap.Height) continue;
                    if (currentLayer[dest.X + i, dest.Y + j].X > 7)
                        flag = true;
                }
            }
            
            //if we find autotiles, do a delta redraw of the destination and its immediate neighbours
            //this ensures that any autotiles will keep their display in sync with their neighbours
            if (flag)
            {
                int x = Math.Max(dest.X - 1, 0);
                int y = Math.Max(dest.Y - 1, 0);
                int w = Math.Min(dest.Width + 2, tMap.Width - x);
                int h = Math.Min(dest.Height + 2, tMap.Height - y);
                var rect = new Rectangle(x, y, w, h);

                using (var bitm = DrawLayer((int)LayerMode, rect))
                {
                    using (var gg = Graphics.FromImage(layerImgs[(int)LayerMode]))
                    {
                        gg.CompositingMode = CompositingMode.SourceCopy;
                        gg.DrawImage(bitm, rect.X * tSize, rect.Y * tSize);
                    }
                }
                return;
            }

            //update image [non-autotiles]
            using (var g = Graphics.FromImage(layerImgs[(int)LayerMode]))
            {
                for (int i = 0; i < dest.Width; i += Clipboard.Section.Width)
                    for (int j = 0; j < dest.Height; j += Clipboard.Section.Height)
                    {
                        var w2 = Math.Min(Clipboard.Section.Width, dest.Width - (i % dest.Width)) * tSize;
                        var h2 = Math.Min(Clipboard.Section.Height, dest.Height - (j % dest.Height)) * tSize;
                        var sr = new Rectangle(0, 0, w2, h2);
                        var dr = new Rectangle((dest.X + i) * tSize, (dest.Y + j) * tSize, w2, h2);

                        g.CompositingMode = CompositingMode.SourceCopy;
                        if (cButton == MouseButtons.Right) g.FillRectangle(new SolidBrush(Color.Transparent), dr);
                        else g.DrawImage(Clipboard.SectionImg, dr, sr, GraphicsUnit.Pixel);
                    }
            }
        }
        #endregion

        #region Drawing functions
        /// <summary>
        /// Draws the entirety of the specified layer of the currently opened map to a bitmap.
        /// </summary>
        /// <param name="layer">The index of the layer to draw.</param>
        /// <returns></returns>
        private Bitmap DrawLayer(int layer)
        {
            return DrawLayer(layer, new Rectangle(0, 0, tMap.Width, tMap.Height));
        }

        /// <summary>
        /// Draws a section of the specified layer of the currently opened map to a bitmap.
        /// All coordinates are in tile-space.
        /// </summary>
        /// <param name="layer">The index of the layer to draw.</param>
        /// <param name="xStart">The X position to start from.</param>
        /// <param name="xLength">The width of the section to draw.</param>
        /// <param name="yStart">The Y position to start from.</param>
        /// <param name="yLength">The height of the section to draw.</param>
        /// <returns></returns>
        private Bitmap DrawLayer(int layer, Rectangle section)
        {
            var img = new Bitmap(section.Width * tSize, section.Height * tSize, PixelFormat.Format32bppArgb);
            var g = Graphics.FromImage(img);

            for (int i = section.X; i < section.X + section.Width; i++)
            {
                if (i < 0 || i >= tMap.Width) continue;
                for (int j = section.Y; j < section.Y + section.Height; j++)
                {
                    if (j < 0 || j >= tMap.Height) continue;

                    var p = tMap.Layers[layer][i, j];
                    if (p.X == 0xFF && layer < 3) continue;

                    var sr = new Rectangle(p.X * tSize, p.Y * tSize, tSize, tSize);
                    var dr = new Rectangle((i - section.X) * tSize, (j - section.Y) * tSize, tSize, tSize);

                    if (layer == 3) //draw collision tiles
                    {
                        int tHalf = tSize / 2;

                        var dr2 = new Rectangle(dr.X + tSize / 4, dr.Y + tSize / 4, tHalf, tHalf);
                        var pen = new Pen(Color.Black, 1f);
                        var b0 = new SolidBrush(Color.Green);
                        var b1 = new SolidBrush(Color.DarkRed);
                        var b3 = new SolidBrush(Color.White);
                        var pts = new Point[] { 
                            new Point(dr.X, dr.Y), new Point(dr.X + tSize, dr.Y), new Point(dr.X + tSize, dr.Y + tSize), new Point(dr.X, dr.Y + tSize),
                            new Point(dr.X + tHalf, dr.Y + tHalf), new Point(dr.X + tHalf + 1, dr.Y + tHalf), new Point(dr.X + tHalf + 1, dr.Y + tHalf + 1), new Point(dr.X + tHalf, dr.Y + tHalf + 1) };

                        g.FillRectangle(b0, dr); //default to green

                        //draw collision indicators
                        if ((p.Y & 1) == 1) g.FillPolygon(b1, new Point[] { pts[0], pts[4], pts[7], pts[3] });
                        if ((p.Y & 2) == 2) g.FillPolygon(b1, new Point[] { pts[3], pts[7], pts[6], pts[2] });
                        if ((p.Y & 4) == 4) g.FillPolygon(b1, new Point[] { pts[1], pts[5], pts[6], pts[2] });
                        if ((p.Y & 8) == 8) g.FillPolygon(b1, new Point[] { pts[0], pts[1], pts[5], pts[4] });

                        //draw outlines
                        g.DrawRectangle(pen, dr);
                        g.DrawLine(pen, pts[0], pts[2]);
                        g.DrawLine(pen, pts[1], pts[3]);

                        g.FillRectangle(b3, dr2);
                        g.DrawRectangle(pen, dr2);

                        //draw layer value
                        var ff = new FontFamily("Lucida Console");
                        var f = new Font(ff, tSize / 2, FontStyle.Regular, GraphicsUnit.Pixel);
                        var sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                        g.DrawString((p.Y >> 4).ToString("X"), f, new SolidBrush(Color.Black), new RectangleF(dr2.X, dr2.Y+1, dr2.Width, dr2.Height), sf);
                    }
                    else
                    {
                        try
                        {
                            if (p.X > 7) sr = TCRefSheet.GetSourceRect(tProj.MultiTiles[p.X - 8], tProj.TileSize, tMap.Layers[layer], i, j, p.Y);
                            using (var bitm = (p.X < 8) ? (Image)Tileset.Clone() : (Image)TCRefSheet.GenerateReference(tProj.MultiTiles[p.X - 8], tProj.TileSize)) g.DrawImage(bitm, dr, sr, GraphicsUnit.Pixel);
                        }
                        catch
                        {
                            //draw red X on black fill if an error occurs such as
                            //tile index > tileset height or invalid multitile ID
                            var pen = new Pen(Color.Red, 2f);
                            g.FillRectangle(new SolidBrush(Color.Black), dr);
                            g.DrawLine(pen, dr.Location, new Point(dr.X + dr.Width, dr.Y + dr.Height));
                            g.DrawLine(pen, new Point(dr.X + dr.Width, dr.Y), new Point(dr.X, dr.Y + dr.Height));
                        }
                    }
                }
            }

            g.Dispose();

            return img;
        }

        /// <summary>
        /// Draws the Upper and Lower bitmaps. 
        /// These are used to draw all layers above/below the current layer as two static images to save draw time.
        /// </summary>
        private void DrawSandwich()
        {
            if (tMap == null) return;

            int maxLayers = (LayerMode == LayerEditMode.Collision) ? 4 : 3;

            var r = new Rectangle(0, 0, tMap.Width * tSize, tMap.Height * tSize);
            
            Lower = new Bitmap(r.Width, r.Height, PixelFormat.Format32bppArgb);
            var g = Graphics.FromImage(Lower);

            for (int i = 0; i < (int)LayerMode; i++)
                g.DrawImage(layerImgs[i], r);

            Upper = new Bitmap(r.Width, r.Height, PixelFormat.Format32bppArgb);
            g = Graphics.FromImage(Upper);

            for (int i = (int)LayerMode + 1; i < maxLayers; i++)
                g.DrawImage(layerImgs[i], r);

            g.Dispose();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (tMap == null) return;
            Graphics g;

            #region UpdateQueue
            bool flag = false;
            while (updateQueue.Count > 0)
            {
                var tile = updateQueue.Dequeue();
                g = Graphics.FromImage(layerImgs[tile.Key]);
                g.CompositingMode = CompositingMode.SourceCopy;
                using (var bitm = DrawLayer(tile.Key, new Rectangle(tile.Value.X, tile.Value.Y, 1, 1)))
                {
                    if (currentLayer[tile.Value.X, tile.Value.Y].X == 0xFF && LayerMode != LayerEditMode.Collision) 
                        g.FillRectangle(new SolidBrush(Color.Transparent), new Rectangle(tile.Value.X * tSize, tile.Value.Y * tSize, tSize, tSize));
                    else 
                        g.DrawImage(bitm, new Point(tile.Value.X * tSize, tile.Value.Y * tSize));
                }
                g.Dispose();
                if (tile.Key != (int)LayerMode) flag = true;
            }
            if (flag) DrawSandwich(); //if anything in the queue was from another layer, redraw Upper and Lower
            #endregion

            #region Draw Tilemap
            if (Tilemap != null) Tilemap.Dispose();       
            Tilemap = new Bitmap(tMap.Width * tSize, tMap.Height * tSize, PixelFormat.Format32bppArgb);

            bool dim = (DimLayers && !(UnDimOnExit && !isMouseOver));

            var dimMat = new ColorMatrix() { Matrix00 = 0.8f, Matrix11 = 0.8f, Matrix22 = 0.8f, Matrix33 = 0.40f };
            var ovrMat = new ColorMatrix() { Matrix33 = 0.55f };
            var stdAttr = new ImageAttributes();
            var dimAttr = new ImageAttributes();
            var ovrAttr = new ImageAttributes();
            dimAttr.SetColorMatrix(dimMat);
            ovrAttr.SetColorMatrix(ovrMat);

            g = Graphics.FromImage(Tilemap);
            var dest = new Rectangle(0, 0, layerImgs[0].Width, layerImgs[0].Height);

            if ((int)LayerMode > 0)
                g.DrawImage(Lower, dest, 0, 0, dest.Width, dest.Height, GraphicsUnit.Pixel, dim ? dimAttr : stdAttr);

            if ((int)LayerMode < 3) g.DrawImage(layerImgs[(int)LayerMode], dest);
            
            if (ViewMode == LayerViewMode.AllLayers && (int)LayerMode < 3) 
                g.DrawImage(Upper, dest, 0, 0, dest.Width, dest.Height, GraphicsUnit.Pixel, dim ? dimAttr : stdAttr);

            if(LayerMode == LayerEditMode.Collision)
                g.DrawImage(layerImgs[(int)LayerMode], dest, 0, 0, dest.Width, dest.Height, GraphicsUnit.Pixel, ovrAttr);

            g.Dispose();
            #endregion

            base.OnPaint(e);
            g = e.Graphics;

            var p = new Pen(Color.DarkOrange, 2);

            if (!selRect.IsEmpty)
                g.DrawRectangle(p, new Rectangle(selRect.X * tScale, selRect.Y * tScale, selRect.Width * tScale, selRect.Height * tScale ));

            if (isMouseDown)
            {
                var r = Selection;

                //draw the transparent preveiw over the selection
                if (PaintMode == LayerPaintMode.Rectangle && (Clipboard.Section.Width > 1 || Clipboard.Section.Height > 1))
                {
                    for (int i = 0; i < r.Width; i += Clipboard.Section.Width)
                        for (int j = 0; j < r.Height; j += Clipboard.Section.Height)
                        {
                            var w2 = (int)Math.Min(Clipboard.Section.Width, r.Width - (i % r.Width));
                            var h2 = (int)Math.Min(Clipboard.Section.Height, r.Height - (j % r.Height));
                            var sr = new Rectangle(0, 0, w2 * tSize, h2 * tSize);
                            var dr = new Rectangle((r.X + i) * tScale, (r.Y + j) * tScale, w2 * tScale, h2 * tScale);
                            g.DrawImage(Clipboard.SectionImg, dr, sr.X, sr.Y, sr.Width, sr.Height, GraphicsUnit.Pixel, ovrAttr);
                        }
                }

                p.Color = Color.Red;
                g.DrawRectangle(p, ScaleSelection);
            }

            if (HoverHighlighting && isMouseOver && !isMouseDown)
            {
                p.Color = Color.Blue;
                g.DrawRectangle(p, new Rectangle(hovPoint.X * tScale, hovPoint.Y * tScale, Clipboard.Section.Width * tScale, Clipboard.Section.Height * tScale));
            }
        }

        /// <summary>
        /// Redraws all images from scratch.
        /// "Slow Redraw"
        /// </summary>
        public void RedrawAll()
        {
            for (int i = 0; i < 4; i++)
                layerImgs[i] = DrawLayer(i);

            DrawSandwich();

            Refresh();
        }
        #endregion

        #region Event Handlers
        private void MapCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (Tilemap == null) return;
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) return;

            isMouseDown = true;
            cButton = e.Button;

            SetPoint(ref selPoint, e.X, e.Y);
            SetPoint(ref drgPoint, e.X, e.Y);
            if (PointsChanged != null) PointsChanged(this);

            //add to the undo queue unless we're in Select mode.
            if (lmp != LayerPaintMode.Select)
            {
                undoQueue.Push(TCMap.DeepClone(tMap));
                if (UndoCountChanged != null) UndoCountChanged(this, undoQueue.Count);
            }
            else cButton = MouseButtons.Left; //treat selection mode as always left-click

            //Pencil mode updates tiles on mouse-down and mouse-move rather than mouse-up
            if (PaintMode == LayerPaintMode.Single)
            {
                SetPoint(ref drgPoint, e.X + (Clipboard.Section.Width - 1) * tSize, e.Y + (Clipboard.Section.Height - 1) * tSize);
                PasteClipData(new Rectangle(selPoint.X, selPoint.Y, Clipboard.Section.Width, Clipboard.Section.Height));
            }

            Refresh();
        }

        private void MapCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (Tilemap == null) return;
            if (!isMouseDown) return; //in case the mouse was already down before it entered the control
            if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) return;
            
            //add to the redo queue unless we're in select mode.
            //putting this on mouse-up essentially means every 
            //[mouse-down, move, up] movement is a single
            //action that can be undone and redone.
            if (lmp != LayerPaintMode.Select)
            {
                redoQueue.Clear();
                if (RedoCountChanged != null) RedoCountChanged(this, redoQueue.Count);
            }

            //perform certain actions based on current PaintMode
            switch (PaintMode)
            {
                case LayerPaintMode.Rectangle:
                    PasteClipData(Selection);
                    break;

                case LayerPaintMode.Select:
                    selRect = Selection;
                    break;

                case LayerPaintMode.Fill:
                    FloodFill(Selection.Location, Selection.Location, cButton == MouseButtons.Right);
                    break;
            }

            isMouseDown = false;
            cButton = MouseButtons.None;

            Refresh();
        }

        private void MapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (Tilemap == null) return;

            //update our selection coordinates
            var prevPoint = new BytePoint2D(drgPoint.X, drgPoint.Y);
            if (!(PaintMode == LayerPaintMode.Select && LayerMode == LayerEditMode.Collision) && isMouseDown)
            {
                switch (PaintMode)
                {
                    case LayerPaintMode.Rectangle:
                    case LayerPaintMode.Select:
                        SetPoint(ref drgPoint, e.X, e.Y);
                        if (prevPoint.Equals(drgPoint)) return;
                        break;

                    case LayerPaintMode.Single:
                    case LayerPaintMode.Fill:
                        prevPoint = new BytePoint2D(selPoint.X, selPoint.Y);
                        SetPoint(ref selPoint, e.X, e.Y);
                        SetPoint(ref drgPoint, e.X, e.Y);
                        if (prevPoint.Equals(selPoint)) return;
                        break;
                }
                SetPoint(ref drgPoint, e.X, e.Y);
                if (prevPoint.Equals(drgPoint)) return; //ignore this mouse-move if the cursor hasnt left the tile yet
            }

            prevPoint = new BytePoint2D(hovPoint.X, hovPoint.Y);
            SetPoint(ref hovPoint, e.X, e.Y);

            if (isMouseDown || HoverHighlighting && !prevPoint.Equals(hovPoint))
            {
                if (PaintMode == LayerPaintMode.Single && isMouseDown) //while the mouse down, every tile the cursor enters will be updated.
                {
                    SetPoint(ref drgPoint, e.X + (Clipboard.Section.Width - 1) * tSize, e.Y + (Clipboard.Section.Height - 1) * tSize);
                    PasteClipData(new Rectangle(selPoint.X, selPoint.Y, Clipboard.Section.Width, Clipboard.Section.Height));
                }
                //only redraw if above conditions met so we dont get a redraw every time the cursor moves a pixel.
                Refresh();
            }
            if (PointsChanged != null) PointsChanged(this);
        }
        #endregion

        public event PointsChangedEventHandler PointsChanged; //use this when the selection area changes
        public event UndoCountChangedEventHandler UndoCountChanged; //these are needed to relay the undo/redo count to
        public event RedoCountChangedEventHandler RedoCountChanged; //the main form so it knows when to stop allowing undos
        public event LayerEditModeChangedEventHandler LayerEditModeChanged;
    }
}
