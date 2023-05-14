Shader "Unlit/SkillEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TexRange("TexRange", range(0, 0.4)) = 0.1
        _AlphaCut("AlphaCut", range(0, 1.1)) = 1.1
        _CoolTime("CoolTime", range(0, 1.1)) = 0
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
            float _TexRange;
            float _AlphaCut;
            float _CoolTime;

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
                clip(col.a - _AlphaCut);
                col.a = _CoolTime;

                if (i.uv.y < _TexRange || i.uv.y > 1 - _TexRange)
                    clip(-1);

                return col;
            }
            ENDHLSL
        }
    }
}
