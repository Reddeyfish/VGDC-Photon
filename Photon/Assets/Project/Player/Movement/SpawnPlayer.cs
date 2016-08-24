using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

// todo: probably should rename this script as it does more now
public class SpawnPlayer : Photon.MonoBehaviour {

    [SerializeField]
    protected GameObject mainCameraPrefab;

    [SerializeField]
    protected string playerPrefabName;

    private int seed;

    private LevelGenerator level;
    void Awake() {
        level = GetComponent<LevelGenerator>();
    }

    private void SpawnMyPlayer() {
        Assert.IsFalse(string.IsNullOrEmpty(playerPrefabName));
        GameObject player = PhotonNetwork.Instantiate(playerPrefabName, 10 * (Vector3.up + Random.insideUnitSphere), Quaternion.identity, 0);
        GameObject mainCamera = Instantiate(mainCameraPrefab, player.transform.FindChild("CameraRotator/CameraHolder")) as GameObject;
        mainCamera.transform.localPosition = Vector3.zero;
        mainCamera.transform.localRotation = Quaternion.identity;
        Camera.main.gameObject.SetActive(false);
        Cursor.visible = false;
    }

    public void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.isMasterClient ? "You are the master client" : "You are not the master client");
        Debug.LogFormat("Ping: {0}", PhotonNetwork.GetPing()); Debug.Log(PhotonNetwork.isMasterClient ? "You are the master client" : "You are not the master client");

        if (PhotonNetwork.isMasterClient) {
            seed = Random.Range(0, 1000000);
            level.BuildBuildings(seed);
            SpawnMyPlayer();
        } 
    }

    public void OnPhotonPlayerConnected(PhotonPlayer newPlayer) {
        if (PhotonNetwork.isMasterClient) {
            photonView.RPC("InitializeLevel", newPlayer, seed);
        }
    }

    [PunRPC]
    public void InitializeLevel(int seed) {
        this.seed = seed;
        level.BuildBuildings(seed);
        SpawnMyPlayer();
    }
}
