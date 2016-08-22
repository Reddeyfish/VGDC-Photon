﻿Shader "Custom/LinearRayDistortion"
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
			#include "VGDCCG.cginc"
			
			struct appdata
			{
				float4 color : COLOR;
				half2 uv : TEXCOORD0;
				float4 vertex : POSITION;
			};

			struct v2f
			{
				half2 uv : TEXCOORD0;
				float4 vertex_obj : TEXCOOD1;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			sampler2D _GrabTexture;
			sampler2D _MainTex;
			half _DistortionStrength;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.color = v.color;
				o.uv = v.uv;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.vertex_obj = v.vertex;
				return o;
			}
			
			fixed4 frag (v2f i) : COLOR
			{
				half attenuation = min(60 * i.uv.x, 1);
				fixed4 col = tex2D(_MainTex, i.uv);
				col.rgb *= attenuation * col.a * i.color.a * i.color.rgb;
				half distortion = min(i.uv.y, 1 - i.uv.y);
				distortion = attenuation * 4 * distortion * distortion;
				float4 distortionVector = float4(0, _DistortionStrength * i.color.a * distortion, 0, 0);
				float4 distortedGrabUVs = ComputeGrabScreenPos(UnityObjectToClipPos(i.vertex_obj - distortionVector));
				col += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(distortedGrabUVs));

				return col;
				//return half4(UnityObjectToWorldDir(float3(0, 1, 0)), 1);
			}
			ENDCG
		}
	}
}