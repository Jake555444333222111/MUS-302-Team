
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class KandinskyBeamMapper : MonoBehaviour
{
    [Header("Pattern")]
    [Range(0.005f, 0.06f)]
    public float lineThickness = 0.018f;

    public Color cream = new Color(0.94f, 0.91f, 0.83f);
    public Color black = new Color(0.09f, 0.07f, 0.08f);
    public Color green = new Color(0.39f, 0.49f, 0.30f);
    public Color blue = new Color(0.39f, 0.66f, 0.76f);
    public Color orange = new Color(0.86f, 0.49f, 0.19f);

    [Header("Mapping")]
    public bool reverseLength = false;

    [Header("Generation")]
    public bool regenerate = false;

    private Mesh generatedMesh;
    private Material generatedMaterial;
    private Texture2D generatedTexture;

    private void OnEnable()
    {
        Generate();
    }

    private void OnValidate()
    {
        if (regenerate)
        {
            regenerate = false;
            Generate();
        }
    }

    public void Generate()
    {
        ClearGenerated();

        generatedTexture = CreatePattern();
        generatedMesh = CreateBeamMesh();

        Shader shader = Shader.Find(
            "Universal Render Pipeline/Unlit"
        );

        if (shader == null)
            shader = Shader.Find("Unlit/Texture");

        if (shader == null)
        {
            Debug.LogError("No compatible shader found.");
            return;
        }

        generatedMaterial = new Material(shader);
        generatedMaterial.name = "Kandinsky Beam Material";
        generatedMaterial.mainTexture = generatedTexture;

        GetComponent<MeshFilter>().sharedMesh = generatedMesh;
        GetComponent<MeshRenderer>().sharedMaterial =
            generatedMaterial;
    }

    private Texture2D CreatePattern()
    {
        const int width = 2048;
        const int height = 256;

        Texture2D tex = new Texture2D(
            width, height, TextureFormat.RGBA32, false
        );

        tex.name = "Kandinsky Beam Pattern";
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        // Fill the entire texture with cream.
        Color[] pixels = new Color[width * height];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = cream;

        tex.SetPixels(pixels);

        // Approximate sections from the circled painting.
        // Each entry: start, end, colour.
        Paint(tex, 0.00f, 0.08f, cream);
        Paint(tex, 0.08f, 0.15f, green);
        Paint(tex, 0.15f, 0.29f, cream);
        Paint(tex, 0.29f, 0.36f, black);
        Paint(tex, 0.36f, 0.43f, blue);
        Paint(tex, 0.43f, 0.51f, cream);
        Paint(tex, 0.51f, 0.57f, green);
        Paint(tex, 0.57f, 0.65f, cream);
        Paint(tex, 0.65f, 0.71f, black);
        Paint(tex, 0.71f, 0.77f, orange);
        Paint(tex, 0.77f, 0.88f, cream);
        Paint(tex, 0.88f, 0.94f, black);
        Paint(tex, 0.94f, 1.00f, cream);

        // Black dividing lines between sections.
        float[] divisions =
        {
            0.08f, 0.15f, 0.29f, 0.36f,
            0.43f, 0.51f, 0.57f, 0.65f,
            0.71f, 0.77f, 0.88f, 0.94f
        };

        foreach (float position in divisions)
        {
            Paint(
                tex,
                position - lineThickness / 2f,
                position + lineThickness / 2f,
                black
            );
        }

        // Black lines along both long edges.
        int border = Mathf.RoundToInt(
            height * lineThickness
        );

        for (int y = 0; y < height; y++)
        {
            if (y < border || y >= height - border)
            {
                for (int x = 0; x < width; x++)
                    tex.SetPixel(x, y, black);
            }
        }

        tex.Apply();
        return tex;
    }

    private void Paint(
        Texture2D tex,
        float start,
        float end,
        Color color
    )
    {
        int x0 = Mathf.Clamp(
            Mathf.RoundToInt(start * tex.width),
            0, tex.width
        );

        int x1 = Mathf.Clamp(
            Mathf.RoundToInt(end * tex.width),
            0, tex.width
        );

        for (int x = x0; x < x1; x++)
        {
            for (int y = 0; y < tex.height; y++)
                tex.SetPixel(x, y, color);
        }
    }

    private Mesh CreateBeamMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Kandinsky UV Beam";

        Vector3[] vertices = new Vector3[24];
        Vector2[] uv = new Vector2[24];
        int[] triangles = new int[36];

        int face = 0;

        // Four long faces of the cube.
        AddFace(
            vertices, uv, triangles, ref face,
            new Vector3(-.5f, -.5f, -.5f),
            new Vector3(-.5f, -.5f, .5f),
            new Vector3(-.5f, .5f, .5f),
            new Vector3(-.5f, .5f, -.5f),
            true
        );

        AddFace(
            vertices, uv, triangles, ref face,
            new Vector3(.5f, -.5f, -.5f),
            new Vector3(.5f, -.5f, .5f),
            new Vector3(.5f, .5f, .5f),
            new Vector3(.5f, .5f, -.5f),
            true
        );

        AddFace(
            vertices, uv, triangles, ref face,
            new Vector3(-.5f, .5f, -.5f),
            new Vector3(-.5f, .5f, .5f),
            new Vector3(.5f, .5f, .5f),
            new Vector3(.5f, .5f, -.5f),
            true
        );

        AddFace(
            vertices, uv, triangles, ref face,
            new Vector3(-.5f, -.5f, -.5f),
            new Vector3(-.5f, -.5f, .5f),
            new Vector3(.5f, -.5f, .5f),
            new Vector3(.5f, -.5f, -.5f),
            true
        );

        // End caps.
        AddFace(
            vertices, uv, triangles, ref face,
            new Vector3(-.5f, -.5f, -.5f),
            new Vector3(.5f, -.5f, -.5f),
            new Vector3(.5f, .5f, -.5f),
            new Vector3(-.5f, .5f, -.5f),
            false
        );

        AddFace(
            vertices, uv, triangles, ref face,
            new Vector3(-.5f, -.5f, .5f),
            new Vector3(.5f, -.5f, .5f),
            new Vector3(.5f, .5f, .5f),
            new Vector3(-.5f, .5f, .5f),
            false
        );

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private void AddFace(
        Vector3[] vertices,
        Vector2[] uv,
        int[] triangles,
        ref int face,
        Vector3 a, Vector3 b,
        Vector3 c, Vector3 d,
        bool lengthwise
    )
    {
        int v = face * 4;
        int t = face * 6;

        vertices[v] = a;
        vertices[v + 1] = b;
        vertices[v + 2] = c;
        vertices[v + 3] = d;

        float u0 = reverseLength ? 1f : 0f;
        float u1 = reverseLength ? 0f : 1f;

        uv[v] = new Vector2(u0, 0);
        uv[v + 1] = new Vector2(u1, 0);
        uv[v + 2] = new Vector2(u1, 1);
        uv[v + 3] = new Vector2(u0, 1);

        if (!lengthwise)
        {
            uv[v] = new Vector2(0, 0);
            uv[v + 1] = new Vector2(1, 0);
            uv[v + 2] = new Vector2(1, 1);
            uv[v + 3] = new Vector2(0, 1);
        }

        // Ensure triangles face outwards.
        Vector3 normal = Vector3.Cross(b - a, c - a);
        Vector3 center = (a + b + c + d) / 4f;
        bool outward = Vector3.Dot(normal, center) > 0;

        if (outward)
        {
            triangles[t] = v;
            triangles[t + 1] = v + 1;
            triangles[t + 2] = v + 2;
            triangles[t + 3] = v;
            triangles[t + 4] = v + 2;
            triangles[t + 5] = v + 3;
        }
        else
        {
            triangles[t] = v;
            triangles[t + 1] = v + 2;
            triangles[t + 2] = v + 1;
            triangles[t + 3] = v;
            triangles[t + 4] = v + 3;
            triangles[t + 5] = v + 2;
        }

        face++;
    }

    private void ClearGenerated()
    {
        if (generatedMesh != null)
            DestroyImmediate(generatedMesh);

        if (generatedMaterial != null)
            DestroyImmediate(generatedMaterial);

        if (generatedTexture != null)
            DestroyImmediate(generatedTexture);
    }
}
