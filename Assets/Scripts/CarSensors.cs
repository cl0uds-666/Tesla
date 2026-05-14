using UnityEngine;

// Uses raycasts to detect distances to nearby walls in multiple directions
public class CarSensors : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform sensorOrigin; // Starting point of all sensors 
    [SerializeField] private float sensorLength = 10f; // Max distance each sensor can detect
    [SerializeField] private LayerMask wallMask; // Makes see just wall layer

    [Header("Sensor Outputs (Normalised 0–1)")]
    public float front;
    public float frontLeft;
    public float left;
    public float frontRight;
    public float right;

    private void Update()
    {
        // Cast sensors in 5 directions relative to the car
        front = CastSensor(sensorOrigin.forward);

        // Rotate forward vector to get angled directions
        frontLeft = CastSensor(Quaternion.AngleAxis(-45, Vector3.up) * sensorOrigin.forward);
        left = CastSensor(-sensorOrigin.right);
        frontRight = CastSensor(Quaternion.AngleAxis(45, Vector3.up) * sensorOrigin.forward);
        right = CastSensor(sensorOrigin.right);
    }

    // Casts a ray in a given direction and returns a normalised distance value
    private float CastSensor(Vector3 direction)
    {
        Ray ray = new Ray(sensorOrigin.position, direction);

        // Check if the ray hits something within range
        if (Physics.Raycast(ray, out RaycastHit hit, sensorLength, wallMask))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red);

            // Convert distance to a 0–1 range (0 = very close, 1 = max distance)
            return hit.distance / sensorLength;
        }
        else
        {
            // No hit = nothing detected within range
            Debug.DrawRay(ray.origin, direction * sensorLength, Color.green);

            return 1f; // Max value (no obstacle nearby)
        }
    }
}