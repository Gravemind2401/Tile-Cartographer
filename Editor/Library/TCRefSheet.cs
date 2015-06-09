using System;
using System.Drawing;

namespace TileCartographer.Library
{
    //This is REQUIRED to be static.
    //It was originally a normal class, but there was
    //no good way to keep a refsheet list in perfect
    //sync with the multitile list in the project.
    //Using static refsheets allows them to be generated
    //as they are required, based on a given TCImage.
    //[also means less #if logic in TCProject.cs]
    //
    //These refsheets ignore animations - they are first frame ONLY.
    //Unlike the tester, we do not use more than one frame in the editor.
    public static class TCRefSheet
    {
        public static Rectangle GetSourceRect(TCImage baseImage, TileSize tileSize, BytePoint2D[,] layer, int xPos, int yPos, int forceIndex)
        {
            Rectangle r;

            int tSize = (int)tileSize;
            int qSize = tSize / 2;
            var ImgType = TCImageType.Invalid;

            for (int i = 1; i < Enum.GetNames(typeof(TCImageType)).Length; i++)
                if (baseImage.IsValid((TCImageType)i, tileSize)) { ImgType = (TCImageType)i; break; }

            if (ImgType == TCImageType.Invalid) throw new NotSupportedException("Invalid image format.");

            if (ImgType == TCImageType.AnimTile)
            {
                r = new Rectangle(0, 0, tSize, tSize);
                return r;
            }

            byte index = (byte)forceIndex;
            
            if(index == 0xFF)
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

            r = new Rectangle((index % 8) * tSize, (index / 8) * tSize, tSize, tSize);

            return r;
        }

        public static Bitmap GenerateReference(TCImage baseImage, TileSize tileSize)
        {
            int tSize = (int)tileSize;
            int qSize = tSize / 2;
            var ImgType = TCImageType.Invalid;

            for (int i = 1; i < Enum.GetNames(typeof(TCImageType)).Length; i++)
                if (baseImage.IsValid((TCImageType)i, tileSize)) { ImgType = (TCImageType)i; break; }

            if (ImgType == TCImageType.Invalid) throw new NotSupportedException("Invalid image format.");
            if (ImgType == TCImageType.AnimTile) return (Bitmap)baseImage.GetBitmap();

            var sheet = new Bitmap(tSize * 8, tSize * 6);
            var data = Data.GetReferenceData3x4();
            Rectangle sr, dr;

            using (var bitm = baseImage.GetBitmap())
            {
                for (int x = 0; x < 16; x++)
                {
                    for (int y = 0; y < 12; y++)
                    {
                        int i = data[y, x]; //why is this array backwards?
                        var p = new Point(i % 6, i / 6);

                        sr = new Rectangle(p.X * qSize, p.Y * qSize, qSize, qSize);
                        dr = new Rectangle(x * qSize, y * qSize, qSize, qSize);

                        using (var g = Graphics.FromImage(sheet))
                            g.DrawImage(bitm, dr, sr, GraphicsUnit.Pixel);
                    }
                }
            }

            return sheet;
        }
    }
}
