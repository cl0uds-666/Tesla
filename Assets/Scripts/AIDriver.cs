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
        if (useAIOnStart)
        {
            EnableAI();
        }
    }

    private void FixedUpdate()
    {
        if (carController == null || carSensors == null || neuralTrainer == null)
        {
            return;
        }

        if (carController.CurrentControlMode != CarController.ControlMode.AI)
        {
            return;
        }

        if (neuralTrainer.network == null)
        {
            return;
        }

        float[] inputs = new float[5]
        {
            carSensors.front,
            carSensors.frontLeft,
            carSensors.left,
            carSensors.frontRight,
            carSensors.right
        };

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
