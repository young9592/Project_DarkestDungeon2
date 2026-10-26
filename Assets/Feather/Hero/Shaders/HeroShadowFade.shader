Shader "Feather/Hero Shadow Fade"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tint ("Tint", Color) = (0.22,0.18,0.85,1)
        _TintStrength ("Tint Strength", Range(0,1)) = 0
        _Opacity ("Opacity", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Tint;
            float _TintStrength;
            float _Opacity;
            struct VertexInput { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct VertexOutput { float4 position : SV_POSITION; float2 uv : TEXCOORD0; };
            VertexOutput Vert(VertexInput input)
            {
                VertexOutput output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }
            fixed4 Frag(VertexOutput input) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, input.uv);
                float brightness = max(color.r, max(color.g, color.b));
                float3 shadow = color.rgb * 0.18 + _Tint.rgb * brightness;
                return fixed4(lerp(color.rgb, shadow, _TintStrength), color.a * _Opacity);
            }
            ENDCG
        }
    }
}
