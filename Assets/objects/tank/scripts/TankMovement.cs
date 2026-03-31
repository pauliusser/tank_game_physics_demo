// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.AI;
// using System.Collections;

// public class TankMovement : MonoBehaviour
// {
//     public bool isPlayer = false;
//     public Transform navTarget;
//     public float updateSpeed = 0.1f;
    
//     [Header("Input Actions")]
//     public InputAction moveAction;
    
//     [Header("Tracks")]
//     public GameObject viksraiK;
//     public GameObject viksraiD;
//     private float tracksSpeed;
//     public float trackSpeedCmPerSecond = 30f;
//     private float deltaDistanceK;
//     private float deltaDistanceD;
//     public Rigidbody rbSuspension;
//     public int contactsCount = 0;
//     private int normalsCount = 0;
//     public float turnMultiplier = 1f;
//     private Track trackK;
//     private Track trackD;
//     private Vector2 inpValue;
//     private Vector3 posSum;
//     private Vector3 velSum;
//     private Vector3 norSum;
//     private Vector3 angularMomentumSum;
//     private float speedInPercentK;
//     private float speedInPercentD;
//     private float velocity;
//     private Vector3 tankVelocity;
//     private Vector3 contactsAvgCenter;
//     private Vector3 angularVelocity;
//     private float totalTankMass = 0f;
//     private Vector3 commonCenterOfMass;
//     private FrictionCalculator fc;
//     private float linearFrictionCoef = 1f;
//     private bool isBackingUp;

//     [Header("Tank Physics Properties")]
//     public Vector3 centerOfMass = Vector3.zero;
//     public float mass = 100f;
//     [Header("Track Physics")]
//     public float maxTrackAcceleration = 5f;        // How fast tracks can change speed (units per second)
//     private float currentTrackSpeedK = 0f;          // Current speed of left track (0-1)
//     private float currentTrackSpeedD = 0f;          // Current speed of right track (0-1)      // Max speed change per second

//     [Header("AI Settings")]
//     public float stoppingDistance = 2f;
//     public float facingThreshold = 5f;          // angle (degrees) at which we consider "on course"
//     public float turnInPlaceAngle = 20f; 
    
//     [Header("stuck recovery settings")]
//     public BumperTrigger frontBumper;
//     public float backupDuration = 0.2f;
//     private float backupTimer = 0.2f;
//     public float wallDetectionDistance = 0.7f;
//     public LayerMask groundLayerMask;

//     private Vector2 aiInputValue;
//     private NavMeshPath aiPath; // for debug visualization

//     private void OnEnable() => moveAction.Enable();
//     private void OnDisable() => moveAction.Disable();
// // MARK: start
//     void Start()
//     {
//         trackK = viksraiK.GetComponent<Track>();
//         trackD = viksraiD.GetComponent<Track>();
//         fc = GetComponent<FrictionCalculator>();
//         rbSuspension.angularDamping = 5f;

//         angularMomentumSum = Vector3.zero;

//         Rigidbody[] allParts = gameObject.GetComponentsInChildren<Rigidbody>();
//         foreach (var part in allParts)
//         {
//             totalTankMass += part.mass;
//         }

//         StartCoroutine(FollowTarget());
//     }
// // MARK: upd
//     void Update()
//     {
//         if (isPlayer)
//         {
//             Vector2 inpV = moveAction.ReadValue<Vector2>();
//             float inpX = Mathf.Round(inpV.x);
//             float inpY = Mathf.Round(inpV.y);
//             inpValue = new Vector2(inpX, inpY);
//         }
//         else
//         {
//             // AI input with wall avoidance
//             if (isBackingUp)
//             {
//                 // Continue reversing with same steering
//                 backupTimer -= Time.deltaTime;
//                 if (backupTimer <= 0f)
//                 {
//                     isBackingUp = false;
//                 }
                
//                 // Keep the same steering, force reverse
//                 inpValue = new Vector2(aiInputValue.x, -1f);
//             }
//             else
//             {
//                 if (frontBumper.isTouchingWall)
//                 {
//                     // Wall detected! Start backing up
//                     isBackingUp = true;
//                     backupTimer = backupDuration;
                    
//                     // Reverse while keeping original steering
//                     inpValue = new Vector2(aiInputValue.x, -1f);
                    
//                     Debug.Log("Wall detected! Backing up...");
//                 }
//                 else
//                 {
//                     // No wall, use normal AI input
//                     inpValue = aiInputValue;
//                 }
//             }
//         }
//     }
// // MARK: fixed upd
//     void FixedUpdate()
//     {
//         float scaleCompensation = 0.1f;
//         deltaDistanceK = trackSpeedCmPerSecond * scaleCompensation * Time.fixedDeltaTime;
//         speedInPercentK = deltaDistanceK / trackK.totalLen * 100f;
//         deltaDistanceD = trackSpeedCmPerSecond * scaleCompensation * Time.fixedDeltaTime;
//         speedInPercentD = deltaDistanceD / trackD.totalLen * 100f;

//         contactsCount = 0;
//         normalsCount = 0;
//         posSum = Vector3.zero;
//         velSum = Vector3.zero;
//         norSum = Vector3.zero;
//         angularMomentumSum = Vector3.zero;

//         processTrack(trackK);
//         processTrack(trackD);

//         if (contactsCount > 0 && rbSuspension != null)
//         {
//             Vector3 avgSurfaceNormal = norSum.normalized;
//             contactsAvgCenter = posSum / contactsCount;
//             tankVelocity = velSum / contactsCount;

//             Vector3 netForce = CalculateNetForce(
//                 mass: totalTankMass,
//                 gravity: Physics.gravity,
//                 currentVelocity: rbSuspension.linearVelocity,
//                 desiredVelocity: tankVelocity / Time.fixedDeltaTime,
//                 surfaceNormal: avgSurfaceNormal
//             );

//             rbSuspension.AddForceAtPosition(netForce, contactsAvgCenter, ForceMode.Force);

//             commonCenterOfMass = CalculateTotalCenterOfMass();

//             int maxCont = 18;
//             int minCont = 9;
//             float cancelationStr = Mathf.Clamp01((float)(normalsCount - minCont) / (maxCont - minCont));
//             linearFrictionCoef = cancelationStr;

//             if (normalsCount > minCont)
//                 CancelGravityTorque(commonCenterOfMass, contactsAvgCenter, avgSurfaceNormal, cancelationStr);

//             angularVelocity = CalculateAngularVelocity(angularMomentumSum);
//             Vector3 calculatedAngularVelocity = angularVelocity / Time.fixedDeltaTime * turnMultiplier;
//             rbSuspension.angularVelocity = calculatedAngularVelocity;
//         }
//     }
// // MARK: folow target
//     private IEnumerator FollowTarget()
//     {
//         WaitForSeconds wait = new WaitForSeconds(updateSpeed);
//         NavMeshPath path = new NavMeshPath();
//         NavMeshHit startHit, targetHit;

//         while (enabled)
//         {
//             if (!isPlayer && navTarget != null)
//             {
//                 // Sample start position on NavMesh
//                 Vector3 startPos = transform.position;
//                 if (NavMesh.SamplePosition(startPos, out startHit, 2f, NavMesh.AllAreas))
//                     startPos = startHit.position;

//                 // Sample target position on NavMesh
//                 Vector3 targetPos = navTarget.position;
//                 if (NavMesh.SamplePosition(targetPos, out targetHit, 2f, NavMesh.AllAreas))
//                     targetPos = targetHit.position;

//                 if (NavMesh.CalculatePath(startPos, targetPos, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
//                 {
//                     aiPath = path;
                    
//                     Vector3 nextTarget = transform.position;
//                     if (path.corners.Length > 1)
//                         nextTarget = path.corners[1];
//                     else if (path.corners.Length == 1)
//                         nextTarget = path.corners[0];

//                     aiInputValue = CalculateAIMovement(nextTarget);
//                 }
//                 else
//                 {
//                     aiPath = null;
//                     aiInputValue = CalculateAIMovement(navTarget.position);
//                 }
//             }
//             yield return wait;
//         }
//     }
// // MARK: AI movement
//     private Vector2 CalculateAIMovement(Vector3 nextCorner)
//     {
//         float distanceToFinal = navTarget != null ? Vector3.Distance(transform.position, navTarget.position) : float.MaxValue;
//         float distanceToNext = Vector3.Distance(transform.position, nextCorner);
        
//         // FLATTEN to horizontal plane for stable angle calculation
//         Vector3 tankForwardFlat = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
//         Vector3 directionToNextFlat = Vector3.ProjectOnPlane((nextCorner - transform.position), Vector3.up).normalized;
        
//         // Now this is stable - using world up axis on flattened vectors
//         float angle = Vector3.SignedAngle(tankForwardFlat, directionToNextFlat, Vector3.up);
//         float absAngle = Mathf.Abs(angle);
        
//         // Dot products still use full 3D for slope climbing
//         float forwardDot = Vector3.Dot(transform.forward, directionToNextFlat);
//         float rightDot = Vector3.Dot(transform.right, directionToNextFlat);
        
//         float moveDirection = 0f;
//         float turnDirection = 0f;
        
//         // Turn logic based on flattened angle
//         if (distanceToNext < 1f)
//         {
//             turnDirection = Mathf.Clamp(angle / 90f, -1f, 1f);
//         }
//         else
//         {
//             float turn = Mathf.Clamp(angle / 90f, -1f, 1f);
//             turnDirection = Mathf.Lerp(0f, turn, distanceToNext);
//         }
        
//         if (distanceToFinal > stoppingDistance)
//         {
//             float alignmentFactor = Mathf.Clamp01(1f - (absAngle / 90f));
            
//             if (absAngle > turnInPlaceAngle && distanceToNext > 2f)
//             {
//                 moveDirection = 0.01f;
//             }
//             else 
//             {
//                 float minSpeed = 0.05f;
//                 moveDirection = Mathf.Lerp(minSpeed, 1f, alignmentFactor);
//             }
//         }
//         else
//         {
//             turnDirection = 0f;
//             moveDirection = 0f;
//         }
        
//         return new Vector2(turnDirection, moveDirection);
//     }
// // MARK: center of mass
//     Vector3 CalculateTotalCenterOfMass()
//     {
//         Rigidbody[] allRigidbodies = GetComponentsInChildren<Rigidbody>();
//         float totalMass = 0f;
//         Vector3 weightedPositionSum = Vector3.zero;

//         foreach (Rigidbody rb in allRigidbodies)
//         {
//             totalMass += rb.mass;
//             weightedPositionSum += rb.worldCenterOfMass * rb.mass;
//         }

//         if (totalMass > 0)
//             return weightedPositionSum / totalMass;
//         else
//             return transform.position;
//     }
// // MARK: cancel g torq
//     void CancelGravityTorque(Vector3 com, Vector3 rotationCenter, Vector3 surfaceNormal, float strength = 0.8f)
//     {
//         Vector3 centerOfMass = com - Vector3.Project(com - rotationCenter, rbSuspension.transform.forward);
//         Vector3 leverArm = centerOfMass - rotationCenter;

//         Vector3 gravityParallel = Vector3.ProjectOnPlane(Physics.gravity, surfaceNormal);
//         Vector3 gravityTorque = Vector3.Cross(leverArm, gravityParallel * totalTankMass);
//         Vector3 cancelTorque = -gravityTorque * strength;

//         rbSuspension.AddTorque(cancelTorque, ForceMode.Force);

//         Debug.DrawRay(rotationCenter, leverArm, Color.red, Time.fixedDeltaTime);
//         Debug.DrawRay(centerOfMass, gravityParallel, Color.blue, Time.fixedDeltaTime);
//         Debug.DrawRay(rotationCenter, gravityTorque * 0.001f, Color.magenta, Time.fixedDeltaTime);
//         Debug.DrawRay(rotationCenter, cancelTorque * 0.001f, Color.green, Time.fixedDeltaTime);
//     }
// // MARK: calc net force
//     public Vector3 CalculateNetForce(
//         float mass,
//         Vector3 gravity,
//         Vector3 currentVelocity,
//         Vector3 desiredVelocity,
//         Vector3 surfaceNormal,
//         float maxForce = 10000f)
//     {
//         Vector3 netForce = mass / Time.fixedDeltaTime *
//             (Vector3.ProjectOnPlane(desiredVelocity, surfaceNormal) -
//              Vector3.ProjectOnPlane(currentVelocity, surfaceNormal))
//             - mass * Vector3.ProjectOnPlane(gravity, surfaceNormal) * linearFrictionCoef;

//         return Vector3.ClampMagnitude(netForce, maxForce);
//     }
// // MARK: process track
//     void processTrack(Track track)
//     {
//         // Calculate desired track input (raw from AI/player)
//         float desiredTrackInput;
//         if (track.isRight)
//         {
//             desiredTrackInput = inpValue.y - inpValue.x;
//             tracksSpeed = speedInPercentD;
//             velocity = deltaDistanceD;
//         }
//         else
//         {
//             desiredTrackInput = inpValue.y + inpValue.x;
//             tracksSpeed = speedInPercentK;
//             velocity = deltaDistanceK;
//         }
        
//         // Apply acceleration limiting to track speed
//         float currentSpeed = track.isRight ? currentTrackSpeedD : currentTrackSpeedK;
//         float maxDelta = maxTrackAcceleration * Time.fixedDeltaTime;
//         float newSpeed = Mathf.MoveTowards(currentSpeed, desiredTrackInput, maxDelta);
        
//         // Store new speed
//         if (track.isRight)
//             currentTrackSpeedD = newSpeed;
//         else
//             currentTrackSpeedK = newSpeed;
        
//         // Use the smoothed speed for track movement
//         float trackInput = newSpeed;
//         track.trackProgress += trackInput * tracksSpeed;

//         foreach (var controller in track.tlc)
//         {
//             if (controller.isTouchingGround)
//             {
//                 contactsCount++;
//                 posSum += controller.contactPosition;

//                 // Use smoothed trackInput for velocity calculation
//                 Vector3 contactVel = controller.contactFront.normalized * velocity * trackInput;
//                 velSum += contactVel;
//                 Debug.DrawRay(controller.contactPosition, contactVel * 10f, Color.red);

//                 angularMomentumSum += CalculateAngularMomentum(
//                     controller.contactPosition,
//                     contactVel
//                 );

//                 int groundLayerMask = 1 << LayerMask.NameToLayer("Ground");

//                 if (Physics.Raycast(controller.contactPosition, controller.contactDown, out RaycastHit hit, 0.03f, groundLayerMask))
//                 {
//                     Debug.DrawRay(controller.contactPosition, controller.contactDown.normalized * hit.distance, Color.blue, Time.deltaTime);
//                     Debug.DrawRay(hit.point, hit.normal * 0.1f, Color.yellow, Time.deltaTime);
//                     norSum += hit.normal;
//                     normalsCount++;
//                 }
//             }
//         }
//     }
// // MARK: angular momentum
//     Vector3 CalculateAngularMomentum(Vector3 contactPosition, Vector3 velocity)
//     {
//         Vector3 worldCenterOfMass = transform.TransformPoint(centerOfMass);
//         Vector3 radiusVector = contactPosition - worldCenterOfMass;
//         Vector3 angularMomentum = Vector3.Cross(radiusVector, velocity);
//         return angularMomentum;
//     }

//     Vector3 CalculateAngularVelocity(Vector3 totalAngularMomentum)
//     {
//         float radius = 1f;
//         float momentOfInertia = 0.5f * mass * radius * radius;
//         return totalAngularMomentum / momentOfInertia;
//     }
// // MARK: gizmos
//     void OnDrawGizmos()
//     {
//         if (Application.isPlaying && contactsCount > 0)
//         {
//             Gizmos.color = Color.yellow;
//             Vector3 worldCenterOfMass = transform.TransformPoint(centerOfMass);
//             Gizmos.DrawSphere(worldCenterOfMass, 0.1f);

//             Gizmos.color = Color.magenta;
//             Gizmos.DrawRay(worldCenterOfMass, angularMomentumSum.normalized * 2f);

//             Gizmos.color = Color.cyan;
//             Gizmos.DrawRay(worldCenterOfMass, angularVelocity * 0.5f);
//         }

//         if (aiPath != null && aiPath.corners.Length > 0)
//         {
//             Gizmos.color = Color.green;
//             for (int i = 0; i < aiPath.corners.Length; i++)
//             {
//                 Gizmos.DrawSphere(aiPath.corners[i], 0.2f);
//                 if (i < aiPath.corners.Length - 1)
//                     Gizmos.DrawLine(aiPath.corners[i], aiPath.corners[i + 1]);
//             }
//         }

//         if (!isPlayer && aiPath != null && aiPath.corners.Length > 0)
//         {
//             Vector3 nextTarget = aiPath.corners.Length > 1 ? aiPath.corners[1] : aiPath.corners[0];
//             Gizmos.color = Color.blue;
//             Gizmos.DrawLine(transform.position, nextTarget);
            
//             Gizmos.color = Color.yellow;
//             Vector3 rightLimit = Quaternion.Euler(0, facingThreshold, 0) * transform.forward;
//             Vector3 leftLimit = Quaternion.Euler(0, -facingThreshold, 0) * transform.forward;
//             Gizmos.DrawRay(transform.position, rightLimit * 2f);
//             Gizmos.DrawRay(transform.position, leftLimit * 2f);
//         }
//     }
// }


using UnityEngine;

public class TankMovement : MonoBehaviour, IDrivable
{
    [Header("Tracks")]
    public GameObject viksraiK;
    public GameObject viksraiD;
    public float trackSpeedCmPerSecond = 30f;

    [Header("Physics")]
    public Rigidbody rbSuspension;
    public Vector3 centerOfMass = Vector3.zero;
    public float mass = 100f;
    public float turnMultiplier = 1f;
    public float accelerationFactor = 5f;
    public float decelerationFactor = 6f;
    public LayerMask groundLayerMask;

    // IDrivable implementation
    [Range(-1f, 1f)] public float DriveX { get; set; }
    [Range(-1f, 1f)] public float DriveY { get; set; }

    private Track trackK;
    private Track trackD;
    private float currentTrackSpeedK = 0f;
    private float currentTrackSpeedD = 0f;
    private float totalTankMass = 0f;

    private int contactsCount = 0;
    private int normalsCount = 0;
    private float tracksSpeed;
    private float deltaDistanceK;
    private float deltaDistanceD;
    private Vector3 posSum;
    private Vector3 velSum;
    private Vector3 norSum;
    private Vector3 angularMomentumSum;
    private float linearFrictionCoef = 1f;

    private Vector3 commonCenterOfMass;
    private float velocity;
    public GameObject leftGroundPenetrationTrigger;
    public GameObject rigthGroundPenetrationTrigger;
    private GroundPenetrationDetector LGT;
    private GroundPenetrationDetector RGT;
    private bool isPenetratedGround = false;
    // void OnEnable()
    // {
    // }

    void Start()
    {
        LGT = leftGroundPenetrationTrigger.GetComponent<GroundPenetrationDetector>();
        RGT = rigthGroundPenetrationTrigger.GetComponent<GroundPenetrationDetector>();
        trackK = viksraiK.GetComponent<Track>();
        trackD = viksraiD.GetComponent<Track>();
        rbSuspension.angularDamping = 5f;

        Rigidbody[] allParts = GetComponentsInChildren<Rigidbody>();
        foreach (var part in allParts)
        {
            totalTankMass += part.mass;
        }
    }

    void FixedUpdate()
    {
        isPenetratedGround = LGT.IsPenetrating || RGT.IsPenetrating;

        float scaleCompensation = 0.1f;
        deltaDistanceK = trackSpeedCmPerSecond * scaleCompensation * Time.fixedDeltaTime;
        float speedInPercentK = deltaDistanceK / trackK.totalLen * 100f;
        deltaDistanceD = trackSpeedCmPerSecond * scaleCompensation * Time.fixedDeltaTime;
        float speedInPercentD = deltaDistanceD / trackD.totalLen * 100f;

        contactsCount = 0;
        normalsCount = 0;
        posSum = Vector3.zero;
        velSum = Vector3.zero;
        norSum = Vector3.zero;
        angularMomentumSum = Vector3.zero;

        ProcessTrack(trackK, speedInPercentK, deltaDistanceK);
        ProcessTrack(trackD, speedInPercentD, deltaDistanceD);

        if (contactsCount > 0 && rbSuspension != null)
        {
            Vector3 avgSurfaceNormal = norSum.normalized;
            Vector3 contactsAvgCenter = posSum / contactsCount;
            Vector3 tankVelocity = velSum / contactsCount;

            Vector3 netForce = CalculateNetForce(
                mass: totalTankMass,
                gravity: Physics.gravity,
                currentVelocity: rbSuspension.linearVelocity,
                desiredVelocity: tankVelocity / Time.fixedDeltaTime,
                surfaceNormal: avgSurfaceNormal
            );

            if (!isPenetratedGround) rbSuspension.AddForceAtPosition(netForce, contactsAvgCenter, ForceMode.Force);

            commonCenterOfMass = CalculateTotalCenterOfMass();

            int maxCont = 18;
            int minCont = 9;
            float cancelationStr = Mathf.Clamp01((float)(normalsCount - minCont) / (maxCont - minCont));
            linearFrictionCoef = cancelationStr;

            if (normalsCount > minCont)
                CancelGravityTorque(commonCenterOfMass, contactsAvgCenter, avgSurfaceNormal, cancelationStr);

            Vector3 angularVelocity = CalculateAngularVelocity(angularMomentumSum);
            Vector3 calculatedAngularVelocity = angularVelocity / Time.fixedDeltaTime * turnMultiplier;
            rbSuspension.angularVelocity = calculatedAngularVelocity;
        }
    }

    void ProcessTrack(Track track, float speedPercent, float deltaDist)
    {
        float desiredTrackInput;
        if (track.isRight)
        {
            desiredTrackInput = DriveY - DriveX;   // right track
            tracksSpeed = speedPercent;
            velocity = deltaDist;
        }
        else
        {
            desiredTrackInput = DriveY + DriveX;   // left track
            tracksSpeed = speedPercent;
            velocity = deltaDist;
        }

        float currentSpeed = track.isRight ? currentTrackSpeedD : currentTrackSpeedK;
        float maxDelta = accelerationFactor * Time.fixedDeltaTime;
        if (Mathf.Abs(DriveX + DriveX) == 0f)
        {
            maxDelta = decelerationFactor * Time.fixedDeltaTime;
        }
        
        float newSpeed = Mathf.MoveTowards(currentSpeed, desiredTrackInput, maxDelta);

        if (track.isRight)
            currentTrackSpeedD = newSpeed;
        else
            currentTrackSpeedK = newSpeed;

        float trackInput = newSpeed;
        track.trackProgress += trackInput * tracksSpeed;

        foreach (var controller in track.tlc)
        {
            if (controller.isTouchingGround)
            {
                contactsCount++;
                posSum += controller.contactPosition;

                Vector3 contactVel = controller.contactFront.normalized * velocity * trackInput;
                velSum += contactVel;
                Debug.DrawRay(controller.contactPosition, contactVel * 10f, Color.red);

                angularMomentumSum += CalculateAngularMomentum(controller.contactPosition, contactVel);

                if (Physics.Raycast(controller.contactPosition, controller.contactDown, out RaycastHit hit, 0.03f, groundLayerMask))
                {
                    Debug.DrawRay(controller.contactPosition, controller.contactDown.normalized * hit.distance, Color.blue, Time.deltaTime);
                    Debug.DrawRay(hit.point, hit.normal * 0.1f, Color.yellow, Time.deltaTime);
                    norSum += hit.normal;
                    normalsCount++;
                }
            }
        }
    }

    Vector3 CalculateNetForce(float mass, Vector3 gravity, Vector3 currentVelocity, Vector3 desiredVelocity, Vector3 surfaceNormal, float maxForce = 10000f)
    {
        Vector3 netForce = mass / Time.fixedDeltaTime *
            (Vector3.ProjectOnPlane(desiredVelocity, surfaceNormal) -
             Vector3.ProjectOnPlane(currentVelocity, surfaceNormal))
            - mass * Vector3.ProjectOnPlane(gravity, surfaceNormal) * linearFrictionCoef;

        return Vector3.ClampMagnitude(netForce, maxForce);
    }

    Vector3 CalculateAngularMomentum(Vector3 contactPosition, Vector3 velocity)
    {
        Vector3 worldCenterOfMass = transform.TransformPoint(centerOfMass);
        Vector3 radiusVector = contactPosition - worldCenterOfMass;
        return Vector3.Cross(radiusVector, velocity);
    }

    Vector3 CalculateAngularVelocity(Vector3 totalAngularMomentum)
    {
        float radius = 1f;
        float momentOfInertia = 0.5f * mass * radius * radius;
        return totalAngularMomentum / momentOfInertia;
    }

    void CancelGravityTorque(Vector3 com, Vector3 rotationCenter, Vector3 surfaceNormal, float strength = 0.8f)
    {
        Vector3 centerOfMass = com - Vector3.Project(com - rotationCenter, rbSuspension.transform.forward);
        Vector3 leverArm = centerOfMass - rotationCenter;

        Vector3 gravityParallel = Vector3.ProjectOnPlane(Physics.gravity, surfaceNormal);
        Vector3 gravityTorque = Vector3.Cross(leverArm, gravityParallel * totalTankMass);
        Vector3 cancelTorque = -gravityTorque * strength;

        rbSuspension.AddTorque(cancelTorque, ForceMode.Force);

        Debug.DrawRay(rotationCenter, leverArm, Color.red, Time.fixedDeltaTime);
        Debug.DrawRay(centerOfMass, gravityParallel, Color.blue, Time.fixedDeltaTime);
        Debug.DrawRay(rotationCenter, gravityTorque * 0.001f, Color.magenta, Time.fixedDeltaTime);
        Debug.DrawRay(rotationCenter, cancelTorque * 0.001f, Color.green, Time.fixedDeltaTime);
    }

    Vector3 CalculateTotalCenterOfMass()
    {
        Rigidbody[] allRigidbodies = GetComponentsInChildren<Rigidbody>();
        float totalMass = 0f;
        Vector3 weightedPositionSum = Vector3.zero;

        foreach (Rigidbody rb in allRigidbodies)
        {
            totalMass += rb.mass;
            weightedPositionSum += rb.worldCenterOfMass * rb.mass;
        }

        if (totalMass > 0)
            return weightedPositionSum / totalMass;
        else
            return transform.position;
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying && contactsCount > 0)
        {
            Gizmos.color = Color.yellow;
            Vector3 worldCenterOfMass = transform.TransformPoint(centerOfMass);
            Gizmos.DrawSphere(worldCenterOfMass, 0.1f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(worldCenterOfMass, angularMomentumSum.normalized * 2f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(worldCenterOfMass, (angularMomentumSum / (0.5f * mass * 1f * 1f)) * 0.5f);
        }
    }
}