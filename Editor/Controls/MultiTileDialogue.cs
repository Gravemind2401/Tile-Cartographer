using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TileCartographer.Library;

namespace TileCartographer.Controls
{
    public partial class MultiTileDialogue : Form
    {
        private byte forceIndex = 0xFF;

        public MultiTileDialogue()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Shows the form as an application modal dialogue.
        /// </summary>
        /// <param name="owner">The parent window of the dialogue.</param>
        /// <param name="tProj">The project containg the tile resources.</param>
        /// <param name="index">The index of the multitile to be expanded.</param>
        /// <returns>The reference tile index selected by the user.</returns>
        public byte ShowDialog(IWin32Window owner, TCProject tProj, int index)
        {
            referenceTilePicker1.LoadRefSheet(tProj, index);

            base.ShowDialog(owner);

            return forceIndex;
        }

        private void referenceTilePicker1_IndexChanged(object sender, byte index)
        {
            //once the user has selected a tile, update
            //the index value and close the form. If the
            //form was opened using ShowDialogue, the 
            //index will then be returned to the caller.
            forceIndex = index;
            this.Close();
        }
    }
}
