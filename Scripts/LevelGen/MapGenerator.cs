using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room {
    public int x;
    public int y;
    public int width;
    public int height;
    public Vector2Int center;
    public int centerX;
    public int centerY;

    public Room(int x, int y, int width, int height) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.center = new Vector2Int(x + width / 2, y + height / 2);

        this.centerX = x + (width/2);
        this.centerY = y + (height/2);  
    }
}


public class MapGenerator : MonoBehaviour {
    public static MapGenerator Instance { get; private set; } //singleton
    [Header("Entity spawning")]
    public Rigidbody2D PlayerRb;
    public int EnemySpawnChance = 2;
    //controls the spawning of enemies in a 1/x fashion, meaning that if this is set to 2, the chance to spawn enemies in a room will be 1/2, if 3 then 1/3 etc.
    public GameObject enemyPrefab;
    public int maxEnemiesPerRoom = 5;
    public int minEnemiesPerRoom = 1;
    public Rigidbody2D exitRb;
    [Header("Tilemap generator")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public TileBase floorTile;
    public TileBase wallTile;
    public TileBase doorTile;

    [Header("Generation settings")]
    public int mapWidth;
    public int mapHeight;
    public int maxRooms;
    public int minRoomSize;
    public int maxROomSize;

    public int[,] grid;
    public List<Room> rooms = new List<Room>();
    public int playerSpawnRoom;
    public int SpawnExitRoom;


    void Start() {
        GenerateFloor();
    }

    public void GenerateFloor() {
        grid = new int[mapWidth, mapHeight];
        rooms.Clear();
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();



        for(int i = 0; i < maxRooms; i++) {
            int w = Random.Range(minRoomSize, maxROomSize +1);
            int h = Random.Range(minRoomSize, maxROomSize +1);

            int x = Random.Range(1, mapWidth - w - 1);
            int y = Random.Range(1 , mapHeight - h - 1);


            Room newRoom = new Room(x, y, w,h);

            bool overlap = false;
            foreach(Room otherRoom in rooms) {
                if (newRoom.x < otherRoom.x + otherRoom.width &&
                    newRoom.x + newRoom.width > otherRoom.x &&
                    newRoom.y < otherRoom.y + otherRoom.height &&
                    newRoom.y + newRoom.height > otherRoom.y)
                {
                    overlap = true;
                    break;
                }
            }
            if(!overlap) {
                CarveRoom(newRoom);
                rooms.Add(newRoom);
            }
        }
        ConnectAllRooms();
        RenderTilemaps();
        SpawnPlayer();
        SpawnExit();
        SpawnEnemies();
    }
        private void CarveRoom(Room room) {
            for (int x = room.x; x < room.x + room.width - 1; x++)
        {
            for (int y = room.y; y < room.y + room.height - 1; y++)
            {
                grid[x, y] = 1; // Mark as floor data
            }
        }
    }
    private void RenderTilemaps() {
        for(int x = 0; x < mapWidth; x++){
        for(int y = 0; y < mapHeight; y++) {
            Vector3Int tilePos = new Vector3Int(x,y,0);
            if(grid[x,y] == 1) {
                floorTilemap.SetTile(tilePos, floorTile);
            } else if(grid[x,y] == 0) {
                wallTilemap.SetTile(tilePos, wallTile);
            } else if(grid[x,y] == 2) {
                wallTilemap.SetTile(tilePos, doorTile);
            }
        }
    }
        }

       private void CarveHorizontalCorridor(int xStart, int xEnd, int yPosition) {
    int start = Mathf.Min(xStart, xEnd);
    int end = Mathf.Max(xStart, xEnd);
    
    // Track if we have placed the starting doors and ending doors for this specific corridor execution
    bool placedStartDoors = false;

    for (int x = start; x <= end; x++) {
        // Ensure we are inside map boundaries for both lanes of the 2-wide corridor
        if (x >= 0 && x < mapWidth && yPosition >= 0 && yPosition + 1 < mapHeight) {
            
            // Check if both lanes are currently solid walls (0)
            bool isCurrentTileWall = (grid[x, yPosition] == 0 && grid[x, yPosition + 1] == 0);
            
            // Look ahead: is the NEXT step going to be a floor/room?
            // Look ahead: Is the NEXT step a floor, and is that floor actually inside a ROOM?
            bool isNextTileFloor = false;
            if (x + 1 <= end) {
                bool hasFloorAhead = (grid[x + 1, yPosition] == 1 || grid[x + 1, yPosition + 1] == 1);
                // Only count it as a room entry if it's within the boundaries of an actual room from your list
                isNextTileFloor = hasFloorAhead && IsInsideAnyRoom(x + 1, yPosition);
}

            // 1. PLACE STARTING DOORS: The very first transition from a room into a wall
            if (isCurrentTileWall && !placedStartDoors) {
                grid[x, yPosition] = 2;
                grid[x, yPosition + 1] = 2;
                placedStartDoors = true; 
                continue; // Skip carving regular floor over these doors on this loop cycle
            }

            // 2. PLACE ENDING DOORS: The final transition from wall back into the target room
            if (isCurrentTileWall && isNextTileFloor) {
                grid[x, yPosition] = 2;
                grid[x, yPosition + 1] = 2;
                continue;
            }

            // Carve normal 2-wide floor tiles if it's not a door boundary
            grid[x, yPosition] = 1;
            grid[x, yPosition + 1] = 1;
        }
    }
}

        private void CarveVerticalCorridor(int yStart, int yEnd, int xPosition) {
    int start = Mathf.Min(yStart, yEnd);
    int end = Mathf.Max(yStart, yEnd);

    // Track if we have placed the starting doors for this specific corridor execution
    bool placedStartDoors = false;

    for (int y = start; y <= end; y++) {
        // Ensure we are inside map boundaries for both lanes of the 2-wide corridor
        if (xPosition >= 0 && xPosition + 1 < mapWidth && y >= 0 && y < mapHeight) {
            
            // Check if both lanes are currently solid walls (0)
            bool isCurrentTileWall = (grid[xPosition, y] == 0 && grid[xPosition + 1, y] == 0);
            
            // Look ahead: is the NEXT step going to be a floor/room?
            // Look ahead: Is the NEXT step a floor, and is that floor actually inside a ROOM?
            bool isNextTileFloor = false;
            if (y + 1 <= end) {
                bool hasFloorAhead = (grid[xPosition, y + 1] == 1 || grid[xPosition + 1, y + 1] == 1);
                // Only count it as a room entry if it's within the boundaries of an actual room from your list
                isNextTileFloor = hasFloorAhead && IsInsideAnyRoom(xPosition, y + 1);
}

            // 1. PLACE STARTING DOORS: Moving from a room into a wall
            if (isCurrentTileWall && !placedStartDoors) {
                grid[xPosition, y] = 2;
                grid[xPosition + 1, y] = 2;
                placedStartDoors = true;
                continue;
            }

            // 2. PLACE ENDING DOORS: The last wall tile before hitting the target room floor
            if (isCurrentTileWall && isNextTileFloor) {
                grid[xPosition, y] = 2;
                grid[xPosition + 1, y] = 2;
                continue;
            }

            // Carve normal 2-wide floor tiles if it's not a door boundary
            grid[xPosition, y] = 1;
            grid[xPosition + 1, y] = 1;
        }
    }
}

        public void ConnectAllRooms() {
            for(int i = 1; i < rooms.Count; i++) {
                Room previousRoom = rooms[i - 1];
                Room currentRoom = rooms[i];

             if (Random.Range(0, 2) == 0) {
            CarveHorizontalCorridor(previousRoom.centerX, currentRoom.centerX, previousRoom.centerY);
            CarveVerticalCorridor(previousRoom.centerY, currentRoom.centerY, currentRoom.centerX);
        } else {
            CarveVerticalCorridor(previousRoom.centerY, currentRoom.centerY, previousRoom.centerX);
            CarveHorizontalCorridor(previousRoom.centerX, currentRoom.centerX, currentRoom.centerY);
        }
            }
        }

        public void SpawnPlayer() {
            playerSpawnRoom = Random.Range(0, rooms.Count);
            int spawnX = rooms[playerSpawnRoom].centerX;
            int spawnY = rooms[playerSpawnRoom].centerY;
            Vector2 spawnPos = new Vector2(spawnX, spawnY);

            PlayerRb.position = spawnPos;
        }
        public void SpawnExit() {
            SpawnExitRoom = Random.Range(0, rooms.Count);
            Debug.Log($"trying exit at: {SpawnExitRoom}, player at: {playerSpawnRoom}");
            if(SpawnExitRoom != playerSpawnRoom) {
                int exitX = rooms[SpawnExitRoom].centerX;
                int exitY = rooms[SpawnExitRoom].centerY;
                Vector2 exitPos = new Vector2(exitX, exitY);

                exitRb.position = exitPos;
            } else {
                SpawnExitRoom = rooms.Count - playerSpawnRoom;
                Debug.Log($"trying exit at: {SpawnExitRoom}, player at: {playerSpawnRoom}");
                 int exitX = rooms[SpawnExitRoom].centerX;
                int exitY = rooms[SpawnExitRoom].centerY;
                Vector2 exitPos = new Vector2(exitX, exitY);
            }

            
        }
        public void SpawnEnemies() {
          for(int i = 0; i < rooms.Count; i++) {
            if(i == playerSpawnRoom || i == SpawnExitRoom) {
                continue;
            }

            int roll = Random.Range(1, EnemySpawnChance + 1);

            if(roll == EnemySpawnChance) {
                Debug.Log($"Spawning enemies in room: {i}");
                int enemyCount = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);

                for(int j = 0; j < enemyCount; j++) {
                int halfW = (rooms[i].width / 2) - 1;
                int halfH = (rooms[i].height / 2) - 1;

                float enemyX = rooms[i].centerX + Random.Range(-halfW, halfW + 1) + 0.5f;
                float enemyY = rooms[i].centerY + Random.Range(-halfH, halfH + 1) + 0.5f;
                
                Vector3 enemyPos = new Vector3(enemyX, enemyY, 0f);

                GameObject newEnemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
                }

            }
          }
        }
        private bool IsInsideAnyRoom(int x, int y) {
            foreach (Room room in rooms) {
            // Check if the coordinate falls cleanly within the boundaries of an existing room
              if (x >= room.x && x < room.x + room.width &&
                  y >= room.y && y < room.y + room.height) {
                     return true;
              }
            }
         return false;
        }
        
    }
    

