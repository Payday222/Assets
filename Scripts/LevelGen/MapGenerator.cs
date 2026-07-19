using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Collections;
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
    public GameObject doorPrefab;
    public Transform doorParent;
    public List<GameObject> doors;
    // public NativeHashMap<int, int> doorCoords;
    [Header("Tilemap generator")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;
    public TileBase floorTile;
    public TileBase wallTile;

    [Header("Generation settings")]
    public int mapWidth;
    public int mapHeight;
    public int maxRooms;
    public int minRoomSize;
    public int maxROomSize;
    public int corridorWidth = 2;

    public int[,] grid;
    public List<Room> rooms = new List<Room>();
    public int playerSpawnRoom;
    public int SpawnExitRoom;

    // Tracks every grid tile that already has a door, so two different rooms'
    // edge-crawls can never spawn two doors on (or right next to) the same corridor opening.
    private HashSet<Vector2Int> spawnedDoorTiles = new HashSet<Vector2Int>();


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
        SpawnDoorsWithCrawler();
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
            } else {
                wallTilemap.SetTile(tilePos, wallTile);
            }
        }
    }
        }

        // Returns true if (x,y) is inside "room" or within a 1-tile buffer around it.
        // This uses the same buffer that the door crawler scans (room.x-1 .. room.x+width-1),
        // so "is this near a room" is defined consistently in both places.
        private bool IsInRoomBuffer(int x, int y, Room room) {
            return x >= room.x - 1 && x <= room.x + room.width - 1 &&
                   y >= room.y - 1 && y <= room.y + room.height - 1;
        }

        // A corridor tile is "blocked" if it falls inside or right next to any room
        // OTHER than the two rooms this corridor segment is actually connecting.
        // This is what stops a corridor (or its width padding) from punching a hole
        // in the wall of some unrelated room it merely happens to pass near.
        private bool IsBlockedByForeignRoom(int x, int y, Room source, Room destination) {
            foreach (Room r in rooms) {
                if (r == source || r == destination) continue;
                if (IsInRoomBuffer(x, y, r)) return true;
            }
            return false;
        }

        private void CarveHorizontalCorridor(int xStart, int xEnd, int yPosition, Room source, Room destination) {
            int start = Mathf.Min(xStart, xEnd);
            int end = Mathf.Max(xStart, xEnd);

            for (int x = start; x <= end; x++) {
                for (int i = 0; i < corridorWidth; i++) {
                    int y = yPosition - i;
                    if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight) continue;
                    // i == 0 is the guaranteed connecting line: always carve it so a
                    // source/destination pair can never end up fully sealed off just
                    // because a third room happens to sit close to the path. Only the
                    // extra width (i > 0) gets trimmed near foreign rooms.
                    if (i > 0 && IsBlockedByForeignRoom(x, y, source, destination)) continue;
                    grid[x, y] = 1;
                }
            }
        }

        private void CarveVerticalCorridor(int yStart, int yEnd, int xPosition, Room source, Room destination) {
            int start = Mathf.Min(yStart, yEnd);
            int end = Mathf.Max(yStart, yEnd);

            for (int y = start; y <= end; y++) {
                for (int i = 0; i < corridorWidth; i++) {
                    int x = xPosition + i;
                    if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight) continue;
                    if (i > 0 && IsBlockedByForeignRoom(x, y, source, destination)) continue;
                    grid[x, y] = 1;
                }
            }
        }

        public void ConnectAllRooms() {
            for(int i = 1; i < rooms.Count; i++) {
                Room previousRoom = rooms[i - 1];
                Room currentRoom = rooms[i];

             if (Random.Range(0, 2) == 0) {
            CarveHorizontalCorridor(previousRoom.centerX, currentRoom.centerX, previousRoom.centerY, previousRoom, currentRoom);
            CarveVerticalCorridor(previousRoom.centerY, currentRoom.centerY, currentRoom.centerX, previousRoom, currentRoom);
        } else {
            CarveVerticalCorridor(previousRoom.centerY, currentRoom.centerY, previousRoom.centerX, previousRoom, currentRoom);
            CarveHorizontalCorridor(previousRoom.centerX, currentRoom.centerX, currentRoom.centerY, previousRoom, currentRoom);
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
            if (rooms.Count > 1) {
                do {
                    SpawnExitRoom = Random.Range(0, rooms.Count);
                } while (SpawnExitRoom == playerSpawnRoom);
            } else {
                SpawnExitRoom = playerSpawnRoom;
            }

            int exitX = rooms[SpawnExitRoom].centerX;
            int exitY = rooms[SpawnExitRoom].centerY;
            exitRb.position = new Vector2(exitX, exitY);
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
                int halfW = Mathf.Max(0, (rooms[i].width / 2) - 1);
                int halfH = Mathf.Max(0, (rooms[i].height / 2) - 1);

                float enemyX = rooms[i].centerX + Random.Range(-halfW, halfW + 1) + 0.5f;
                float enemyY = rooms[i].centerY + Random.Range(-halfH, halfH + 1) + 0.5f;
                
                Vector3 enemyPos = new Vector3(enemyX, enemyY, 0f);

                GameObject newEnemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
                }

            }
          }
        }


public void SpawnDoorsWithCrawler() {
    spawnedDoorTiles.Clear();

    foreach (Room room in rooms) {
        // 1. Crawl Left and Right outer edges
        CrawlVerticalEdge(room.x - 1, room.y, room.height);                  // Left wall line
        CrawlVerticalEdge(room.x + room.width - 1, room.y, room.height); // Right wall line

        // 2. Crawl Bottom and Top outer edges
        CrawlHorizontalEdge(room.x, room.y - 1, room.width);                   // Bottom wall line
        CrawlHorizontalEdge(room.x, room.y + room.height - 1, room.width); // Top wall line
    }
}

// Walks a fixed column (x) from yStart to yStart+height, looking for floor tiles.
// Instead of spawning a door on every floor tile it sees, it groups consecutive
// floor tiles into a single "run" and places ONE door at the run's midpoint.
// This stops a corridor that runs parallel to a room's wall from producing a
// whole row of doors, and (combined with spawnedDoorTiles) stops two rooms that
// are only 1 tile apart from both spawning a door on the same connecting tile.
private void CrawlVerticalEdge(int x, int yStart, int height) {
    if (x < 0 || x >= mapWidth) return;

    int runStart = -1;
    int yEnd = yStart + height;

    for (int y = yStart; y <= yEnd; y++) {
        bool isFloor = (y >= 0 && y < mapHeight) && grid[x, y] == 1;

        if (isFloor) {
            if (runStart == -1) runStart = y;
        } else if (runStart != -1) {
            PlaceDoorsForRun(x, runStart, y - 1, vertical: true);
            runStart = -1;
        }
    }

    // Flush a run that extends all the way to the end of the edge
    if (runStart != -1) {
        PlaceDoorsForRun(x, runStart, yEnd, vertical: true);
    }
}

// Same idea as CrawlVerticalEdge, but walks a fixed row (y) from xStart to xStart+width.
private void CrawlHorizontalEdge(int xStart, int y, int width) {
    if (y < 0 || y >= mapHeight) return;

    int runStart = -1;
    int xEnd = xStart + width;

    for (int x = xStart; x <= xEnd; x++) {
        bool isFloor = (x >= 0 && x < mapWidth) && grid[x, y] == 1;

        if (isFloor) {
            if (runStart == -1) runStart = x;
        } else if (runStart != -1) {
            PlaceDoorsForRun(y, runStart, x - 1, vertical: false);
            runStart = -1;
        }
    }

    if (runStart != -1) {
        PlaceDoorsForRun(y, runStart, xEnd, vertical: false);
    }
}

// Places doors for a contiguous run of floor tiles found along an edge, sized to match
// the corridor's width. "fixedCoord" is the edge's constant coordinate (x for vertical
// edges, y for horizontal edges); "runStart"/"runEnd" are the range along the varying axis.
//
// - A run exactly corridorWidth tiles long (a normal doorway) gets one door per tile,
//   so the doors span the full width of the corridor.
// - A run shorter than corridorWidth (e.g. clipped by the map edge) gets one door per
//   available tile.
// - A run longer than corridorWidth (e.g. a corridor that runs alongside the wall for a
//   stretch) still only gets corridorWidth doors, centered in the run, instead of one
//   door per tile.
//
// Deduplicates against spawnedDoorTiles so the same physical opening never gets doors
// spawned twice from two different rooms' crawls.
private void PlaceDoorsForRun(int fixedCoord, int runStart, int runEnd, bool vertical) {
    int runLength = runEnd - runStart + 1;
    int doorsToPlace = Mathf.Min(runLength, corridorWidth);
    int blockStart = runStart + (runLength - doorsToPlace) / 2;

    for (int i = 0; i < doorsToPlace; i++) {
        int coord = blockStart + i;
        Vector2Int doorTile = vertical ? new Vector2Int(fixedCoord, coord) : new Vector2Int(coord, fixedCoord);

        if (spawnedDoorTiles.Contains(doorTile)) continue;

        spawnedDoorTiles.Add(doorTile);
        SpawnDoor(doorTile.x, doorTile.y);
    }
}

private void SpawnDoor(int x, int y) {
    // Convert grid coordinate to Unity world position (+0.5f centers it on the tile)
    Vector3 spawnPos = new Vector3(x + 0.5f, y + 0.5f, 0);
    
    // Spawn the physical door gameobject
    GameObject newdoor =Instantiate(doorPrefab, spawnPos, Quaternion.identity, doorParent);

    doors.Add(newdoor);
    // doorCoords.Add(x, y);

}
// private void DoorCleanup() {
//     foreach(var pair in doorCoords) { // key = x, value = y
//         int validNeighborCount = 0;
//         int x = pair.Key;
//         int y = pair.Value;

//         if (grid[x, y + 1] == 0 || grid[x, y + 1] == 2) validNeighborCount++; // Up
//         if (grid[x, y - 1] == 0 || grid[x, y - 1] == 2) validNeighborCount++; // Down
//         if (grid[x - 1, y] == 0 || grid[x - 1, y] == 2) validNeighborCount++; // Left
//         if (grid[x + 1, y] == 0 || grid[x + 1, y] == 2) validNeighborCount++; // Right

//         if(validNeighborCount < 2) {
            
//         }
//         }
//     }
}
    


