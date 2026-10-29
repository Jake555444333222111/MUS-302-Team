using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Triangle3D : MonoBehaviour
{
    [Header("Triangle Corners (Local Position)")]
    [SerializeField] private Vector3 cornerA = new Vector3(0f, 2f, 0f);
    [SerializeField] private Vector3 cornerB = new Vector3(-1f, 0f, 0f);
    [SerializeField] private Vector3 cornerC = new Vector3(1f, 0f, 0f);

    [Header("Triangle Settings")]
    [Min(0.001f)][SerializeField] private float thickness = 0.25f;
    [Header("Mesh Settings")]
    [SerializeField] private bool generateCollider = true;

    private Mesh generatedMesh;
    private Vector3 lastA, lastB, lastC;
    private float lastThickness;
    private bool lastColliderSetting;
    private bool built;

    private void OnEnable() { Rebuild(); }

    private void OnValidate()
    {
        thickness = Mathf.Max(0.001f, thickness);
        // Do not destroy or rebuild meshes inside Unity's validation callback.
        built = false;
    }

    private void Update()
    {
        if (!built || cornerA != lastA || cornerB != lastB || cornerC != lastC ||
            !Mathf.Approximately(thickness, lastThickness) || generateCollider != lastColliderSetting)
            Rebuild();
    }

    [ContextMenu("Rebuild Triangle UV Mesh")]
    public void Rebuild()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter == null) return;

        float half = Mathf.Max(0.001f, thickness) * 0.5f;
        Vector3[] front = { cornerA + Vector3.back * half, cornerB + Vector3.back * half, cornerC + Vector3.back * half };
        Vector3[] back = { cornerA + Vector3.forward * half, cornerB + Vector3.forward * half, cornerC + Vector3.forward * half };

        float minX = Mathf.Min(cornerA.x, cornerB.x, cornerC.x);
        float maxX = Mathf.Max(cornerA.x, cornerB.x, cornerC.x);
        float minY = Mathf.Min(cornerA.y, cornerB.y, cornerC.y);
        float maxY = Mathf.Max(cornerA.y, cornerB.y, cornerC.y);
        float dx = Mathf.Max(0.0001f, maxX - minX);
        float dy = Mathf.Max(0.0001f, maxY - minY);
        Vector2[] faceUV = {
            new Vector2((cornerA.x-minX)/dx, (cornerA.y-minY)/dy),
            new Vector2((cornerB.x-minX)/dx, (cornerB.y-minY)/dy),
            new Vector2((cornerC.x-minX)/dx, (cornerC.y-minY)/dy)
        };

        var vertices = new System.Collections.Generic.List<Vector3>();
        var uv = new System.Collections.Generic.List<Vector2>();
        var triangles = new System.Collections.Generic.List<int>();

        // Separate vertices per face prevent UVs and normals from being shared across edges.
        AddTriangle(vertices, uv, triangles, front[0], front[1], front[2], faceUV[0], faceUV[1], faceUV[2]);
        AddTriangle(vertices, uv, triangles, back[0], back[2], back[1], faceUV[0], faceUV[2], faceUV[1]);
        for (int i = 0; i < 3; i++)
        {
            int j = (i + 1) % 3;
            float length = Vector3.Distance(front[i], front[j]);
            float uEnd = length / Mathf.Max(dx, dy);
            AddTriangle(vertices, uv, triangles, front[i], back[i], front[j],
                new Vector2(0, 0), new Vector2(0, 1), new Vector2(uEnd, 0));
            AddTriangle(vertices, uv, triangles, front[j], back[i], back[j],
                new Vector2(uEnd, 0), new Vector2(0, 1), new Vector2(uEnd, 1));
        }

        Mesh mesh = new Mesh { name = "Generated Triangle 3D UV" };
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uv);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        Mesh old = generatedMesh;
        generatedMesh = mesh;
        filter.sharedMesh = mesh;

        MeshCollider collider = GetComponent<MeshCollider>();
        if (generateCollider)
        {
            if (collider == null) collider = gameObject.AddComponent<MeshCollider>();
            collider.enabled = true;
            collider.sharedMesh = null;
            collider.sharedMesh = mesh;
        }
        else if (collider != null)
        {
            collider.sharedMesh = null;
            collider.enabled = false;
        }

        if (old != null && old != mesh)
        {
            if (Application.isPlaying) Destroy(old);
            else DestroyImmediate(old);
        }

        lastA = cornerA; lastB = cornerB; lastC = cornerC;
        lastThickness = thickness; lastColliderSetting = generateCollider;
        built = true;
    }

    private static void AddTriangle(System.Collections.Generic.List<Vector3> vertices,
        System.Collections.Generic.List<Vector2> uv, System.Collections.Generic.List<int> triangles,
        Vector3 a, Vector3 b, Vector3 c, Vector2 ua, Vector2 ub, Vector2 uc)
    {
        int index = vertices.Count;
        vertices.Add(a); vertices.Add(b); vertices.Add(c);
        uv.Add(ua); uv.Add(ub); uv.Add(uc);
        triangles.Add(index); triangles.Add(index + 1); triangles.Add(index + 2);
    }

    private void OnDestroy()
    {
        if (generatedMesh == null) return;

        Mesh mesh = generatedMesh;

        if (Application.isPlaying)
            Destroy(mesh);
        else
            DestroyImmediate(mesh);

        generatedMesh = null;
    }
}