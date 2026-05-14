using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class NeuralTrainer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CSVDataLoader dataLoader;

    [Header("Network Shape")]
    [SerializeField] private int inputSize = 5;
    [SerializeField] private int hidden1Size = 8;
    [SerializeField] private int hidden2Size = 6;
    [SerializeField] private int outputSize = 2;

    [Header("Training Settings")]
    [SerializeField] private float learningRate = 0.05f;
    [SerializeField] private int epochs = 200;
    [SerializeField] private bool trainOnStart = false;

    [Header("Dataset Balancing")]
    [SerializeField] private bool balanceStraightSamples = true;
    [SerializeField] private float straightSteeringThreshold = 0.1f;
    [Range(0f, 1f)]
    [SerializeField] private float straightSampleKeepPercentage = 0.25f;

    public NeuralNetwork network;
    public List<float> epochErrors = new List<float>();

    private void Start()
    {
        if (trainOnStart)
        {
            TrainNetwork();
        }
    }

    [ContextMenu("Train Network")]
    public void TrainNetwork()
    {
        if (dataLoader == null)
        {
            Debug.LogError("No CSVDataLoader assigned.");
            return;
        }

        if (dataLoader.samples == null || dataLoader.samples.Count == 0)
        {
            Debug.LogError("No training samples loaded.");
            return;
        }

        List<TrainingSample> trainingSamples = dataLoader.samples;

        if (balanceStraightSamples)
        {
            trainingSamples = BuildBalancedTrainingSet(dataLoader.samples);

            if (trainingSamples.Count == 0)
            {
                Debug.LogError("Dataset balancing removed all samples. Increase keep percentage or threshold.");
                return;
            }
        }

        network = new NeuralNetwork(inputSize, hidden1Size, hidden2Size, outputSize);
        epochErrors.Clear();

        Debug.Log("Training started...");

        for (int epoch = 0; epoch < epochs; epoch++)
        {
            float totalError = 0f;

            for (int i = 0; i < trainingSamples.Count; i++)
            {
                TrainingSample sample = trainingSamples[i];
                totalError += network.Train(sample.inputs, sample.outputs, learningRate);
            }

            float averageError = totalError / trainingSamples.Count;
            epochErrors.Add(averageError);

            Debug.Log($"Epoch {epoch + 1}/{epochs} - Error: {averageError:F6}");
        }

        SaveErrorLog();
        Debug.Log("Training complete.");
    }

    private List<TrainingSample> BuildBalancedTrainingSet(List<TrainingSample> originalSamples)
    {
        List<TrainingSample> balancedSamples = new List<TrainingSample>(originalSamples.Count);
        int removedStraightSamples = 0;

        Debug.Log($"[Balance] Original sample count: {originalSamples.Count}");
        Debug.Log($"[Balance] Steering distribution before filtering: {GetSteeringDistributionSummary(originalSamples)}");

        for (int i = 0; i < originalSamples.Count; i++)
        {
            TrainingSample sample = originalSamples[i];
            if (sample.outputs == null || sample.outputs.Length == 0)
            {
                continue;
            }

            float steer = sample.outputs[0];
            bool isStraight = Mathf.Abs(steer) <= straightSteeringThreshold;

            if (!isStraight)
            {
                balancedSamples.Add(sample);
                continue;
            }

            if (Random.value <= straightSampleKeepPercentage)
            {
                balancedSamples.Add(sample);
            }
            else
            {
                removedStraightSamples++;
            }
        }

        Debug.Log($"[Balance] Straight samples removed: {removedStraightSamples}");
        Debug.Log($"[Balance] Final sample count: {balancedSamples.Count}");
        Debug.Log($"[Balance] Steering distribution after filtering: {GetSteeringDistributionSummary(balancedSamples)}");

        return balancedSamples;
    }

    private string GetSteeringDistributionSummary(List<TrainingSample> samples)
    {
        List<float> steeringValues = samples
            .Where(sample => sample.outputs != null && sample.outputs.Length > 0)
            .Select(sample => sample.outputs[0])
            .ToList();

        if (steeringValues.Count == 0)
        {
            return "No valid steering samples.";
        }

        int hardLeft = steeringValues.Count(value => value < -0.3f);
        int slightLeft = steeringValues.Count(value => value >= -0.3f && value < -straightSteeringThreshold);
        int straight = steeringValues.Count(value => Mathf.Abs(value) <= straightSteeringThreshold);
        int slightRight = steeringValues.Count(value => value > straightSteeringThreshold && value <= 0.3f);
        int hardRight = steeringValues.Count(value => value > 0.3f);

        float min = steeringValues.Min();
        float max = steeringValues.Max();
        float averageAbs = steeringValues.Select(Mathf.Abs).Average();

        return $"count={steeringValues.Count}, min={min:F3}, max={max:F3}, avgAbs={averageAbs:F3}, bins[hardLeft={hardLeft}, slightLeft={slightLeft}, straight={straight}, slightRight={slightRight}, hardRight={hardRight}]";
    }

    private void SaveErrorLog()
    {
        string filePath = Path.Combine(Application.dataPath, "training_error_log.csv");

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("epoch,error");

        for (int i = 0; i < epochErrors.Count; i++)
        {
            sb.AppendLine($"{i + 1},{epochErrors[i]}");
        }

        File.WriteAllText(filePath, sb.ToString());
        Debug.Log("Training error log saved to: " + filePath);
    }
}
