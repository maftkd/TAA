Shader "Unlit/DeferredReplacement"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                LIGHTING_COORDS(1, 2)
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD3;
                float3 viewPos : TEXCOORD4;
                float3 worldPos : TEXCOORD5;
                float4 prevPos : TEXCOORD6;
                float4 curPos : TEXCOORD7;
            };

            struct fragmentOutput
            {
                float4 albedo : SV_Target0;
                float4 normal : SV_Target1;
                float4 position : SV_Target2;
                float2 velocity : SV_Target3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4x4 _ViewMatrix;
            float4x4 _PrevViewProject;
            float4 _JitterVectors[16];
            int _FrameCount;
            
            v2f vert (appdata v)
            {
                v2f o;
                float4 curPosUnjittered = UnityObjectToClipPos(v.vertex);
                o.curPos = curPosUnjittered;

                //assume that the object is static. i.e. model matrix is same from frame to frame
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float4 prevPosUnjittered = mul(_PrevViewProject, worldPos);
                o.prevPos = prevPosUnjittered;
                
                float2 jitter = _JitterVectors[_FrameCount];
                jitter = ((jitter - float2(0.5,0.5)) / _ScreenParams.xy) * 2;
                o.pos = curPosUnjittered + float4(jitter * curPosUnjittered.w, 0, 0);
                
                o.uv = v.uv;
                o.normal = normalize(mul((float3x3)UNITY_MATRIX_MV, v.normal));
                o.viewPos = UnityObjectToViewPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }
            
            float2 CalcVelocity(float4 newPos, float4 oldPos)
            {
                oldPos /= oldPos.w;
                oldPos.xy = (oldPos.xy+1)/2.0f;
                //oldPos.y = 1 - oldPos.y;
                
                newPos /= newPos.w;
                newPos.xy = (newPos.xy+1)/2.0f;
                newPos.y = 1 - newPos.y;
                
                return (newPos - oldPos).xy;
            }

            fragmentOutput frag (v2f i) : SV_Target
            {
                fragmentOutput o;

                //lighting
                float receivedShadow = 1 - UNITY_SHADOW_ATTENUATION(i, i.worldPos);
                float3 lightPos = _WorldSpaceLightPos0.xyz;
                lightPos = mul(_ViewMatrix, float4(lightPos, 0)).xyz;
                float lightDot = saturate(dot(lightPos, i.normal));
                float shadowed = saturate(lightDot + receivedShadow);
                o.albedo = _Color * (1 - shadowed) + _Color * shadowed * float4(0.8, 0.9, 0.95, 1.0) * 0.5;

                //hack in a reflection factor based on world space
                o.albedo.a = step(i.worldPos.y, 0.01) * step(-0.1, i.worldPos.y);
                
                o.normal = float4(i.normal, 1.0);
                o.position = float4(i.viewPos, 1.0);

                o.velocity = CalcVelocity(i.curPos, i.prevPos);
                return o;
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
