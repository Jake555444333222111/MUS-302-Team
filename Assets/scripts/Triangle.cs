using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Triangle3D : MonoBehaviour
{
    [Header("Triangle Corners (Local Position)")]

    [SerializeField]
    private Vector3 cornerA = new Vector3(0f, 2f, 0f);

    [SerializeField]
    private Vector3 cornerB = new Vector3(-1f, 0f, 0f);

    [SerializeField]
    private Vector3 cornerC = new Vector3(1f, 0f, 0f);

    [Header("Triangle Settings")]

    [Min(0.001f)]
    [SerializeField]
    private float thickness = 0.25f;

    [Header("Mesh Settings")]

    [SerializeField]
    private bool generateCollider = true;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Mesh generatedMesh;

    private Vector3 lastA;
    private Vector3 lastB;
    private Vector3 lastC;
    private float lastThickness;
    private bool lastColliderSetting;

    private void Awake()
    {
        Initialise();
        GenerateTriangle();
    }

    private void OnEnable()
    {
        Initialise();
        GenerateTriangle();
    }

    private void Initialise()
    {
        meshFilter = GetComponent<MeshFilter>();

        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        if (GetComponent<MeshRenderer>() == null)
            gameObject.AddComponent<MeshRenderer>();

        if (generateCollider && meshCollider == null)
        {
            meshCollider = GetComponent<MeshCollider>();

            if (meshCollider == null)
                meshCollider = gameObject.AddComponent<MeshCollider>();
        }
    }

    private void OnValidate()
    {
        thickness = Mathf.Max(0.001f, thickness);

        if (!Application.isPlaying)
        {
            Initialise();
            GenerateTriangle();
        }
    }

    private void Update()
    {
        if (cornerA != lastA ||
            cornerB != lastB ||
            cornerC != lastC ||
            !Mathf.Approximately(thickness, lastThickness) ||
            generateCollider != lastColliderSetting)
        {
            Initialise();
            GenerateTriangle();
        }
    }

    private void GenerateTriangle()
    {
        if (meshFilter == null)
            return;

        // Release the previous generated mesh.
        if (generatedMesh != null)
        {
            if (Application.isPlaying)
                Destroy(generatedMesh);
            else
                DestroyImmediate(generatedMesh);
        }

        generatedMesh = new Mesh();
        generatedMesh.name = "Generated Triangle 3D";

        float halfThickness = thickness * 0.5f;

        // Six vertices: three on the front and three on the back.
        Vector3[] vertices = new Vector3[]
        {
            cornerA + Vector3.back * halfThickness,
            cornerB + Vector3.back * halfThickness,
            cornerC + Vector3.back * halfThickness,

            cornerA + Vector3.forward * halfThickness,
            cornerB + Vector3.forward * halfThickness,
            cornerC + Vector3.forward * halfThickness
        };

        // Front, back and three side faces.
        int[] triangles = new int[]
        {
            // Front face
            0, 1, 2,

            // Back face
            3, 5, 4,

            // Side AB
            0, 3, 1,
            1, 3, 4,

            // Side BC
            1, 4, 2,
            2, 4, 5,

            // Side CA
            2, 5, 0,
            0, 5, 3
        };

        generatedMesh.vertices = vertices;
        generatedMesh.triangles = triangles;

        generatedMesh.RecalculateNormals();
        generatedMesh.RecalculateBounds();

        meshFilter.sharedMesh = generatedMesh;

        // Add or update the optional collision mesh.
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

            meshCollider = null;
        }

        // Remember settings to detect Inspector changes.
        lastA = cornerA;
        lastB = cornerB;
        lastC = cornerC;
        lastThickness = thickness;
        lastColliderSetting = generateCollider;
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
}
