using System.Collections.Generic;
using UnityEngine;

public class TemporalAA : MonoBehaviour, IPostProcessLayer
{
    public Shader shader;
    private Material _mat;

    private int _frameCount;
    // Start is called before the first frame update
    void Start()
    {
        //use halton sequence to generate an array of jitter vectors
        List<float> haltonSequenceX = GenerateHaltonSequence(2, 16);
        List<float> haltonSequenceY = GenerateHaltonSequence(3, 16);
        List<Vector4> jitterVectors = new List<Vector4>();
        for (int i = 0; i < 16; i++)
        {
            jitterVectors.Add(new Vector2(haltonSequenceX[i], haltonSequenceY[i]));
        }
        
        Shader.SetGlobalVectorArray("_JitterVectors", jitterVectors.ToArray());
    }

    List<float> GenerateHaltonSequence(int basePrime, int count)
    {
        List<float> haltonSequence = new List<float>();
        for (int i = 1; i <= count; i++)
        {
            int num = i;
            int iter = 1;
            float val = 0;
            while (num > 0)
            {
                int quotient = num / basePrime;
                int remainder = num % basePrime;

                val += remainder / Mathf.Pow(basePrime, iter);
                num = quotient;
                iter++;
            }
            
            haltonSequence.Add(val);
        }

        return haltonSequence;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_mat == null)
        {
            _mat = new Material(shader);
        }
        Shader.SetGlobalInt("_FrameCount", _frameCount);
        Graphics.Blit(source, destination, _mat, 0);

        _frameCount++;
        if (_frameCount > 15)
        {
            _frameCount = 0;
        }
    }
}
