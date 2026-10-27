Shader "HITME/FoliageVertexWind"
{
    Properties { _MainTex ("Sprite", 2D) = "white" {} _Color ("Tint", Color) = (1,1,1,1) _WindStrength ("Wind Strength", Range(0,0.2)) = 0.03 _WindFrequency ("Wind Frequency", Range(0,4)) = 1 _WindPhase ("Wind Phase", Range(0,6.283)) = 0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
        Blend SrcAlpha OneMinusSrcAlpha Cull Off Lighting Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
            sampler2D _MainTex; float4 _Color; float _WindStrength; float _WindFrequency; float _WindPhase;
            v2f vert(appdata v) { v2f o; float pin = saturate(v.uv.y); v.vertex.x += sin(_Time.y * _WindFrequency + _WindPhase + v.vertex.y * 2.0) * _WindStrength * pin; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color*_Color; return o; }
            fixed4 frag(v2f i):SV_Target { return tex2D(_MainTex,i.uv)*i.color; }
            ENDCG
        }
    }
}
