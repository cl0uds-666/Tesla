using System;

public class NeuralNetwork
{
    private int inputSize;
    private int hidden1Size;
    private int hidden2Size;
    private int outputSize;

    private float[,] weightsInputHidden1;
    private float[,] weightsHidden1Hidden2;
    private float[,] weightsHidden2Output;

    private float[] biasHidden1;
    private float[] biasHidden2;
    private float[] biasOutput;

    private float[] hiddenLayer1;
    private float[] hiddenLayer2;
    private float[] outputLayer;

    private Random random;

    public NeuralNetwork(int inputSize, int hidden1Size, int hidden2Size, int outputSize)
    {
        this.inputSize = inputSize;
        this.hidden1Size = hidden1Size;
        this.hidden2Size = hidden2Size;
        this.outputSize = outputSize;

        random = new Random();

        // Allocate weights and biases for each layer connection.
        weightsInputHidden1 = new float[inputSize, hidden1Size];
        weightsHidden1Hidden2 = new float[hidden1Size, hidden2Size];
        weightsHidden2Output = new float[hidden2Size, outputSize];

        biasHidden1 = new float[hidden1Size];
        biasHidden2 = new float[hidden2Size];
        biasOutput = new float[outputSize];

        hiddenLayer1 = new float[hidden1Size];
        hiddenLayer2 = new float[hidden2Size];
        outputLayer = new float[outputSize];

        InitialiseWeights();
    }

    private void InitialiseWeights()
    {
        // Fill all weights with small random values.
        for (int i = 0; i < inputSize; i++)
            for (int j = 0; j < hidden1Size; j++)
                weightsInputHidden1[i, j] = RandomWeight();

        for (int i = 0; i < hidden1Size; i++)
            for (int j = 0; j < hidden2Size; j++)
                weightsHidden1Hidden2[i, j] = RandomWeight();

        for (int i = 0; i < hidden2Size; i++)
            for (int j = 0; j < outputSize; j++)
                weightsHidden2Output[i, j] = RandomWeight();

        for (int i = 0; i < hidden1Size; i++)
            biasHidden1[i] = RandomWeight();

        for (int i = 0; i < hidden2Size; i++)
            biasHidden2[i] = RandomWeight();

        for (int i = 0; i < outputSize; i++)
            biasOutput[i] = RandomWeight();
    }

    private float RandomWeight()
    {
        return (float)(random.NextDouble() * 2.0 - 1.0) * 0.5f;
    }

    public float[] FeedForward(float[] inputs)
    {
        if (inputs.Length != inputSize)
        {
            throw new ArgumentException($"Expected {inputSize} inputs, got {inputs.Length}");
        }

        // Layer 1 forward pass.
        for (int j = 0; j < hidden1Size; j++)
        {
            float sum = biasHidden1[j];
            for (int i = 0; i < inputSize; i++)
                sum += inputs[i] * weightsInputHidden1[i, j];

            hiddenLayer1[j] = Sigmoid(sum);
        }

        // Layer 2 forward pass.
        for (int j = 0; j < hidden2Size; j++)
        {
            float sum = biasHidden2[j];
            for (int i = 0; i < hidden1Size; i++)
                sum += hiddenLayer1[i] * weightsHidden1Hidden2[i, j];

            hiddenLayer2[j] = Sigmoid(sum);
        }

        // Output layer forward pass.
        for (int j = 0; j < outputSize; j++)
        {
            float sum = biasOutput[j];
            for (int i = 0; i < hidden2Size; i++)
                sum += hiddenLayer2[i] * weightsHidden2Output[i, j];

            outputLayer[j] = Tanh(sum);
        }

        float[] result = new float[outputSize];
        Array.Copy(outputLayer, result, outputSize);
        return result;
    }

    public float Train(float[] inputs, float[] expectedOutputs, float learningRate)
    {
        // First get current prediction.
        float[] predictedOutputs = FeedForward(inputs);

        float[] outputErrors = new float[outputSize];
        float[] outputDeltas = new float[outputSize];
        float[] hidden2Errors = new float[hidden2Size];
        float[] hidden2Deltas = new float[hidden2Size];
        float[] hidden1Errors = new float[hidden1Size];
        float[] hidden1Deltas = new float[hidden1Size];

        float sampleError = 0f;

        // Output error + delta.
        for (int i = 0; i < outputSize; i++)
        {
            float error = expectedOutputs[i] - predictedOutputs[i];
            outputErrors[i] = error;
            outputDeltas[i] = error * TanhDerivative(predictedOutputs[i]);
            sampleError += error * error;
        }

        // Backprop into hidden layer 2.
        for (int i = 0; i < hidden2Size; i++)
        {
            float error = 0f;
            for (int j = 0; j < outputSize; j++)
                error += outputDeltas[j] * weightsHidden2Output[i, j];

            hidden2Errors[i] = error;
            hidden2Deltas[i] = error * SigmoidDerivative(hiddenLayer2[i]);
        }

        // Backprop into hidden layer 1.
        for (int i = 0; i < hidden1Size; i++)
        {
            float error = 0f;
            for (int j = 0; j < hidden2Size; j++)
                error += hidden2Deltas[j] * weightsHidden1Hidden2[i, j];

            hidden1Errors[i] = error;
            hidden1Deltas[i] = error * SigmoidDerivative(hiddenLayer1[i]);
        }

        // Gradient step: hidden2 -> output weights.
        for (int i = 0; i < hidden2Size; i++)
            for (int j = 0; j < outputSize; j++)
                weightsHidden2Output[i, j] += learningRate * outputDeltas[j] * hiddenLayer2[i];

        // Gradient step: output biases.
        for (int i = 0; i < outputSize; i++)
            biasOutput[i] += learningRate * outputDeltas[i];

        // Gradient step: hidden1 -> hidden2 weights.
        for (int i = 0; i < hidden1Size; i++)
            for (int j = 0; j < hidden2Size; j++)
                weightsHidden1Hidden2[i, j] += learningRate * hidden2Deltas[j] * hiddenLayer1[i];

        // Gradient step: hidden2 biases.
        for (int i = 0; i < hidden2Size; i++)
            biasHidden2[i] += learningRate * hidden2Deltas[i];

        // Gradient step: input -> hidden1 weights.
        for (int i = 0; i < inputSize; i++)
            for (int j = 0; j < hidden1Size; j++)
                weightsInputHidden1[i, j] += learningRate * hidden1Deltas[j] * inputs[i];

        // Gradient step: hidden1 biases.
        for (int i = 0; i < hidden1Size; i++)
            biasHidden1[i] += learningRate * hidden1Deltas[i];

        // Return per-sample mean squared error.
        return sampleError / outputSize;
    }

    private float Sigmoid(float x) => 1f / (1f + (float)Math.Exp(-x));

    private float SigmoidDerivative(float output) => output * (1f - output);

    private float Tanh(float x) => (float)Math.Tanh(x);

    private float TanhDerivative(float output) => 1f - (output * output);
}
