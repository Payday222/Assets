using UnityEngine;

public class CameraFollow : MonoBehaviour
{
public Transform playerPos;
public Vector3 offset = new Vector3(0,0,-10);
private void LateUpdate() {
    transform.position = playerPos.position + offset;
}

}
