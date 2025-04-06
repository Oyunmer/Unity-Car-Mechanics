using UnityEngine;

public class CarMovement : MonoBehaviour
{
    [Header("Motor ve fren")]
    [SerializeField] private float motorForce = 1500f;
    [SerializeField] private float brakeForce = 3000f;
    [SerializeField] private float maxSteerAngle = 30f;
    [SerializeField] private float maxSpeed = 200f;

    [Header("Süspansiyon ve şasi")]
    [SerializeField] private float suspensionDistance = 0.1f;
    [SerializeField] private float suspensionSpring = 10000f;
    [SerializeField] private float suspensionDamper = 450f;

    [Header("Zemin")]
    [SerializeField] private float frictionCoefficient = 1f;

    // WheelColliders
    [SerializeField] private WheelCollider frontLeftWheel, frontRightWheel;
    [SerializeField] private WheelCollider rearLeftWheel, rearRightWheel;

    // Transforms
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    private float horizontalInput, verticalInput;
    private bool isBraking;
    private float currentSpeed;

    // Helpers
    private float currentSteerAngle;
    private float currentBrakeForce;

    private void Start()
    {
        
    }

    private void Update()
    {
        GetInput();
        UpdateWheelPositions();
    }

    private void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        HandleBraking();
        HandleSuspension();
        ApplyFriction();
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBraking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor()
    {
        currentSpeed = (frontLeftWheel.rpm + rearLeftWheel.rpm) / 2 * 60 / 1000 * Mathf.PI * frontLeftWheel.radius; // Hızı hesapla

        if (currentSpeed < maxSpeed)
        {
            // Forward
            frontLeftWheel.motorTorque = verticalInput * motorForce;
            frontRightWheel.motorTorque = verticalInput * motorForce;
        }
        else
        {
            // Motor maxa gelince dursun baba
            frontLeftWheel.motorTorque = 0;
            frontRightWheel.motorTorque = 0;
        }
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheel.steerAngle = currentSteerAngle;
        frontRightWheel.steerAngle = currentSteerAngle;
    }

    private void HandleBraking()
    {
        currentBrakeForce = isBraking ? brakeForce : 0f;
        ApplyBrakes();
    }

    private void ApplyBrakes()
    {
        frontLeftWheel.brakeTorque = currentBrakeForce;
        frontRightWheel.brakeTorque = currentBrakeForce;
        rearLeftWheel.brakeTorque = currentBrakeForce;
        rearRightWheel.brakeTorque = currentBrakeForce;
    }

    // Süspansiyon ve şasi etkileşimi
    private void HandleSuspension()
    {
        ApplySuspension(frontLeftWheel);
        ApplySuspension(frontRightWheel);
        ApplySuspension(rearLeftWheel);
        ApplySuspension(rearRightWheel);
    }

    // Süspansiyon kuvveti
    private void ApplySuspension(WheelCollider wheel)
    {
        JointSpring spring = wheel.suspensionSpring;
        spring.spring = suspensionSpring;
        spring.damper = suspensionDamper;
        spring.targetPosition = suspensionDistance;
        wheel.suspensionSpring = spring;
    }

    private void UpdateWheelPositions()
    {
        UpdateSingleWheel(frontLeftWheel, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheel, frontRightWheelTransform);
        UpdateSingleWheel(rearLeftWheel, rearLeftWheelTransform);
        UpdateSingleWheel(rearRightWheel, rearRightWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    private void ApplyFriction()
    {
        WheelHit wheelHit;
        if (frontLeftWheel.GetGroundHit(out wheelHit))
        {
            // Zemine temas ve friktion
            Vector3 forward = transform.forward;
            Vector3 groundNormal = wheelHit.normal;
            float angle = Vector3.Angle(forward, groundNormal);
            float friction = Mathf.Lerp(frictionCoefficient, frictionCoefficient * 2, angle / 90f);
            frontLeftWheel.sidewaysFriction = new WheelFrictionCurve
            {
                stiffness = friction
            };
        }
    }
}
