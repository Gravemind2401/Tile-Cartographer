using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using RC_Framework;
using TileCartographer.Library;

namespace TileCartographer.Library
{
    public class TCMultiTile
    {
        private TCImage baseImage;
        private TCImageType ImgType;
        public Texture2D Texture;
        
        private int fWidth, fHeight, fCount, cFrame;
        private int tSize;
        private int qSize { get { return tSize / 2; } } //quarter tile

        public TCMultiTile(TCImage baseImg, GraphicsDevice gd, TileSize tileSize)
        {
            baseImage = baseImg;

            tSize = (int)tileSize;
            ImgType = TCImageType.Invalid;
            
            for (int i = 1; i < Enum.GetNames(typeof(TCImageType)).Length; i++)
                if (baseImage.IsValid((TCImageType)i, tileSize)) { ImgType = (TCImageType)i; break; }

            if (ImgType == TCImageType.Invalid) throw new NotSupportedException("Invalid image format.");

            switch (ImgType)
            {
                case TCImageType.AnimTile:
                    this.Texture = baseImage.Texture;
                    fWidth = fHeight = (int)tileSize;
                    break;
                case TCImageType.AutoTile2x3:
                    fWidth = (int)tileSize * 2;
                    fHeight = (int)tileSize * 3;
                    break;
                case TCImageType.AutoTile3x4:
                    fWidth = (int)tileSize * 3;
                    fHeight = (int)tileSize * 4;
                    break;
                default:
                    throw new NotSupportedException("Image type not appropriate for MultiTile.");
            }

            fCount = baseImage.Texture.Width / fWidth;
            cFrame = 0;

            if (ImgType == TCImageType.AnimTile) return;
            ImgType = TCImageType.Reference;

            GenerateReference(gd);
        }

        public void AnimationTick()
        {
            cFrame++;
            cFrame %= fCount;
        }

        public Rectangle GetSourceRect(BytePoint2D[,] layer, int xPos, int yPos, int forceIndex)
        {
            Rectangle r;

            if (ImgType == TCImageType.AnimTile)
            {
                r = new Rectangle(tSize * cFrame, 0, tSize, tSize);
                return r;
            }

            byte index = (byte)forceIndex;
            //if the index has not been overridden, look it up
            if (index == 0xFF)
            {
                byte mask;
                byte hSum = 0, dSum = 0;

                //horizontal neighbour sum
                try { if (layer[xPos + 0, yPos - 1].X == layer[xPos, yPos].X) hSum += 1; } catch { } //use try incase the block is on
                try { if (layer[xPos + 1, yPos + 0].X == layer[xPos, yPos].X) hSum += 2; } catch { } //the map border, so we dont get
                try { if (layer[xPos + 0, yPos + 1].X == layer[xPos, yPos].X) hSum += 4; } catch { } //index out of range exceptions
                try { if (layer[xPos - 1, yPos + 0].X == layer[xPos, yPos].X) hSum += 8; } catch { } //(too lazy to check bounds here)
                //diagonal neighbour sum
                try { if (layer[xPos + 1, yPos - 1].X == layer[xPos, yPos].X) dSum += 1; } catch { }
                try { if (layer[xPos + 1, yPos + 1].X == layer[xPos, yPos].X) dSum += 2; } catch { }
                try { if (layer[xPos - 1, yPos + 1].X == layer[xPos, yPos].X) dSum += 4; } catch { }
                try { if (layer[xPos - 1, yPos - 1].X == layer[xPos, yPos].X) dSum += 8; } catch { }

                if (Data.GetMaskDict().TryGetValue(hSum, out mask))
                    dSum &= mask; //this mask removes any unwanted bits from dSum

                byte hash = (byte)((dSum << 4) | hSum);

                if (!Data.GetReferenceDict().TryGetValue(hash, out index))
                    index = hSum; //lookup the index in the dictionary, or fallback to the hSum as index
            }

            r = new Rectangle(fWidth * cFrame + (index % 8) * tSize, (index / 8) * tSize, tSize, tSize);

            return r;
        }

        private void GenerateReference(GraphicsDevice gd)
        {
            var r = new RenderTarget2D(gd, tSize * 8 * fCount, tSize * 6);
            gd.SetRenderTarget(r);

            var s = new SpriteBatch(gd);
            s.Begin();

            var data = Data.GetReferenceData3x4();
            Rectangle sr, dr;

            for (int f = 0; f < fCount; f++)
            {
                for (int x = 0; x < 16; x++)
                {
                    for (int y = 0; y < 12; y++)
                    {
                        int i = data[y, x]; //why is this array backwards?
                        var p = new Point(i % 6, i / 6);

                        sr = new Rectangle(f * (tSize * 3) + p.X * qSize, p.Y * qSize, qSize, qSize);
                        dr = new Rectangle(f * (tSize * 8) + x * qSize, y * qSize, qSize, qSize);
                        s.Draw(baseImage.Texture, dr, sr, Color.White);
                    }
                }
            }

            s.End();
            this.Texture = r;
            fWidth = tSize * 8;
            gd.SetRenderTarget(null);
        }
    }
}
