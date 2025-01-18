Shader "Hidden/TemporalAA"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _HistoryBuffer;
            sampler2D _GVelocity;
            float _ModulationFactor;

            fixed4 frag (v2f i) : SV_Target
            {
                float2 vel = tex2D(_GVelocity, i.uv).rg;
                if(vel.x == -99)
                {
                    //return 0;
                }
                float2 prevUv = i.uv - vel;
                //return float4(prevUv, 0, 1);
                //return float4(vel, 0, 1);
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 history = tex2D(_HistoryBuffer, prevUv);

                //clamp history
                float2 texelSize = 1.0 / _ScreenParams.xy;
                float4 neighbor1 = tex2D(_HistoryBuffer, i.uv + float2(texelSize.x, 0));
                float4 neighbor2 = tex2D(_HistoryBuffer, i.uv + float2(-texelSize.x, 0));
                float4 neighbor3 = tex2D(_HistoryBuffer, i.uv + float2(0, texelSize.y));
                float4 neighbor4 = tex2D(_HistoryBuffer, i.uv + float2(0, -texelSize.y));

                float4 boxMin = min(min(min(min(neighbor1, neighbor2), neighbor3), neighbor4), col);
                float4 boxMax = max(max(max(max(neighbor1, neighbor2), neighbor3), neighbor4), col);

                history = clamp(history, boxMin, boxMax);

                
                
                return lerp(col, history, _ModulationFactor);
            }
            ENDCG
        }
    }
}
