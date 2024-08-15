using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Texture2DExtensions
{
    public static Texture2D DeepCopy(this Texture2D original)
    {
        // Create a new texture with the same dimensions and format as the original
        Texture2D newTexture = new Texture2D(original.width, original.height, original.format, original.mipmapCount > 1);

        // Copy the pixel data from the original texture to the new texture
        newTexture.SetPixels(original.GetPixels());

        newTexture.Apply();

        return newTexture;
    }

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
