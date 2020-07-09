using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudGen : MonoBehaviour
{
    public Material CloudMaterial;

    // Note that you can tile the material texture 2x or even 4x, and set the scale similarly.
    // This causes the appearance of higher resolution directly overhead, while causing repetition at distance.
    // TODO might be possible to reduce repetition by offsetting the distant areas, or via masking.
    [Range(0.01f, 1024f)]
    public float CloudScale;
    private float _LastCloudScale;

    PerlinGenerator perlin = new PerlinGenerator(-1234, 41292, 6);
    PerlinGenerator perlinMask = new PerlinGenerator(2391, -9121, 1);

    private Texture2D[] CloudTextures;

    [Range(1, 4096)]
    public int Width = 32;

    [Range(1, 4096)]
    public int Height = 32;

    [Range(1, 128)]
    public int Depth = 1;

    private int _LastWidth;
    private int _LastHeight;
    private int _LastDepth;

    // Should we use the Billow algorithm (sum of absolute value of Perlin noise ranging from -0.5 to 0.5)
    public bool Billow = false;

    // How dense is the cloud layer?
    [Range(0.0f, 1.0f)]
    public float Density = 0.5f;
    private float _LastDensity;

    public bool ShouldUseDynamicAsset = false;
    public bool ShouldCreateAsset = false;

    // Start is called before the first frame update
    void Start()
    {
        //if (CloudMaterial == null) 
        //{
        //    //Debug.LogError("Failed to create cloud texture- material was null!");
        //    //return;
        //}
        //Debug.Log("Cloud material dimensions: " + CloudMaterial.mainTexture.dimension);

        CloudTextures = new Texture2D[Depth];
        //Color[] dataPoints = new Color[width * height * depth];
        for (int z = 0; z < Depth; z++)
        {
            CloudTextures[z] = new Texture2D(Width, Height, TextureFormat.RGBA4444, true);
        }

        if (ShouldUseDynamicAsset)
        {
            Update();
        }

        if (ShouldCreateAsset)
        {
            string parentDir = Application.dataPath;
            string fileName = "CloudNoise.png";
            Debug.Log("Writing out to directory: " + parentDir + "/" + fileName);
            System.IO.File.WriteAllBytes(parentDir + "/" + fileName, CloudTextures[0].EncodeToPNG());
        }

        //CloudTexture = new Texture3D(width, height, depth, TextureFormat.RGBA32, true);
        //CloudTexture.SetPixels(dataPoints);
        //CloudTexture.Apply();

        // Set the texture of the material to be the one we generated
        if (ShouldUseDynamicAsset)
        {
            if (CloudMaterial == null)
            {
                Debug.LogError("Failed to create cloud texture- material was null!");
                return;
            }
            CloudMaterial.mainTexture = CloudTextures[0];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!ShouldUseDynamicAsset)
        {
            return;
        }

        if (Depth != _LastDepth)
        {
            CloudTextures = new Texture2D[Depth];
            ResizeTextures();
            RedrawTextures();
            _LastDepth = Depth;
        }

        if (Width != _LastWidth || Height != _LastHeight || !Mathf.Approximately(Density, _LastDensity))
        {
            ResizeTextures();
            RedrawTextures();
            _LastWidth = Width;
            _LastHeight = Height;
            _LastDensity = Density;
        }

        if (!Mathf.Approximately(_LastCloudScale, CloudScale))
        {
            // Update all the textures to reflect new changes
            RedrawTextures();
            _LastCloudScale = CloudScale;
        }

        if (CloudMaterial == null)
        {
            Debug.LogError("Failed to create cloud texture- material was null!");
            return;
        }
        CloudMaterial.mainTexture = CloudTextures[0];

        // General update
        CloudMaterial.SetFloat("_OffsetX", Time.time / 100f);
        CloudMaterial.SetFloat("_OffsetY", Time.time / 100f);
        //CloudMaterial.SetFloat("_CloudDensity", (Mathf.Sin(Time.time) / 2.0f) + 0.5f);
    }

    // Does some post-processing on the noise to adjust based on Density
    private float AdjustNoise(float noise) 
    {
        float delta = (Density * 2.0f) - 1.0f;
        noise += delta; // Reduce by an amount

        // Note: this is the original way. Looks good overall for both Billow and regular.
        //noise += delta; // Reduce by an amount
        //noise = Mathf.Clamp(noise, 0f, 1f); // Clamp to [0, 1]
        //noise /= Density * 2.0f; // Normalize range back to [0, 1]
        //if (Density < 0.5f)
        //{
        //    noise *= 1.0f + (2.0f * (0.5f - Density));
        //}


        // Modified- uses a clear -> gray -> white gradient for range [0, 2.0].
        // Looks better on regular noise, worse on Billow.
        if (Density > 0.5f)
        {

        }
        else
        {
            noise = Mathf.Clamp(noise, 0f, 1f); // Clamp to [0, 1]
            noise /= Density * 2.0f; // Normalize range back to [0, 1]

             // This just looks good by correcting for the average darkening effect that happens at
             // low densities. Should do something similar for high density.
            if (Density < 0.5f)
            {
                noise *= 1.0f + (2.0f * (0.5f - Density));
            }
            else
            {
                // TODO something similar for Density > 0.5?
            }
        }
        return noise;
    }

    void RedrawTextures()
    {
        Debug.Log("Delta = " + ((Density * 2.0f) - 1.0f) + ", and dividing by " + ((Density * 2.0f)));
        float delta = (Density * 2.0f) - 1.0f;
        Color[] dataPoints = new Color[Width * Height];
        for (int z = 0; z < Depth; z++)
        {
            // Generate W/2 by H/2 noise texture
            for (int x = 0; x < Width / 2; x++)
            {
                for (int y = 0; y < Height / 2; y++)
                {
                    float noise;
                    if (Billow)
                    {
                        noise = perlin.GetBillowNormalized(0.33f + (x / CloudScale), -0.45f + (y / CloudScale));
                    }
                    else
                    {
                        noise = perlin.GetValueNormalized(0.33f + (x / CloudScale), -0.45f + (y / CloudScale));
                    }
                    //noise = AdjustNoise(noise);

                    // Masking layer
                    // TODO make this adjustable in the shader
                    noise *= perlinMask.GetValueNormalized(0.33f + (x / CloudScale / 2f), -0.45f + (y / CloudScale / 2f));

                    //noise = Mathf.Sqrt(noise);



                    //Debug.Log("Noise=" + noise);
                    // Note: clear -> white looks good for low density. clear -> gray -> white looks good for high density.
                    // I think clear -> gray -> white is good for an overall brightness (maybe slightly modified would be best);
                    // but there should be a separate slider for storminess that makes things darker or lighter.
                    //if (noise > 1.0f)
                    //{
                    //    dataPoints[(x * Height) + y] = Color.Lerp(Color.gray, Color.white, noise - 1.0f);
                    //}
                    //else
                    //{
                        dataPoints[(x * Height) + y] = Color.Lerp(Color.clear, Color.white, noise);
                    //}
                }
            }

            // Now we mirror the texture 4 ways about the origin. This is a cheap trick to make the texture seamless.

            // Mirror the contents across the Y axis (left-right)
            for (int y = 0; y < Height / 2; y++)
            {
                for (int x = 0; x < Width / 2; x++)
                {
                    dataPoints[(((Width / 2) + x) * Height) + y] = dataPoints[(((Width / 2) - x - 1) * Height) + y];
                }
            }

            // Mirror the new contents across the X axis (up-down)
            for (int y = 0; y < Height / 2; y++) 
            {
                for (int x = 0; x < Width; x++)
                {
                    dataPoints[(x * Height) + (Height / 2) + y] = dataPoints[(x * Height) + (Height / 2) - y - 1];
                }
            }

            CloudTextures[z].SetPixels(dataPoints);
            CloudTextures[z].Apply();
        }
    }

    void ResizeTextures()
    {
        for (int z = 0; z < Depth; z++)
        {
            if (CloudTextures[z] == null)
            {
                CloudTextures[z] = new Texture2D(Width, Height, TextureFormat.RGBA32, true);
            }
            else
            {
                CloudTextures[z].Resize(Width, Height);
            }
        }
    }
}
