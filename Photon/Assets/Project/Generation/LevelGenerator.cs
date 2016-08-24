using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour {

    public Material buildingMat;
    public int numBuildings = 200;

    GameObject buildings;
    public void BuildBuildings(int seed) {
        GameObject oldBuildings = GameObject.Find("Buildings");
        if (oldBuildings) {
            Destroy(oldBuildings);
        }
        buildings = new GameObject("Buildings");

        // build level boundaries
        float levelSize = 100.0f;
        float wallThickness = 1.0f;
        float wallHeight = 10.0f;
        GameObject wall;
        wall = BuildBoxObject("Wall-X", new Vector3(wallThickness, wallHeight, levelSize));
        wall.transform.position = new Vector3(-levelSize / 2, 0.0f, 0.0f);
        wall = BuildBoxObject("Wall+X", new Vector3(wallThickness, wallHeight, levelSize));
        wall.transform.position = new Vector3(levelSize / 2, 0.0f, 0.0f);
        wall = BuildBoxObject("Wall-Z", new Vector3(levelSize, wallHeight, wallThickness));
        wall.transform.position = new Vector3(0.0f, 0.0f, -levelSize / 2);
        wall = BuildBoxObject("Wall+Z", new Vector3(levelSize, wallHeight, wallThickness));
        wall.transform.position = new Vector3(0.0f, 0.0f, levelSize / 2);

        Random.InitState(seed);

        const float size = 35.0f;
        Vector3 centerOfCity = Vector3.zero;
        for (int i = 0; i < numBuildings; ++i) {
            float x = Random.Range(-size, size);
            float z = Random.Range(-size, size);

            float sx = Random.Range(1.0f, 5.0f);
            float sz = Random.Range(1.0f, 5.0f);
            float sy = Random.Range(2.0f, 5.0f);

            // adds more height to buildings farther from center
            Vector3 pos = new Vector3(x, 0.0f, z);
            float dist = Vector3.Distance(pos, centerOfCity);
            float b = blend(dist, 0.0f, size);
            sy += b * 10.0f;

            GameObject go = BuildBoxObject("Building " + i, new Vector3(sx,sy,sz), true);
            go.transform.parent = buildings.transform;
            go.transform.localPosition = pos;

            // randomly rotate now
            float tilt = 20.0f - sy;
            if (tilt < 0.0f) tilt = -tilt;
            float rx = Random.Range(-tilt, tilt);
            float rz = Random.Range(-tilt, tilt);
            float ry = Random.Range(0.0f, 360.0f);
            go.transform.localRotation = Quaternion.Euler(new Vector3(rx, ry, rz));
        }

    }

    private float blend(float value, float low, float high) {
        return Mathf.Clamp01(cubic((value - low) / (high - low)));
    }

    private static float cubic(float v) {
        return v * v * v;
    }

    // builds box game object with origin centered on bottom
    // scale is length of edges in respective dimensions
    private GameObject BuildBoxObject(string name, Vector3 scale, bool randomColor = false) {
        verts.Clear();
        uvs.Clear();
        tris.Clear();
        tri = 0;


        Vector3 halfScale = scale / 2.0f;
        Vector3[] vrt = {
            new Vector3(-halfScale.x, 0.0f, -halfScale.z),
            new Vector3(+halfScale.x, 0.0f, -halfScale.z),
            new Vector3(+halfScale.x, 0.0f, +halfScale.z),
            new Vector3(-halfScale.x, 0.0f, +halfScale.z)
        };

        // 4 sides
        Vector3 hi = Vector3.up * scale.y;
        for (int i = 0; i < 4; i++) {
            Vector3 u = vrt[i];
            Vector3 v = vrt[0];
            if (i < vrt.Length - 1) {
                v = vrt[i + 1];
            }

            verts.Add(u);
            verts.Add(u + hi);
            verts.Add(v + hi);
            verts.Add(v);
        }
        //top
        for (int i = 3; i >= 0; i--) {
            verts.Add(vrt[i] + hi);
        }
        // bottom
        for (int i = 0; i < 4; i++) {
            verts.Add(vrt[i]);
        }
        // uvs and triangles
        for (int i = 0; i < 6; i++) {
            uvs.Add(new Vector2(0.0f, 0.0f));
            uvs.Add(new Vector2(1.0f, 0.0f));
            uvs.Add(new Vector2(1.0f, 1.0f));
            uvs.Add(new Vector2(0.0f, 1.0f));

            tris.Add(tri++);
            tris.Add(tri++);
            tris.Add(tri++);
            tris.Add(tri - 1);
            tris.Add(tri++);
            tris.Add(tri - 4);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GameObject go = new GameObject(name);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.layer = LayerMask.NameToLayer(Tags.Layers.Static);
        //go.isStatic = true;
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.material = buildingMat;

        // give each building a random hue (kinda ruins auto batching)
        if (randomColor) {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            Color color = Color.HSVToRGB(Random.value, 1.0f, 1.0f);
            mpb.SetColor("_Color", color);
            mr.SetPropertyBlock(mpb);
        }

        BoxCollider bc = go.AddComponent<BoxCollider>();
        bc.center = mesh.bounds.center;
        bc.size = mesh.bounds.size;

        return go;
    }
    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();
    int tri = 0;
}
