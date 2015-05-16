using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TileCartographer.Utils
{
    //this is required so the file browser only allows image files when importing resources
    public class ImageFileNameEditor : FileNameEditor
    {
        protected override void InitializeDialog(OpenFileDialog openFileDialog)
        {
            base.InitializeDialog(openFileDialog);
            //openFileDialog.Filter = "Image Files|*.bmp;*.jpg;*.jpeg;*.png";
            openFileDialog.Filter = "PNG Files|*.png";
            //leaving this on PNG for now since anything else crashes the tester...
        }
    }
}
