﻿Shader "Custom/LinearRayDistortion"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white"
		_DistortionStrength ("DistortionStrength", Float) = 1
		_Cutoff("Distance Cutoff", Float) = 1
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
				float4 uv_grab : TEXCOORD1;
				float3 rayDirectionClip : TEXCOORD2;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			sampler2D _GrabTexture;
			sampler2D _MainTex;
			half _DistortionStrength;
			fixed _Cutoff;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.color = v.color;
				o.uv = v.uv;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv_grab = ComputeGrabScreenPos(o.vertex);
				o.rayDirectionClip = UnityObjectToClipVec(float3(0, 1, 0));
				return o;
			}
			
			fixed4 frag (v2f i) : COLOR
			{
				if(i.uv.x > _Cutoff)
				{
					return tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uv_grab));
				}
				half attenuation = min(60 * i.uv.x, 1);

				fixed4 col = tex2D(_MainTex, i.uv);
				col.rgb *= attenuation * col.a * i.color.a * i.color.rgb;

				half distortion = min(i.uv.y, 1 - i.uv.y);
				distortion = attenuation * 4 * distortion * distortion;

				float3 distortionVector = _DistortionStrength * i.color.a * distortion * i.rayDirectionClip;
				float4 distortedGrabUVs = i.uv_grab;
				distortedGrabUVs.xyz -= distortionVector;
				col += tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(distortedGrabUVs));

				return col;
			}
			ENDCG
		}
	}
}
