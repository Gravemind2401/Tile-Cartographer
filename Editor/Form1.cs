using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TileCartographer.Library;
using TileCartographer.Controls;

namespace TileCartographer
{
    public partial class Form1 : Form
    {
        TCSettings settings;
        string Filename = "";
        TCProject mainProj;

        public Form1(string[] args)
        {
            InitializeComponent();

            settings = TCSettings.Load(Application.StartupPath + "\\settings.dat");

            SetViewMode(settings.ViewMode);
            SetEditMode(settings.EditMode);
            SetDrawMode(settings.PaintMode);
            showGridToolStripMenuItem.Checked = showGridToolStripButton.Checked = MapEditor.MapCanvas.ShowGrid = settings.Flags.HasFlag(TCSettingsFlags.ShowGrid);
            dimOtherLayersToolStripMenuItem.Checked = MapEditor.MapCanvas.DimLayers = settings.Flags.HasFlag(TCSettingsFlags.DimLayers);
            undimOnExitToolStripMenuItem.Checked = MapEditor.MapCanvas.UnDimOnExit = settings.Flags.HasFlag(TCSettingsFlags.UnDimOnExit);
            SetButtonsEnabled(false);

            if (args.Length > 0)
            {
                mainProj = TCProject.FromFile(args[0]);
                projectExplorer1.LoadProject(mainProj, false);
                Filename = args[0];
                SetButtonsEnabled(true);
            }
        }

        #region Private Methods
        private void SetViewMode(LayerViewMode mode)
        {
            currentAndBelowToolStripMenuItem.Checked = (mode == LayerViewMode.CurrentAndBelow);
            allLayersToolStripMenuItem.Checked = (mode == LayerViewMode.AllLayers);
            settings.ViewMode = MapEditor.MapCanvas.ViewMode = mode;
        }

        private void SetEditMode(LayerEditMode mode)
        {
            layer1ToolStripMenuItem.Checked = layer1ToolStripButton.Checked = (mode == LayerEditMode.Layer1);
            layer2ToolStripMenuItem.Checked = layer2ToolStripButton.Checked = (mode == LayerEditMode.Layer2);
            layer3ToolStripMenuItem.Checked = layer3ToolStripButton.Checked = (mode == LayerEditMode.Layer3);
            collisionToolStripMenuItem.Checked = collisionToolStripButton.Checked = (mode == LayerEditMode.Collision);
            settings.EditMode = MapEditor.MapCanvas.LayerMode = mode;
        }

        private void SetDrawMode(LayerPaintMode mode)
        {
            pencilToolStripMenuItem.Checked = pencilToolStripButton.Checked = (mode == LayerPaintMode.Single);
            rectangleToolStripMenuItem.Checked = rectangleToolStripButton.Checked = (mode == LayerPaintMode.Rectangle);
            selectToolStripMenuItem.Checked = selectToolStripButton.Checked = (mode == LayerPaintMode.Select);
            fillToolStripMenuItem.Checked = fillToolStripButton.Checked = (mode == LayerPaintMode.Fill);
            settings.PaintMode = MapEditor.MapCanvas.PaintMode = mode;
        }

        private void SetButtonsEnabled(bool enabled)
        {
            //File Menu
            closeToolStripMenuItem.Enabled = saveToolStripMenuItem.Enabled = saveAsToolStripMenuItem.Enabled = enabled;

            //Edit Menu
            /*undoToolStripMenuItem.Enabled = redoToolStripMenuItem.Enabled =*/
            cutToolStripMenuItem.Enabled = copyToolStripMenuItem.Enabled = deleteToolStripMenuItem.Enabled
            = shiftUpToolStripMenuItem.Enabled = shiftLeftToolStripMenuItem.Enabled = shiftRightToolStripMenuItem.Enabled
            = shiftDownToolStripMenuItem.Enabled = enabled;

            //View Menu
            allLayersToolStripMenuItem.Enabled = currentAndBelowToolStripMenuItem.Enabled = showGridToolStripMenuItem.Enabled
            = dimOtherLayersToolStripMenuItem.Enabled = undimOnExitToolStripMenuItem.Enabled = refreshToolStripMenuItem.Enabled = enabled;

            //Mode Menu
            layer1ToolStripMenuItem.Enabled = layer2ToolStripMenuItem.Enabled = layer3ToolStripMenuItem.Enabled 
            = collisionToolStripMenuItem.Enabled = enabled;

            //Draw Menu
            pencilToolStripMenuItem.Enabled = rectangleToolStripMenuItem.Enabled = fillToolStripMenuItem.Enabled
            = selectToolStripMenuItem.Enabled = enabled;

            //Scale Menu
            zoomInToolStripMenuItem.Enabled = zoomOutToolStripMenuItem.Enabled = enabled;

            //Tools Menu
            exportToImageToolStripMenuItem.Enabled = enabled;

            //Game Menu
            playtestToolStripMenuItem.Enabled = exportToolStripMenuItem.Enabled = enabled;

            //Toolbar
            saveToolStripButton.Enabled = cutToolStripButton.Enabled = copyToolStripButton.Enabled = deleteToolStripButton.Enabled
            /*= undoToolStripButton.Enabled = redoToolStripButton.Enabled*/ = pencilToolStripButton.Enabled = rectangleToolStripButton.Enabled
            = fillToolStripButton.Enabled = selectToolStripButton.Enabled = layer1ToolStripButton.Enabled = layer2ToolStripButton.Enabled 
            = layer3ToolStripButton.Enabled = collisionToolStripButton.Enabled = showGridToolStripButton.Enabled = zoomInToolStripButton.Enabled 
            = zoomOutToolStripButton.Enabled = playtestToolStripButton.Enabled = shiftUpToolStripButton.Enabled = shiftLeftToolStripButton.Enabled 
            = shiftRightToolStripButton.Enabled = shiftDownToolStripButton.Enabled = exportToolStripButton.Enabled = enabled;
        }
        #endregion

        #region Menu Event Handlers
        #region File Menu
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mainProj = new TCProject() { Title = "New Project" };
            projectExplorer1.LoadProject(mainProj, true);
            SetButtonsEnabled(true);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "TCProject Files|*.tcproj";

            if (ofd.ShowDialog() != DialogResult.OK) return;

            if (mainProj != null) closeToolStripMenuItem_Click(sender, e);

            mainProj = TCProject.FromFile(ofd.FileName);
            projectExplorer1.LoadProject(mainProj, false);
            Filename = ofd.FileName;
            SetButtonsEnabled(true);
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapEditor.CloseMap();
            projectExplorer1.CloseProject();
            mainProj = null;
            Filename = string.Empty;
            SetButtonsEnabled(false);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Filename == "")
            {
                saveAsToolStripMenuItem_Click(this, e);
                return;
            }

            mainProj.Save(Filename);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.FileName = mainProj.Title;
            sfd.Filter = "TCProject Files|*.tcproj";

            if (sfd.ShowDialog() != DialogResult.OK) return;

            Filename = sfd.FileName;
            saveToolStripMenuItem_Click(this, e);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            settings.Save(Application.StartupPath + "\\settings.dat");
            Application.Exit();
        }
        #endregion

        #region Edit Menu
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapEditor.MapCanvas.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapEditor.MapCanvas.Redo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapEditor.MapCanvas.CutSelection();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapEditor.MapCanvas.CopySelection();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapEditor.MapCanvas.DeleteSelection();
        }

        private void shiftUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapEditor.MapCanvas.ShiftSelection(0, -1);
        }

        private void shiftLeftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapEditor.MapCanvas.ShiftSelection(-1, 0);
        }

        private void shiftRightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapEditor.MapCanvas.ShiftSelection(1, 0);
        }

        private void shiftDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapEditor.MapCanvas.ShiftSelection(0, 1);
        }
        #endregion

        #region View Menu
        private void allLayersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetViewMode(LayerViewMode.AllLayers);
        }

        private void currentAndBelowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetViewMode(LayerViewMode.CurrentAndBelow);
        }

        private void showGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapEditor.MapCanvas.ShowGrid = !MapEditor.MapCanvas.ShowGrid;
            showGridToolStripMenuItem.Checked = showGridToolStripButton.Checked = MapEditor.MapCanvas.ShowGrid;

            if (MapEditor.MapCanvas.ShowGrid)
                settings.Flags |= TCSettingsFlags.ShowGrid;
            else
                settings.Flags &= ~TCSettingsFlags.ShowGrid;
        }

        private void dimOtherLayersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapEditor.MapCanvas.DimLayers = !MapEditor.MapCanvas.DimLayers;
            dimOtherLayersToolStripMenuItem.Checked = MapEditor.MapCanvas.DimLayers;

            if (MapEditor.MapCanvas.DimLayers)
                settings.Flags |= TCSettingsFlags.DimLayers;
            else
                settings.Flags &= ~TCSettingsFlags.DimLayers;
        }

        private void undimOnExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapEditor.MapCanvas.UnDimOnExit = !MapEditor.MapCanvas.UnDimOnExit;
            undimOnExitToolStripMenuItem.Checked = MapEditor.MapCanvas.UnDimOnExit;

            if (MapEditor.MapCanvas.UnDimOnExit)
                settings.Flags |= TCSettingsFlags.UnDimOnExit;
            else
                settings.Flags &= ~TCSettingsFlags.UnDimOnExit;
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapEditor.MapCanvas.RedrawAll();
        }
        #endregion

        #region Mode Menu
        private void layer1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetEditMode(LayerEditMode.Layer1);
        }

        private void layer2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetEditMode(LayerEditMode.Layer2);
        }

        private void layer3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetEditMode(LayerEditMode.Layer3);
        }

        private void collisionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetEditMode(LayerEditMode.Collision);
        }
        #endregion

        #region Draw Menu
        private void pencilToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDrawMode(LayerPaintMode.Single);
        }

        private void rectangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDrawMode(LayerPaintMode.Rectangle);
        }

        private void fillToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDrawMode(LayerPaintMode.Fill);
        }

        private void selectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SetDrawMode(LayerPaintMode.Select);
        }
        #endregion

        #region Scale Menu
        private void zoomInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapEditor.MapCanvas.ZoomPower++;
        }

        private void zoomOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapEditor.MapCanvas.ZoomPower--;
        }
        #endregion

        #region Tools Menu
        private void exportToImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MapEditor.MapCanvas.tMap == null)
            {
                MessageBox.Show("No map selected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            var sfd = new SaveFileDialog() { Filter = "PNG Files|*.png", FileName = MapEditor.MapCanvas.tMap.InternalName };

            if (sfd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            MapEditor.MapCanvas.SaveToImage(sfd.FileName);
        }
        #endregion

        #region Game Menu
        private void playtestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string msg;
            if (!mainProj.ValidateAll(out msg))
            {
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            var dir = System.IO.Directory.GetParent(mainProj.FilePath).FullName;
            var fname = dir + "\\bin\\playtest.tcgame";

            mainProj.SaveContiguous(fname);

            var proc = new Process() { StartInfo = new ProcessStartInfo(Application.StartupPath + "\\TCTester.exe", "\"" + fname + "\"") };
            this.Enabled = false;
            proc.Start();
            proc.WaitForExit();
            this.Enabled = true;
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string msg;
            if (!mainProj.ValidateAll(out msg))
            {
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            var sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.FileName = mainProj.Title;
            sfd.Filter = "TCGame Files|*.tcgame";

            if (sfd.ShowDialog() != DialogResult.OK) return;

            mainProj.SaveContiguous(sfd.FileName);
        }
        #endregion
        #endregion

        #region Control Event Handlers
        private void projectExplorer1_SelectedMapChanged(object sender, TCMap map)
        {
            if (map != null)
                MapEditor.LoadMap(mainProj, map);
            else
                MapEditor.CloseMap();

            undoToolStripButton.Enabled = undoToolStripMenuItem.Enabled = false;
            redoToolStripButton.Enabled = redoToolStripMenuItem.Enabled = false;
        }

        private void projectExplorer1_MapPropertyChanged(object sender, TCMap map, PropertyValueChangedEventArgs e)
        {
            if (map != null)
                MapEditor.LoadMap(mainProj, map);
        }

        private void MapEditor_CanvasPointsChanged(object sender)
        {
            var p = MapEditor.MapCanvas.HoverPoint;
            var r = MapEditor.MapCanvas.Selection;
            string s = string.Format("X: {0}, Y: {1} || W: {2}, H: {3}", p.X.ToString("D3"), p.Y.ToString("D3"), r.Width.ToString("D3"), r.Height.ToString("D3"));

            toolStripStatusLabel1.Text = s;
        }

        private void MapEditor_CanvasUndoChanged(object sender, int count)
        {
            this.Size = this.Size;
            undoToolStripMenuItem.Enabled = undoToolStripButton.Enabled = count > 0;
        }

        private void MapEditor_CanvasRedoChanged(object sender, int count)
        {
            redoToolStripMenuItem.Enabled = redoToolStripButton.Enabled = count > 0;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            settings.Save(Application.StartupPath + "\\settings.dat");
        }
        #endregion
    }
}
