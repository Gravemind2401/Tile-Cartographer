using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using TileCartographer.Library;

namespace TileCartographer.Library
{
    //TODO: 
    //      export/import newer values
    public class TCProject
    {
        private string fName;
        public string FilePath { get { return fName; } }

        public ProjectFlags Flags;
        public string Title { get; set; }

        public TileSize TileSize { get; set; }

        public List<TCImage> Tilesets { get; set; }
        public List<TCImage> MultiTiles { get; set; }
        public List<TCImage> Sprites { get; set; } //TODO: implement (?)
        public List<TCMap> MapList;

        public byte PlayerSpriteID { get; set; }
        public string StartMap { get; set; }
        public BytePoint2D StartLocation { get; set; }

        public TCProject()
        {
            Tilesets = new List<TCImage>();
            MultiTiles = new List<TCImage>();
            Sprites = new List<TCImage>();
            MapList = new List<TCMap>();
            TileSize = TileSize.x32;
        }

        public TCMap GetMapByName(string Name)
        {
            foreach (var map in MapList)
                if (Name.ToLower() == map.FullPath.ToLower()) return map;

            return null;
        }

        public static TCProject FromFile(string Filename)
        {
            var proj = new TCProject();
            var dir = Directory.GetParent(Filename).FullName;
            var br = new BinaryReader(new FileStream(Filename, FileMode.Open, FileAccess.Read));

            proj.fName = Filename;
            proj.Flags = (ProjectFlags)br.ReadByte();
            proj.Title = br.ReadString();

            proj.TileSize = (TileSize)br.ReadByte();
            proj.PlayerSpriteID = br.ReadByte();
            proj.StartMap = br.ReadString();
            proj.StartLocation = new BytePoint2D(br.ReadByte(), br.ReadByte());

            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
                proj.Tilesets.Add(new TCImage(br.ReadString().Replace("$(ProjectDir)", dir)));

            count = br.ReadInt32();
            for (int i = 0; i < count; i++)
                proj.MultiTiles.Add(new TCImage(br.ReadString().Replace("$(ProjectDir)", dir)));

            count = br.ReadInt32();
            for (int i = 0; i < count; i++)
                proj.Sprites.Add(new TCImage(br.ReadString().Replace("$(ProjectDir)", dir)));

            count = br.ReadInt32();
            for (int i = 0; i < count; i++)
                proj.MapList.Add(TCMap.FromFile(dir + "\\Maps\\" + br.ReadString()));

            br.Close();
            br.Dispose();

            return proj;
        }

        public void Save(string Filename)
        {
            var dir = Directory.GetParent(Filename).FullName;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var br = new BinaryWriter(new FileStream(Filename, FileMode.Create, FileAccess.Write));

            br.Write((byte)Flags);
            br.Write(Title);

            br.Write((byte)TileSize);
            br.Write(PlayerSpriteID);
            br.Write(StartMap);
            br.Write(StartLocation.X);
            br.Write(StartLocation.Y);

            br.Write(Tilesets.Count);
            foreach (var set in Tilesets)
                br.Write(set.FilePath.Replace(dir, "$(ProjectDir)")); //if file is in working dir, convert to relative path

            br.Write(MultiTiles.Count);
            foreach (var set in MultiTiles)
                br.Write(set.FilePath.Replace(dir, "$(ProjectDir)"));

            br.Write(Sprites.Count);
            foreach (var set in Sprites)
                br.Write(set.FilePath.Replace(dir, "$(ProjectDir)"));

            br.Write(MapList.Count);
            for (int i = 0; i < MapList.Count; i++)
            {
                br.Write("Map" + i.ToString("D3") + ".tcmap");
                MapList[i].Save(dir + "\\Maps\\Map" + i.ToString("D3") + ".tcmap");
            }

            br.Close();
            br.Dispose();
        }

#if WINFORMS
        public bool ValidateAll(out string message)
        {
            message = string.Empty;

            if (Tilesets.Count == 0) { message = "No tilesets!"; return false; }
            if (Sprites.Count == 0) { message = "No sprites!"; return false; }

            foreach (var map in MapList)
            {
                if (map.TileSetIndex >= Tilesets.Count)
                {
                    message = map.InternalName + " has an invalid tileset index."; 
                    return false;
                }

                for(int x = 0; x < 3; x++)
                    for(int i = 0; i < map.Width; i++)
                        for (int j = 0; j < map.Height; j++)
                            if (map.Layers[x][i, j].X != 255 && (map.Layers[x][i, j].X - 8) >= MultiTiles.Count)
                            {
                                message = map.InternalName + " has invalid multitile IDs.";
                                return false;
                            }
            }
            if (PlayerSpriteID >= Sprites.Count) { message = "Invalid PlayerSpriteID!"; return false; }

            foreach (var img in Tilesets)
                if (!img.IsValid(TCImageType.Tileset, TileSize)) { message = img.ToString() + " is an invalid tileset."; return false; }

            foreach (var img in MultiTiles)
                if (!(img.IsValid(TCImageType.AnimTile, TileSize) || img.IsValid(TCImageType.AutoTile3x4, TileSize))) { message = img.ToString() + " is an invalid MultiTile."; return false; }

            foreach (var img in Sprites)
                if (!(img.IsValid(TCImageType.Sprite3x4, TileSize) || img.IsValid(TCImageType.Sprite4x4, TileSize))) { message = img.ToString() + " is an invalid sprite."; return false; }

            return true;
        }

        public void SaveContiguous(string Filename)
        {
            var dir = Directory.GetParent(Filename).FullName;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var br = new BinaryWriter(new FileStream(Filename, FileMode.Create, FileAccess.Write));

            br.Write((byte)Flags);
            br.Write(Title);

            br.Write((byte)TileSize);
            br.Write(PlayerSpriteID);
            br.Write(StartMap);
            br.Write(StartLocation.X);
            br.Write(StartLocation.Y);

            br.Write(Tilesets.Count);
            foreach (var set in Tilesets)
                using (var bitm = set.GetBitmap())
                {
                    var cv = new ImageConverter();
                    byte[] arr = (byte[])cv.ConvertTo(bitm, typeof(byte[]));
                    br.Write(arr.Length);
                    br.Write(arr);
                }

            br.Write(MultiTiles.Count);
            foreach (var set in MultiTiles)
                using (var bitm = set.GetBitmap())
                {
                    var cv = new ImageConverter();
                    byte[] arr = (byte[])cv.ConvertTo(bitm, typeof(byte[]));
                    br.Write(arr.Length);
                    br.Write(arr);
                }

            br.Write(Sprites.Count);
            foreach (var set in Sprites)
                using (var bitm = set.GetBitmap())
                {
                    var cv = new ImageConverter();
                    byte[] arr = (byte[])cv.ConvertTo(bitm, typeof(byte[]));
                    br.Write(arr.Length);
                    br.Write(arr);
                }

            br.Write(MapList.Count);
            for (int i = 0; i < MapList.Count; i++)
                MapList[i].Save(br);

            br.Close();
            br.Dispose();
        }
#else
        public static TCProject FromContiguous(string Filename, Microsoft.Xna.Framework.Graphics.GraphicsDevice device)
        {
            var proj = new TCProject();
            var dir = Directory.GetParent(Filename);
            var br = new BinaryReader(new FileStream(Filename, FileMode.Open, FileAccess.Read));

            proj.Flags = (ProjectFlags)br.ReadByte();
            proj.Title = br.ReadString();

            proj.TileSize = (TileSize)br.ReadByte();
            proj.PlayerSpriteID = br.ReadByte();
            proj.StartMap = br.ReadString();
            proj.StartLocation = new BytePoint2D(br.ReadByte(), br.ReadByte());

            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
                proj.Tilesets.Add(new TCImage(device, br));

            count = br.ReadInt32();
            for (int i = 0; i < count; i++)
                proj.MultiTiles.Add(new TCImage(device, br));

            count = br.ReadInt32();
            for (int i = 0; i < count; i++)
                proj.Sprites.Add(new TCImage(device, br));

            count = br.ReadInt32();
            for (int i = 0; i < count; i++)
                proj.MapList.Add(TCMap.FromStream(br));

            br.Close();
            br.Dispose();

            return proj;
        }
#endif
    }
}
