using UnityEngine;

[ExecuteAlways]
public class TextureScaler3D : MonoBehaviour
{
    public Renderer targetRenderer;
    public float pixelsPerMeter = 2094.5f;
    public int textureResolution = 4096;

       void OnValidate()
{
    if (targetRenderer == null)
    {
        targetRenderer = GetComponent<Renderer>();
    }
}

    void Update()
    {
        if (targetRenderer == null) return;

        Vector3 scale = transform.lossyScale;

        // Use X and Z for horizontal tiling in 3D
        float tilingX = (pixelsPerMeter / textureResolution) * scale.x;
        float tilingY = (pixelsPerMeter / textureResolution) * scale.z;

        // Apply tiling safely depending on mode
        if (!Application.isPlaying)
        {
            targetRenderer.sharedMaterial.mainTextureScale = new Vector2(tilingX, tilingY);
        }
        else
        {
            targetRenderer.material.mainTextureScale = new Vector2(tilingX, tilingY);
        }
    }
}