using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Pathfinding;
using System;
using SharpDX.Direct2D1.Effects;
using System.IO;

namespace AStarPathfindingDemo
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Texture2D rectangle;
        Dictionary<Vector2, int> map = new Dictionary<Vector2, int>();

        NodeMap nodeMap;
        Pathfinder pathfinder;

        List<Vector2> startingPositions = new List<Vector2>();
        HashSet<Vector2> wallPositions = new HashSet<Vector2>();
        Stack<Stack<Vector2>> paths = new Stack<Stack<Vector2>>();

        Vector3 cameraData = new Vector3();

        MouseState currentMouseState;
        MouseState previousMouseState;

        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;

        Keys modifierKey = Keys.A;

        Vector2? currentEndPosition = null;
        Vector2? previousEndPosition = null;


        int mapWidth = 50;
        int mapHeight = 50;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    map.Add(new Vector2(x, y), 1);
                }
            }

            pathfinder = new Pathfinder(Pathfinding.CalculateWeight);
            nodeMap = new NodeMap(mapWidth, mapHeight);
             
            Pathfinding.InitalizeVisualizationTiles(mapWidth, mapHeight, nodeMap);

            base.Initialize();

            cameraData.X = 80;
            cameraData.Y = 80;
            cameraData.Z = 1;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            rectangle = new Texture2D(_graphics.GraphicsDevice, 1, 1);
            rectangle.SetData(new Color[] { Color.White });


            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            currentMouseState = Mouse.GetState();
            currentKeyboardState = Keyboard.GetState();
            Vector2 mousePosition = Pathfinding.ConvertGlobalToLocal(new Vector2(currentMouseState.X, currentMouseState.Y) - new Vector2(cameraData.X, cameraData.Y));

            if (currentKeyboardState.IsKeyDown(Keys.D))
            {
                modifierKey = Keys.D;
            }

            else if (currentKeyboardState.IsKeyDown(Keys.A))
            {
                modifierKey = Keys.A;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Left))
            {
                cameraData.X += 10;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Right))
            {
                cameraData.X -= 10;
            }

            if (currentKeyboardState.IsKeyDown(Keys.Up))
            {
                cameraData.Y += 10;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Down))
            {
                cameraData.Y -= 10;
            }

            if (currentKeyboardState.IsKeyDown(Keys.I) && previousKeyboardState.IsKeyUp(Keys.I))
            {
                Pathfinding.TILE_SIZE += 1;
            }

            if (currentKeyboardState.IsKeyDown(Keys.K) && previousKeyboardState.IsKeyUp(Keys.K))
            {
                if (Pathfinding.TILE_SIZE > 1)
                {
                    Pathfinding.TILE_SIZE += -1;
                }
            }

            if ((mousePosition.X >= 0 && mousePosition.Y >= 0) && mousePosition.X < mapWidth && mousePosition.Y < mapHeight)
            {
                if (currentMouseState.LeftButton == ButtonState.Pressed)
                {
                    if (modifierKey == Keys.D)
                    {
                        if (wallPositions.Contains(mousePosition))
                        {
                            wallPositions.Remove(mousePosition);
                            Pathfinding.tiles[mousePosition].type = TileType.Floor;
                            Pathfinding.tiles[mousePosition].weight = 1;

                        }
                    }

                    else if (modifierKey == Keys.A)
                    {
                        wallPositions.Add(mousePosition);
                        nodeMap.SetNodeWeight((int)mousePosition.X, (int)mousePosition.Y, float.PositiveInfinity);
                        Pathfinding.tiles[mousePosition].weight = float.PositiveInfinity;
                    }

                }

                if (currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released)
                {
                    startingPositions.Add(mousePosition);
                }

                currentEndPosition = mousePosition;

                if (currentEndPosition != null && previousEndPosition != currentEndPosition)
                {
                    while (paths.Count != 0)
                    {
                        Pathfinding.UndoTileVisualization(paths.Pop());
                    }

                    foreach (var wall in wallPositions)
                    {
                        Pathfinding.tiles[wall].type = TileType.Wall;
                    }
                    foreach (var start in startingPositions)
                    {
                        paths.Push(Pathfinding.GetPath(start, (Vector2)currentEndPosition, pathfinder, nodeMap));
                    }

                    previousEndPosition = currentEndPosition;
                }
            }

            previousMouseState = currentMouseState;
            previousKeyboardState = currentKeyboardState;

            base.Update(gameTime);

        }

        protected override void Draw(GameTime gameTime)
        {
            Matrix camera = Matrix.CreateTranslation(cameraData.X, cameraData.Y, 0);

            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(transformMatrix: camera);
            foreach (var tile in Pathfinding.tiles.Values)
            {
                tile.Draw(_spriteBatch, rectangle, cameraData.Z);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
