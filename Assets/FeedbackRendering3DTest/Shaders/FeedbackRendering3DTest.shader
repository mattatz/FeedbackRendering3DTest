Shader "FeedbackRendering3DTest"
{

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Velocity ("Velocity", 3D) = "" {}
		_Color ("Color", 3D) = "" {}

		_Size ("Size", Float) = 0.25
		_Alpha ("Alpha", Range(0.0, 1.0)) = 0.5

		[Toggle] _Debug ("Debug", Float) = 0.0
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Blend One One
		ZWrite Off
		ZTest Always

		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata {
				float4 vertex : POSITION;
			};

			struct v2g {
				float4 pos : SV_POSITION;
				float4 color : COLOR;
			};

			struct g2f {
				float4 pos : SV_POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			sampler3D _Velocity, _Color;

			float _Size, _Alpha;
			fixed _Debug;

			v2g vert(appdata v)
			{
				v2g o;
				float3 vertex = v.vertex.xyz;
				o.pos = mul(unity_ObjectToWorld, float4(vertex, 1));

				float4 uv = float4(vertex + 0.5, 0);
				float3 vel = tex3Dlod(_Velocity, uv).xyz;
				float3 col = tex3Dlod(_Color, uv).xyz;
				// o.color = float4(abs(vel), 1);
				o.color = lerp(float4(col, 1) * saturate(length(vel)), float4(col, 1), _Debug);
				// o.color = float4(saturate(col), 0.25);
				return o;
			}

			[maxvertexcount(4)]
			void geom(point v2g IN[1], inout TriangleStream<g2f> triStream) {
				float3 up = float3(0, 1, 0);
				float3 look = _WorldSpaceCameraPos - IN[0].pos;
				look.y = 0;
				look = normalize(look);
				float3 right = cross(up, look);

				float halfS = 0.5f * _Size;

				float4 v[4];
				v[0] = float4(IN[0].pos + halfS * right - halfS * up, 1.0f);
				v[1] = float4(IN[0].pos + halfS * right + halfS * up, 1.0f);
				v[2] = float4(IN[0].pos - halfS * right - halfS * up, 1.0f);
				v[3] = float4(IN[0].pos - halfS * right + halfS * up, 1.0f);

				float4x4 vp = UNITY_MATRIX_VP;

				g2f pIn;
				pIn.pos = mul(vp, v[0]);
				pIn.color = IN[0].color;
				pIn.uv = float2(1.0f, 0.0f);
				triStream.Append(pIn);

				pIn.pos = mul(vp, v[1]);
				pIn.uv = float2(1.0f, 1.0f);
				triStream.Append(pIn);

				pIn.pos = mul(vp, v[2]);
				pIn.uv = float2(0.0f, 0.0f);
				triStream.Append(pIn);

				pIn.pos = mul(vp, v[3]);
				pIn.uv = float2(0.0f, 1.0f);
				triStream.Append(pIn);
			}

			fixed4 frag(g2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				return col * _Alpha * i.color;
			}

			ENDCG
		}
	}
}
