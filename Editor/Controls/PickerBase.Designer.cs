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
            this.SuspendLayout();
            // 
            // PickerBase
            // 
            this.AutoScroll = true;
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PickerBase_MouseDown);
            this.MouseEnter += new System.EventHandler(this.PickerBase_MouseEnter);
            this.MouseLeave += new System.EventHandler(this.PickerBase_MouseLeave);
            this.ResumeLayout(false);

        }

        #endregion


    }
}
