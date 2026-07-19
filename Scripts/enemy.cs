using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class enemy : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D playerRb;
    public Rigidbody2D rb;
    private Pathfinding pathfinding;
    private List<Vector2Int> currentPath;

    [Header("Settings")]
    public float detectionRadius = 5f;
    public float moveSpeed = 3f; // Added speed control

    private void Start() 
    {
        rb = GetComponent<Rigidbody2D>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) 
        {
            playerRb = playerObj.GetComponent<Rigidbody2D>();
        }

        // Initialize A* grid using the tilemap approach
        if (MapGenerator.Instance != null) 
        {
            InitializePathfinder();
        }
    }

    private void FixedUpdate() 
    {
        if (playerRb == null || rb == null) return;

        float distanceToPlayer = Vector2.Distance(rb.position, playerRb.position);
        
        if (distanceToPlayer <= detectionRadius) 
        {
            Debug.Log("Player detected!");

            Vector2 moveDirection = (playerRb.position - rb.position).normalized;

            rb.velocity = moveDirection * moveSpeed;

            // * combat logic goes here
        }
        else 
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void InitializePathfinder() 
    {
        Tilemap walls = MapGenerator.Instance.wallTilemap;
        int width = MapGenerator.Instance.mapWidth;
        int height = MapGenerator.Instance.mapHeight;

        pathfinding = new Pathfinding(walls, width, height);
    }
}