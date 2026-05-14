using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

// Loads training data from a CSV file and converts it into TrainingSample objects
public class CSVDataLoader : MonoBehaviour
{
    [Header("CSV Settings")]
    [SerializeField] private string fileName = "driving_data.csv"; // Name of the CSV file
    [SerializeField] private bool loadOnStart = true; // Automatically load on Start

    public List<TrainingSample> samples = new List<TrainingSample>(); // Stores all loaded samples

    private void Start()
    {
        // Optionally load data when the scene starts
        if (loadOnStart)
        {
            LoadCSV();
        }
    }

    // Allows manual triggering from the Unity Inspector
    [ContextMenu("Load CSV")]
    public void LoadCSV()
    {
        samples.Clear(); // Clear any existing data before loading

        // Build full file path
        string filePath = Path.Combine(Application.dataPath, fileName);

        // Check if file exists
        if (!File.Exists(filePath))
        {
            Debug.LogError("CSV file not found at: " + filePath);
            return;
        }

        // Read all lines from the CSV
        string[] lines = File.ReadAllLines(filePath);

        // Check if file has data (ignoring header)
        if (lines.Length <= 1)
        {
            Debug.LogWarning("CSV file is empty or only contains header.");
            return;
        }

        // Start from index 1 to skip header row
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();

            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            // Split CSV row into individual values
            string[] values = line.Split(',');

            // Expect exactly 7 values (5 inputs + 2 outputs)
            if (values.Length != 7)
            {
                Debug.LogWarning($"Skipping invalid row {i + 1}: expected 7 values, got {values.Length}");
                continue;
            }

            float[] inputs = new float[5];  // Sensor data
            float[] outputs = new float[2]; // Player actions

            bool parseFailed = false;

            // Parse input values (sensor data)
            for (int j = 0; j < 5; j++)
            {
                if (!float.TryParse(values[j], NumberStyles.Float, CultureInfo.InvariantCulture, out inputs[j]))
                {
                    parseFailed = true;
                    break;
                }
            }

            // Parse output values (steering + throttle)
            for (int j = 0; j < 2; j++)
            {
                if (!float.TryParse(values[j + 5], NumberStyles.Float, CultureInfo.InvariantCulture, out outputs[j]))
                {
                    parseFailed = true;
                    break;
                }
            }

            // Skip row if any value failed to parse
            if (parseFailed)
            {
                Debug.LogWarning($"Skipping row {i + 1}: failed to parse one or more values.");
                continue;
            }

            // Create training sample and add to dataset
            samples.Add(new TrainingSample(inputs, outputs));
        }

        Debug.Log($"CSV loaded successfully. Total samples: {samples.Count}");
    }
}