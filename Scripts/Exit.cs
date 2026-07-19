
using UnityEngine;

public class Exit : MonoBehaviour
{

public MapGenerator mapgen;



/// <summary>
/// Sent when another object enters a trigger collider attached to this
/// object (2D physics only).
/// </summary>
/// <param name="other">The other Collider2D involved in this collision.</param>
private void OnTriggerEnter2D(Collider2D other)
{
    Debug.Log($"alaaa kurwa jak chodzisz cwelu");
    mapgen.GenerateFloor();
}
}
