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

namespace TCGame
{
    public class DebugOptions
    {
        public bool IsOn;

        private bool showBorders;
        private bool showBG;
        private bool showConnections;
        private bool collisionCheck;
        private bool boundsCheck;
        private bool animTick;
        private bool autoTile;

        public bool ShowBorders { get { return IsOn && showBorders; } set { showBorders = value; } }
        public bool ShowBG { get { return !IsOn || showBG; } set { showBG = value; } }
        public bool ShowConnections { get { return !IsOn || showConnections; } set { showConnections = value; } }
        public bool CollisionCheck { get { return !IsOn || collisionCheck; } set { collisionCheck = value; } }
        public bool BoundsCheck { get { return !IsOn || boundsCheck; } set { boundsCheck = value; } }
        public bool AutoTile { get { return !IsOn || autoTile; } set { autoTile = value; } }
        public bool AnimTick { get { return !IsOn || animTick; } set { animTick = value; } }

        public DebugOptions()
        {
            IsOn = false;

            showBorders = false;
            showBG = showConnections = collisionCheck = boundsCheck = autoTile = animTick = true;
        }
    }

    class MAIN_LEVEL : RC_GameStateParent
    {
        DebugOptions Debug;
        GraphicsDeviceManager gdm;

        float scale = 1; //zoom sale
        int power = 5;   //zoom factor (powers of 2)

        ulong ticks = 0;
        int tWidth = 17;  //total tile count across screen
        int tHeight = 13; //total tile count down screen
        
        int tSize;
        Vector2 tOffset; //tile offset for tile panning during movement
        Vector2 pOffset; //player offset for rendering player in center

        bool moving;
        Vector2 mDirection;

        TCProject Project;

        Sprite16Step player;

        Texture2D[] Tilesets;
        TCMultiTile[] MultiTiles;

        TCMap currentMap;
        Vector2 playerOffset = new Vector2(8, 6);   //player offset in tile-space
        Vector2 currentPos;                         //top left corner in tile-space
        Vector2 playerPos { get { return currentPos + playerOffset; } } //player location in tile-space

        public MAIN_LEVEL(GraphicsDeviceManager g, string[] args)
        {
            var fname = args[0];

            Project = TCProject.FromContiguous(fname, g.GraphicsDevice);

            tSize = (int)Project.TileSize;
            tOffset = new Vector2(0, -0.5f * tSize);
            pOffset = new Vector2(-1f * tSize, -1.5f * tSize);
            
            //set starting map/coords
            currentMap = Project.GetMapByName(Project.StartMap);
            if (currentMap == null) currentMap = Project.MapList[0];
            currentPos = new Vector2(Project.StartLocation.X, Project.StartLocation.Y);
            currentPos -= playerOffset;

            gdm = g;
            UpdateRenderSize();

            Debug = new DebugOptions();
        }

        public override void LoadContent()
        {
            player = new Sprite16Step(true, Project.Sprites[Project.PlayerSpriteID].Texture, playerOffset.X * tSize + pOffset.X, playerOffset.Y * tSize + pOffset.Y, tSize, tSize * 2, 0);
            player.setHSoffset(new Vector2(0, tSize));
            player.animationStart12(4);
            player.setStandAsStill();
            player.depth = 0.17f;

            Tilesets = new Texture2D[Project.Tilesets.Count];
            for (int i = 0; i < Tilesets.Length; i++)
                Tilesets[i] = Project.Tilesets[i].Texture;

            MultiTiles = new TCMultiTile[Project.MultiTiles.Count];
            for (int i = 0; i < MultiTiles.Length; i++)
                MultiTiles[i] = new TCMultiTile(Project.MultiTiles[i], graphicsDevice, Project.TileSize);
        }

        public override void UnloadContent()
        {
            for (int i = 0; i < Tilesets.Length; i++)
                Tilesets[i].Dispose();

            for (int i = 0; i < MultiTiles.Length; i++)
                MultiTiles[i].Texture.Dispose();
        }

        #region Update/Check Methods
        public override void Update(GameTime gameTime)
        {
            ticks++;

            if (ticks % 20 == 0 && Debug.AnimTick)
            {
                for (int i = 0; i < MultiTiles.Length; i++)
                    MultiTiles[i].AnimationTick();
            }

            prevKeyState = keyState;
            keyState = Keyboard.GetState();

            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            #region KeyStates
            if (keyState.IsKeyDown(Keys.F1) && prevKeyState.IsKeyUp(Keys.F1)) Debug.IsOn = !Debug.IsOn;

            if (keyState.IsKeyDown(Keys.Add) && prevKeyState.IsKeyUp(Keys.Add))
            {
                if (power < 6) power++;
                UpdateRenderSize();
            }
            if (keyState.IsKeyDown(Keys.Subtract) && prevKeyState.IsKeyUp(Keys.Subtract))
            {
                if (power > 4) power--;
                UpdateRenderSize();
            }

            if (Debug.IsOn)
            {
                if (keyState.IsKeyDown(Keys.F2) && prevKeyState.IsKeyUp(Keys.F2)) Debug.ShowBG = !Debug.ShowBG;
                if (keyState.IsKeyDown(Keys.F3) && prevKeyState.IsKeyUp(Keys.F3)) Debug.ShowConnections = !Debug.ShowConnections;
                if (keyState.IsKeyDown(Keys.F4) && prevKeyState.IsKeyUp(Keys.F4)) Debug.ShowBorders = !Debug.ShowBorders;
                if (keyState.IsKeyDown(Keys.F5) && prevKeyState.IsKeyUp(Keys.F5)) Debug.CollisionCheck = !Debug.CollisionCheck;
                if (keyState.IsKeyDown(Keys.F6) && prevKeyState.IsKeyUp(Keys.F6)) Debug.BoundsCheck = !Debug.BoundsCheck;
                if (keyState.IsKeyDown(Keys.F7) && prevKeyState.IsKeyUp(Keys.F7)) Debug.AutoTile = !Debug.AutoTile;
                if (keyState.IsKeyDown(Keys.F8) && prevKeyState.IsKeyUp(Keys.F8)) Debug.AnimTick = !Debug.AnimTick;
            }
            #endregion

            if (moving)
            {
                if (ticks % 1 == 0)
                {
                    int mult = (int)(tSize / 16f);
                    if (keyState.IsKeyDown(Keys.Space)) mult = (int)(tSize / 2f);
                    //mDirection should always be in range -1 to +1
                    tOffset.X -= (int)mDirection.X * mult;
                    tOffset.Y -= (int)mDirection.Y * mult;

                    if (tOffset.Y <= (-1.5f * tSize) || tOffset.Y >= (0.5f * tSize))
                    {
                        tOffset.Y = -0.5f * tSize;
                        currentPos += mDirection;
                        UpdatePosition();
                        CheckPostMove();
                    }
                    if (tOffset.X <= (-1f * tSize) || tOffset.X >= tSize)
                    {
                        tOffset.X = 0;
                        currentPos += mDirection;
                        UpdatePosition();
                        CheckPostMove();
                    }
                }
            }
            else
                CheckPreMove();

            player.animationTick();
        }

        //TODO: change face direction without moving
        public void CheckPreMove()
        {
            if (keyState.IsKeyDown(Keys.Up) /*&& prevKeyState.IsKeyUp(Keys.Up)*/) mDirection.Y--;
            else if (keyState.IsKeyDown(Keys.Down) /*&& prevKeyState.IsKeyUp(Keys.Down)*/) mDirection.Y++;
            else if (keyState.IsKeyDown(Keys.Left) /*&& prevKeyState.IsKeyUp(Keys.Left)*/) mDirection.X--;
            else if (keyState.IsKeyDown(Keys.Right) /*&& prevKeyState.IsKeyUp(Keys.Right)*/) mDirection.X++;

            if (!mDirection.Equals(Vector2.Zero))
                if (CheckCollision(playerPos + mDirection))
                {
                    moving = true;
                    SetAnimDir(false);
                }
                else mDirection = Vector2.Zero;
        }

        public void CheckPostMove()
        {
            var tDir = Vector2.Zero;

            if (keyState.IsKeyDown(Keys.Up)) tDir.Y--;
            else if (keyState.IsKeyDown(Keys.Down)) tDir.Y++;
            else if (keyState.IsKeyDown(Keys.Left)) tDir.X--;
            else if (keyState.IsKeyDown(Keys.Right)) tDir.X++;

            if (!tDir.Equals(Vector2.Zero) && CheckCollision(playerPos + tDir))
            {
                //only change animation if direction is different
                if (!tDir.Equals(mDirection))
                {
                    mDirection = tDir;
                    SetAnimDir(false);
                }
            }
            else
            {
                SetAnimDir(true);

                moving = false;
                mDirection = Vector2.Zero;
            }
        }

        public void SetAnimDir(bool stand)
        {
            int dir = 0;
            if (mDirection.X < 0) dir = 3;
            if (mDirection.X > 0) dir = 1;
            if (mDirection.Y < 0) dir = 0;
            if (mDirection.Y > 0) dir = 2;

            if (stand) dir += 4;

            player.animationStart12(dir);
        }

        public bool CheckCollision(Vector2 newCoord)
        {
            if (!Debug.BoundsCheck) return true;

            CollisionByte c0, c1;

            #region West Bounds Check
            if (newCoord.X < 0)
            {
                for (int i = 0; i < currentMap.Connections.Count; i++)
                {
                    var con = currentMap.Connections[i];
                    if (con.Direction != ConnectionType.West) continue;

                    var newMap = Project.GetMapByName(con.MapName);
                    if (newMap == null) continue;

                    if (con.Offset > newCoord.Y || con.Offset + newMap.Height <= newCoord.Y) continue;

                    c0 = currentMap.Layers[3][(int)playerPos.X, (int)playerPos.Y].Y;
                    c1 = newMap.Layers[3][newMap.Width - 1, (int)newCoord.Y - con.Offset].Y; //check right-most column

                    return CanPass(c0, c1, mDirection);
                }

                return false;
            }
            #endregion

            #region East Bounds Check
            if (newCoord.X >= currentMap.Width)
            {
                for (int i = 0; i < currentMap.Connections.Count; i++)
                {
                    var con = currentMap.Connections[i];
                    if (con.Direction != ConnectionType.East) continue;

                    var newMap = Project.GetMapByName(con.MapName);
                    if (newMap == null) continue;

                    if (con.Offset > newCoord.Y || con.Offset + newMap.Height <= newCoord.Y) continue;

                    c0 = currentMap.Layers[3][(int)playerPos.X, (int)playerPos.Y].Y;
                    c1 = newMap.Layers[3][0, (int)newCoord.Y - con.Offset].Y; //check left-most column

                    return CanPass(c0, c1, mDirection);
                }

                return false;
            }
            #endregion

            #region North Bounds Check
            if (newCoord.Y < 0)
            {
                for (int i = 0; i < currentMap.Connections.Count; i++)
                {
                    var con = currentMap.Connections[i];
                    if (con.Direction != ConnectionType.North) continue;

                    var newMap = Project.GetMapByName(con.MapName);
                    if (newMap == null) continue;

                    if (con.Offset > newCoord.X || con.Offset + newMap.Width <= newCoord.X) continue;

                    c0 = currentMap.Layers[3][(int)playerPos.X, (int)playerPos.Y].Y;
                    c1 = newMap.Layers[3][(int)newCoord.X - con.Offset, newMap.Height - 1].Y; //check lower-most row

                    return CanPass(c0, c1, mDirection);
                }

                return false;
            }
            #endregion

            #region South Bounds Check
            if (newCoord.Y >= currentMap.Height)
            {
                for (int i = 0; i < currentMap.Connections.Count; i++)
                {
                    var con = currentMap.Connections[i];
                    if (con.Direction != ConnectionType.South) continue;

                    var newMap = Project.GetMapByName(con.MapName);
                    if (newMap == null) continue;

                    if (con.Offset > newCoord.X || con.Offset + newMap.Width <= newCoord.X) continue;

                    c0 = currentMap.Layers[3][(int)playerPos.X, (int)playerPos.Y].Y;
                    c1 = newMap.Layers[3][(int)newCoord.X - con.Offset, 0].Y; //check top row

                    return CanPass(c0, c1, mDirection);
                }

                return false;
            }
            #endregion

            if (!Debug.CollisionCheck) return true;

            c0 = currentMap.Layers[3][(int)playerPos.X, (int)playerPos.Y].Y;
            c1 = currentMap.Layers[3][(int)newCoord.X, (int)newCoord.Y].Y;
            return CanPass(c0, c1, mDirection);
        }

        public bool CanPass(CollisionByte c0, CollisionByte c1, Vector2 dir)
        {
            if (dir.Y < 0 && c1.Down ||
                dir.Y > 0 && c1.Up ||
                dir.X < 0 && c1.Right ||
                dir.X > 0 && c1.Left)
                return false;

            if (c0.Layer != c1.Layer && c0.Layer != -1 && c1.Layer != -1)
                return false;

            return true;
        }

        public void UpdatePosition()
        {
            #region Warp Connections
            foreach (var w in currentMap.Warps)
            {
                if (playerPos.X == w.Entry.X && playerPos.Y == w.Entry.Y)
                {
                    var map = Project.GetMapByName(w.MapName);
                    if (map == null) continue;

                    currentPos.X = w.Exit.X - playerOffset.X;
                    currentPos.Y = w.Exit.Y - playerOffset.Y;
                    currentMap = map;
                    return;
                }
            }
            #endregion

            #region West Connection Check
            if (playerPos.X < 0)
            {
                for (int i = 0; i < currentMap.Connections.Count; i++)
                {
                    var con = currentMap.Connections[i];
                    if (con.Direction != ConnectionType.West) continue;

                    var map = Project.GetMapByName(con.MapName);
                    if (map == null) continue;

                    if (con.Offset > playerPos.Y || con.Offset + map.Height <= playerPos.Y) continue;

                    currentPos.Y -= con.Offset;
                    currentPos.X = map.Width - playerOffset.X - 1;
                    currentMap = map;
                    return;
                }

                //currentPos.X = 0 - playerOffset.X;
                //return;
            }
            #endregion

            #region East Connection Check
            if (playerPos.X >= currentMap.Width)
            {
                for (int i = 0; i < currentMap.Connections.Count; i++)
                {
                    var con = currentMap.Connections[i];
                    if (con.Direction != ConnectionType.East) continue;

                    var map = Project.GetMapByName(con.MapName);
                    if (map == null) continue;

                    if (con.Offset > playerPos.Y || con.Offset + map.Height <= playerPos.Y) continue;

                    currentPos.Y -= con.Offset;
                    currentPos.X = 0 - playerOffset.X;
                    currentMap = map;
                    return;
                }

                //currentPos.X = currentMap.Width - playerOffset.X - 1;
                //return;
            }
            #endregion

            #region North Connection Check
            if (playerPos.Y < 0)
            {
                for (int i = 0; i < currentMap.Connections.Count; i++)
                {
                    var con = currentMap.Connections[i];
                    if (con.Direction != ConnectionType.North) continue;

                    var map = Project.GetMapByName(con.MapName);
                    if (map == null) continue;

                    if (con.Offset > playerPos.X || con.Offset + map.Width <= playerPos.X) continue;

                    currentPos.X -= con.Offset;
                    currentPos.Y = map.Height - playerOffset.Y - 1;
                    currentMap = map;
                    return;
                }

                //currentPos.Y = 0 - playerOffset.Y;
                //return;
            }
            #endregion

            #region South Connection Check
            if (playerPos.Y >= currentMap.Height)
            {
                for (int i = 0; i < currentMap.Connections.Count; i++)
                {
                    var con = currentMap.Connections[i];
                    if (con.Direction != ConnectionType.South) continue;

                    var map = Project.GetMapByName(con.MapName);
                    if (map == null) continue;

                    if (con.Offset > playerPos.X || con.Offset + map.Width <= playerPos.X) continue;

                    currentPos.X -= con.Offset;
                    currentPos.Y = 0 - playerOffset.Y;
                    currentMap = map;
                    return;
                }

                //currentPos.Y = currentMap.Height - playerOffset.Y - 1;
                //return;
            }
            #endregion
        }
        #endregion

        #region Draw Methods
        public override void Draw(GameTime gameTime)
        {
            graphicsDevice.Clear(Color.Black);

            //scale all draw output, and dont filter resize
            Matrix scaleMat = new Matrix() { M11 = scale, M22 = scale, M33 = scale, M44 = 1 };
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, scaleMat);

            if (Debug.ShowBG) DrawBackGround();


            DrawLayer(currentMap, 0, 0, tWidth, 0, tHeight, (int)currentPos.X, (int)currentPos.Y);
            DrawLayer(currentMap, 1, 0, tWidth, 0, tHeight, (int)currentPos.X, (int)currentPos.Y);

            if (Debug.ShowConnections) DrawConnections();

            //draw sprites would go here
            player.Draw(spriteBatch);
            
            DrawLayer(currentMap, 2, 0, tWidth, 0, tHeight, (int)currentPos.X, (int)currentPos.Y);

            if (Debug.ShowBorders)
            {
                //DrawGrid();
                DrawBorderLines();
            }
            
            spriteBatch.End();
        }

        public void DrawBackGround()
        {
            var bMap = Project.GetMapByName(currentMap.BorderMap);
            if (bMap == null) return;

            int xStart = (int)currentPos.X % bMap.Width + currentMap.BorderOffset.X;
            int yStart = (int)currentPos.Y % bMap.Height + currentMap.BorderOffset.Y;

            DrawLayer(bMap, 0, 0, tWidth, 0, tHeight, xStart, yStart, true);
            DrawLayer(bMap, 1, 0, tWidth, 0, tHeight, xStart, yStart, true);
            DrawLayer(bMap, 2, 0, tWidth, 0, tHeight, xStart, yStart, true);
        }

        public void DrawBorderLines()
        {
            Vector2 pixPos = MapCoordsToPixelCoords(Vector2.Zero);
            var r1 = new Rectangle((int)pixPos.X + (int)tOffset.X, (int)pixPos.Y + (int)tOffset.Y, currentMap.Width * tSize, currentMap.Height * tSize);
            var r2 = new Rectangle(r1.X - 1, r1.Y - 1, r1.Width + 2, r1.Height + 2);
            LineBatch.drawLineRectangle(spriteBatch, r1, Color.Red);
            LineBatch.drawLineRectangle(spriteBatch, r2, Color.Red);

            for (int i = 0; i < currentMap.Connections.Count; i++)
            {
                var r = GetMapOverlap(currentMap, i);
                if (r.IsEmpty) continue;

                pixPos = MapCoordsToPixelCoords(new Vector2(r.X, r.Y));
                r1 = new Rectangle((int)pixPos.X + (int)tOffset.X, (int)pixPos.Y + (int)tOffset.Y, r.Width * tSize, r.Height * tSize);
                if (r1.Width == 0) r1.Width++;
                if (r1.Height == 0) r1.Height++;
                if (r.X == 0) r1.X--;
                if (r.Y == 0) r1.Y--;
                LineBatch.drawLineRectangle(spriteBatch, r1, Color.Blue);
            }
        }

        /// <summary>
        /// Draws a section of a map's tile data to a section of the screen.
        /// </summary>
        /// <param name="map">The map to retrieve tile data from.</param>
        /// <param name="layer">The index of the layer to retrieve tile data from.</param>
        /// <param name="iStart">Screen X coordinate to begin drawing from.</param>
        /// <param name="iStop">Screen X coordinate to stop drawing at.</param>
        /// <param name="jStart">Screen Y coordinate to begin drawing from.</param>
        /// <param name="jStop">Screen Y coordinate to stop drawing at.</param>
        /// <param name="xStart">Map X coordinate to begin drawing from.</param>
        /// <param name="yStart">Map Y coordinate to begin drawing from.</param>
        /// <param name="bgFlag">Background flag. If true, tiling will be turned on and z-index will be set to 1.</param>
        public void DrawLayer(TCMap map, int layer, int iStart, int iStop, int jStart, int jStop, int xStart, int yStart, bool bgFlag = false)
        {
            for (int i = iStart; i < iStop; i++)
                for (int j = jStart; j < jStop; j++)
                {
                    int xPos = xStart + i - iStart;
                    int yPos = yStart + j - jStart;

                    //z-indexing math
                    int total = tWidth * tHeight;
                    int index = j * tWidth + i % tWidth;
                    float third = 0.9f / 3f;
                    float fraction = ((float)index / (float)total) * third;
                    float depth = 0.9f - (third * layer + fraction);

                    if (bgFlag)
                    {
                        xPos = Math.Abs(xPos % map.Width);
                        yPos = Math.Abs(yPos % map.Height);
                        depth = 1f;
                    }

                    if (xPos < 0 || yPos < 0) continue;
                    if (xPos >= map.Width || yPos >= map.Height) continue;

                    var cPos = map.Layers[layer][xPos, yPos];
                    if (cPos.X == 0xFF) continue;

                    var sImg = Tilesets[map.TileSetIndex];
                    var sr = new Rectangle(cPos.X * tSize, cPos.Y * tSize, tSize, tSize);
                    var dr = new Rectangle(i * tSize + (int)tOffset.X, j * tSize + (int)tOffset.Y, tSize, tSize);

                    if (cPos.X >= 8)
                    {
                        sImg = MultiTiles[cPos.X - 8].Texture;
                        sr = MultiTiles[cPos.X - 8].GetSourceRect(map.Layers[layer], xPos, yPos, (Debug.AutoTile) ? cPos.Y : 0);
                    }

                    //set the origin param to the bottom right of the tile, so it doesnt get culled when the top left is off screen
                    spriteBatch.Draw(sImg, dr, sr, Color.White, 0, new Vector2(tSize, tSize), SpriteEffects.None, 0); //last param here should be depth. currently 0 because for some reason it doesnt work with 16x16 tiles.
                }
        }

        public void DrawGrid()
        {
            for (int i = 0; i < tWidth; i++)
                LineBatch.drawLine(spriteBatch, Color.Red, new Vector2(i * tSize + tOffset.X, tOffset.Y), new Vector2(i * tSize + tOffset.X, tHeight * tSize + tOffset.Y));

            for (int j = 0; j < tHeight; j++)
                LineBatch.drawLine(spriteBatch, Color.Red, new Vector2(tOffset.X, j * tSize + tOffset.Y), new Vector2(tWidth * tSize + tOffset.X, j * tSize + tOffset.Y));
        }

        public void DrawConnections()
        {
            //West
            if (currentPos.X < 0)
            {
                for (int i = 0; i < currentMap.Connections.Count; i++)
                {
                    var con = currentMap.Connections[i];
                    if (con.Direction != ConnectionType.West) continue;

                    var map = Project.GetMapByName(con.MapName);
                    if (map == null) continue;

                    if (con.Offset > currentPos.Y + tHeight || con.Offset + map.Height < currentPos.Y) continue;

                    int overlap = 0 - (int)currentPos.X;
                    int xStart = map.Width - overlap;
                    int yStart = (int)currentPos.Y - con.Offset;

                    DrawLayer(map, 0, 0, overlap, 0, tHeight, xStart, yStart);
                    DrawLayer(map, 1, 0, overlap, 0, tHeight, xStart, yStart);
                    DrawLayer(map, 2, 0, overlap, 0, tHeight, xStart, yStart);
                }
            }

            //East
            if (currentPos.X + tWidth > currentMap.Width)
            {
                for (int i = 0; i < currentMap.Connections.Count; i++)
                {
                    var con = currentMap.Connections[i];
                    if (con.Direction != ConnectionType.East) continue;

                    var map = Project.GetMapByName(con.MapName);
                    if (map == null) continue;

                    if (con.Offset > currentPos.Y + tHeight || con.Offset + map.Height < currentPos.Y) continue;

                    int overlap = ((int)currentPos.X + tWidth) - currentMap.Width;
                    int iStart = tWidth - overlap;
                    int yStart = (int)currentPos.Y - con.Offset;

                    DrawLayer(map, 0, iStart, tWidth, 0, tHeight, 0, yStart);
                    DrawLayer(map, 1, iStart, tWidth, 0, tHeight, 0, yStart);
                    DrawLayer(map, 2, iStart, tWidth, 0, tHeight, 0, yStart);
                }
            }

            //North
            if (currentPos.Y < 0)
            {
                for (int i = 0; i < currentMap.Connections.Count; i++)
                {
                    var con = currentMap.Connections[i];
                    if (con.Direction != ConnectionType.North) continue;

                    var map = Project.GetMapByName(con.MapName);
                    if (map == null) continue;

                    if (con.Offset > currentPos.X + tWidth || con.Offset + map.Width < currentPos.X) continue;

                    int overlap = 0 - (int)currentPos.Y;
                    int xStart = (int)currentPos.X - con.Offset;
                    int yStart = map.Height - overlap;

                    DrawLayer(map, 0, 0, tWidth, 0, overlap, xStart, yStart);
                    DrawLayer(map, 1, 0, tWidth, 0, overlap, xStart, yStart);
                    DrawLayer(map, 2, 0, tWidth, 0, overlap, xStart, yStart);
                }
            }

            //South
            if (currentPos.Y + tHeight > currentMap.Height)
            {
                for (int i = 0; i < currentMap.Connections.Count; i++)
                {
                    var con = currentMap.Connections[i];
                    if (con.Direction != ConnectionType.South) continue;

                    var map = Project.GetMapByName(con.MapName);
                    if (map == null) continue;

                    if (con.Offset > currentPos.X + tWidth || con.Offset + map.Width < currentPos.X) continue;

                    int overlap = ((int)currentPos.Y + tHeight) - currentMap.Height;
                    int jStart = tHeight - overlap;
                    int xStart = (int)currentPos.X - con.Offset;

                    DrawLayer(map, 0, 0, tWidth, jStart, tHeight, xStart, 0);
                    DrawLayer(map, 1, 0, tWidth, jStart, tHeight, xStart, 0);
                    DrawLayer(map, 2, 0, tWidth, jStart, tHeight, xStart, 0);
                }
            }

        }
        #endregion

        public void UpdateRenderSize()
        {
            scale = (float)Math.Pow(2, power) / (float)tSize;
            gdm.PreferredBackBufferWidth = ((int)(15 * tSize * scale));
            gdm.PreferredBackBufferHeight = ((int)(10 * tSize * scale));
            gdm.ApplyChanges();
        }

        //gets the pixel coordinates of a map tile relative to the screen
        public Vector2 MapCoordsToPixelCoords(Vector2 mapCoords)
        {
            Vector2 temp = currentPos * -1 - Vector2.One; //origin offset
            temp += mapCoords; //coord offset
            return temp * tSize; //pixel offset
        }

        //TODO: fix inaccuracies
        public Rectangle GetMapOverlap(TCMap cMap, int conIndex)
        {
            var con = cMap.Connections[conIndex];

            var nMap = Project.GetMapByName(con.MapName);
            if (nMap == null) return Rectangle.Empty;

            Rectangle r = Rectangle.Empty;
            switch (con.Direction)
            {
                case ConnectionType.North:
                    r.X = Math.Max(0, (int)con.Offset);
                    r.Y = 0;
                    r.Width = Math.Min(nMap.Width + con.Offset, cMap.Width - con.Offset);
                    r.Height = 0;
                    break;
                case ConnectionType.West:
                    r.X = 0;
                    r.Y = Math.Max(0, (int)con.Offset);
                    r.Width = 0;
                    r.Height = Math.Min(nMap.Height + con.Offset, cMap.Height - con.Offset);
                    break;
                case ConnectionType.South:
                    r.X = Math.Max(0, (int)con.Offset);
                    r.Y = cMap.Height;
                    r.Width = Math.Min(nMap.Width + con.Offset, cMap.Width - con.Offset);
                    r.Height = 0;
                    break;
                case ConnectionType.East:
                    r.X = cMap.Width;
                    r.Y = Math.Max(0, (int)con.Offset);
                    r.Width = 0;
                    r.Height = Math.Min(nMap.Height + con.Offset, cMap.Height - con.Offset);
                    break;
            }

            return r;
        }
    }
}
