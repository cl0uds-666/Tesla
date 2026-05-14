using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

// Records sensor + driver input snapshots into CSV for training.
public class DrivingDataRecorder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarSensors carSensors;      // Live ray sensor values.
    [SerializeField] private CarController carController; // Live steering/throttle values.

    [Header("Recording")]
    [SerializeField] private bool recordOnPlay = false;   // Start capturing as soon as play begins.
    [SerializeField] private KeyCode toggleRecordKey = KeyCode.R; // Keyboard toggle for recording.
    [SerializeField] private float recordInterval = 0.1f; // Seconds between CSV rows.
    [SerializeField] private bool writeOnlyWhenMoving = true; // Skip idle samples to reduce noise.
    [SerializeField] private float minSpeedToRecord = 0.25f;  // Speed gate for writing rows.

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

        // Applies the startup toggle.
        isRecording = recordOnPlay;

        Debug.Log($"CSV Path: {filePath}");
    }

    private void Update()
    {
        // Press key once to start/stop capture.
        if (Input.GetKeyDown(toggleRecordKey))
        {
            isRecording = !isRecording;
            Debug.Log(isRecording ? "Recording started" : "Recording stopped");
        }
    }

    private void FixedUpdate()
    {
        // No file writes while recording is off.
        if (!isRecording)
        {
            return;
        }

        // Skip tiny movement so the dataset stays useful.
        if (writeOnlyWhenMoving && Mathf.Abs(carController.CurrentSpeed) < minSpeedToRecord)
        {
            return;
        }

        // Uses fixed delta so sample timing stays consistent.
        recordTimer += Time.fixedDeltaTime;

        // Writes one row when interval is reached.
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

        // Writes metadata columns first.
        sb.Append(Time.time.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carController.CurrentSpeed.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carController.CurrentControlMode.ToString()).Append(",");
        sb.Append(collisionFlag ? "1" : "0").Append(",");
        sb.Append(sessionId).Append(",");

        // Adds the five sensor inputs.
        sb.Append(carSensors.front.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carSensors.frontLeft.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carSensors.left.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carSensors.frontRight.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carSensors.right.ToString("F4", CultureInfo.InvariantCulture)).Append(",");

        // Adds steering and throttle targets.
        sb.Append(carController.CurrentSteeringInput.ToString("F4", CultureInfo.InvariantCulture)).Append(",");
        sb.Append(carController.CurrentThrottleInput.ToString("F4", CultureInfo.InvariantCulture)).Append("\n");

        // Appends row to the end of the file.
        File.AppendAllText(filePath, sb.ToString());
    }
}
