Shader "Custom/LinearRayDistortion"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white"
		_DistortionStrength ("DistortionStrength", Float) = 1
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "PreviewType"="Plane"}
		LOD 100

		GrabPass { "_GrabTexture"}

		Pass
		{
			ZWrite Off
			Blend One Zero
			Lighting Off
			Fog { Mode Off }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 color : COLOR;
				half2 uv : TEXCOORD0;
				float4 vertex : POSITION;
			};

			struct v2f
			{
				half2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 vertexObj : TEXCOORD1;
				float4 color : COLOR;
			};

			sampler2D _GrabTexture;
			sampler2D _MainTex;
			half _DistortionStrength;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.color = v.color;
				o.vertexObj = v.vertex;
				o.uv = v.uv;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col.rgb *= col.a * i.color.a * i.color.rgb;
				half distortion = min(i.uv.y, 1 - i.uv.y);
				distortion = 4 * distortion * distortion;
				float4 distortionVector = float4(_DistortionStrength * i.color.a * distortion, 0, 0, 0);
				float2 uv_grab = ComputeGrabScreenPos(mul(UNITY_MATRIX_MVP, i.vertexObj - distortionVector));
				col += tex2D(_GrabTexture, uv_grab);
				return col;
			}
			ENDCG
		}
	}
}
