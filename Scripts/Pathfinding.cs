using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathNode {
        public int x, y;
        public bool isWall;
        public int gCost;
        public int hCost;
        public int fCost => gCost + hCost;

        public PathNode parent;

        public PathNode(int x, int y, bool isWall) {
            this.x = x;
            this.y= y;
            this.isWall = isWall;
        }
    }

public class Pathfinding {
    private PathNode[,] grid;
    private int width, height;

    public Pathfinding(Tilemap wallTilemap, int width, int height) {
        this.width = width;
        this.height = height;
        grid = new PathNode[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                // Convert our loop coordinates into a Vector3Int grid position
                Vector3Int tilePos = new Vector3Int(x, y, 0);

                // If HasTile returns true, there is a physical wall object here!
                bool isWall = wallTilemap.HasTile(tilePos);

                grid[x, y] = new PathNode(x, y, isWall);
            }
        }
    }


    public List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int targetPos) {
        PathNode startNode = grid[startPos.x, startPos.y];
        PathNode targetNode = grid[targetPos.x, targetPos.y];

        List<PathNode> openSet = new List<PathNode> { startNode };
        HashSet<PathNode> closedSet = new HashSet<PathNode>();

        while (openSet.Count > 0) {
            // Find node in openSet with the lowest fCost
            PathNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++) {
                if (openSet[i].fCost < currentNode.fCost || 
                   (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)) {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // Path found!
            if (currentNode == targetNode) {
                return RetracePath(startNode, targetNode);
            }

            foreach (PathNode neighbor in GetNeighbors(currentNode)) {
                if (neighbor.isWall || closedSet.Contains(neighbor)) continue;

                int newMovementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor)) {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor)) {
                        openSet.Add(neighbor);
                    }
                }
            }
        }
        return null; // No path possible
    }

    private List<Vector2Int> RetracePath(PathNode startNode, PathNode endNode) {
        List<Vector2Int> path = new List<Vector2Int>();
        PathNode currentNode = endNode;

        while (currentNode != startNode) {
            path.Add(new Vector2Int(currentNode.x, currentNode.y));
            currentNode = currentNode.parent;
        }
        path.Reverse(); // Flip it so it goes start -> end
        return path;
    }

    private int GetDistance(PathNode nodeA, PathNode nodeB) {
        // Manhattan Distance Heuristic
        int dstX = Mathf.Abs(nodeA.x - nodeB.x);
        int dstY = Mathf.Abs(nodeA.y - nodeB.y);
        return dstX + dstY;
    }

    private List<PathNode> GetNeighbors(PathNode node) {
        List<PathNode> neighbors = new List<PathNode>();
        // Check 4 cardinal directions (up, down, left, right)
        if (node.x + 1 < width) neighbors.Add(grid[node.x + 1, node.y]);
        if (node.x - 1 >= 0) neighbors.Add(grid[node.x - 1, node.y]);
        if (node.y + 1 < height) neighbors.Add(grid[node.x, node.y + 1]);
        if (node.y - 1 >= 0) neighbors.Add(grid[node.x, node.y - 1]);
        return neighbors;
    }
}
