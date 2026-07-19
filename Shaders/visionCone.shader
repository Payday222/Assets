Shader "Custom/VisionCone"
{
    Properties
    {
        _PlayerPos("Player Pos", Vector) = (0.5,0.5,0,0)
        _Direction("Direction", Vector) = (1,0,0,0)
        _Distance("Distance", Float) = 0.35
        _Angle("Angle", Float) = 45
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

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

            float4 _PlayerPos;
            float4 _Direction;
            float _Distance;
            float _Angle;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 diff = i.uv - _PlayerPos.xy;

                float dist = length(diff);

                if(dist < _Distance)
                {
                    float2 dir = normalize(diff);

                    float angle = degrees(acos(dot(dir, normalize(_Direction.xy))));

                    if(angle < _Angle * 0.5)
                    {
                        return float4(0,0,0,0);
                    }
                }

                return float4(0,0,0,1);
            }

            ENDCG
        }
    }
}