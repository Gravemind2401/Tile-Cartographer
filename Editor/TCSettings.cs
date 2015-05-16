using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TileCartographer.Library;
using TileCartographer.Controls;

namespace TileCartographer
{
    public class TCSettings
    {
        public TCSettingsFlags Flags;
        public LayerViewMode ViewMode;
        public LayerEditMode EditMode;
        public LayerPaintMode PaintMode;

        public TCSettings()
        {
            Flags = TCSettingsFlags.ShowGrid | TCSettingsFlags.DimLayers | TCSettingsFlags.UnDimOnExit;
            ViewMode = LayerViewMode.AllLayers;
            EditMode = LayerEditMode.Layer1;
            PaintMode = LayerPaintMode.Single;
        }

        public static TCSettings Load(string filename)
        {
            var settings = new TCSettings();

            if (File.Exists(filename))
            {
                var br = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read));
                settings.ViewMode = (LayerViewMode)br.ReadByte();
                settings.EditMode = (LayerEditMode)br.ReadByte();
                settings.PaintMode = (LayerPaintMode)br.ReadByte();
                settings.Flags = (TCSettingsFlags)br.ReadByte();

                br.Close();
                br.Dispose();
            }

            return settings;
        }

        public void Save(string filename)
        {
            var dir = Directory.GetParent(filename).FullName;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var br = new BinaryWriter(new FileStream(filename, FileMode.Create, FileAccess.Write));
            br.Write((byte)ViewMode);
            br.Write((byte)EditMode);
            br.Write((byte)PaintMode);
            br.Write((byte)Flags);

            br.Close();
            br.Dispose();
        }
    }

    [Flags]
    public enum TCSettingsFlags : byte
    {
        None    = 0,        //00000000
        ShowGrid = 1,       //00000001
        DimLayers = 2,      //00000010
        UnDimOnExit = 4,    //00000100
        Value04 = 8,        //00001000
        Value05 = 16,       //00010000
        Value06 = 32,       //00100000
        Value07 = 64,       //01000000
        Value08 = 128,      //10000000
    }
}
