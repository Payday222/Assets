using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class fieldOfView : MonoBehaviour
{
    [Header("FOV Settings")]
    public float fovAngle = 90f;       
    public float viewDistance = 5f;    
    public int rayCount = 50;          
    public LayerMask wallLayer;        

    private Mesh fovMesh;
    private MeshFilter meshFilter;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        fovMesh = new Mesh();
        fovMesh.name = "FOV Mesh";
        meshFilter.mesh = fovMesh;
    }

    private void LateUpdate()
    {
        MakeFOV();
    }

    private void MakeFOV()
    {
        // 1. Calculate the base angle pointing towards the mouse mouse
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 aimDirection = (mousePos - transform.position).normalized;
        float targetAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // 2. Start casting rays from the leftmost edge of the cone
        float currentAngle = targetAngle + (fovAngle / 2f);
        float angleStep = fovAngle / rayCount;

        // Arrays to hold procedural mesh building data
        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];

        // The first vertex is always the player's local position (0,0,0)
        vertices[0] = Vector3.zero;

        int vertexIndex = 1;
        int triangleIndex = 0;

        for (int i = 0; i <= rayCount; i++)
        {
            // Convert current angle to a directional vector
            Vector3 rayDir = AngleToVector(currentAngle);
            
            // Cast a 2D ray from the player position outwards
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDir, viewDistance, wallLayer);

            Vector3 vertex;
            if (hit.collider != null)
            {
                // Hit a wall! Set vertex to the impact point (converted to local space)
                vertex = transform.InverseTransformPoint(hit.point);
            }
            else
            {
                // Clear shot. Set vertex to maximum view distance
                vertex = transform.InverseTransformPoint(transform.position + rayDir * viewDistance);
            }

            vertices[vertexIndex] = vertex;

            // Build triangles sequentially connecting vertices back to the player origin
            if (i > 0)
            {
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;
                triangleIndex += 3;
            }

            vertexIndex++;
            currentAngle -= angleStep;
        }

        // 3. Update the mesh asset on the GPU
        fovMesh.Clear();
        fovMesh.vertices = vertices;
        fovMesh.triangles = triangles;
        fovMesh.RecalculateBounds();
    }

    // Mathematical translation helper: Degrees -> Directional Vector3
    private Vector3 AngleToVector(float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(radian), Mathf.Sin(radian));
    }
}