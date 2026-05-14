using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

// Records sensor data + player inputs into a CSV dataset for training
public class DrivingDataRecorder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarSensors carSensors;      // Access to current sensor values
    [SerializeField] private CarController carController; // Access to player inputs

    [Header("Recording")]
    [SerializeField] private bool recordOnPlay = false;   // Start recording automatically
    [SerializeField] private KeyCode toggleRecordKey = KeyCode.R; // Key to toggle recording
    [SerializeField] private float recordInterval = 0.1f; // Time between samples
    [SerializeField] private bool writeOnlyWhenMoving = true; // Prevent recording when stationary
    [SerializeField] private float minSpeedToRecord = 0.25f;  // Minimum speed required to record

    private const string SessionFilePrefix = "drive_session_";
    private const string SessionFileExtension = ".csv";

    private string filePath;      // Location of CSV file
    private string sessionId;     // Session ID stored per sample
    private bool isRecording;     // Current recording state
    private bool collisionFlag;   // Whether a collision has happened in this session
    private float recordTimer;    // Tracks time between samples

    private void Start()
    {
        filePath = BuildNextSessionFilePath();
        sessionId = Path.GetFileNameWithoutExtension(filePath);

        File.WriteAllText(filePath, "time,speed,mode,collisionFlag,sessionId,front,frontLeft,left,frontRight,right,steer,throttle\n");

        // Set initial recording state
        isRecording = recordOnPlay;

        Debug.Log($"CSV Path: {filePath}");
    }

    private void Update()
    {
        // Toggle recording on key press
        if (Input.GetKeyDown(toggleRecordKey))
        {
            isRecording = !isRecording;
            Debug.Log(isRecording ? "Recording started" : "Recording stopped");
        }
    }

    private void FixedUpdate()
    {
        // Only record if enabled
        if (!isRecording)
        {
            return;
        }

        // ignore samples when car is nearly stationary
        if (writeOnlyWhenMoving && Mathf.Abs(carController.CurrentSpeed) < minSpeedToRecord)
        {
            return;
        }

        // Use fixed timestep for consistent sampling
        recordTimer += Time.fixedDeltaTime;

        // Record at defined interval
        if (recordTimer >= recordInterval)
        {
            recordTimer = 0f;
            WriteSample();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
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

        // Session metadata
        sb.Append(Time.time.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carController.CurrentSpeed.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carController.CurrentControlMode.ToString()).Append(",");
        sb.Append(collisionFlag ? "1" : "0").Append(",");
        sb.Append(sessionId).Append(",");

        // Append sensor inputs (formatted to fixed decimal precision)
        sb.Append(carSensors.front.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carSensors.frontLeft.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carSensors.left.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carSensors.frontRight.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carSensors.right.ToString("F4", CultureInfo.InvariantCulture)).Append(",");

        // Append player actions (outputs)
        sb.Append(carController.CurrentSteeringInput.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carController.CurrentThrottleInput.ToString("F4", CultureInfo.InvariantCulture)).Append("\n");

        // Write row to CSV without overwriting existing data
        File.AppendAllText(filePath, sb.ToString());
    }
}
