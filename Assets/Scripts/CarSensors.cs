using UnityEngine;

// Reads wall distance around the car using 5 raycasts.
public class CarSensors : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform sensorOrigin; // Raycasts start from this transform.
    [SerializeField] private float sensorLength = 10f; // Max distance each sensor checks.
    [SerializeField] private LayerMask wallMask; // Keeps raycasts focused on wall layers.

    [Header("Sensor Outputs (Normalised 0-1)")]
    public float front;
    public float frontLeft;
    public float left;
    public float frontRight;
    public float right;

    private void Update()
    {
        // Sample all directions every frame.
        front = CastSensor(sensorOrigin.forward);
        frontLeft = CastSensor(Quaternion.AngleAxis(-45, Vector3.up) * sensorOrigin.forward);
        left = CastSensor(-sensorOrigin.right);
        frontRight = CastSensor(Quaternion.AngleAxis(45, Vector3.up) * sensorOrigin.forward);
        right = CastSensor(sensorOrigin.right);
    }

    // Returns a 0-1 distance where 0 is close and 1 is clear.
    private float CastSensor(Vector3 direction)
    {
        Ray ray = new Ray(sensorOrigin.position, direction);

        // Red line means we hit a wall.
        if (Physics.Raycast(ray, out RaycastHit hit, sensorLength, wallMask))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red);
            return hit.distance / sensorLength;
        }

        // Green line means no wall in range.
        Debug.DrawRay(ray.origin, direction * sensorLength, Color.green);
        return 1f;
    }
}
