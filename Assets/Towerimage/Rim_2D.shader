Shader "Custom/Rim2D"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [PerRendererData] _Bump ("Normal Map", 2D) = "bump" {}
        _Pow ("Rim Power", Range(1, 10)) = 5
        [HDR] _RimCol ("Rim Color", Color) = (1,1,1,1)

        [PerRendererData] _FlipX ("Flip X (internal)", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Lambert alpha:blend vertex:vert nofog
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _Bump;
        fixed4 _Color;

        float _Pow;
        float4 _RimCol;
        float _FlipX;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_Bump;
            float3 viewDir;
            fixed4 color;
        };

        void vert (inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.color = v.color; // SpriteRenderer의 Color(틴트) 값을 가져옴
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 mainTex = tex2D(_MainTex, IN.uv_MainTex) * _Color * IN.color;
            float3 bump = UnpackNormal(tex2D(_Bump, IN.uv_Bump));
            bump.x *= _FlipX; // 좌우반전 시 노멀맵 X축도 같이 뒤집어줌

            o.Normal = bump;

            float rim = saturate(dot(o.Normal, IN.viewDir));
            rim = pow(1.0 - rim, _Pow);

            // 원본 색을 Albedo가 아닌 Emission으로 출력 -> 조명 영향을 받지 않음
            o.Albedo = fixed3(0, 0, 0);
            o.Emission = mainTex.rgb + rim * _RimCol.rgb;
            o.Alpha = mainTex.a;
        }
        ENDCG
    }

    Fallback "Sprites/Default"
}
