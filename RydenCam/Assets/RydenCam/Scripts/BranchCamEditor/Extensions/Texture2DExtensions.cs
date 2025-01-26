using UnityEngine;

public static class Texture2DExtensions
{
    public static bool IsTextureEmpty(this Texture2D texture)
    {
        if (texture == null)
            return true; // Consider a null texture as "empty"

        Color32[] pixels = texture.GetPixels32();

        // Assume the texture is empty if all pixels are the same
        Color32 firstPixel = pixels[0];

        foreach (var pixel in pixels)
        {
            if (!pixel.Equals(firstPixel))
            {
                return false; // Found a pixel that is different
            }
        }

        // All pixels are the same, the texture is considered "empty"
        return true;
    }
}
