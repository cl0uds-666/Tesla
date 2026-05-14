public class TrainingSample
{
    // Holds the 5 sensor distances used as network inputs.
    public float[] inputs;

    // Holds the target outputs: steering and throttle.
    public float[] outputs;

    public TrainingSample(float[] inputs, float[] outputs)
    {
        this.inputs = inputs;
        this.outputs = outputs;
    }
}
