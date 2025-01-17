using UnityEngine;

public class TemporalAA : MonoBehaviour, IPostProcessLayer
{
    public Shader shader;
    private Material _mat;
    // Start is called before the first frame update
    void Start()
    {
        
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
