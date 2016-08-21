using UnityEngine;
using System.Collections;

public class SimplePlayerMovement : MonoBehaviour {

    Rigidbody rigid;
    PhotonView view;
    bool isLocallyControlled;
    Vector2 movementInput;

    [SerializeField]
    protected float speed;

    [SerializeField]
    protected float acceleration;

	// Use this for initialization
	void Start () {
        rigid = GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();
        isLocallyControlled = view.isMine;
	}

    void Update()
    {
        if (isLocallyControlled)
        {
            movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); //pull from input script

            Debug.Log(movementInput);
            rigid.velocity = Vector3.MoveTowards(rigid.velocity, movementInput * speed, Time.deltaTime * acceleration);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(rigid.velocity);
        }
        else
        {
            transform.position = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
            rigid.velocity = (Vector3)stream.ReceiveNext();
        }
    }
}
