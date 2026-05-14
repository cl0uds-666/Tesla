using UnityEngine;

public class AIDriver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CarSensors carSensors;
    [SerializeField] private CarController carController;
    [SerializeField] private NeuralTrainer neuralTrainer;

    [Header("AI")]
    [SerializeField] private bool useAIOnStart = false;

    private void Start()
    {
        // Starts in AI mode if this toggle is on.
        if (useAIOnStart)
        {
            EnableAI();
        }
    }

    private void FixedUpdate()
    {
        // Stop early if any required reference is missing.
        if (carController == null || carSensors == null || neuralTrainer == null)
        {
            return;
        }

        // Only drive the car from the network while AI mode is active.
        if (carController.CurrentControlMode != CarController.ControlMode.AI)
        {
            return;
        }

        // Wait until training has created a usable network.
        if (neuralTrainer.network == null)
        {
            return;
        }

        // Packs the five sensor rays into the NN input array.
        float[] inputs = new float[5]
        {
            carSensors.front,
            carSensors.frontLeft,
            carSensors.left,
            carSensors.frontRight,
            carSensors.right
        };

        // Runs one forward pass to predict steering and throttle.
        float[] outputs = neuralTrainer.network.FeedForward(inputs);

        float predictedSteering = outputs[0];
        float predictedThrottle = outputs[1];

        carController.SetAIInputs(predictedSteering, predictedThrottle);
    }

    public void EnableAI()
    {
        if (carController != null)
        {
            carController.SetControlMode(CarController.ControlMode.AI);
        }
    }

    public void DisableAI()
    {
        if (carController != null)
        {
            carController.SetControlMode(CarController.ControlMode.Human);
        }
    }
}
