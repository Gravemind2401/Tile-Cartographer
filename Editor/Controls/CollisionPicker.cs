using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TileCartographer.Library;

namespace TileCartographer.Controls
{
    public partial class CollisionPicker : UserControl
    {
        public CollisionByte value;

        public CollisionPicker()
        {
            InitializeComponent();
            lblValue.Text = value.ToString("X2");
        }

        public void Reset()
        {
            numLayer.Value = 0;
            chkLeft.Checked = chkRight.Checked = chkUp.Checked = chkDown.Checked = false;
            if (CollisionValueChanged != null) CollisionValueChanged(this, value);
        }

        #region Event Handlers
        private void numLayer_ValueChanged(object sender, EventArgs e)
        {
            value.Layer = (int)numLayer.Value;
            lblValue.Text = value.ToString("X2");
            if (CollisionValueChanged != null) CollisionValueChanged(this, value);
        }

        private void chkUp_CheckedChanged(object sender, EventArgs e)
        {
            value.Up = chkUp.Checked;
            lblValue.Text = value.ToString("X2");
            if (CollisionValueChanged != null) CollisionValueChanged(this, value);
        }

        private void chkRight_CheckedChanged(object sender, EventArgs e)
        {
            value.Right = chkRight.Checked;
            lblValue.Text = value.ToString("X2");
            if (CollisionValueChanged != null) CollisionValueChanged(this, value);
        }

        private void chkDown_CheckedChanged(object sender, EventArgs e)
        {
            value.Down = chkDown.Checked;
            lblValue.Text = value.ToString("X2");
            if (CollisionValueChanged != null) CollisionValueChanged(this, value);
        }

        private void chkLeft_CheckedChanged(object sender, EventArgs e)
        {
            value.Left = chkLeft.Checked;
            lblValue.Text = value.ToString("X2");
            if (CollisionValueChanged != null) CollisionValueChanged(this, value);
        }

        private void btn0xFF_Click(object sender, EventArgs e)
        {
            numLayer.Value = -1;
            chkLeft.Checked = chkRight.Checked = chkUp.Checked = chkDown.Checked = true;
            if (CollisionValueChanged != null) CollisionValueChanged(this, value);
        }

        private void btn0x00_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void btn0x_0_Click(object sender, EventArgs e)
        {
            chkLeft.Checked = chkRight.Checked = chkUp.Checked = chkDown.Checked = false;
            if (CollisionValueChanged != null) CollisionValueChanged(this, value);
        }
        #endregion

        public event CollisionValueChangedEventHandler CollisionValueChanged;
    }
}
