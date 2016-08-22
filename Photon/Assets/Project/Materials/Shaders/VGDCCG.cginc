#include "UnityCG.cginc"

// Tranforms vector from object to homogenous space
inline float4 UnityObjectToClipVec( in float3 vec )
{
#ifdef UNITY_USE_PREMULTIPLIED_MATRICES
	return mul(UNITY_MATRIX_MVP, float4(vec, 0.0));
#else
	// More efficient than computing M*VP matrix product
	return mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(vec, 0.0)));
#endif
}

inline float4 UnityObjectToClipVec(float4 vec) // overload for float4; avoids "implicit truncation" warning for existing shaders
{
	return UnityObjectToClipVec(vec.xyz);
}
