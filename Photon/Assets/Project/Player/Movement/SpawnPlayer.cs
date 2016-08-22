using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

public class SpawnPlayer : Photon.MonoBehaviour {

    [SerializeField]
    protected GameObject mainCameraPrefab;

    [SerializeField]
    protected string playerPrefabName;

    public void OnJoinedRoom()
    {
        Assert.IsFalse(string.IsNullOrEmpty(playerPrefabName));
        GameObject player = PhotonNetwork.Instantiate(playerPrefabName, 10 * (Vector3.up + Random.insideUnitSphere), Quaternion.identity, 0);
        GameObject mainCamera = Instantiate(mainCameraPrefab, player.transform.FindChild("CameraRotator/CameraHolder")) as GameObject;
        mainCamera.transform.localPosition = Vector3.zero;
        mainCamera.transform.localRotation = Quaternion.identity;
        Camera.main.gameObject.SetActive(false);
        Debug.Log(PhotonNetwork.isMasterClient ? "You are the master client" : "You are not the master client");
    }
}
