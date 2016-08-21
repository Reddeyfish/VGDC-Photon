using UnityEngine;
using System.Collections;

public class CameraGrab : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (GetComponentInParent<PhotonView>().isMine)
        {
            Transform cameraTransform = Camera.main.transform;
            cameraTransform.SetParent(this.transform, false);
            cameraTransform.localPosition = Vector3.zero;
            cameraTransform.localRotation = Quaternion.identity;
        }
	}
}
