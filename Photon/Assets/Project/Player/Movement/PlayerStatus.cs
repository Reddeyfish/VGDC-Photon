using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerStatus : MonoBehaviour {

    PhotonView view;

    float _health = 1;
    public float health
    {
        get { return _health; }
        set
        {
            _health = value;
            if (_health < 0)
            {
                _health = 1; //cheap respawn logic until that's actually implemented
                this[Stats.DEATHS] += 1;
            }
            healthbar.HealthPercentage = Mathf.Clamp01(_health);
        }
    }

    [SerializeField]
    protected HealthBarView healthbar;

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

    //Stat tracking
    private Dictionary<int, float> stats = new Dictionary<int,float>();

    public float this[int i]
    {
        get { return stats[i]; }
        set { stats[i] = value; }
    }

    /// <summary>
    /// For example, this[Stats.DEATHS] += 1;
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public float this[Stats i]
    {
        get { return stats[i]; }
        set { stats[i] = value; }
    }
}
