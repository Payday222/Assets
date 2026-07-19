using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
    [Header("Entity spawning")]
    public Rigidbody2D PlayerRb;
    public Rigidbody2D exitRb;
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

    private int[,] grid;
    public List<Room> rooms = new List<Room>();
    public int playerSpawnRoom;


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

        private void CarveHorizontalCorridor(int xStart, int xEnd, int yPosition) {
            int start = Mathf.Min(xStart, xEnd);
            int end = Mathf.Max(xStart, xEnd);
            
            for(int x = start; x <= end; x++) {
                if(x >= 0 && x < mapWidth && yPosition >= 0 && yPosition < mapHeight) {
                    Vector3Int tilePos = new Vector3Int(x,yPosition,0);
                    grid[x, yPosition] = 1;
                }
            }
        }

        private void CarveVerticalCorridor(int yStart, int yEnd, int xPosition) {
            int start = Mathf.Min(yStart, yEnd);
            int end = Mathf.Max(yStart, yEnd);

            for(int y = start; y <= end; y++) {
                if(xPosition >= 0 && xPosition < mapWidth && y < mapHeight) {
                    Vector3Int tilePos = new Vector3Int(xPosition, y ,0);
                     grid[xPosition, y] = 1;
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
            int SpawnExitRoom = Random.Range(0, rooms.Count);
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
    }
    

