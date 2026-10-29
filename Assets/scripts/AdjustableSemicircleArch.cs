
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SemiCircle : MonoBehaviour
{
    [Header("Arch Shape")]
    [Min(0.1f)]
    public float outerRadius = 3f;

    [Min(0.01f)]
    public float innerRadius = 2.7f;

    [Min(0.01f)]
    public float depth = 0.5f;

    [Header("Smoothness")]
    [Range(12, 128)]
    public int segments = 48;

    [Header("Collision")]
    public bool generateCollider = true;

    private Mesh generatedMesh;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private void OnEnable()
    {
        GenerateMesh();
    }

    private void OnValidate()
    {
        outerRadius = Mathf.Max(0.1f, outerRadius);
        innerRadius = Mathf.Clamp(
            innerRadius, 0.01f,
            Mathf.Max(0.02f, outerRadius - 0.01f));

        depth = Mathf.Max(0.01f, depth);
        segments = Mathf.Clamp(segments, 12, 128);

        if (isActiveAndEnabled)
            GenerateMesh();
    }

    private void OnDestroy()
    {
        if (generatedMesh != null)
        {
            if (Application.isPlaying)
                Destroy(generatedMesh);
            else
                DestroyImmediate(generatedMesh);
        }
    }

    [ContextMenu("Regenerate Arch")]
    public void GenerateMesh()
    {
        if (outerRadius <= innerRadius ||
            innerRadius <= 0f)
            return;

        meshFilter = GetComponent<MeshFilter>();

        if (meshFilter == null)
            return;

        if (generatedMesh != null)
        {
            if (Application.isPlaying)
                Destroy(generatedMesh);
            else
                DestroyImmediate(generatedMesh);
        }

        generatedMesh = new Mesh();
        generatedMesh.name = "SemiCircle Arch Mesh";

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float halfDepth = depth * 0.5f;

        // Create the front and back corners
        // along the inner and outer semicircles.
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.PI * i / segments;

            float x = Mathf.Cos(angle);
            float z = Mathf.Sin(angle);

            vertices.Add(new Vector3(
                outerRadius * x, -halfDepth, outerRadius * z));

            vertices.Add(new Vector3(
                outerRadius * x, halfDepth, outerRadius * z));

            vertices.Add(new Vector3(
                innerRadius * x, -halfDepth, innerRadius * z));

            vertices.Add(new Vector3(
                innerRadius * x, halfDepth, innerRadius * z));
        }

        // Connect the corners to create solid surfaces.
        for (int i = 0; i < segments; i++)
        {
            int a = i * 4;
            int b = (i + 1) * 4;

            // Outer curved surface
            AddQuad(triangles, a, b, b + 1, a + 1);

            // Inner curved surface
            AddQuad(triangles,
                a + 2, a + 3, b + 3, b + 2);

            // Front surface
            AddQuad(triangles, a, a + 2, b + 2, b);

            // Back surface
            AddQuad(triangles,
                a + 1, b + 1, b + 3, a + 3);
        }

        // Close the two ends.
        int last = segments * 4;

        AddQuad(triangles, 0, 1, 3, 2);

        AddQuad(triangles,
            last, last + 2, last + 3, last + 1);

        generatedMesh.SetVertices(vertices);
        generatedMesh.SetTriangles(triangles, 0);
        generatedMesh.RecalculateNormals();
        generatedMesh.RecalculateBounds();

        meshFilter.sharedMesh = generatedMesh;

        // Create a matching collider.
        meshCollider = GetComponent<MeshCollider>();

        if (generateCollider)
        {
            if (meshCollider == null)
                meshCollider = gameObject.AddComponent<MeshCollider>();

            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = generatedMesh;
        }
        else if (meshCollider != null)
        {
            if (Application.isPlaying)
                Destroy(meshCollider);
            else
                DestroyImmediate(meshCollider);
        }
    }

    private static void AddQuad(
        List<int> triangles,
        int a, int b, int c, int d)
    {
        triangles.Add(a);
        triangles.Add(b);
        triangles.Add(c);

        triangles.Add(a);
        triangles.Add(c);
        triangles.Add(d);
    }
}
