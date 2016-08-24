using UnityEngine;
using System.Collections;

public class WorldSpaceUIPositioning : MonoBehaviour {

    RectTransform thisTransform;

    [SerializeField]
    protected Transform target;

    void Start()
    {
        thisTransform = (RectTransform)transform;
        if (transform.root == Camera.main.transform.root)
        {
            //this.gameObject.SetActive(false);
        }
    }

	// Update is called once per frame
	void LateUpdate () {
        Vector3 worldPos = target.position;
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        thisTransform.position = screenPos;
	}
}
