using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TileCartographer.Library
{
    [Serializable]
#if WINFORMS
    [TypeConverter(typeof(TileCartographer.Utils.ValueTypeTypeConverter))]
#endif
    public struct BytePoint2D
    {
        public byte X { get; set; }
        public byte Y { get; set; }

        public BytePoint2D(byte X, byte Y) : this()
        {
            this.X = X;
            this.Y = Y;
        }

        public BytePoint2D(int X, byte Y) : this()
        {
            this.X = (byte)X;
            this.Y = Y;
        }

        public BytePoint2D(byte X, int Y) : this()
        {
            this.X = X;
            this.Y = (byte)Y;
        }

        public BytePoint2D(int X, int Y) : this()
        {
            this.X = (byte)X;
            this.Y = (byte)Y;
        }

        public bool Equals(BytePoint2D Point)
        {
            if (X == Point.X && Y == Point.Y) return true;
            return false;
        }

        public override string ToString()
        {
            return X.ToString() + ", " + Y.ToString();
        }
    }

    //convert bytes to CollisionBytes for easier access to collision data
    public struct CollisionByte
    {
        private byte bVal;

        public CollisionByte(byte val) { bVal = val; }

        public int Layer
        {
            get { return (bVal >> 4) == 15 ? -1 : (bVal >> 4); }
            set { bVal &= 0x0F; bVal |= (byte)(value << 4); }
        }

        public bool Right
        {
            get { return GetBit(0); }
            set { SetBit(0, value); }
        }

        public bool Down
        {
            get { return GetBit(1); }
            set { SetBit(1, value); }
        }

        public bool Left
        {
            get { return GetBit(2); }
            set { SetBit(2, value); }
        }

        public bool Up
        {
            get { return GetBit(3); }
            set { SetBit(3, value); }
        }

        private bool GetBit(int pos)
        {
            return (bVal & (1 << pos)) != 0;
        }

        private void SetBit(int pos, bool val)
        {
            if (val)
                bVal |= (byte)(1 << pos);
            else
                bVal &= (byte)(~(1 << pos) & 0xFF);
        }

        public static implicit operator byte(CollisionByte val) { return val.bVal; }
        public static implicit operator CollisionByte(byte val) { return new CollisionByte(val); }

        public override string ToString()
        {
            return bVal.ToString();
        }

        public string ToString(string format)
        {
            return bVal.ToString(format);
        }
    }

    [Serializable]
    public struct MapConnection
    {
        [Description("The name of the map to connect to.")]
        public string MapName { get; set; }
        [Description("The direction of the map to connect to.")]
        public ConnectionType Direction { get; set; }
        [Description("Where to align the map being connected to.")]
        public short Offset { get; set; }

        public override string ToString()
        {
            if (MapName == null) return "[" + Direction.ToString() + "]";
            else return "[" + Direction.ToString() + "] " + MapName.Substring(MapName.LastIndexOf("\\") + 1);
        }
    }

    [Serializable]
    public struct WarpConnection
    {
        [Description("The name of the map to warp to.")]
        public string MapName { get; set; }
        [Description("The entry coordinates on the current map.")]
        public BytePoint2D Entry { get; set; }
        [Description("The exit coordinates on the target map.")]
        public BytePoint2D Exit { get; set; }

        public override string ToString()
        {
            if (MapName == null) return "[" + Exit.ToString() + "]";
            else return "[" + Exit.ToString() + "] " + MapName.Substring(MapName.LastIndexOf("\\") + 1);
        }
    }

    public enum TileSize : byte
    {
        x08 = 08,
        x16 = 16,
        x32 = 32,
    }

    public enum ConnectionType : byte
    {
        North,
        East,
        South,
        West,
    }

    public enum TCImageType : byte
    {
        Invalid = 0,
        Tileset = 1,
        AnimTile = 2,
        AutoTile2x3 = 3,
        AutoTile3x4 = 4,
        Sprite3x4 = 5,
        Sprite4x4 = 6,
        Reference = 7,
    }

    [Flags]
    public enum ProjectFlags : byte
    {
        None    = 0,       //0000000000000000
        Value01 = 1,       //0000000000000001
        Value02 = 2,       //0000000000000010
        Value03 = 4,       //0000000000000100
        Value04 = 8,       //0000000000001000
        Value05 = 16,      //0000000000010000
        Value06 = 32,      //0000000000100000
        Value07 = 64,      //0000000001000000
        Value08 = 128,     //0000000010000000
        //Value09 = 256,     //0000000100000000
        //Value10 = 512,     //0000001000000000
        //Value11 = 1024,    //0000010000000000
        //Value12 = 2048,    //0000100000000000
        //Value13 = 4096,    //0001000000000000
        //Value14 = 8192,    //0010000000000000
        //Value15 = 16384,   //0100000000000000
        //Value16 = 32768,   //1000000000000000   
    }

    [Flags]
    public enum MapFlags : byte
    {
        None    = 0,       //00000000
        Value01 = 1,       //00000001
        Value02 = 2,       //00000010
        Value03 = 4,       //00000100
        Value04 = 8,       //00001000
        Value05 = 16,      //00010000
        Value06 = 32,      //00100000
        Value07 = 64,      //01000000
        Value08 = 128,     //10000000
    }
}
