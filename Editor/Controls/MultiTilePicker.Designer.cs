namespace TileCartographer.Controls
{
    partial class MultiTilePicker
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
            ((System.ComponentModel.ISupportInitialize)(this.imgViewport)).BeginInit();
            this.SuspendLayout();
            // 
            // imgViewport
            // 
            this.imgViewport.DoubleClick += new System.EventHandler(this.imgTileset_DoubleClick);
            this.imgViewport.MouseDown += new System.Windows.Forms.MouseEventHandler(this.imgTileset_MouseDown);
            this.imgViewport.MouseEnter += new System.EventHandler(this.imgTileset_MouseEnter);
            this.imgViewport.MouseLeave += new System.EventHandler(this.imgTileset_MouseLeave);
            this.imgViewport.MouseMove += new System.Windows.Forms.MouseEventHandler(this.imgTileset_MouseMove);
            this.imgViewport.MouseUp += new System.Windows.Forms.MouseEventHandler(this.imgTileset_MouseUp);
            // 
            // MultiTilePicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "MultiTilePicker";
            ((System.ComponentModel.ISupportInitialize)(this.imgViewport)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion


    }
}
