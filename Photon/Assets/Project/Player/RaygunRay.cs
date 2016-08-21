using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RaygunRay : MonoBehaviour {

    [SerializeField]
    protected float width;

    [SerializeField]
    protected float rayDuration;

    ParticleSystem vfx;
    MeshRenderer firedRay;
    Mesh mesh;
	// Use this for initialization
	void Awake () {
        vfx = GetComponentInChildren<ParticleSystem>();
        firedRay = GetComponentInChildren<MeshRenderer>();
        firedRay.sortingLayerName = Tags.SortingLayers.overlay;
        MeshFilter filter = GetComponent<MeshFilter>();
        mesh = filter.mesh;

        Vector3[] vertices = new Vector3[] { new Vector3(0, -width / 2, 0), new Vector3(0, width / 2, 0), new Vector3(60, -width / 2, 0), new Vector3(60, width / 2, 0)};
        Vector2[] uvs = new Vector2[] {Vector2.zero, Vector2.up, Vector2.right, Vector2.one};
        int[] tris = new int[]{3, 2, 1, 0, 1, 2};

        //apply to mesh
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.SetTriangles(tris, 0);

        playShotVFX(Vector3.up, Vector3.up);
	}

    public void playShotVFX(Vector2 origin, Vector2 direction)
    {
        transform.position = origin;
        transform.rotation = direction.ToRotation();
        vfx.Play();
        firedRay.enabled = true;
        Callback.DoLerp((float l) =>
        {
            Color rayColor = Color.Lerp(Color.white, Color.black, l);
            rayColor.a = 1 - l;
            mesh.colors = new Color[] { rayColor, rayColor, rayColor, rayColor };
        }, rayDuration, this).FollowedBy(() => firedRay.enabled = false, this);
    }
}
