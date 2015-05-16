using System;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TileCartographer.Library
{
    //Use this so we can keep track of tiles
    //without keeping all the images in memory.
    //Just use GetBitmap when needed.

    public class TCImage
    {
#if WINFORMS
        [EditorAttribute(typeof(TileCartographer.Utils.ImageFileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
#endif
        public string FilePath { get; set; }

        public TCImage()
        {
            FilePath = "";
        }

        public TCImage(string Filename)
        {
            FilePath = Filename;
        }

        public override string ToString()
        {
            return FilePath.Substring(FilePath.LastIndexOf('\\') + 1);
        }

#if WINFORMS
        public bool IsValid(TCImageType ImgType, TileSize tSize)
        {
            var s = (int)tSize;

            using (var bitm = GetBitmap())
            {
                switch (ImgType)
                {
                    case TCImageType.Tileset:
                        if (bitm.Width == s * 8 && bitm.Height % s == 0) return true; //bitm is 8xN tiles
                        break;
                    case TCImageType.AnimTile:
                        if (bitm.Width % (1 * s) == 0 && bitm.Height == 1 * s) return true; //bitm is 1x1 tiles, N frames
                        break;
                    case TCImageType.AutoTile2x3:
                        if (bitm.Width % (2 * s) == 0 && bitm.Height == 3 * s) return true; //bitm is 2x3 tiles, N frames
                        break;
                    case TCImageType.AutoTile3x4:
                        if (bitm.Width % (3 * s) == 0 && bitm.Height == 4 * s) return true; //bitm is 3x4 tiles, N frames
                        break;
                    case TCImageType.Sprite3x4:
                        if (bitm.Width == s * 3 && bitm.Height == s * 8) return true; //bitm is 3x8 tiles (each frame is 2 tiles high)
                        break;
                    case TCImageType.Sprite4x4:
                        if (bitm.Width == s * 4 && bitm.Height == s * 8) return true; //bitm is 4x8 tiles (each frame is 2 tiles high)
                        break;
                    case TCImageType.Reference:
                        if (bitm.Width == s * 8 && bitm.Height == s * 6) return true; //bitm is 4x6 tiles (single frame)
                        break;
                }
            }

            return false; //dimensions don't match
        }

        public static bool IsValid(Bitmap bitm, TCImageType ImgType, TileSize tSize)
        {
            var s = (int)tSize;

            switch (ImgType)
            {
                case TCImageType.Tileset:
                    if (bitm.Width == s * 8 && bitm.Height % s == 0) return true; //bitm is 8xN tiles
                    break;
                case TCImageType.AnimTile:
                    if (bitm.Width % (1 * s) == 0 && bitm.Height == 1 * s) return true; //bitm is 1x1 tiles, N frames
                    break;
                case TCImageType.AutoTile2x3:
                    if (bitm.Width % (2 * s) == 0 && bitm.Height == 3 * s) return true; //bitm is 2x3 tiles, N frames
                    break;
                case TCImageType.AutoTile3x4:
                    if (bitm.Width % (3 * s) == 0 && bitm.Height == 4 * s) return true; //bitm is 3x4 tiles, N frames
                    break;
                case TCImageType.Sprite3x4:
                    if (bitm.Width == s * 3 && bitm.Height == s * 8) return true; //bitm is 3x8 tiles (each frame is 2 tiles high)
                    break;
                case TCImageType.Sprite4x4:
                    if (bitm.Width == s * 4 && bitm.Height == s * 8) return true; //bitm is 4x8 tiles (each frame is 2 tiles high)
                    break;
                case TCImageType.Reference:
                    if (bitm.Width == s * 8 && bitm.Height == s * 6) return true; //bitm is 4x6 tiles (single frame)
                    break;
            }

            return false; //dimensions don't match
        }

        public System.Drawing.Image GetBitmap()
        {
            return System.Drawing.Bitmap.FromFile(FilePath);
        }
#else
        public Microsoft.Xna.Framework.Graphics.Texture2D Texture;
        public TCImage(Microsoft.Xna.Framework.Graphics.GraphicsDevice device, BinaryReader br)
        {
            int len = br.ReadInt32();
            MemoryStream ms = new MemoryStream(br.ReadBytes(len));

            Texture = Microsoft.Xna.Framework.Graphics.Texture2D.FromStream(device, ms);

            ms.Close();
            ms.Dispose();
        }

        public bool IsValid(TCImageType ImgType, TileSize tSize)
        {
            var s = (int)tSize;

            switch (ImgType)
            {
                case TCImageType.Tileset:
                    if (Texture.Width == s * 8 && Texture.Height % s == 0) return true; //Texture is 8xN tiles
                    break;
                case TCImageType.AnimTile:
                    if (Texture.Width % (1 * s) == 0 && Texture.Height == 1 * s) return true; //Texture is 1x1 tiles, N frames
                    break;
                case TCImageType.AutoTile2x3:
                    if (Texture.Width % (2 * s) == 0 && Texture.Height == 3 * s) return true; //Texture is 2x3 tiles, N frames
                    break;
                case TCImageType.AutoTile3x4:
                    if (Texture.Width % (3 * s) == 0 && Texture.Height == 4 * s) return true; //Texture is 3x4 tiles, N frames
                    break;
                case TCImageType.Sprite3x4:
                    if (Texture.Width == s * 3 && Texture.Height == s * 8) return true; //Texture is 3x8 tiles (each frame is 2 tiles high)
                    break;
                case TCImageType.Sprite4x4:
                    if (Texture.Width == s * 4 && Texture.Height == s * 8) return true; //Texture is 4x8 tiles (each frame is 2 tiles high)
                    break;
            }

            return false; //dimensions don't match
        }
#endif
    }
}
