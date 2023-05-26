Shader "MyShaders/RollProgressedURP"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        _BumpMap ("Normal Map", 2D) = "bump" {}
        
        _Tess ("Tessellation", Range(1,32)) = 4
        _UpDir ("Up Dir", Vector) = (1,0,0,0)
        _MeshTop ("Mesh Top", float) = 0.5
        _RollDir ("Roll Dir", Vector) = (0,1,0,0)
        _Radius ("Radius", float) = 0
        _Deviation ("Deviation", float) = 0.5
        _PointX ("PointX", float) = 0
        _PointY ("PointY", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma multi_compile_instancing
        #include "UnityCG.cginc"

        #pragma target 4.6
        #pragma require tessellation tesshw

        sampler2D _MainTex;
        sampler2D _BumpMap;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        float _Tess;
		float4 _UpDir;
        float _MeshTop;
		float4 _RollDir;
		half _Radius;
		half _Deviation;
        half _PointX;
        half _PointY;
        //UNITY_INSTANCING_BUFFER_START(Props)
        //   UNITY_DEFINE_INSTANCED_PROP(float, _CycleY)
        //UNITY_INSTANCING_BUFFER_END(Props)

/*         struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
        }; */

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

        float4 tessFixed()
        {
            return _Tess;
        }

        float4 _MainTex_ST;

		v2f vert( appdata v)
		{
            float3 v0 = v.vertex.xyz;

            float3 upDir = normalize(_UpDir);
            float3 rollDir = normalize(_RollDir);

			//float y = UNITY_ACCESS_INSTANCED_PROP(Props, _PointY);
            float y = _PointY;
            float dP = dot(v0 - upDir * y, upDir);
            dP = max(0, dP);
            float3 fromInitialPos = upDir * dP;
            v0 -= fromInitialPos;

            float radius = _Radius + _Deviation * max(0, -(y - _MeshTop));
            float length = 2 * UNITY_PI * (radius - _Deviation * max(0, -(y - _MeshTop)) / 2);
            float r = dP / max(0, length);
            float a = 2 * r * UNITY_PI;
            
            float s = sin(a);
            float c = cos(a);
            float one_minus_c = 1.0 - c;

            float3 axis = normalize(cross(upDir, rollDir));
            float3x3 rot_mat = 
            {   one_minus_c * axis.x * axis.x + c, one_minus_c * axis.x * axis.y - axis.z * s, one_minus_c * axis.z * axis.x + axis.y * s,
                one_minus_c * axis.x * axis.y + axis.z * s, one_minus_c * axis.y * axis.y + c, one_minus_c * axis.y * axis.z - axis.x * s,
                one_minus_c * axis.z * axis.x - axis.y * s, one_minus_c * axis.y * axis.z + axis.x * s, one_minus_c * axis.z * axis.z + c
            };
            float3 cycleCenter = rollDir * _PointX + rollDir * radius + upDir * y;

            float3 fromCenter = v0.xyz - cycleCenter;
            float3 shiftFromCenterAxis = cross(axis, fromCenter);
            shiftFromCenterAxis = cross(shiftFromCenterAxis, axis);
            shiftFromCenterAxis = normalize(shiftFromCenterAxis);
            fromCenter -= shiftFromCenterAxis * _Deviation * dP;// * ;

            v0.xyz = mul(rot_mat, fromCenter) + cycleCenter;

			v.vertex.xyz = v0;

            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
           // v.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
           // v.texcoord=float4(1,1,0,1);
           // v.texcoord=TRANSFORM_TEX(v.texcoord, _MainTex);

              //  UNITY_TRANSFER_FOG(o,o.vertex);
           return o; 
		}

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture

                fixed4 col = tex2D(_MainTex, i.uv)* _Color;
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
}
