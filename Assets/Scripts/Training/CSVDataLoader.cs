using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

// Loads CSV driving data and turns each valid row into a TrainingSample.
public class CSVDataLoader : MonoBehaviour
{
    [Header("CSV Settings")]
    [SerializeField] private bool loadOnStart = true; // Auto-load when play mode starts.
    [SerializeField] private bool loadSessionFiles = true; // Read every per-session recording file.
    [SerializeField] private string sessionFilePattern = "drive_session_*.csv"; // File pattern used for session recordings.
    [SerializeField] private string fallbackSingleFileName = "driving_data.csv"; // Old single-file fallback name.

    [Header("Optional Filters")]
    [SerializeField] private bool includeCollisionRows = true; // Skip crash rows when this is off.
    [SerializeField] private bool onlyUseHumanMode = false; // Keep only Human-driving rows.

    public List<TrainingSample> samples = new List<TrainingSample>(); // All parsed samples end up here.

    private const int LegacyColumnCount = 7;
    private const int MetadataColumnCount = 12;

    private void Start()
    {
        if (loadOnStart)
        {
            LoadCSV();
        }
    }

    [ContextMenu("Load CSV")]
    public void LoadCSV()
    {
        samples.Clear();

        string assetsPath = Application.dataPath;
        List<string> filesToLoad = new List<string>();

        if (loadSessionFiles)
        {
            filesToLoad.AddRange(Directory.GetFiles(assetsPath, sessionFilePattern, SearchOption.TopDirectoryOnly));
            filesToLoad.Sort();
        }

        if (filesToLoad.Count == 0)
        {
            string fallbackPath = Path.Combine(assetsPath, fallbackSingleFileName);
            if (File.Exists(fallbackPath))
            {
                filesToLoad.Add(fallbackPath);
            }
        }

        if (filesToLoad.Count == 0)
        {
            Debug.LogError($"No CSV files found. Looked for '{sessionFilePattern}' and fallback '{fallbackSingleFileName}' in: {assetsPath}");
            return;
        }

        int totalRowsProcessed = 0;

        for (int fileIndex = 0; fileIndex < filesToLoad.Count; fileIndex++)
        {
            string filePath = filesToLoad[fileIndex];
            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length <= 1)
            {
                Debug.LogWarning($"Skipping {Path.GetFileName(filePath)}: empty or header-only file.");
                continue;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                string[] values = line.Split(',');

                if (values.Length == LegacyColumnCount)
                {
                    if (TryParseLegacyRow(values, out TrainingSample sample))
                    {
                        samples.Add(sample);
                        totalRowsProcessed++;
                    }

                    continue;
                }

                if (values.Length == MetadataColumnCount)
                {
                    if (TryParseMetadataRow(values, out TrainingSample sampleWithMetadata))
                    {
                        samples.Add(sampleWithMetadata);
                        totalRowsProcessed++;
                    }

                    continue;
                }

                Debug.LogWarning($"Skipping invalid row {i + 1} in {Path.GetFileName(filePath)}: unsupported column count {values.Length}.");
            }
        }

        Debug.Log($"CSV loading complete. Files loaded: {filesToLoad.Count}, total training samples: {samples.Count}, parsed rows: {totalRowsProcessed}");
    }

    private bool TryParseLegacyRow(string[] values, out TrainingSample sample)
    {
        sample = null;

        float[] inputs = new float[5];
        float[] outputs = new float[2];

        for (int j = 0; j < 5; j++)
        {
            if (!float.TryParse(values[j], NumberStyles.Float, CultureInfo.InvariantCulture, out inputs[j]))
            {
                return false;
            }
        }

        for (int j = 0; j < 2; j++)
        {
            if (!float.TryParse(values[j + 5], NumberStyles.Float, CultureInfo.InvariantCulture, out outputs[j]))
            {
                return false;
            }
        }

        sample = new TrainingSample(inputs, outputs);
        return true;
    }

    private bool TryParseMetadataRow(string[] values, out TrainingSample sample)
    {
        sample = null;

        string mode = values[2].Trim();
        bool collision = values[3].Trim() == "1";

        if (!includeCollisionRows && collision)
        {
            return false;
        }

        if (onlyUseHumanMode && mode != "Human")
        {
            return false;
        }

        float[] inputs = new float[5];
        float[] outputs = new float[2];

        // Only keep the five sensor fields as model inputs.
        int sensorStartIndex = 5;
        for (int j = 0; j < 5; j++)
        {
            if (!float.TryParse(values[sensorStartIndex + j], NumberStyles.Float, CultureInfo.InvariantCulture, out inputs[j]))
            {
                return false;
            }
        }

        if (!float.TryParse(values[10], NumberStyles.Float, CultureInfo.InvariantCulture, out outputs[0]))
        {
            return false;
        }

        if (!float.TryParse(values[11], NumberStyles.Float, CultureInfo.InvariantCulture, out outputs[1]))
        {
            return false;
        }

        sample = new TrainingSample(inputs, outputs);
        return true;
    }
}
