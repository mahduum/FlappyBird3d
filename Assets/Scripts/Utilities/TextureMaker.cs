using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class TextureMaker : MonoBehaviour
{
    [Range(2, 512)]
    public int resolution = 256;

    public float frequency = 1f;
    
    [Range(1f, 4f)]
    public float lacunarity = 2f;//the factor by which frequency changes

    [Range(0f, 1f)]
    public float persistence = 0.5f;//the multiplier by which amplitude changes
    
    [Range(1, 8)]
    public int octaves = 1;
    
    [Range(1, 3)]
    public int dimensions = 3;

    public FilterMode filterMode;

    public Gradient coloring;
    
    private Texture2D texture;

    private void OnEnable ()
    {
        if (texture == null)
        {
            texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);
            texture.name = "Procedural Texture";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = filterMode;
            texture.anisoLevel = 9;
            GetComponent<MeshRenderer>().material.mainTexture = texture;
        }

        FillTexture();
    }
    
    public void FillTexture ()
    {
        if (texture.width != resolution)
        {
            texture.Reinitialize(resolution, resolution);
        }
        
        Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f,-0.5f));
        Vector3 point10 = transform.TransformPoint(new Vector3( 0.5f,-0.5f));
        Vector3 point01 = transform.TransformPoint(new Vector3(-0.5f, 0.5f));
        Vector3 point11 = transform.TransformPoint(new Vector3( 0.5f, 0.5f));

        float stepSize = 1f / resolution;
        
        for (int y = 0; y < resolution; y++)
        {
            Vector3 point0 = Vector3.Lerp(point00, point01, (y + 0.5f) * stepSize);
            Vector3 point1 = Vector3.Lerp(point10, point11, (y + 0.5f) * stepSize);
            for (int x = 0; x < resolution; x++)
            {
                Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
                float sample = Noise.Sum(point, frequency, octaves, lacunarity, persistence);

                sample = sample * 0.5f + 0.5f;
                
                texture.SetPixel(x, y, coloring.Evaluate(sample));
                //texture.SetPixel(x, y, Color.white * sample);
                //texture.SetPixel(x, y, new Color(point.x, point.y, point.z));
                //texture.SetPixel(x, y, new Color((x + 0.5f) * stepSize%0.1f, (y + 0.5f) * stepSize%0.1f, 0f) * 10f);
            }
        }
        texture.Apply();
    }
    
    private void Update () {
        if (transform.hasChanged) {
            transform.hasChanged = false;
            FillTexture();
        }
    }
}

public static class Noise
{
            
    private static Vector2[] gradients2D = {
        new Vector2( 1f, 0f),
        new Vector2(-1f, 0f),
        new Vector2( 0f, 1f),
        new Vector2( 0f,-1f),
        new Vector2( 1f, 1f).normalized,
        new Vector2(-1f, 1f).normalized,
        new Vector2( 1f,-1f).normalized,
        new Vector2(-1f,-1f).normalized
    };
	
    private const int gradientsMask2D = 7;

    private static float Dot (Vector2 g, float x, float y)
    {
        return g.x * x + g.y * y;
    }
        
    private static float sqr2 = Mathf.Sqrt(2f);
    
    private static int hashMask = 255;
    
    private static int[] hash =
        {
            151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
            140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
            247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
            57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
            74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
            60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
            65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
            200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
            52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
            207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
            119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
            129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
            218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
            81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
            184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
            222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180,
            
            151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
            140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
            247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
            57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
            74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
            60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
            65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
            200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
            52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
            207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
            119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
            129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
            218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
            81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
            184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
            222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
        };
    
    private static float Smooth (float t)
    {
        return t * t * t * (t * (t * 6f - 15f) + 10f);
    }
    
    public static float Sum (Vector3 point, float frequency, int octaves, float lacunarity, float persistence)
    {
        float sum = Perlin2D(point, frequency);
        float amplitude = 1f;
        float range = 1f;
        for (int o = 0; o < octaves; o++)
        {
            frequency *= lacunarity;//2f;//doubling frequency
            amplitude *= persistence;//0.5f;//halving amplitude
            range += amplitude;//increasing range
            sum += Perlin2D(point, frequency) * amplitude;
        }

        return sum / range;
        //return (method(point, frequency) + method(point * 2f, frequency * 2f) * 0.5f) / 1.5f;//dividing by 1.5f for normalizing -1, 1
    }

    public static float Perlin2D (Vector3 point, float frequency) {
        point *= frequency;
        int ix0 = Mathf.FloorToInt(point.x);
        int iy0 = Mathf.FloorToInt(point.y);
        float tx0 = point.x - ix0;
        float ty0 = point.y - iy0;
        float tx1 = tx0 - 1f;
        float ty1 = ty0 - 1f;
        ix0 &= hashMask;
        iy0 &= hashMask;
        int ix1 = ix0 + 1;
        int iy1 = iy0 + 1;
		
        int h0 = hash[ix0];
        int h1 = hash[ix1];
        Vector2 g00 = gradients2D[hash[h0 + iy0] & gradientsMask2D];//gradient mask will get the values from 0 to 7 setting one out of 8 vectors
        Vector2 g10 = gradients2D[hash[h1 + iy0] & gradientsMask2D];
        Vector2 g01 = gradients2D[hash[h0 + iy1] & gradientsMask2D];
        Vector2 g11 = gradients2D[hash[h1 + iy1] & gradientsMask2D];

        float v00 = Dot(g00, tx0, ty0);//dot product between random vector and the coordinates of the point within unit grid sector
        float v10 = Dot(g10, tx1, ty0);
        float v01 = Dot(g01, tx0, ty1);
        float v11 = Dot(g11, tx1, ty1);
		
        float tx = Smooth(tx0);
        float ty = Smooth(ty0);
        return Mathf.Lerp(
            Mathf.Lerp(v00, v10, tx),
            Mathf.Lerp(v01, v11, tx),
            ty) * sqr2;
    }
}
