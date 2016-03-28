using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TileCartographer.Library
{
    [Serializable]
    public class TCMap
    {
        private BytePoint2D size;

        #region Viewable Properties
        public MapFlags Flags { get; set; }
        public string DisplayName { get; set; }
        public byte Width { get { return size.X; } set { size.X = value; Resize(); } }
        public byte Height { get { return size.Y; } set { size.Y = value; Resize(); } }
        public List<MapConnection> Connections { get; set; }
        public List<WarpConnection> Warps { get; set; }
        public int TileSetIndex { get; set; }
        public string BorderMap { get; set; }
        public BytePoint2D BorderOffset { get; set; }
        #endregion

        public string InternalName;
        public string InternalPath;
        public string FullPath { get { return InternalPath + InternalName; } }
        public BytePoint2D[][,] Layers;

        public TCMap(byte Width, byte Height)
        {
            Flags = 0;
            size = new BytePoint2D(Width, Height);
            InternalPath = "";
            InternalName = "";
            DisplayName = "";
            BorderMap = "";

            Layers = new BytePoint2D[4][,];

            for (int x = 0; x < 4; x++)
                Layers[x] = new BytePoint2D[Width, Height];

            for (int x = 0; x < 4; x++)
                for (int i = 0; i < Width; i++)
                    for (int j = 0; j < Height; j++) //0xFF represents -1. when the X coordinate is -1, this indicates the tile is empty.
                        Layers[x][i, j].X = 0xFF;    //the value zero is not used because [0, 0] would refer to the first tile in the tileset.


            Connections = new List<MapConnection>();
            Warps = new List<WarpConnection>();
        }

        //make sure to update this with any new fields
        public void CopyFrom(TCMap newMap)
        {
            size = newMap.size;

            InternalPath = newMap.InternalPath;
            InternalName = newMap.InternalName;
            Layers = newMap.Layers;

            Flags = newMap.Flags;
            DisplayName = newMap.DisplayName;
            Connections = newMap.Connections;
            Warps = newMap.Warps;
            TileSetIndex = newMap.TileSetIndex;
            BorderMap = newMap.BorderMap;
            BorderOffset = newMap.BorderOffset;
        }

        public static TCMap DeepClone(TCMap obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (TCMap)formatter.Deserialize(ms);
            }
        }

        public static TCMap FromFile(string Filename)
        {
            var br = new BinaryReader(new FileStream(Filename, FileMode.Open, FileAccess.Read));
            var map = FromStream(br);

            br.Close();
            br.Dispose();

            return map;
        }

        public static TCMap FromStream(BinaryReader br)
        {
            var map = new TCMap(1, 1);
            map.Flags = (MapFlags)br.ReadByte();
            map.InternalPath = br.ReadString();
            map.InternalName = br.ReadString();
            map.DisplayName = br.ReadString();
            map.Width = br.ReadByte();
            map.Height = br.ReadByte();
            int count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var con = new MapConnection();
                con.Direction = (ConnectionType)br.ReadByte();
                con.MapName = br.ReadString();
                con.Offset = br.ReadInt16();
                map.Connections.Add(con);
            }
            count = br.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var warp = new WarpConnection();
                warp.MapName = br.ReadString();
                warp.Entry = new BytePoint2D(br.ReadByte(), br.ReadByte());
                warp.Exit = new BytePoint2D(br.ReadByte(), br.ReadByte());
                map.Warps.Add(warp);
            }
            map.BorderMap = br.ReadString();
            map.BorderOffset = new BytePoint2D(br.ReadByte(), br.ReadByte());
            map.TileSetIndex = br.ReadInt32();
            for (int x = 0; x < 4; x++)
                for (int i = 0; i < map.size.X; i++)
                    for (int j = 0; j < map.size.Y; j++)
                    {
                        map.Layers[x][i, j].X = br.ReadByte();
                        map.Layers[x][i, j].Y = br.ReadByte();
                    }
            return map;
        }

        public void Save(string Filename)
        {
            var dir = Directory.GetParent(Filename).FullName;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            var br = new BinaryWriter(new FileStream(Filename, FileMode.Create, FileAccess.Write));

            Save(br);

            br.Close();
            br.Dispose();
        }

        public void Save(BinaryWriter br)
        {
            br.Write((byte)Flags);
            br.Write(InternalPath);
            br.Write(InternalName);
            br.Write(DisplayName);
            br.Write(size.X);
            br.Write(size.Y);
            br.Write(Connections.Count);
            for (int i = 0; i < Connections.Count; i++)
            {
                br.Write((byte)Connections[i].Direction);
                br.Write(Connections[i].MapName);
                br.Write(Connections[i].Offset);
            }
            br.Write(Warps.Count);
            for (int i = 0; i < Warps.Count; i++)
            {
                br.Write(Warps[i].MapName);
                br.Write(Warps[i].Entry.X);
                br.Write(Warps[i].Entry.Y);
                br.Write(Warps[i].Exit.X);
                br.Write(Warps[i].Exit.Y);
            }
            br.Write(BorderMap);
            br.Write(BorderOffset.X);
            br.Write(BorderOffset.Y);
            br.Write(TileSetIndex);
            for (int x = 0; x < 4; x++)
                for (int i = 0; i < size.X; i++)
                    for (int j = 0; j < size.Y; j++)
                    {
                        br.Write(Layers[x][i, j].X);
                        br.Write(Layers[x][i, j].Y);
                    }
        }

        public override string ToString()
        {
            return InternalPath + InternalName;
        }

        private void Resize()
        {
            var arr = new BytePoint2D[4][,];

            for (int x = 0; x < 4; x++)
                arr[x] = new BytePoint2D[size.X, size.Y];

            for (int x = 0; x < 4; x++)
                for (int i = 0; i < Width; i++)
                    for (int j = 0; j < Height; j++)
                        arr[x][i, j].X = 0xFF;

            for (int x = 0; x < 4; x++)
                for (int i = 0; i < Math.Min(Layers[x].GetLength(0), arr[x].GetLength(0)); i++)
                    for (int j = 0; j < Math.Min(Layers[x].GetLength(1), arr[x].GetLength(1)); j++)
                        arr[x][i, j] = Layers[x][i, j];

            Layers = arr;
        }
    }
}
