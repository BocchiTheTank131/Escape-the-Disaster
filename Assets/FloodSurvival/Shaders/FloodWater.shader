Shader "FloodSurvival/Water"
{
    Properties { _BaseColor("Water",Color)=(.07,.28,.31,.91) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct A { float4 positionOS:POSITION; };
            struct V { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; };
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            CBUFFER_END
            float _FloodFrontZ;
            V vert(A i) { V o; o.world=TransformObjectToWorld(i.positionOS.xyz); o.positionCS=TransformWorldToHClip(o.world); return o; }
            half4 frag(V i):SV_Target
            {
                clip(i.world.z-_FloodFrontZ);
                float a=sin(i.world.x*1.4+_Time.y*1.5+sin(i.world.z*.6));
                float b=sin(i.world.z*2.1-_Time.y*2+sin(i.world.x*.7));
                float crest=1-saturate((i.world.z-_FloodFrontZ)/6);
                float foam=pow(saturate(a*b),16)*.25+crest*.7;
                half3 color=_BaseColor.rgb+foam*.3+(a+b)*.012;
                float fog=saturate(length(GetCameraPositionWS()-i.world)/200);
                return half4(lerp(color,half3(.24,.33,.37),fog*.55),_BaseColor.a);
            }
            ENDHLSL
        }
    }
}
