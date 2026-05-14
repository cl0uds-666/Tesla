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
        // Lets us boot straight into AI mode for demos.
        if (useAIOnStart)
        {
            EnableAI();
        }
    }

    private void FixedUpdate()
    {
        // Stop early if references are missing.
        if (carController == null || carSensors == null || neuralTrainer == null)
        {
            return;
        }

        // Only drive when the car is in AI mode.
        if (carController.CurrentControlMode != CarController.ControlMode.AI)
        {
            return;
        }

        // No network yet means no prediction.
        if (neuralTrainer.network == null)
        {
            return;
        }

        // Pack the 5 sensor values into the network input array.
        float[] inputs = new float[5]
        {
            carSensors.front,
            carSensors.frontLeft,
            carSensors.left,
            carSensors.frontRight,
            carSensors.right
        };

        // Run one forward pass and read steer/throttle.
        float[] outputs = neuralTrainer.network.FeedForward(inputs);

        float predictedSteering = outputs[0];
        float predictedThrottle = outputs[1];

        // Send the predicted controls into the car.
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
