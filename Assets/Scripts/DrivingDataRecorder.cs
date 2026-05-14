using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

// Writes driving samples to CSV while you control the car.
public class DrivingDataRecorder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarSensors carSensors; // Reads live sensor values.
    [SerializeField] private CarController carController; // Reads current controls and speed.

    [Header("Recording")]
    [SerializeField] private bool recordOnPlay = false; // Auto-start recording when scene begins.
    [SerializeField] private KeyCode toggleRecordKey = KeyCode.R; // Manual start/stop key.
    [SerializeField] private float recordInterval = 0.1f; // Time gap between rows.
    [SerializeField] private bool writeOnlyWhenMoving = true; // Skip parked/noise data.
    [SerializeField] private float minSpeedToRecord = 0.25f; // Movement threshold for recording.

    private const string SessionFilePrefix = "drive_session_";
    private const string SessionFileExtension = ".csv";

    private string filePath; // Output CSV path.
    private string sessionId; // Session id stored in each row.
    private bool isRecording; // Current recording state.
    private bool collisionFlag; // Flips true after first collision.
    private float recordTimer; // Tracks interval timing.

    private void Start()
    {
        // Create a new numbered session file and write header.
        filePath = BuildNextSessionFilePath();
        sessionId = Path.GetFileNameWithoutExtension(filePath);

        File.WriteAllText(filePath, "time,speed,mode,collisionFlag,sessionId,front,frontLeft,left,frontRight,right,steer,throttle\n");

        isRecording = recordOnPlay;
        Debug.Log($"CSV Path: {filePath}");
    }

    private void Update()
    {
        // Toggle recording while driving.
        if (Input.GetKeyDown(toggleRecordKey))
        {
            isRecording = !isRecording;
            Debug.Log(isRecording ? "Recording started" : "Recording stopped");
        }
    }

    private void FixedUpdate()
    {
        // No write when recording is off.
        if (!isRecording)
        {
            return;
        }

        // Skip slow/idle rows if enabled.
        if (writeOnlyWhenMoving && Mathf.Abs(carController.CurrentSpeed) < minSpeedToRecord)
        {
            return;
        }

        // Use fixed delta so sampling stays consistent.
        recordTimer += Time.fixedDeltaTime;

        if (recordTimer >= recordInterval)
        {
            recordTimer = 0f;
            WriteSample();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Mark this session as collided from this point on.
        collisionFlag = true;
    }

    private string BuildNextSessionFilePath()
    {
        string assetsPath = Application.dataPath;
        string[] existingSessionFiles = Directory.GetFiles(
            assetsPath,
            $"{SessionFilePrefix}*{SessionFileExtension}",
            SearchOption.TopDirectoryOnly);

        int maxSessionNumber = 0;

        // Find the highest existing session number.
        foreach (string existingFilePath in existingSessionFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(existingFilePath);

            if (!fileName.StartsWith(SessionFilePrefix))
            {
                continue;
            }

            string numericSegment = fileName.Substring(SessionFilePrefix.Length);
            if (int.TryParse(numericSegment, out int sessionNumber) && sessionNumber > maxSessionNumber)
            {
                maxSessionNumber = sessionNumber;
            }
        }

        int nextSessionNumber = maxSessionNumber + 1;
        string nextSessionFileName = $"{SessionFilePrefix}{nextSessionNumber:D3}{SessionFileExtension}";

        return Path.Combine(assetsPath, nextSessionFileName);
    }

    private void WriteSample()
    {
        StringBuilder sb = new StringBuilder();

        // Write metadata first so rows are easy to filter later.
        sb.Append(Time.time.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carController.CurrentSpeed.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carController.CurrentControlMode.ToString()).Append(",");
        sb.Append(collisionFlag ? "1" : "0").Append(",");
        sb.Append(sessionId).Append(",");

        // Then write the five sensor inputs.
        sb.Append(carSensors.front.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carSensors.frontLeft.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carSensors.left.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carSensors.frontRight.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carSensors.right.ToString("F4", CultureInfo.InvariantCulture)).Append(",");

        // Last two values are the targets for training.
        sb.Append(carController.CurrentSteeringInput.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carController.CurrentThrottleInput.ToString("F4", CultureInfo.InvariantCulture)).Append("\n");

        // Append keeps existing rows intact.
        File.AppendAllText(filePath, sb.ToString());
    }
}
