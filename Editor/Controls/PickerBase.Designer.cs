namespace TileCartographer.Controls
{
    partial class PickerBase
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
            this.imgViewport = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.imgViewport)).BeginInit();
            this.SuspendLayout();
            // 
            // imgViewport
            // 
            this.imgViewport.Location = new System.Drawing.Point(0, 0);
            this.imgViewport.Name = "imgViewport";
            this.imgViewport.Size = new System.Drawing.Size(150, 150);
            this.imgViewport.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.imgViewport.TabIndex = 1;
            this.imgViewport.TabStop = false;
            // 
            // PickerBase
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.imgViewport);
            this.Name = "PickerBase";
            ((System.ComponentModel.ISupportInitialize)(this.imgViewport)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.PictureBox imgViewport;



    }
}
