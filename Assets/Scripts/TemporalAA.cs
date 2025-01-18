using System.Collections.Generic;
using UnityEngine;

public class TemporalAA : MonoBehaviour, IPostProcessLayer
{
    public Shader shader;
    private Material _mat;
    // Start is called before the first frame update
    void Start()
    {
        //use halton sequence to generate an array of jitter vectors
        int basePrime = 2;
        List<float> haltonSequence = new List<float>();
        for (int i = 1; i <= 16; i++)
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
        foreach(float f in haltonSequence)
        {
            Debug.Log(f);
        }
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
        Graphics.Blit(source, destination, _mat, 0);
    }
}
