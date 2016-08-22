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
    Vector3 prevPos = Vector3.zero;
    TimeCollider col;
    bool grounded = false;
    float timeSinceGrounded = 10.0f;
    float timeSinceHitJump = 10.0f;

    Queue<TimestampedData<Vector3>> bufferedTargetPositions = new Queue<TimestampedData<Vector3>>();
    TimestampedData<Vector3> previousTargetPosition;
    /// <summary>
    /// The most recent target position data whose outputTime is in the past.
    /// </summary>
    TimestampedData<Vector3> currentTargetPosition;

    Queue<TimestampedData<Quaternion>> bufferedTargetRotations = new Queue<TimestampedData<Quaternion>>();
    /// <summary>
    /// THe most recent target rotation data whose outputTime is in the past.
    /// </summary>
    TimestampedData<Quaternion> currentTargetRotation;

    Stack<TimestampedData<Quaternion>> rotationHistory = new Stack<TimestampedData<Quaternion>>();

    [SerializeField]
    protected float speed;

    [SerializeField]
    protected float acceleration;

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
        currentTargetPosition = previousTargetPosition = new TimestampedData<Vector3>(PhotonNetwork.time, this.transform.position);
        currentTargetRotation = new TimestampedData<Quaternion>(PhotonNetwork.time, this.transform.rotation);
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
            newVel.y = 10.0f;
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
        while (bufferedTargetPositions.Count != 0 && bufferedTargetPositions.Peek().outputTime < PhotonNetwork.time)
        {
            //we have new data we can use
            previousTargetPosition = currentTargetPosition;
            currentTargetPosition = bufferedTargetPositions.Dequeue();
        }


        if (bufferedTargetPositions.Count > 0)
        {
            float lerpValue = Mathd.InverseLerp(currentTargetPosition.outputTime, bufferedTargetPositions.Peek().outputTime, PhotonNetwork.time);

            Vector3 newPosition = Vector3.LerpUnclamped(currentTargetPosition, bufferedTargetPositions.Peek(), lerpValue);

            Debug.DrawRay(Vector3.zero, newPosition);

            transform.position = newPosition;
        }
        else
        {
            Debug.LogError("Buffer Empty");
        }
    }

    void UpdateRotation()
    {
        if (bufferedTargetRotations.Count != 0 && bufferedTargetRotations.Peek().outputTime < PhotonNetwork.time)
        {
            //we have new data we can use
            currentTargetRotation = bufferedTargetRotations.Dequeue();
        }

        if (bufferedTargetRotations.Count != 0)
        {
            float lerpValue = Mathd.InverseLerp(currentTargetRotation.outputTime, bufferedTargetRotations.Peek().outputTime, PhotonNetwork.time);

            transform.rotation = Quaternion.Slerp(currentTargetRotation, bufferedTargetRotations.Peek(), lerpValue);
        }
        else{
            transform.rotation = (currentTargetRotation);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        TimestampedData<Vector3> positionData;
        TimestampedData<Quaternion> rotationData;

        if (stream.isWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);

            positionData = new TimestampedData<Vector3>(info.timestamp, transform.position);
            rotationData = new TimestampedData<Quaternion>(info.timestamp, transform.rotation);

            col.Add(positionData);
            rotationHistory.Push(rotationData);
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
            Quaternion rotation = (Quaternion)stream.ReceiveNext();

            positionData = new TimestampedData<Vector3>(outputTime, position);
            rotationData = new TimestampedData<Quaternion>(outputTime, rotation);

            bufferedTargetPositions.Enqueue(positionData);
            bufferedTargetRotations.Enqueue(rotationData);
        }

        col.Add(positionData);
        rotationHistory.Push(rotationData);
    }
}