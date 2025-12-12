Shader "Custom/FlagWave"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Speed ("Wave Speed", Float) = 2.0
        _Amplitude ("Wave Height", Float) = 0.5
        _Frequency ("Wave Frequency", Float) = 4.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard vertex:vert
        sampler2D _MainTex;
        float _Speed, _Amplitude, _Frequency;

        struct Input
        {
            float2 uv_MainTex;
        };

        void vert(inout appdata_full v)
        {
            // ±Íπﬂ√≥∑≥ Y√‡ ±‚¡ÿ¿∏∑Œ ¡¬øÏ ∆ﬁ∑∞¿Ã∞‘
            float wave = sin(_Time.y * _Speed + v.vertex.y * _Frequency) * _Amplitude;
            v.vertex.x += wave;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}