using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Pathfinding;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection.Metadata;

namespace AStarPathfindingDemo
{
    enum TileType
    {
        Floor,
        Wall,
        Open,
        Closed,
        Pathed,
        Start,
        End,
    }
    static class Pathfinding
    {
        public static int TILE_SIZE = 8;

        public static Dictionary<Vector2, VisualizationTile> tiles = new Dictionary<Vector2, VisualizationTile>();

        public static Stack<Vector2> GetPath(Vector2 startPosition, Vector2 endPosition, Pathfinder pathfinder, NodeMap nc)
        {
            Stack<Vector2> processedPath = new Stack<Vector2>();

            var path = pathfinder.GetPath((int)startPosition.X, (int)startPosition.Y, (int)endPosition.X, (int)endPosition.Y, nc);

            if (path.Count > 1)
            {
                int[] node = path.Pop();
                Vector2 position = new Vector2(node[0], node[1]);
                tiles[position].type = TileType.End;
                processedPath.Push(position);


                while (path.Count > 1)
                {
                    node = path.Pop();
                    position = new Vector2(node[0], node[1]);
                    tiles[new Vector2(node[0], node[1])].type = TileType.Pathed;
                    processedPath.Push(position);
                }

                node = path.Pop();
                position = new Vector2(node[0], node[1]);
                tiles[new Vector2(node[0], node[1])].type = TileType.Start;
                processedPath.Push(position);
            }

            return processedPath;
        }
        public static void UndoTileVisualization(Stack<Vector2> changedTiles)
        {
            while (changedTiles.Count > 0)
            {
                tiles[changedTiles.Pop()].type = TileType.Floor;
            }
        }


        public static void InitalizeVisualizationTiles(int mapWidth, int mapHeight, NodeMap nodeContainer)
        {


            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    Vector2 position = new Vector2(x, y);
                    tiles.Add(position, new VisualizationTile(position));
                }
            }
        }
        public static Vector2 ConvertGlobalToLocal(Vector2 globalPosition)
        {
            return new Vector2((float)Math.Floor(globalPosition.X / TILE_SIZE), (float)Math.Floor(globalPosition.Y / TILE_SIZE));
        }
        public static Vector2 ConvertLocalToGlobal(Vector2 localPosition, float scale = 1)
        {
            return new Vector2((localPosition.X * TILE_SIZE) * scale, (localPosition.Y * TILE_SIZE) * scale);
        }

        public static float CalculateWeight(int x, int y)
        {
            return tiles[new Vector2(x, y)].weight;
    }

    }


    class VisualizationTile
    {
        public Vector2 position;
        public float weight = 0;
        public Color color
        {
            get
            {
                switch (type)
                {
                    case TileType.Floor:
                        return Color.White;
                    case TileType.Wall:
                        return Color.Black;
                    case TileType.Open:
                        return Color.Yellow;
                    case TileType.Closed:
                        return Color.Orange;
                    case TileType.Pathed:
                        return Color.Green;
                    case TileType.Start:
                        return Color.Blue;
                    case TileType.End:
                        return Color.Red;
                    default:
                        return Color.MintCream;
                }
            }
        }
        public TileType type = TileType.Floor;

        public VisualizationTile(Vector2 position)
        {
            this.position = position;
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D rectangleTexture, float scale)
        {
            Point position = Pathfinding.ConvertLocalToGlobal(this.position, 1).ToPoint();
            Point dimensions = new Point((int)(Pathfinding.TILE_SIZE * 1));

            spriteBatch.Draw(rectangleTexture, new Rectangle(position, dimensions), color);
        }
    }
}
