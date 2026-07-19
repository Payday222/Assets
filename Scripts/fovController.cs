using UnityEngine;
using UnityEngine.UI;

public class fovController : MonoBehaviour
{
    public Transform player;

    public Material visionMaterial;

    [Range(0.05f, 1f)]
    public float distance = 0.35f;

    [Range(1,180)]
    public float coneAngle = 70f;

    Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        Vector3 mouseWorld =
            cam.ScreenToWorldPoint(Input.mousePosition);

        mouseWorld.z = player.position.z;

        Vector2 dir =
            (mouseWorld - player.position).normalized;

        Vector3 screen =
            cam.WorldToViewportPoint(player.position);

        visionMaterial.SetVector(
            "_PlayerPos",
            new Vector4(screen.x, screen.y,0,0));

        visionMaterial.SetVector(
            "_Direction",
            new Vector4(dir.x, dir.y,0,0));

        visionMaterial.SetFloat(
            "_Distance",
            distance);

        visionMaterial.SetFloat(
            "_Angle",
            coneAngle);
    }
}