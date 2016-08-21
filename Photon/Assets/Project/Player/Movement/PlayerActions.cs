using UnityEngine;
using System.Collections.Generic;

public class PlayerActions : MonoBehaviour {

    PhotonView view;
    PlayerMovement movement;

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
        view.RPC("FireWeapon", PhotonTargets.All, muzzle.position, playerCamera.forward);
    }

    [PunRPC]
    void FireWeapon(Vector3 position, Vector3 direction, PhotonMessageInfo info)
    {
        if (view.isMine)
        {
            Destroy(Instantiate(muzzleVFXPrefab, position, Quaternion.identity), 1);

            GameObject bulletVFX = Instantiate(bulletVFXPrefab);
            LineRenderer bulletLineRenderer = bulletVFX.GetComponent<LineRenderer>();
            bulletLineRenderer.SetPosition(0, position);
            bulletLineRenderer.SetPosition(1, position + 100 * direction);
            Destroy(bulletVFX, 2);
        }
        else
        {
            Destroy(Instantiate(muzzleVFXPrefab, muzzle.position, Quaternion.identity), 1);

            foreach (KeyValuePair<PhotonView, PlayerMovement> player in PlayerMovement.playerMovements)
            {
                if (player.Key == view)
                {
                    continue;
                }

                if (player.Value.positionHistory.Count == 0)
                {
                    continue;
                }

                //acquire player position at moment shot was fired
                double shotTime = info.timestamp - movement.BufferDelaySecs;

                TimestampedData<Vector3> previous = player.Value.positionHistory.Peek();
                foreach (TimestampedData<Vector3> timestampedPosition in player.Value.positionHistory)
                {
                    if (timestampedPosition.outputTime < shotTime)
                    {
                        float lerpValue = Mathd.InverseLerp(timestampedPosition.outputTime, previous.outputTime, shotTime);

                        Vector3 targetPosition = Vector3.Lerp(timestampedPosition, previous, lerpValue);

                        Destroy(Instantiate(targetSpherePrefab, targetPosition, Quaternion.identity), 2);

                        bool hit = TimePhysics.SphereIntersection(targetPosition, 0.5f, position, direction);

                        if (hit)
                        {
                            player.Value.GetComponent<PlayerStatus>().health += 1;
                            //TODO: sort by distance
                            goto END; //break out of all loops
                        }

                        break;
                    }
                    else
                    {
                        previous = timestampedPosition;
                    }
                }
            }

        END:GameObject bulletVFX = Instantiate(bulletVFXPrefab);
            LineRenderer bulletLineRenderer = bulletVFX.GetComponent<LineRenderer>();
            bulletLineRenderer.SetPosition(0, position);
            bulletLineRenderer.SetPosition(1, position + 100 * direction);
            Destroy(bulletVFX, 2);
        }

        
    }
}
