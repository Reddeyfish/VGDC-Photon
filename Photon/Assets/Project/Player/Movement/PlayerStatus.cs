using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerStatus : MonoBehaviour {

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
}
