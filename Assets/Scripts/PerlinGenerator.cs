using UnityEngine;

/// <summary>
/// Helper class for creating Perlin Octaves.
/// </summary>
[System.Serializable]
public class PerlinGenerator {

    [SerializeField]
    [Range(1, 12)]
    [Tooltip("The level of detail in the Perlin noise. Higher = slower, but more detailed.")]
	protected int octaves = 8;

    [SerializeField]
    [Range(0.001f, 4f)]
    [Tooltip("How zoomed in the noise is. Higher = more zoomed in, so less detail.")]
    protected float frequency = 1.0f;

    [SerializeField]
    [Range(0.001f, 4f)]
    [Tooltip("How much we zoom out for finer details.")]
    protected float lacunarity = 2.0f;

    [SerializeField]
    [Range(0.001f, 4f)]
    [Tooltip("How much influence the finer details have. Lower = smoother, higher = more chaotic")]
	protected float persistence = 0.5f;

    [SerializeField]
    [Range(-10000f, 10000f)]
    [Tooltip("X offset for noise")]
	protected float xOffset = 0.0f;

    [SerializeField]
    [Range(-10000f, 10000f)]
    [Tooltip("Z offset for noise")]
    protected float zOffset = 0.0f;

	public PerlinGenerator(float xOffset=0.0f, float zOffset=0.0f, int octaves=8, float frequency=1.0f, float lacunarity=2.0f, float persistence=0.5f) {
		this.xOffset = xOffset;
		this.zOffset = zOffset;
		this.octaves = octaves;
		this.frequency = frequency;
		this.lacunarity = lacunarity;
		this.persistence = persistence;
    }

    /// <summary>
    /// Gets the value at a given (x, z) coordinate.
    /// </summary>
    /// <returns>A noise value in (roughly) [0, 1], based on the specified parameters.</returns>
    /// <param name="x">The x coordinate.</param>
    /// <param name="z">The z coordinate.</param>
	public float GetValue(float x, float z) {
        //return _FastNoise.GetPerlinFractal(x, z);
        //return _FastNoise.GetSimplexFractal(x, z);

        float value = 0.0f;
		float multiplier = 1.0f;
		x = (x + xOffset) * frequency;
		z = (z + zOffset) * frequency;
		for (int i = 0; i < octaves; i++) {
            value += multiplier * Mathf.PerlinNoise (x, z);
            // TODO why is FastNoise so slow here?
            //value += multiplier * _FastNoise.GetSimplex(x, z);
            //value += multiplier * _FastNoise.GetPerlin(x, z);

            multiplier *= persistence;
			x *= lacunarity;
			z *= lacunarity;
		}
		return value;
	}

    public float GetValueNormalized(float x, float z) 
    {
        float value = 0.0f;
        float multiplier = 1.0f;
        float normalizer = 0.0f;
        x = (x + xOffset) * frequency;
        z = (z + zOffset) * frequency;
        for (int i = 0; i < octaves; i++)
        {
            //float temp = multiplier * Mathf.PerlinNoise(x, z);
            //value += Mathf.Abs(temp - (multiplier / 2.0f));
            value += multiplier * Mathf.PerlinNoise(x, z);
            normalizer += multiplier;

            multiplier *= persistence;
            x *= lacunarity;
            z *= lacunarity;
        }
        //return 2 * value / normalizer;
        return value / normalizer;
    }

    public float GetBillowNormalized(float x, float z)
    {
        float value = 0.0f;
        float multiplier = 1.0f;
        float normalizer = 0.0f;
        x = (x + xOffset) * frequency;
        z = (z + zOffset) * frequency;
        for (int i = 0; i < octaves; i++)
        {
            float temp = multiplier * Mathf.PerlinNoise(x, z);
            value += Mathf.Abs(temp - (multiplier / 2.0f));
            normalizer += multiplier;

            multiplier *= persistence;
            x *= lacunarity;
            z *= lacunarity;
        }
        return 2 * value / normalizer;
    }
}
