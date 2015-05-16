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
    public partial class ProjectExplorer : UserControl
    {
        private TCProject cProj;
        private TCMap cMap;

        public ProjectExplorer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loads a TCProject and displays it's map hierarchy.
        /// </summary>
        /// <param name="Proj">The project to open.</param>
        /// <param name="New">Whether the project is new (has been saved to disk).</param>
        public void LoadProject(TCProject Proj, bool New)
        {
            cProj = Proj;
            BuildProjectTree();
            tvExplorer.SelectedNode = tvExplorer.Nodes[0];
            pgItemProperties.SelectedObject = cProj;
            if (New) tvExplorer.SelectedNode.BeginEdit();
        }

        /// <summary>
        /// Closes the project and resets the controls.
        /// </summary>
        public void CloseProject()
        {
            tvExplorer.Nodes.Clear();
            pgItemProperties.SelectedObject = null;
            cProj = null;
        }

        /// <summary>
        /// Populates the treeview control with the project's map hierarchy.
        /// </summary>
        private void BuildProjectTree()
        {
            var nList = new List<TreeNode>();
            var dic = new Dictionary<string, TreeNode>();

            foreach (TCMap map in cProj.MapList)
            {
                var path = (map.InternalPath + map.InternalName).Split('\\');
                var mapName = map.InternalName;
                var node = new TreeNode(path[0]) { Name = path[0] };

                if (path.Length == 1)
                {
                    node.Text = node.Name = path[0];
                    node.Tag = map;
                    node.ImageIndex = node.SelectedImageIndex = 1;
                    nList.Add(node);
                    continue;
                }

                if (!dic.TryGetValue(path[0], out node))
                {
                    node = new TreeNode(path[0]) { Name = path[0] + "\\", ImageIndex = 2, SelectedImageIndex = 2 };
                    dic.Add(path[0], node);
                    nList.Add(node);
                }

                var current = path[0] + "\\";

                for (int i = 1; i < path.Length; i++)
                {
                    current += path[i] + "\\";
                    try { node = node.Nodes.Find(current, false)[0]; }
                    catch
                    {
                        node = node.Nodes.Add(path[i]);
                        node.Name = current;
                        node.ImageIndex = node.SelectedImageIndex = 2;
                    }
                }

                node.ImageIndex = node.SelectedImageIndex = 1;
                node.Tag = map;
            }

            tvExplorer.Nodes.Clear();
            var pNode = new TreeNode(cProj.Title) { Name = string.Empty, Tag = cProj };
            pNode.Nodes.AddRange(nList.ToArray());
            tvExplorer.Nodes.Add(pNode);

            tvExplorer.Sort();
            tvExplorer.ExpandAll();
        }

        /// <summary>
        /// Renames all children of a given folder to ensure their file paths are in sync.
        /// </summary>
        /// <param name="root">The folder whose childen to rename.</param>
        private void RecursiveRename(TreeNode root)
        {
            for (int i = 0; i < root.Nodes.Count; i++)
            {
                var node = root.Nodes[i];
                if (node.Tag == null)
                {
                    node.Name = root.Name + node.Text + "\\";
                    RecursiveRename(node);
                }
                else if (node.Tag is TCMap) 
                { 
                    node.Name = root.Name + node.Text; 
                    ((TCMap)node.Tag).InternalPath = root.Name;
                    continue;
                }
            }
        }

        /// <summary>
        /// Finds all map files decendent from a given directory and removes them from the project.
        /// </summary>
        /// <param name="root">The folder to remove decendent maps from.</param>
        private void RecursiveDelete(TreeNode root)
        {
            for (int i = 0; i < root.Nodes.Count; i++)
                RecursiveDelete(root.Nodes[i]);

            if (root.Tag is TCMap) 
                cProj.MapList.Remove((TCMap)root.Tag);
        }

        #region Event Handlers
        private void tvExplorer_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            tvExplorer.SelectedNode = e.Node;
        }

        private void tvExplorer_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //make sure the context menu options match the selected object
            var node = e.Node;
            addFolderToolStripMenuItem.Visible = addMapToolStripMenuItem.Visible = node.Tag is TCProject || node.Tag == null;
            deleteToolStripMenuItem.Visible = !(node.Tag is TCProject);
        }

        private void tvExplorer_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label == null) return; //ignore cancelled edits
            if (e.Label == "")
            {
                MessageBox.Show("Name cannot be blank.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.CancelEdit = true;
                return;
            }

            var node = e.Node;
            var parent = node.Parent;
            var label = e.Label;

            //make sure the there will be no filename conflict with the edit
            if (node.Tag is TCProject) ((TCProject)node.Tag).Title = label;
            if (node.Tag is TCMap)
            {
                var map = (TCMap)node.Tag;

                if (parent.Nodes.Find(parent.Name + label, false).Length > 0)
                {
                    MessageBox.Show("Map already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    e.CancelEdit = true;
                }
                else
                {
                    node.Name = node.Parent.Name + label;
                    ((TCMap)node.Tag).InternalName = e.Label;
                }
            }
            if (node.Tag == null)
            {
                if (parent.Nodes.Find(parent.Name + label + "\\", false).Length > 0)
                {
                    MessageBox.Show("Folder already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    e.CancelEdit = true;
                }
                else
                {
                    node.Name = node.Parent.Name + label + "\\";
                    RecursiveRename(node);
                }
            }

            //refresh the property grid to update changes
            pgItemProperties.Refresh();
        }

        private void tvExplorer_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var node = e.Node;
            if (node.Tag == null) return; //dont react to double-clicking a folder

            //load the object into the property grid for editing
            if (node.Tag is TCProject)
            {
                tvExplorer.Nodes[0].Expand();
                cMap = null;
                pgItemProperties.SelectedObject = node.Tag;
                if (SelectedMapChanged != null) SelectedMapChanged(this, null); //null map when project file is selected
            }
            if (node.Tag is TCMap)
            {
                var map = (TCMap)node.Tag;
                cMap = map;
                pgItemProperties.SelectedObject = map;
                if (SelectedMapChanged != null) SelectedMapChanged(this, map);
            }
        }

        private void addMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = tvExplorer.SelectedNode;
            var map = new TCMap(15, 15) { InternalName = "Map" + cProj.MapList.Count.ToString("D3"), InternalPath = node.Name };
            
            cProj.MapList.Add(map);
            cMap = map;
            pgItemProperties.SelectedObject = map;
            
            var newNode = node.Nodes.Add(map.InternalName);
            newNode.Name = map.FullPath;
            newNode.ImageIndex = newNode.SelectedImageIndex = 1;
            newNode.Tag = map;
            tvExplorer.SelectedNode = newNode;
            
            if (SelectedMapChanged != null) SelectedMapChanged(this, map);
            newNode.BeginEdit();
        }

        private void addFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = tvExplorer.SelectedNode;
            var newNode = node.Nodes.Add("New Folder");
            newNode.Name = node.Name + newNode.Text + "\\";

            //make sure the folder name is unique
            int index = 1;
            while (node.Nodes.Find(newNode.Name, false).Length > 1)
            {
                newNode.Text = "New Folder (" + index++.ToString() + ")";
                newNode.Name = node.Name + newNode.Text + "\\";
            }

            newNode.ImageIndex = newNode.SelectedImageIndex = 2;
            tvExplorer.SelectedNode = newNode;
            newNode.BeginEdit();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure? Map deletion cannot be undone.", "Warning!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;

            //need to recursive delete instead of 
            //plain node delete so we account for each 
            //map file being removed from the project
            RecursiveDelete(tvExplorer.SelectedNode);
            tvExplorer.SelectedNode.Remove();
            if (SelectedMapChanged != null) SelectedMapChanged(this, null);
        }

        private void pgItemProperties_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            //relay the PropertyChanged event to anything using this control
            if (MapPropertyChanged != null) MapPropertyChanged(this, cMap, e);
        }
        #endregion

        public event SelectedMapChangedEventHandler SelectedMapChanged;
        public event MapPropertyChangedEventHandler MapPropertyChanged;
    }
}
