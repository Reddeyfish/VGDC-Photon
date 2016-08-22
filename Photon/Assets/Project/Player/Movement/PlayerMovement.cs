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
        if (view.isMine)
        {
            Vector3 movementInput = (transform.right * Input.GetAxis("Horizontal")) + (transform.forward * Input.GetAxis("Vertical")); //pull from input script
            Vector3 rigidbodyXZ = rigid.velocity;
            rigidbodyXZ.y = 0;
            Vector3 movementXZ = Vector3.MoveTowards(rigidbodyXZ, movementInput * speed, Time.deltaTime * acceleration);
            Vector3 newRigidVelocity = new Vector3(movementXZ.x, rigid.velocity.y, movementXZ.z);

            if (Input.GetKeyDown(KeyCode.Space)) //pull from input script
            {
                newRigidVelocity.y += 10;
            }

            rigid.velocity = newRigidVelocity;

            Vector3 angles = rigid.rotation.eulerAngles;
            angles.y += Input.GetAxis("HorizontalMouse");
            Quaternion rotation = Quaternion.Euler(angles);
            rigid.rotation = rotation;

            angles = cameraRotator.rotation.eulerAngles;
            angles.x -= Input.GetAxis("VerticalMouse");
            rotation = Quaternion.Euler(angles);
            cameraRotator.rotation = rotation;
        }
        else
        {
            UpdatePosition();
            UpdateRotation();
        }
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
        if (stream.isWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);

            TimestampedData<Vector3> positionData = new TimestampedData<Vector3>(info.timestamp, transform.position);
            TimestampedData<Quaternion> rotationData = new TimestampedData<Quaternion>(info.timestamp, transform.rotation);

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

            TimestampedData<Vector3> positionData = new TimestampedData<Vector3>(outputTime, position);
            TimestampedData<Quaternion> rotationData = new TimestampedData<Quaternion>(outputTime, rotation);

            bufferedTargetPositions.Enqueue(positionData);
            bufferedTargetRotations.Enqueue(rotationData);

            col.Add(positionData);
            rotationHistory.Push(rotationData);
        }
    }
}