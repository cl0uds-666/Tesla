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
    [SerializeField] private float acceleration = 12f;   // Force applied when accelerating
    [SerializeField] private float maxSpeed = 10f;       // Forward speed limit
    [SerializeField] private float reverseSpeed = 4f;    // Reverse speed limit
    [SerializeField] private float dragOnGround = 2f;    // Simulated ground resistance

    [Header("Turning")]
    [SerializeField] private float turnSpeed = 120f;     // Turning speed 
    [SerializeField] private float minSpeedForTurning = 0.5f; // Prevents turning when nearly stationary

    private Rigidbody rb;

    private float throttleInput; // Forward/backward input
    private float steeringInput; // Left/right input
    private float aiThrottleInput;
    private float aiSteeringInput;

    [Header("Control")]
    [SerializeField] private ControlMode controlMode = ControlMode.Human;

    // Expose inputs so other systems can read them
    public float CurrentThrottleInput => throttleInput;
    public float CurrentSteeringInput => steeringInput;
    public ControlMode CurrentControlMode => controlMode;

    // Forward speed relative to the cars direction
    public float CurrentSpeed => Vector3.Dot(rb.linearVelocity, transform.forward);

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (controlMode == ControlMode.Human)
        {
            // Read raw player input (no smoothing)
            throttleInput = Input.GetAxisRaw("Vertical");
            steeringInput = Input.GetAxisRaw("Horizontal");
        }
        else
        {
            // AI inputs are assigned by AIDriver
            throttleInput = aiThrottleInput;
            steeringInput = aiSteeringInput;
        }
    }

    private void FixedUpdate()
    {
        // Physics based movement handled in FixedUpdate
        ApplyMovement();
        ApplyTurning();
        ApplyGroundDrag();
    }

    private void ApplyMovement()
    {
        // Speed in the cars forward direction
        float currentForwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);

        // Different speed limits for forward and reverse
        float allowedMaxSpeed = throttleInput >= 0f ? maxSpeed : reverseSpeed;

        // Only apply force if under speed limit or changing direction
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

        // Prevent turning when almost stationary
        if (speedAmount < minSpeedForTurning)
        {
            return;
        }

        // Adjust turning based on movement direction (forward vs reverse)
        float direction = Mathf.Sign(currentForwardSpeed);
        float turnAmount = steeringInput * turnSpeed * direction * Time.fixedDeltaTime;

        Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    private void ApplyGroundDrag()
    {
        // Remove vertical movement to only apply drag on the ground plane
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Apply opposing force to simulate friction
        Vector3 dragForce = -flatVelocity * dragOnGround;

        rb.AddForce(dragForce, ForceMode.Acceleration);
    }

    public void SetControlMode(ControlMode mode)
    {
        controlMode = mode;
    }

    public void SetAIInputs(float steering, float throttle)
    {
        aiSteeringInput = Mathf.Clamp(steering, -1f, 1f);
        aiThrottleInput = Mathf.Clamp(throttle, -1f, 1f);
    }
}
