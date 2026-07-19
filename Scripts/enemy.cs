using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy : MonoBehaviour
{
    public Rigidbody2D playerRb;
    public Rigidbody2D rb;

    [Header("settings")]
    public float detectionRadius;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate() {
        //* handle player detection
        float distanceToPlayer = Vector2.Distance(rb.position, playerRb.position);
        if(distanceToPlayer <= detectionRadius) {
            Debug.Log($"player detected");
            //*combat logic
        }
    }
}
