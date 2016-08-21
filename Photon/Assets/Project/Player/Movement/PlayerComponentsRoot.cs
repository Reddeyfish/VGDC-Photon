using UnityEngine;
using System.Collections;

public class PlayerComponentsRoot : MonoBehaviour {

    PlayerMovement movement;
    PlayerStatus status;
	// Use this for initialization
	void Start () {
        movement = GetComponent<PlayerMovement>();
        status = GetComponent<PlayerStatus>();
	}

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        movement.OnPhotonSerializeView(stream, info);
        status.OnPhotonSerializeView(stream, info);
    }
}
