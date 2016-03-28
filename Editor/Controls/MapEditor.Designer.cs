namespace TileCartographer.Controls
{
    partial class MapEditor
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.CollisionPicker = new TileCartographer.Controls.CollisionPicker();
            this.TilePicker = new TileCartographer.Controls.TilePicker();
            this.MultiTilePicker = new TileCartographer.Controls.MultiTilePicker();
            this.panel1 = new System.Windows.Forms.Panel();
            this.MapCanvas = new TileCartographer.Controls.MapCanvas();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Left;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.CollisionPicker);
            this.splitContainer1.Panel1.Controls.Add(this.TilePicker);
            this.splitContainer1.Panel1MinSize = 384;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.MultiTilePicker);
            this.splitContainer1.Panel2MinSize = 32;
            this.splitContainer1.Size = new System.Drawing.Size(277, 659);
            this.splitContainer1.SplitterDistance = 619;
            this.splitContainer1.TabIndex = 0;
            // 
            // CollisionPicker
            // 
            this.CollisionPicker.Location = new System.Drawing.Point(19, 38);
            this.CollisionPicker.Name = "CollisionPicker";
            this.CollisionPicker.Size = new System.Drawing.Size(235, 430);
            this.CollisionPicker.TabIndex = 1;
            this.CollisionPicker.Visible = false;
            this.CollisionPicker.CollisionValueChanged += new TileCartographer.Controls.CollisionValueChangedEventHandler(this.collisionPicker1_CollisionValueChanged);
            // 
            // TilePicker
            // 
            this.TilePicker.MultiSelect = true;
            this.TilePicker.AutoScroll = true;
            this.TilePicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TilePicker.HoverHighlighting = false;
            this.TilePicker.Location = new System.Drawing.Point(0, 0);
            this.TilePicker.Name = "TilePicker";
            this.TilePicker.ShowGrid = false;
            this.TilePicker.Size = new System.Drawing.Size(273, 615);
            this.TilePicker.TabIndex = 0;
            this.TilePicker.ZoomPower = 5;
            this.TilePicker.PointsChanged += new TileCartographer.Controls.PointsChangedEventHandler(this.TilePicker_PointsChanged);
            // 
            // MultiTilePicker
            // 
            this.MultiTilePicker.MultiSelect = false;
            this.MultiTilePicker.AutoScroll = true;
            this.MultiTilePicker.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MultiTilePicker.HoverHighlighting = false;
            this.MultiTilePicker.Location = new System.Drawing.Point(0, 0);
            this.MultiTilePicker.Name = "MultiTilePicker";
            this.MultiTilePicker.ShowGrid = false;
            this.MultiTilePicker.Size = new System.Drawing.Size(273, 32);
            this.MultiTilePicker.TabIndex = 0;
            this.MultiTilePicker.ZoomPower = 5;
            this.MultiTilePicker.PointsChanged += new TileCartographer.Controls.PointsChangedEventHandler(this.MultiTilePicker_PointsChanged);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel1.Controls.Add(this.MapCanvas);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(277, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(827, 659);
            this.panel1.TabIndex = 1;
            // 
            // MapCanvas
            // 
            this.MapCanvas.AutoScroll = true;
            this.MapCanvas.DimLayers = false;
            this.MapCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MapCanvas.HoverHighlighting = true;
            this.MapCanvas.LayerMode = TileCartographer.Controls.LayerEditMode.Layer1;
            this.MapCanvas.Location = new System.Drawing.Point(0, 0);
            this.MapCanvas.Name = "MapCanvas";
            this.MapCanvas.PaintMode = TileCartographer.Controls.LayerPaintMode.Single;
            this.MapCanvas.ShowGrid = false;
            this.MapCanvas.Size = new System.Drawing.Size(823, 655);
            this.MapCanvas.TabIndex = 0;
            this.MapCanvas.UnDimOnExit = false;
            this.MapCanvas.ViewMode = TileCartographer.Controls.LayerViewMode.AllLayers;
            this.MapCanvas.ZoomPower = 5;
            this.MapCanvas.PointsChanged += new TileCartographer.Controls.PointsChangedEventHandler(this.MapCanvas_PointsChanged);
            this.MapCanvas.UndoCountChanged += new TileCartographer.Controls.UndoCountChangedEventHandler(this.MapCanvas_UndoCountChanged);
            this.MapCanvas.RedoCountChanged += new TileCartographer.Controls.RedoCountChangedEventHandler(this.MapCanvas_RedoCountChanged);
            this.MapCanvas.LayerEditModeChanged += new TileCartographer.Controls.LayerEditModeChangedEventHandler(this.MapCanvas_LayerEditModeChanged);
            // 
            // MapEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitContainer1);
            this.Name = "MapEditor";
            this.Size = new System.Drawing.Size(1104, 659);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel1;
        private CollisionPicker CollisionPicker;
        public TilePicker TilePicker;
        public MapCanvas MapCanvas;
        public MultiTilePicker MultiTilePicker;
    }
}
