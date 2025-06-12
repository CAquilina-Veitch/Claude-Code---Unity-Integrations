using UnityEngine;

public class ClickableColorChanger : MonoBehaviour
{
    private Renderer objectRenderer;
    private Color[] predefinedColors = {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.magenta,
        Color.cyan,
        Color.white
    };
    private int currentColorIndex = 0;

    void Start()
    {
        SetupCube();
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogError("ClickableColorChanger requires a Renderer component!");
        }
    }

    void SetupCube()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        
        if (meshFilter != null && meshRenderer != null)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            meshFilter.mesh = cube.GetComponent<MeshFilter>().mesh;
            meshRenderer.material = cube.GetComponent<MeshRenderer>().material;
            DestroyImmediate(cube);
        }
    }

    void OnMouseDown()
    {
        if (objectRenderer != null)
        {
            currentColorIndex = (currentColorIndex + 1) % predefinedColors.Length;
            objectRenderer.material.color = predefinedColors[currentColorIndex];
        }
    }
}