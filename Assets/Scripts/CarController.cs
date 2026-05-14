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
    [SerializeField] private float acceleration = 12f;   // Push force when throttle is pressed.
    [SerializeField] private float maxSpeed = 10f;       // Forward speed cap.
    [SerializeField] private float reverseSpeed = 4f;    // Reverse speed cap.
    [SerializeField] private float dragOnGround = 2f;    // Fake tire/ground resistance.

    [Header("Turning")]
    [SerializeField] private float turnSpeed = 120f;     // How fast the car yaws left/right.
    [SerializeField] private float minSpeedForTurning = 0.5f; // Blocks spin-in-place at low speed.

    private Rigidbody rb;

    private float throttleInput; // Current forward/back input used this frame.
    private float steeringInput; // Current left/right input used this frame.
    private float aiThrottleInput;
    private float aiSteeringInput;

    [Header("Control")]
    [SerializeField] private ControlMode controlMode = ControlMode.Human;

    // Exposed so recorder/HUD/AI can read the live values.
    public float CurrentThrottleInput => throttleInput;
    public float CurrentSteeringInput => steeringInput;
    public ControlMode CurrentControlMode => controlMode;

    // Signed speed along the car's forward axis.
    public float CurrentSpeed => Vector3.Dot(rb.linearVelocity, transform.forward);

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (controlMode == ControlMode.Human)
        {
            // Human mode reads keyboard/controller axes directly.
            throttleInput = Input.GetAxisRaw("Vertical");
            steeringInput = Input.GetAxisRaw("Horizontal");
        }
        else
        {
            // AI mode reuses values pushed in by AIDriver.
            throttleInput = aiThrottleInput;
            steeringInput = aiSteeringInput;
        }
    }

    private void FixedUpdate()
    {
        // Keep physics work in FixedUpdate for stable movement.
        ApplyMovement();
        ApplyTurning();
        ApplyGroundDrag();
    }

    private void ApplyMovement()
    {
        float currentForwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);

        // Reverse has a smaller speed cap than forward.
        float allowedMaxSpeed = throttleInput >= 0f ? maxSpeed : reverseSpeed;

        // Still allow force while changing direction so braking feels responsive.
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

        // Do not rotate if the car is almost stopped.
        if (speedAmount < minSpeedForTurning)
        {
            return;
        }

        // Flip steering direction while reversing.
        float direction = Mathf.Sign(currentForwardSpeed);
        float turnAmount = steeringInput * turnSpeed * direction * Time.fixedDeltaTime;

        Quaternion turnRotation = Quaternion.Euler(0f, turnAmount, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    private void ApplyGroundDrag()
    {
        // Ignore Y so drag only affects horizontal motion.
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
        // Clamp predictions so bad outputs cannot exceed input range.
        aiSteeringInput = Mathf.Clamp(steering, -1f, 1f);
        aiThrottleInput = Mathf.Clamp(throttle, -1f, 1f);
    }
}
