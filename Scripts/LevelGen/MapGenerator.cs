using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room {
    public int x;
    public int y;
    public int width;
    public int height;
    public Vector2Int center;

    public Room(int x, int y, int width, int height) {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.center = new Vector2Int(x + width / 2, y + height / 2);
    }
}


public class MapGenerator : MonoBehaviour {
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
    private List<Room> rooms = new List<Room>();


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
    
        RenderTilemaps();
    }
        private void CarveRoom(Room room) {
            for (int x = room.x + 1; x < room.x + room.width - 1; x++)
        {
            for (int y = room.y + 1; y < room.y + room.height - 1; y++)
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
    }
    

