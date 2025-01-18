using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderPipelineManager : MonoBehaviour
{
    private Camera _cam;

    private RenderTexture _albedo;
    private RenderTexture _normal;
    private RenderTexture _position;
    private RenderTexture _velocity;
    
    private int _prevWidth;
    private int _prevHeight;

    public Shader deferredReplacement;
    
    private Matrix4x4 _prevViewProjectionMatrix;
    // Start is called before the first frame update
    void Start()
    {
        //Application.targetFrameRate = 60;
        _cam = GetComponent<Camera>();
        _cam.SetReplacementShader(deferredReplacement, "RenderType");
        SetupRenderTextures();
    }
    
    public void SetupRenderTextures()
    {
        if (_albedo != null)
        {
            _albedo.Release();
        }
        if (_normal != null)
        {
            _normal.Release();
        }
        if (_position != null)
        {
            _position.Release();
        }
        if(_velocity != null)
        {
            _velocity.Release();
        }

        _prevWidth = Screen.width;
        _prevHeight = Screen.height;
        
        _albedo = new RenderTexture(_prevWidth, _prevHeight, 32, RenderTextureFormat.ARGB32);
        _albedo.wrapMode = TextureWrapMode.Clamp;
        Shader.SetGlobalTexture("_GAlbedo", _albedo);
        
        _normal = new RenderTexture(_prevWidth, _prevHeight, 0, RenderTextureFormat.ARGBHalf);
        _normal.wrapMode = TextureWrapMode.Clamp;
        _normal.filterMode = FilterMode.Point;
        Shader.SetGlobalTexture("_GNormal", _normal);
        
        _position = new RenderTexture(_prevWidth, _prevHeight, 0, RenderTextureFormat.ARGBFloat);
        _position.wrapMode = TextureWrapMode.Clamp;
        _position.filterMode = FilterMode.Point;
        Shader.SetGlobalTexture("_GPosition", _position);
        
        _velocity = new RenderTexture(_prevWidth, _prevHeight, 0, RenderTextureFormat.RGFloat);
        _velocity.wrapMode = TextureWrapMode.Clamp;
        _velocity.filterMode = FilterMode.Point;
        Shader.SetGlobalTexture("_GVelocity", _velocity);
        
        if (_cam != null)
        {
            RenderBuffer[] colorBuffers = new[]
            {
                _albedo.colorBuffer, 
                _normal.colorBuffer,
                _position.colorBuffer,
                _velocity.colorBuffer
            };
            _cam.SetTargetBuffers(colorBuffers, _albedo.depthBuffer);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Screen.width != _prevWidth || Screen.height != _prevHeight)
        {
            Debug.Log("Setting up rts");
            SetupRenderTextures();
        }

        Shader.SetGlobalMatrix("_ProjectionMatrix", _cam.projectionMatrix);
        Shader.SetGlobalMatrix("_ViewMatrix", _cam.worldToCameraMatrix);
        Shader.SetGlobalMatrix("_PrevViewProject", _prevViewProjectionMatrix);
        
        Matrix4x4 viewProjectionMatrix = _cam.projectionMatrix * _cam.worldToCameraMatrix;
        _prevViewProjectionMatrix = viewProjectionMatrix;
    }
}
