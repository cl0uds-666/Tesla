public class TrainingSample
{
    // 5 sensor inputs fed into the network.
    public float[] inputs;

    // Target outputs: steering and throttle.
    public float[] outputs;

    // Simple container for one training row.
    public TrainingSample(float[] inputs, float[] outputs)
    {
        this.inputs = inputs;
        this.outputs = outputs;
    }
}
