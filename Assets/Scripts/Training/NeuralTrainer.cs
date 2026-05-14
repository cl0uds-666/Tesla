//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using UnityEngine;

//public class NeuralTrainer : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] private CSVDataLoader dataLoader;

//    [Header("Network Shape")]
//    [SerializeField] private int inputSize = 5;
//    [SerializeField] private int hidden1Size = 8;
//    [SerializeField] private int hidden2Size = 6;
//    [SerializeField] private int outputSize = 2;

//    [Header("Training Settings")]
//    [SerializeField] private float learningRate = 0.05f;
//    [SerializeField] private int epochs = 200;
//    [SerializeField] private bool trainOnStart = false;

//    public NeuralNetwork network;
//    public List<float> epochErrors = new List<float>();

//    private void Start()
//    {
//        if (trainOnStart)
//        {
//            TrainNetwork();
//        }
//    }

//    [ContextMenu("Train Network")]
//    public void TrainNetwork()
//    {
//        if (dataLoader == null)
//        {
//            Debug.LogError("No CSVDataLoader assigned.");
//            return;
//        }

//        if (dataLoader.samples == null || dataLoader.samples.Count == 0)
//        {
//            Debug.LogError("No training samples loaded.");
//            return;
//        }

//        network = new NeuralNetwork(inputSize, hidden1Size, hidden2Size, outputSize);
//        epochErrors.Clear();

//        Debug.Log("Training started...");

//        for (int epoch = 0; epoch < epochs; epoch++)
//        {
//            float totalError = 0f;

//            for (int i = 0; i < dataLoader.samples.Count; i++)
//            {
//                TrainingSample sample = dataLoader.samples[i];
//                totalError += network.Train(sample.inputs, sample.outputs, learningRate);
//            }

//            float averageError = totalError / dataLoader.samples.Count;
//            epochErrors.Add(averageError);

//            Debug.Log($"Epoch {epoch + 1}/{epochs} - Error: {averageError:F6}");
//        }

//        SaveErrorLog();
//        Debug.Log("Training complete.");
//    }

//    private void SaveErrorLog()
//    {
//        string filePath = Path.Combine(Application.dataPath, "training_error_log.csv");

//        StringBuilder sb = new StringBuilder();
//        sb.AppendLine("epoch,error");

//        for (int i = 0; i < epochErrors.Count; i++)
//        {
//            sb.AppendLine($"{i + 1},{epochErrors[i]}");
//        }

//        File.WriteAllText(filePath, sb.ToString());
//        Debug.Log("Training error log saved to: " + filePath);
//    }
//}