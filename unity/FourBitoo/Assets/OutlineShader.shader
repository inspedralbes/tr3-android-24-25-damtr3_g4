Shader "Custom/OutlineShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1, 0, 0, 1)
        _OutlineThickness ("Outline Thickness", Range(0, 0.1)) = 0.02
    }
    SubShader
    {
        Tags { "Queue"="Overlay" }

        Pass
        {
            Name "OUTLINE"
            Tags { "LightMode"="Always" }

            Cull Front
            ZWrite On
            ZTest Always

            Offset 15, 15 // Desplazamiento para el contorno

            ColorMask RGB
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _OutlineColor;
            float _OutlineThickness;

            // Vertex shader, realiza la expansión para el contorno
            v2f vert(appdata_t IN)
            {
                v2f OUT;
                float3 offset = float3(0, 0, _OutlineThickness);
                OUT.vertex = UnityObjectToClipPos(IN.vertex) + float4(offset, 0);
                OUT.uv = IN.uv;
                return OUT;
            }

            // Fragment shader, dibuja el contorno
            fixed4 frag(v2f IN) : SV_Target
            {
                return _OutlineColor; // Usa el color del contorno
            }

            ENDCG
        }

        // El resto de pasadas para el objeto principal (no se necesita modificar)
        Pass {
            Name "FORWARD"
            Tags { "LightMode"="ForwardBase" }
            ZWrite On
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            SetTexture[_MainTex] { combine primary }
        }
    }

    FallBack "Diffuse"
}
