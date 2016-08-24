﻿using UnityEngine;
using System.Collections.Generic;

public class PlayerActions : MonoBehaviour {

    PhotonView view;
    PlayerMovement movement;
    LayerMask staticOnly;

    [SerializeField]
    protected Transform muzzle;

    [SerializeField]
    protected Transform playerCamera;

    [SerializeField]
    protected GameObject muzzleVFXPrefab;

    [SerializeField]
    protected GameObject bulletVFXPrefab;

    [SerializeField]
    protected GameObject targetSpherePrefab;

	// Use this for initialization
	void Start () {
        view = GetComponent<PhotonView>();
        movement = GetComponent<PlayerMovement>();
        staticOnly = LayerMask.GetMask(Tags.Layers.Static);
	}
	
	// Update is called once per frame
	void Update () {
        if (!view.isMine)
        {
            return;
        }

        if(Input.GetMouseButtonDown(0)) //pull from input script
        {
            FireWeapon();
        }
	}

    void FireWeapon()
    {
        view.RPC("FireWeapon", PhotonTargets.All, muzzle.position, playerCamera.forward.normalized);
    }

    [PunRPC]
    void FireWeapon(Vector3 position, Vector3 direction, PhotonMessageInfo info)
    {
        GameObject bulletVFX = Instantiate(bulletVFXPrefab);
        bulletVFX.GetComponent<RaygunRay>().playShotVFX(position, direction);
        Destroy(bulletVFX, 2);

        if (view.isMine)
        {
            Destroy(Instantiate(muzzleVFXPrefab, position, Quaternion.identity), 1);
        }
        else
        {
            Destroy(Instantiate(muzzleVFXPrefab, muzzle.position, Quaternion.identity), 1);

            List<TimeCollider> hitColliders = TimePhysics.RaycastAll(position, direction, info.timestamp - movement.BufferDelaySecs);

            foreach (TimeCollider col in hitColliders)
            {
                if (col.transform != this.transform)
                {
                    col.GetComponent<PlayerStatus>().health -= 0.25f;
#if UNITY_EDITOR
                    Destroy(Instantiate(targetSpherePrefab, col.PositionAtTime(info.timestamp - movement.BufferDelaySecs), Quaternion.identity), 2);
#endif
                    break;

                }
            }
        }
    }
}
