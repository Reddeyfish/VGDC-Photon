using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(TimeCollider))]
public class PlayerMovement : MonoBehaviour
{
    Rigidbody rigid;
    PhotonView view;
    Vector3 movementInput;
    double latestData = 0;
    TimeCollider col;
    bool grounded = false;
    float timeSinceGrounded = 10.0f;
    float timeSinceHitJump = 10.0f;

    Queue<TimestampedData<Vector3>> bufferedTargetPositions = new Queue<TimestampedData<Vector3>>();

    /// <summary>
    /// The position we are using as the start point of interpolation.
    /// </summary>
    TimestampedData<Vector3> previousTargetPosition;
    /// <summary>
    /// The position we are using as the endpoint of interpolation.
    /// </summary>
    TimestampedData<Vector3> nextTargetPosition;

    Queue<TimestampedData<FPSRotation>> bufferedTargetRotations = new Queue<TimestampedData<FPSRotation>>();

    /// <summary>
    /// The rotation we are using as the start point of interpolation.
    /// </summary>
    TimestampedData<FPSRotation> previousTargetRotation;
    /// <summary>
    /// The rotation we are using as the endpoint of interpolation.
    /// </summary>
    TimestampedData<FPSRotation> nextTargetRotation;

    Stack<TimestampedData<FPSRotation>> rotationHistory = new Stack<TimestampedData<FPSRotation>>();

    [SerializeField]
    protected float speed;

    [SerializeField]
    protected float acceleration;

    [SerializeField]
    [Tooltip("jump speed in meters per second")]
    protected float jumpStrengthMS;

    [SerializeField]
    protected double bufferDelaySecs;
    public double BufferDelaySecs { get { return bufferDelaySecs; } }

    [SerializeField]
    protected Transform cameraRotator;

    public TimePhysicsLayers Layer { get { return TimePhysicsLayers.PLAYERS; } }
    public float Radius { get { return 1; } }

	// Use this for initialization
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();
        col = GetComponent<TimeCollider>();
        previousTargetPosition = new TimestampedData<Vector3>(PhotonNetwork.time - 1, this.transform.position);
        nextTargetPosition = new TimestampedData<Vector3>(PhotonNetwork.time, this.transform.position);
        previousTargetRotation = new TimestampedData<FPSRotation>(PhotonNetwork.time - 1, cameraRotator.rotation);
        nextTargetRotation = new TimestampedData<FPSRotation>(PhotonNetwork.time, cameraRotator.rotation);
        if (!view.isMine)
        {
            Destroy(rigid);
            rigid = null;
        }
    }

    void Update()
    {
        if (!view.isMine) {
            UpdatePosition();
            UpdateRotation();
            return;
        }

        Vector3 movementInput = (transform.right * Input.GetAxis("Horizontal")) + (transform.forward * Input.GetAxis("Vertical")); //pull from input script
        Vector3 rigidbodyXZ = rigid.velocity;
        rigidbodyXZ.y = 0;
        Vector3 movementXZ = Vector3.MoveTowards(rigidbodyXZ, movementInput * speed, Time.deltaTime * acceleration);
        Vector3 newVel = new Vector3(movementXZ.x, rigid.velocity.y, movementXZ.z);

        RaycastHit info; // incase needed later
        Vector3 start = transform.position + Vector3.up * 0.5f;
        grounded = Physics.SphereCast(start, 0.4f, Vector3.down, out info, 0.2f);
        grounded &= newVel.y < 10.0f;  // ensure player didn't just jump

        if (grounded) {
            timeSinceGrounded = 0.0f;
            //Debug.DrawRay(transform.position, Vector3.up, Color.green, 10.0f);
        } else {
            timeSinceGrounded += Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.Space)) {  // todo: pull from input script
            timeSinceHitJump = 0.0f;
        } else {
            timeSinceHitJump += Time.deltaTime;
        }

        // jump if recently grounded and recently hit jump button
        if (timeSinceGrounded < 0.25f && timeSinceHitJump < 0.25f)
        {
            newVel.y = jumpStrengthMS;
            timeSinceGrounded = 10.0f;
            timeSinceHitJump = 10.0f;
            grounded = false;
        }

        rigid.velocity = newVel;

        // horizontal mouse look rotates transform around y axis
        Vector3 angles = rigid.rotation.eulerAngles;
        angles.y += Input.GetAxis("HorizontalMouse");
        Quaternion rotation = Quaternion.Euler(angles);
        rigid.rotation = rotation;

        // vertical mouse look rotates camera around x axis
        angles = cameraRotator.rotation.eulerAngles;
        if (angles.x > 180.0f) angles.x -= 360.0f;
        angles.x -= Input.GetAxis("VerticalMouse");
        angles.x = Mathf.Clamp(angles.x, -89.0f, 89.0f);
        rotation = Quaternion.Euler(angles);
        cameraRotator.rotation = rotation;

    }

    void UpdatePosition()
    {
        while (bufferedTargetPositions.Count != 0 && nextTargetPosition.outputTime < PhotonNetwork.time)
        {
            //we have new data we can use
            previousTargetPosition = nextTargetPosition;
            nextTargetPosition = bufferedTargetPositions.Dequeue();
        }

        float lerpValue = Mathd.InverseLerp(previousTargetPosition.outputTime, nextTargetPosition.outputTime, PhotonNetwork.time);
        Vector3 targetPosition = Vector3.LerpUnclamped(previousTargetPosition, nextTargetPosition, lerpValue);

        Debug.DrawLine(previousTargetPosition, nextTargetPosition);

        //limit our movement each frame to our max speed (with respect to the different dimensions). This prevents the jittery appearance of extrapolated data
        Vector3 currentPositionXZ = transform.position;
        currentPositionXZ.y = 0;
        Vector3 targetPositionXZ = targetPosition;
        targetPositionXZ.y = 0;

        Vector3 newPosition = Vector3.MoveTowards(currentPositionXZ, targetPositionXZ, speed);

        float currentPositionY = transform.position.y;
        float targetPositionY = targetPosition.y;
        newPosition.y = Mathf.MoveTowards(currentPositionY, targetPositionY, jumpStrengthMS);

        transform.position = newPosition;
    }

    void UpdateRotation()
    {
        while (bufferedTargetRotations.Count != 0 && nextTargetRotation.outputTime < PhotonNetwork.time)
        {
            //we have new data we can use
            previousTargetRotation = nextTargetRotation;
            nextTargetRotation = bufferedTargetRotations.Dequeue();
        }

        float lerpValue = Mathd.InverseLerp(previousTargetRotation.outputTime, nextTargetRotation.outputTime, PhotonNetwork.time);
        FPSRotation newRotation = Quaternion.SlerpUnclamped((FPSRotation)previousTargetRotation, (FPSRotation)nextTargetRotation, lerpValue);

        transform.localRotation = Quaternion.Euler(0, newRotation.yaw, 0);
        cameraRotator.localRotation = Quaternion.Euler(newRotation.pitch, 0, 0);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        TimestampedData<Vector3> positionData;
        TimestampedData<FPSRotation> rotationData;

        if (stream.isWriting)
        {
            positionData = new TimestampedData<Vector3>(info.timestamp, transform.position);
            rotationData = new TimestampedData<FPSRotation>(info.timestamp, cameraRotator.rotation);

            // We own this player: send the others our data
            stream.SendNext(positionData.data);
            stream.SendNext(rotationData.data.pitch);
            stream.SendNext(rotationData.data.yaw);
        }
        else
        {
            // Network player, receive data
            double recieveTime = info.timestamp;
            if (recieveTime <= latestData)
            {
                Debug.LogError("out of order");
            }
            else
            {
                latestData = recieveTime;
            }
            double outputTime = recieveTime + bufferDelaySecs; //This is the time at which we will display the player at this position.

            Vector3 position = (Vector3)stream.ReceiveNext();
            FPSRotation rotation = new FPSRotation((float)stream.ReceiveNext(), (float)stream.ReceiveNext());

            positionData = new TimestampedData<Vector3>(outputTime, position);
            rotationData = new TimestampedData<FPSRotation>(outputTime, rotation);

            //Debug.Log(bufferedTargetPositions.Count);

            bufferedTargetPositions.Enqueue(positionData);
            bufferedTargetRotations.Enqueue(rotationData);
        }

        col.Add(positionData);
        rotationHistory.Push(rotationData);
    }
}