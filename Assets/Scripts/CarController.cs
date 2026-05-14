using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    public enum ControlMode
    {
        Human,
        AI
    }

    [Header("Movement")]
    [SerializeField] private float acceleration = 12f; // Push force for throttle input.
    [SerializeField] private float maxSpeed = 10f; // Max forward speed.
    [SerializeField] private float reverseSpeed = 4f; // Max reverse speed.
    [SerializeField] private float dragOnGround = 2f; // Flat drag to stop endless sliding.

    [Header("Turning")]
    [SerializeField] private float turnSpeed = 120f; // Yaw speed while moving.
    [SerializeField] private float minSpeedForTurning = 0.5f; // Stops spin-in-place behavior.

    private Rigidbody rb;

    private float throttleInput; // Final throttle used by physics.
    private float steeringInput; // Final steering used by physics.
    private float aiThrottleInput;
    private float aiSteeringInput;

    [Header("Control")]
    [SerializeField] private ControlMode controlMode = ControlMode.Human;

    // Expose live control values to recorder/HUD.
    public float CurrentThrottleInput => throttleInput;
    public float CurrentSteeringInput => steeringInput;
    public ControlMode CurrentControlMode => controlMode;

    // Signed speed in the car's forward axis.
    public float CurrentSpeed => Vector3.Dot(rb.linearVelocity, transform.forward);

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (controlMode == ControlMode.Human)
        {
            // Read player controls directly.
            throttleInput = Input.GetAxisRaw("Vertical");
            steeringInput = Input.GetAxisRaw("Horizontal");
        }
        else
        {
            // AI controls come from AIDriver.
            throttleInput = aiThrottleInput;
            steeringInput = aiSteeringInput;
        }
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyTurning();
        ApplyGroundDrag();
    }

    private void ApplyMovement()
    {
        float currentForwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);

        // Use different speed cap for reverse.
        float allowedMaxSpeed = throttleInput >= 0f ? maxSpeed : reverseSpeed;

        // Allow acceleration if under cap or trying to change direction.
        if (Mathf.Abs(currentForwardSpeed) < allowedMaxSpeed || Mathf.Sign(throttleInput) != Mathf.Sign(currentForwardSpeed))
        {
            Vector3 force = transform.forward * throttleInput * acceleration;
            rb.AddForce(force, ForceMode.Acceleration);
        }
    }

    private void ApplyTurning()
    {
        float currentForwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float speedAmount = Mathf.Abs(currentForwardSpeed);

        if (speedAmount < minSpeedForTurning)
        {
            return;
        }

        // Flip steering while reversing so controls feel natural.
        float direction = Mathf.Sign(currentForwardSpeed);
        float turnAmount = steeringInput * turnSpeed * direction * Time.fixedDeltaTime;

        Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    private void ApplyGroundDrag()
    {
        // Ignore Y so drag only affects ground-plane motion.
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        Vector3 dragForce = -flatVelocity * dragOnGround;
        rb.AddForce(dragForce, ForceMode.Acceleration);
    }

    public void SetControlMode(ControlMode mode)
    {
        controlMode = mode;
    }

    public void SetAIInputs(float steering, float throttle)
    {
        // Clamp for safety before applying AI outputs.
        aiSteeringInput = Mathf.Clamp(steering, -1f, 1f);
        aiThrottleInput = Mathf.Clamp(throttle, -1f, 1f);
    }
}
