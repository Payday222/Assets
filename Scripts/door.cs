using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class door : MonoBehaviour
{
    public Rigidbody2D playerRb;
    public Rigidbody2D rb;
    public GameObject playerObj;
    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        playerObj = GameObject.FindGameObjectWithTag("Player");
        if(playerObj != null) {
            playerRb = playerObj.GetComponent<Rigidbody2D>();
        }
    }

    public void Update() {
        if(CalculateDistance() < 2f && Input.GetKeyDown(KeyCode.E)) {
            Debug.Log($"cheesy michael");
            this.rb.gameObject.SetActive(false);
        } 
    }
    
    public float CalculateDistance() {
        float distance = Vector2.Distance(playerRb.position, rb.position);
        return distance;
    }
}
