using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TileCartographer.Library;

namespace TileCartographer.Controls
{
    /// <summary>
    /// This is essentially a tile picker and a canvas glued together.
    /// This class is a bridge between the user and the base controls,
    /// handling all communication between the picker and canvas allowing
    /// for far easier code management on forms requiring these controls.
    /// </summary>
    public partial class MapEditor : UserControl
    {
        private TCProject cProj;
        private TCMap cMap;

        #region Methods
        public MapEditor()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loads the specified map using resources from the specified project.
        /// </summary>
        /// <param name="Project"></param>
        /// <param name="Map"></param>
        public void LoadMap(TCProject Project, TCMap Map)
        {
            cProj = Project;
            cMap = Map;

            TilePicker.Visible = MapCanvas.LayerMode != LayerEditMode.Collision;
            CollisionPicker.Visible = MapCanvas.LayerMode == LayerEditMode.Collision;

            if (cMap.TileSetIndex >= cProj.Tilesets.Count)
            {
                MessageBox.Show("Ivalid Tilset Index! Select valid index or add more tilesets.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                CloseMap();
                return;
            }

            if (!cProj.Tilesets[cMap.TileSetIndex].IsValid(TCImageType.Tileset, cProj.TileSize))
            {
                MessageBox.Show("This map is linked to an invalid tilset. The tilset must be 8xN tiles using the tilesize specified by the project.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                CloseMap();
                return;
            }

            var img = cProj.Tilesets[cMap.TileSetIndex].GetBitmap();

            TilePicker.LoadTileset(cProj, img);
            CollisionPicker.Reset();
            MultiTilePicker.LoadMultiTiles(cProj);
            MultiTilePicker.HideSelection();
            MapCanvas.LoadMap(cProj, cMap, img, TilePicker.GetClip());
        }

        /// <summary>
        /// Closes the currently open map and disposes of all editing resources.
        /// </summary>
        public void CloseMap()
        {
            CollisionPicker.Visible = false;

            TilePicker.CloseTileset();
            MultiTilePicker.CloseMultiTiles();
            MapCanvas.CloseMap();
        }
        #endregion

        #region Event Handlers
        private void TilePicker_PointsChanged(object sender)
        {
            MultiTilePicker.HideSelection(); //hide the multitile selection (so we dont get confused which tile is selected)
            TilePicker.ShowSelection();
            MapCanvas.Clipboard = TilePicker.GetClip(); //update the canvas' clip in sync with the tile picker changes.
        }

        private void MultiTilePicker_PointsChanged(object sender)
        {
            TilePicker.HideSelection(); //hide the tileset selection so we dont get confused which tile is selected
            MultiTilePicker.ShowSelection();
            MapCanvas.Clipboard = MultiTilePicker.GetClip(); //update the canvas' clip in sync with the tile picker changes.
        }

        private void MapCanvas_PointsChanged(object sender)
        {
            //relay the canvas' PointsChanged event to anything using this control
            if (CanvasPointsChanged != null) CanvasPointsChanged(this);
        }

        private void collisionPicker1_CollisionValueChanged(object sender, byte newValue)
        {
            MapCanvas.Clipboard = new TileClip() 
            { Origin = ClipOrigin.Collision, Section = new Rectangle(0, 0, 1, 1), Data = new BytePoint2D[,] { { new BytePoint2D(0xFF, CollisionPicker.value) } } };       
        }

        private void MapCanvas_UndoCountChanged(object sender, int count)
        {
            //relay the canvas' UndoChanged event to anything using this control
            if (CanvasUndoChanged != null) CanvasUndoChanged(sender, count);
        }

        private void MapCanvas_RedoCountChanged(object sender, int count)
        {
            //relay the canvas' RedoChanged event to anything using this control
            if (CanvasRedoChanged != null) CanvasRedoChanged(sender, count);
        }

        private void MapCanvas_LayerEditModeChanged(object sender, LayerEditMode mode)
        {
            CollisionPicker.Visible = (mode == LayerEditMode.Collision);
            TilePicker.Visible = (mode != LayerEditMode.Collision);
        }
        #endregion

        //these events allow access to the cavas events through
        //this control rather than accessing them directly.
        public event PointsChangedEventHandler CanvasPointsChanged;
        public event UndoCountChangedEventHandler CanvasUndoChanged;
        public event RedoCountChangedEventHandler CanvasRedoChanged;
    }
}
