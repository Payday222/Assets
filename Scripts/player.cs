using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{
    public Rigidbody2D rb;
    [Header("Variables")]
    public float movespeed = 2;


    Vector2 movementDirection = new Vector2();
    private void Update() {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        movementDirection = new Vector2(horizontal, vertical);
    }
    private void FixedUpdate() {
        //movement

        rb.velocity = movementDirection.normalized * movespeed;

    }
}
