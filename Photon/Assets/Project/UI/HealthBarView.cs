using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBarView : MonoBehaviour {

    [SerializeField]
    protected Image fillMask;

    [SerializeField]
    protected Image backgroundFillMask;

    float healthPercentage = 1;
    public float HealthPercentage
    {
        get { return healthPercentage; }
        set
        {
            healthPercentage = value;
            fillMask.fillAmount = backgroundFillMask.fillAmount = healthPercentage;
        }
    }
}
