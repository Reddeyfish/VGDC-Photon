using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerStatus : MonoBehaviour {

    PhotonView view;

    float _health = 0;
    public float health
    {
        get { return _health; }
        set
        {
            _health = value;
            healthbar.text = _health.ToString();
        }
    }

    [SerializeField]
    protected Text healthbar;

    [SerializeField]
    protected Text username;

    public void Start()
    {
        view = GetComponentInParent<PhotonView>();
        UpdateUsername();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(health);
        }
        else
        {
            health = (float)stream.ReceiveNext();
        }
    }

    void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        UpdateUsername();
    }

    public void UpdateUsername()
    {
        username.text = view.owner.name;
    }
}
