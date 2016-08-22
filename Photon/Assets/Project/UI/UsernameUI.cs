using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(InputField))]
public class UsernameUI : MonoBehaviour {

    InputField usernameField;

	// Use this for initialization
	void Start () {
        usernameField = GetComponent<InputField>();
        string existingName = PhotonNetwork.player.name;
        if (!string.IsNullOrEmpty(existingName))
        {
            usernameField.text = existingName;
        }
	}

    public void OnEndEdit()
    {
        PhotonNetwork.playerName = usernameField.text;
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
        customProperties["name"] = usernameField.text;

        Debug.LogFormat("name changed to {0}", usernameField.text);
        PhotonNetwork.SetPlayerCustomProperties(customProperties);
    }
}
