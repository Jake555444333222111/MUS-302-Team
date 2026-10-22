
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CurvedCube : MonoBehaviour
{
    [Header("Cube Dimensions")]
    [Min(0.1f)] public float length = 4f;
    [Min(0.1f)] public float width = 1f;
    [Min(0.1f)] public float height = 1f;

    [Header("Bending")]
    [Tooltip("Moves the middle along X. Both ends stay fixed.")]
    public float controlPointX = 1f;

    [Header("Curve Quality")]
    [Range(4, 64)] public int subdivisions = 24;

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
        length = Mathf.Max(0.1f, length);
        width = Mathf.Max(0.1f, width);
        height = Mathf.Max(0.1f, height);
        subdivisions = Mathf.Clamp(subdivisions, 4, 64);

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

    [ContextMenu("Regenerate Mesh")]
    public void GenerateMesh()
    {
        if (meshFilter == null)
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
        generatedMesh.name = "Curved Cube Mesh";

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;

        // Create cross-sections along the length.
        for (int i = 0; i <= subdivisions; i++)
        {
            float t = (float)i / subdivisions;

            // Fixed endpoints: bend is exactly zero at t = 0 and t = 1.
            float bend = 4f * t * (1f - t) * controlPointX;

            // A second curve term counteracts end rotation,
            // keeping the end faces square.
            float tangentX = 4f * (1f - 2f * t) * controlPointX;

            Vector3 center = new Vector3(
                bend,
                0f,
                (t - 0.5f) * length
            );

            // Local cross-section orientation.
            // At each end, tangentX is opposite in sign, so instead
            // use a smooth orientation that returns to zero at both ends.
            float endFactor = Mathf.Sin(Mathf.PI * t);
            float angle = Mathf.Atan(tangentX) * endFactor * 0.5f;

            Vector3 right = new Vector3(
                Mathf.Cos(angle), 0f, -Mathf.Sin(angle));

            Vector3 up = Vector3.up;

            vertices.Add(center - right * halfWidth - up * halfHeight);
            vertices.Add(center + right * halfWidth - up * halfHeight);
            vertices.Add(center + right * halfWidth + up * halfHeight);
            vertices.Add(center - right * halfWidth + up * halfHeight);
        }

        // Connect adjacent cross-sections.
        for (int i = 0; i < subdivisions; i++)
        {
            int a = i * 4;
            int b = a + 4;

            // Bottom
            AddQuad(triangles, a, b, b + 1, a + 1);

            // Right side
            AddQuad(triangles, a + 1, b + 1, b + 2, a + 2);

            // Top
            AddQuad(triangles, a + 2, b + 2, b + 3, a + 3);

            // Left side
            AddQuad(triangles, a + 3, b + 3, b, a);
        }

        // Cap both ends.
        int last = subdivisions * 4;

        AddQuad(triangles, 0, 3, 2, 1);
        AddQuad(triangles, last, last + 1, last + 2, last + 3);

        generatedMesh.SetVertices(vertices);
        generatedMesh.SetTriangles(triangles, 0);
        generatedMesh.RecalculateNormals();
        generatedMesh.RecalculateBounds();

        meshFilter.sharedMesh = generatedMesh;

        // Keep collision aligned with the generated mesh.
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
