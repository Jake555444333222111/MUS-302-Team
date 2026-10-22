
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CurvedBlockout : MonoBehaviour
{
    [System.Serializable]
    public class ControlPoint
    {
        public Vector3 position;

        public ControlPoint(Vector3 pos)
        {
            position = pos;
        }
    }

    [Header("Control Points")]
    public List<ControlPoint> points = new List<ControlPoint>
    {
        new ControlPoint(new Vector3(0f, 0f, 0f)),
        new ControlPoint(new Vector3(0f, 0f, 2f)),
        new ControlPoint(new Vector3(1f, 0f, 4f)),
        new ControlPoint(new Vector3(2f, 0f, 6f))
    };

    [Header("Blockout Dimensions")]
    [Min(0.01f)]
    public float width = 1f;

    [Min(0.01f)]
    public float height = 1f;

    [Header("Curve Quality")]
    [Range(2, 32)]
    public int subdivisionsPerSection = 8;

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
        subdivisionsPerSection =
            Mathf.Max(2, subdivisionsPerSection);

        width = Mathf.Max(0.01f, width);
        height = Mathf.Max(0.01f, height);

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
        if (points == null || points.Count < 2)
            return;

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
        generatedMesh.name = "Curved Blockout Mesh";

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        int sectionCount = points.Count - 1;
        int ringCount =
            sectionCount * subdivisionsPerSection + 1;

        Vector3 previousRight = Vector3.right;

        // Create rectangular cross-sections along the curve.
        for (int i = 0; i < ringCount; i++)
        {
            float t = (float)i / subdivisionsPerSection;
            int section = Mathf.Min(
                Mathf.FloorToInt(t), sectionCount - 1);

            float sectionT = t - section;

            if (i == ringCount - 1)
                sectionT = 1f;

            Vector3 center = GetCurvePoint(section, sectionT);
            Vector3 tangent = GetCurveTangent(section, sectionT);

            if (tangent.sqrMagnitude < 0.0001f)
                tangent = Vector3.forward;

            tangent.Normalize();

            // Keep the rectangular cross-section perpendicular
            // to the direction of the curve.
            Vector3 right = Vector3.Cross(
                Vector3.up, tangent);

            if (right.sqrMagnitude < 0.0001f)
            {
                right = previousRight -
                    Vector3.Dot(previousRight, tangent) * tangent;

                if (right.sqrMagnitude < 0.0001f)
                    right = Vector3.Cross(Vector3.forward, tangent);
            }

            right.Normalize();
            previousRight = right;

            Vector3 up = Vector3.Cross(tangent, right).normalized;

            Vector3 halfWidth = right * (width * 0.5f);
            Vector3 halfHeight = up * (height * 0.5f);

            // Four corners of the rectangular cross-section.
            vertices.Add(center - halfWidth - halfHeight);
            vertices.Add(center + halfWidth - halfHeight);
            vertices.Add(center + halfWidth + halfHeight);
            vertices.Add(center - halfWidth + halfHeight);
        }

        // Connect every cross-section to the next one.
        for (int i = 0; i < ringCount - 1; i++)
        {
            int current = i * 4;
            int next = current + 4;

            for (int corner = 0; corner < 4; corner++)
            {
                int nextCorner = (corner + 1) % 4;

                int a = current + corner;
                int b = current + nextCorner;
                int c = next + nextCorner;
                int d = next + corner;

                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(c);

                triangles.Add(a);
                triangles.Add(c);
                triangles.Add(d);
            }
        }

        // Cap the start and end to make a closed solid.
        int startCenter = vertices.Count;
        vertices.Add(GetCurvePoint(0, 0));

        int endCenter = vertices.Count;
        vertices.Add(GetCurvePoint(sectionCount - 1, 1));

        for (int corner = 0; corner < 4; corner++)
        {
            int nextCorner = (corner + 1) % 4;

            // Start cap.
            triangles.Add(startCenter);
            triangles.Add(nextCorner);
            triangles.Add(corner);

            // End cap.
            int lastRing = (ringCount - 1) * 4;

            triangles.Add(endCenter);
            triangles.Add(lastRing + corner);
            triangles.Add(lastRing + nextCorner);
        }

        generatedMesh.SetVertices(vertices);
        generatedMesh.SetTriangles(triangles, 0);
        generatedMesh.RecalculateNormals();
        generatedMesh.RecalculateBounds();

        meshFilter.sharedMesh = generatedMesh;

        // Optionally generate collision matching the mesh.
        if (generateCollider)
        {
            if (meshCollider == null)
                meshCollider = GetComponent<MeshCollider>();

            if (meshCollider == null)
                meshCollider = gameObject.AddComponent<MeshCollider>();

            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = generatedMesh;
        }
        else
        {
            meshCollider = GetComponent<MeshCollider>();

            if (meshCollider != null)
            {
                if (Application.isPlaying)
                    Destroy(meshCollider);
                else
                    DestroyImmediate(meshCollider);
            }
        }
    }

    private Vector3 GetCurvePoint(int section, float t)
    {
        Vector3 p0 = points[Mathf.Max(section - 1, 0)].position;
        Vector3 p1 = points[section].position;
        Vector3 p2 = points[section + 1].position;
        Vector3 p3 = points[Mathf.Min(section + 2, points.Count - 1)].position;

        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    private Vector3 GetCurveTangent(int section, float t)
    {
        const float delta = 0.001f;

        Vector3 a = GetCurvePoint(
            section, Mathf.Max(0f, t - delta));

        Vector3 b = GetCurvePoint(
            section, Mathf.Min(1f, t + delta));

        return b - a;
    }
}

