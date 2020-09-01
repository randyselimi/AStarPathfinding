using System;
using System.Collections.Generic;
using Pathfinding.DataStructures;

namespace Pathfinding
{
    public enum Heuristic
    {
        Euclidean,
        Manhattan
    }

    public class Pathfinder
    {
        //a.x, a.y; returns w
        public delegate float calculateWeight(int x, int y);

        private readonly calculateHeuristic calculateH;
        private readonly calculateWeight calculateW;

        public Pathfinder(calculateWeight calculateW, Heuristic heuristic = Heuristic.Euclidean)
        {
            this.calculateW = calculateW;

            switch (heuristic)
            {
                case Heuristic.Euclidean:
                    calculateH = GetEuclideanDistance;
                    break;
                case Heuristic.Manhattan:
                    calculateH = GetManhattanDistance;
                    break;
            }
        }

        public Stack<int[]> GetPath(int startX, int startY, int endX, int endY, NodeMap nodeMap)
        {
            //Initalize start and end position using Vector2
            var startPosition = new Vector2(startX, startY);
            var endPosition = new Vector2(endX, endY);

            //Initalize MinHeap PriorityQueue
            IPriorityQueue<Node, float> openQueue = new BinaryHeap<Node, float>(PriorityQueueType.Minimum);
            //Initalize stack which tracks which Nodes are modified in finding path
            IPriorityQueue<Node, float> modifiedQueue = new BinaryHeap<Node, float>(PriorityQueueType.Minimum);
            //Initalize list which holds Nodes adjacent to currentNode
            var adjacentNodes = new List<Node>();
            //Initialize stack which contains path
            var path = new Stack<int[]>();

            //Set startNode and endNode and begin pathfinding
            var startNode = nodeMap.GetNode(startPosition);
            var endNode = nodeMap.GetNode(endPosition);

            startNode.Reset();
            endNode.Reset();

            startNode.h = calculateH(startPosition, endPosition);
            startNode.f = startNode.h;
            startNode.opened = true;
            modifiedQueue.Enqueue(startNode, startNode.h);
            openQueue.Enqueue(startNode, startNode.f);


            //Check if either startNode or endNode have a weight of infinity
            //if (float.IsInfinity(startNode.w) || float.IsInfinity(endNode.w))
                //If so, a path cannot exist so terminate pathfinding
                //return path;

            while (openQueue.Count != 0)
            {
                //Set currentNode to Node with the lowest F value
                var currentNode = openQueue.Dequeue();
                //Mark it as closed since it has been checked
                currentNode.closed = true;

                //Check if this Node is the end position
                if (currentNode.position.X == endPosition.X && currentNode.position.Y == endPosition.Y)
                {
                    //If so, construct final path 
                    endNode = currentNode;
                    path = Traceback(endNode);
                    break;
                }

                //Get all Nodes adjacent to the current position
                adjacentNodes = nodeMap.GetAdjacentNodes(currentNode.position);

                foreach (var adjacentNode in adjacentNodes)
                {
                    if (adjacentNode != endNode)
                    {
                        adjacentNode.w = CalculateWeight(adjacentNode.position);
                    }

                    //Check if Node has been closed or the Node has a weight of infinity
                    if (adjacentNode.closed || float.IsInfinity(adjacentNode.w))
                        //If so, skip Node
                        continue;

                    //Calculate the total greed valuie from currentNode and adjacent Node
                    var gtotal = currentNode.g + adjacentNode.g + adjacentNode.w;

                    //Check if this Node has not yet been observed yet
                    if (adjacentNode.opened == false)
                    {
                        //If not, initialize Node
                        adjacentNode.g = gtotal;
                        adjacentNode.h = CalculateHeuristic(adjacentNode.position, endPosition);
                        adjacentNode.f = adjacentNode.g + adjacentNode.h;
                        adjacentNode.parent = currentNode;
                        adjacentNode.opened = true;

                        //Add it to the openQueue
                        openQueue.Enqueue(adjacentNode, adjacentNode.f);
                        //Every Node that is opened is modified so add Node to modifiedQueue
                        modifiedQueue.Enqueue(adjacentNode, adjacentNode.h);
                    }

                    else
                    {
                        //Otherwise check if this Node is a better connection
                        if (gtotal + adjacentNode.h < adjacentNode.f)
                        {
                            //If so, update Node's greed value and parent
                            adjacentNode.g = gtotal;
                            adjacentNode.f = adjacentNode.g + adjacentNode.h;
                            adjacentNode.parent = currentNode;
                        }
                    }
                }
            }

            //Check if no valid path is found
            if (path.Count == 0)
            {
                //If so, find best possible path to Node closest to goal
                path = Traceback(modifiedQueue.Peek);
            }

            //Reset every modifiedNode for future pathfinding
            while (modifiedQueue.Count != 0) modifiedQueue.Dequeue().Reset();

            return path;
        }

        private Stack<int[]> Traceback(Node endNode)
        {
            var path = new Stack<int[]>();
            while (endNode != null)
            {
                path.Push(new[] {endNode.position.X, endNode.position.Y});
                endNode = endNode.parent;
            }

            return path;
        }

        private float CalculateWeight(Vector2 position)
        {
            return calculateW(position.X, position.Y);
        }

        private float CalculateHeuristic(Vector2 a, Vector2 b)
        {
            return calculateH(a, b);
        }

        private float GetManhattanDistance(Vector2 a, Vector2 b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private float GetEuclideanDistance(Vector2 a, Vector2 b)
        {
            return (float) Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        //a.x, a.y, b.x, b.y; returns h
        private delegate float calculateHeuristic(Vector2 a, Vector2 b);
    }


    /// <summary>
    ///     Represents a Node in NodeMap
    /// </summary>
    internal class Node
    {
        //processed flag
        internal bool closed;

        //Sum of h and g
        internal float f;

        //Greedy value. Sum of parent g values and weight
        internal float g;

        //Heuristic value. Approximate distance from goal
        internal float h;

        //observed flag
        internal bool opened;

        //Reference to parent of Node
        internal Node parent;

        //Position of Node
        internal Vector2 position;

        //Weight value. Abstraction of cost of moving onto node
        internal float w;

        internal Node(Vector2 position, float weight, float h, Node parent)
        {
            this.position = position;
            g = g;
            this.h = h;
            f = g + h;

            this.parent = parent;
        }

        internal Node(int x, int y, float weight, float h, Node parent)
        {
            position.X = x;
            position.Y = y;
            g = g;
            this.h = h;
            f = g + h;

            this.parent = parent;
        }

        //resets Node to default values
        internal void Reset()
        {
            w = 0;
            g = 0;
            h = 0;
            f = 0;
            parent = null;
            closed = false;
            opened = false;
        }
    }

    /// <summary>
    ///     Stores, generates, and initalizes Nodes.
    /// </summary>
    public class NodeMap
    {
        internal readonly int height;

        internal readonly int width;
        internal Dictionary<Vector2, Node> nodeMap = new Dictionary<Vector2, Node>();

        public NodeMap(int width, int height)
        {
            this.width = width;
            this.height = height;

            InitalizeNodeMap();
        }

        public void InitalizeNodeMap()
        {
            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                nodeMap.Add(new Vector2(x, y), new Node(new Vector2(x, y), 1, 0, null));
        }

        internal Node GetNode(Vector2 position)
        {
            if (position.X >= 0 && position.Y >= 0 && position.X < width && position.Y < height)
                return nodeMap[position];

            return null;
        }

        internal Node GetNode(int x, int y)
        {
            return GetNode(new Vector2(x, y));
        }

        public void SetNodeWeight(int x, int y, float weight)
        {
            nodeMap[new Vector2(x, y)].w = weight;
        }

        internal List<Node> GetAdjacentNodes(Vector2 position)
        {
            var adjacentNodes = new List<Node>();

            Node nodeToAdd = null;

            nodeToAdd = GetNode(position.X - 1, position.Y);
            if (nodeToAdd != null) adjacentNodes.Add(nodeToAdd);

            nodeToAdd = GetNode(position.X + 1, position.Y);
            if (nodeToAdd != null) adjacentNodes.Add(nodeToAdd);

            nodeToAdd = GetNode(position.X, position.Y - 1);
            if (nodeToAdd != null) adjacentNodes.Add(nodeToAdd);

            nodeToAdd = GetNode(position.X, position.Y + 1);
            if (nodeToAdd != null) adjacentNodes.Add(nodeToAdd);

            return adjacentNodes;
        }
    }

    internal struct Vector2
    {
        public int X, Y;

        public Vector2(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }
}