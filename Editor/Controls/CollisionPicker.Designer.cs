namespace TileCartographer.Controls
{
    partial class CollisionPicker
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CollisionPicker));
            this.chkUp = new System.Windows.Forms.CheckBox();
            this.chkRight = new System.Windows.Forms.CheckBox();
            this.chkDown = new System.Windows.Forms.CheckBox();
            this.chkLeft = new System.Windows.Forms.CheckBox();
            this.numLayer = new System.Windows.Forms.NumericUpDown();
            this.lblValue = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numLayer)).BeginInit();
            this.SuspendLayout();
            // 
            // chkUp
            // 
            this.chkUp.AutoSize = true;
            this.chkUp.CheckAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.chkUp.Location = new System.Drawing.Point(101, 3);
            this.chkUp.Name = "chkUp";
            this.chkUp.Size = new System.Drawing.Size(23, 31);
            this.chkUp.TabIndex = 0;
            this.chkUp.Text = "↓";
            this.chkUp.UseVisualStyleBackColor = true;
            this.chkUp.CheckedChanged += new System.EventHandler(this.chkUp_CheckedChanged);
            // 
            // chkRight
            // 
            this.chkRight.AutoSize = true;
            this.chkRight.Location = new System.Drawing.Point(134, 41);
            this.chkRight.Name = "chkRight";
            this.chkRight.Size = new System.Drawing.Size(38, 17);
            this.chkRight.TabIndex = 1;
            this.chkRight.Text = "←";
            this.chkRight.UseVisualStyleBackColor = true;
            this.chkRight.CheckedChanged += new System.EventHandler(this.chkRight_CheckedChanged);
            // 
            // chkDown
            // 
            this.chkDown.AutoSize = true;
            this.chkDown.CheckAlign = System.Drawing.ContentAlignment.TopCenter;
            this.chkDown.Location = new System.Drawing.Point(101, 66);
            this.chkDown.Name = "chkDown";
            this.chkDown.Size = new System.Drawing.Size(23, 31);
            this.chkDown.TabIndex = 2;
            this.chkDown.Text = "↑";
            this.chkDown.UseVisualStyleBackColor = true;
            this.chkDown.CheckedChanged += new System.EventHandler(this.chkDown_CheckedChanged);
            // 
            // chkLeft
            // 
            this.chkLeft.AutoSize = true;
            this.chkLeft.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkLeft.Location = new System.Drawing.Point(52, 41);
            this.chkLeft.Name = "chkLeft";
            this.chkLeft.Size = new System.Drawing.Size(38, 17);
            this.chkLeft.TabIndex = 3;
            this.chkLeft.Text = "→";
            this.chkLeft.UseVisualStyleBackColor = true;
            this.chkLeft.CheckedChanged += new System.EventHandler(this.chkLeft_CheckedChanged);
            // 
            // numLayer
            // 
            this.numLayer.Location = new System.Drawing.Point(96, 40);
            this.numLayer.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.numLayer.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.numLayer.Name = "numLayer";
            this.numLayer.Size = new System.Drawing.Size(32, 20);
            this.numLayer.TabIndex = 4;
            this.numLayer.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numLayer.ValueChanged += new System.EventHandler(this.numLayer_ValueChanged);
            // 
            // lblValue
            // 
            this.lblValue.BackColor = System.Drawing.Color.White;
            this.lblValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblValue.Font = new System.Drawing.Font("Lucida Console", 33.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblValue.Location = new System.Drawing.Point(72, 113);
            this.lblValue.Name = "lblValue";
            this.lblValue.Size = new System.Drawing.Size(80, 80);
            this.lblValue.TabIndex = 5;
            this.lblValue.Text = "FF";
            this.lblValue.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(4, 213);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(217, 265);
            this.label1.TabIndex = 6;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // CollisionPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblValue);
            this.Controls.Add(this.numLayer);
            this.Controls.Add(this.chkRight);
            this.Controls.Add(this.chkLeft);
            this.Controls.Add(this.chkDown);
            this.Controls.Add(this.chkUp);
            this.Name = "CollisionPicker";
            this.Size = new System.Drawing.Size(224, 524);
            ((System.ComponentModel.ISupportInitialize)(this.numLayer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkUp;
        private System.Windows.Forms.CheckBox chkRight;
        private System.Windows.Forms.CheckBox chkDown;
        private System.Windows.Forms.CheckBox chkLeft;
        private System.Windows.Forms.NumericUpDown numLayer;
        private System.Windows.Forms.Label lblValue;
        private System.Windows.Forms.Label label1;
    }
}
