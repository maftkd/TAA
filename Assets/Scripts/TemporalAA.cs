using System.Collections.Generic;
using UnityEngine;

public class TemporalAA : MonoBehaviour, IPostProcessLayer
{
    public Shader shader;
    private Material _mat;

    private int _frameCount;

    private RenderTexture _historyBuffer;

    [Range(0, 1)]
    public float modulationFactor;
    
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
        
        Shader.SetGlobalInt("_FrameCount", _frameCount);
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
        if(_historyBuffer == null || _historyBuffer.width != destination.width || _historyBuffer.height != destination.height)
        {
            _historyBuffer = new RenderTexture(destination.width, destination.height, 0, destination.format);
            _historyBuffer.filterMode = FilterMode.Bilinear;
            _historyBuffer.wrapMode = TextureWrapMode.Clamp;
            _mat.SetTexture("_HistoryBuffer", _historyBuffer);
        }
        _mat.SetFloat("_ModulationFactor", modulationFactor);
        
        RenderTexture tmp = RenderTexture.GetTemporary(_historyBuffer.width, _historyBuffer.height, 0, destination.format);

        source.filterMode = FilterMode.Point;
        Graphics.Blit(source, tmp, _mat, 0);
        Graphics.Blit(tmp, _historyBuffer);
        Graphics.Blit(_historyBuffer, destination);
        //Graphics.Blit(source, destination);

        _frameCount++;
        if (_frameCount > 15)
        {
            _frameCount = 0;
        }
        Shader.SetGlobalInt("_FrameCount", _frameCount);
        
        RenderTexture.ReleaseTemporary(tmp);
    }
}
