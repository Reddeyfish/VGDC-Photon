#include "UnityCG.cginc"

// Tranforms vector from object to homogenous space
inline float3 UnityObjectToClipVec( in float3 vec )
{
#ifdef UNITY_USE_PREMULTIPLIED_MATRICES
	return mul((float3x3)UNITY_MATRIX_MVP, vec);
#else
	// More efficient than computing M*VP matrix product
	return mul((float3x3)UNITY_MATRIX_VP, mul((float3x3)unity_ObjectToWorld, vec));
#endif
}

inline float3 UnityObjectToClipVec(float4 vec) // overload for float4; avoids "implicit truncation" warning for existing shaders
{
	return UnityObjectToClipVec(vec.xyz);
}
