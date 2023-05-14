Shader "Unlit/CoolTime"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CoolTime("CoolTime", range(0, 1.1)) = 1
        _AlphaCut("AlphaCut", range(0, 1.1)) = 1.1
        _BCan("BCan", float) = 0
        _Stage("Stage", float) = 0
    }
    SubShader
    {
        Tags {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _CoolTime;
            float _AlphaCut;
            float _BCan;
            float _Stage;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float4 col = tex2D(_MainTex, i.uv);

                if (col.a <= 0 && _BCan > 1)
                    col = float4(_Stage, _Stage, _Stage, 1.2 - 8 * pow(length(i.uv - float2(0.55, 0.5)), 3));
                else
                    clip(col.a - _AlphaCut);
                if (i.uv.y > _CoolTime)
                    col.a = 0.3f;
            
                return col;
            }
            ENDHLSL
        }
    }
}
