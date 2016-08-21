using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

public class SpawnPlayer : Photon.MonoBehaviour {

    [SerializeField]
    protected string playerPrefabName;

    public void OnJoinedRoom()
    {
        Assert.IsFalse(string.IsNullOrEmpty(playerPrefabName));
        PhotonNetwork.Instantiate(playerPrefabName, 10 * (Vector3.up + Random.insideUnitSphere), Quaternion.identity, 0);
    }
}
