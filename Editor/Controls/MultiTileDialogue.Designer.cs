namespace TileCartographer.Controls
{
    partial class MultiTileDialogue
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.referenceTilePicker1 = new TileCartographer.Controls.RefIndexPicker();
            this.SuspendLayout();
            // 
            // referenceTilePicker1
            // 
            this.referenceTilePicker1.HoverHighlighting = false;
            this.referenceTilePicker1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.referenceTilePicker1.Location = new System.Drawing.Point(0, 0);
            this.referenceTilePicker1.Name = "referenceTilePicker1";
            this.referenceTilePicker1.Size = new System.Drawing.Size(256, 192);
            this.referenceTilePicker1.TabIndex = 0;
            this.referenceTilePicker1.IndexChanged += new TileCartographer.Controls.IndexChangedEventHandler(this.referenceTilePicker1_IndexChanged);
            // 
            // MultiTileDialogue
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(256, 192);
            this.Controls.Add(this.referenceTilePicker1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MultiTileDialogue";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "MultiTile Index Override";
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.RefIndexPicker referenceTilePicker1;
    }
}