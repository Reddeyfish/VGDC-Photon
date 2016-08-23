using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RaygunRay : MonoBehaviour {

    ParticleSystem vfx;
    MeshRenderer firedRay;
    Mesh mesh;
    Vector3 direction;

    [SerializeField]
    protected float width;

    [SerializeField]
    protected float rayDuration;

    
	// Use this for initialization
	void Awake () {
        vfx = GetComponentInChildren<ParticleSystem>();
        firedRay = GetComponentInChildren<MeshRenderer>();
        firedRay.sortingLayerName = Tags.SortingLayers.overlay;
        MeshFilter filter = GetComponent<MeshFilter>();
        mesh = filter.mesh;

        Vector3[] vertices = new Vector3[] { new Vector3(-width / 2, 0, 0), new Vector3(width / 2, 0, 0), new Vector3(-width / 2, 60, 0), new Vector3(width / 2, 60, 0) };
        Vector2[] uvs = new Vector2[] {Vector2.zero, Vector2.up, Vector2.right, Vector2.one};
        int[] tris = new int[]{3, 2, 1, 0, 1, 2};

        //apply to mesh
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.SetTriangles(tris, 0);
	}

    /// <summary>
    /// For billboarding
    /// </summary>
    void OnWillRenderObject()
    {
        Vector3 viewDir = Camera.current.transform.position - transform.position;
        viewDir -= Vector3.Project(viewDir, direction);
        transform.rotation = Quaternion.LookRotation(viewDir, direction);
    }

    public void playShotVFX(Vector3 origin, Vector3 direction)
    {
        transform.position = origin;
        this.direction = direction;
        vfx.Play();
        firedRay.enabled = true;
        Callback.DoLerp((float l) =>
        {
            Color rayColor = Color.Lerp(Color.white, Color.black, l);
            rayColor.a = 1 - l;
            mesh.colors = new Color[] { rayColor, rayColor, rayColor, rayColor };
        }, rayDuration, this).FollowedBy(() => firedRay.enabled = false, this);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawMesh(mesh);
    }
}
