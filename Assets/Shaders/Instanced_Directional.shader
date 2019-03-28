// Author: George Adamopoulos
// Summary: Simple variant of Unity's Surface shader, that supports per-instance properties 
// and is designed to work with the DrawMeshInstancedIndirect() command.
// Its instanced properties are:
// 1) positionBuffer
// 2) directionBuffer

Shader "Adamon Shaders/Instanced Surface-Directional" {
	Properties{
		[HDR] _EmissionColor("EmissionColor",Color) = (0,0,0,0)
		_Emission("Emission", Range(0,100)) = 0
		_Scale("Scale", Range(0,100)) = 1
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		//========== COMPILER DIRECTIVES ==========//
		// Physically based Standard lighting model
		#pragma surface surf Standard addshadow fullforwardshadows
				// Gpu instancing supported
		#pragma multi_compile_instancing
				// Instancing options:
				// 1) The "setup" function will be called every frame and perform the necessary operations per instance
				// 2) The instances will not have lightmaps
				// 3) The instances will not be affected by light probes 
		#pragma instancing_options procedural:setup nolightmap nolightmap

		struct Input 
		{
			float2 uv_MainTex;
		};

		//========== UNIFORMS ==========//
		// The buffers that our C# scripts are writing to
		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			StructuredBuffer<float3> positionBuffer;
			StructuredBuffer<float3> directionBuffer;
		//	StructuredBuffer<float4> colorBuffer;
		#endif

		// The transformation matrix of the object that controls the Voxel Grid
		float4x4 voxelGridMatrix;

		float _Emission;
		float _Scale;
		float3 _EmissionColor;

		//half _Glossiness;
		//half _Metallic;

		float3 _WorldPosition;
		float3 _WorldDirection;

		//========== PER INSTANCE SETUP ==========//
		void setup()
		{
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				_WorldPosition = positionBuffer[unity_InstanceID];
				_WorldDirection = normalize(directionBuffer[unity_InstanceID]);

				// USE THE Z VECTOR TO DETERMINE THE X and Y VECTORS OF THE MATRIX				// DEFAULT VALUES
				float3 z = _WorldDirection * _Scale;											//float3 z = float3(0,0,1) *Scale;
				float3 x = normalize(cross(z, float3(0,1,0)));									//float3 x = float3(1,0,0) *Scale;
				float3 y = normalize(cross(x, z));												//float3 y = float3(0,1,0) *Scale;

				// SETTING THE X AXIS
				unity_ObjectToWorld._11_21_31_41 = float4(x.xyz,0);
				// SETTING THE Y AXIS
				unity_ObjectToWorld._12_22_32_42 = float4(y.xyz,0);
				//SETTING THE Z AXIS
				unity_ObjectToWorld._13_23_33_43 = float4(z.xyz ,0);
				// SETTING THE POSITION
				unity_ObjectToWorld._14_24_34_44 = float4(_WorldPosition.xyz, 1);


				// APPLYING THE TRANSFORMATION MATRIX OF THE PARENT
				unity_ObjectToWorld = mul(voxelGridMatrix, unity_ObjectToWorld);
			#endif
		}

		//========== PER-INSTANCE SURFACE SHADING ==========//
	void surf(Input IN, inout SurfaceOutputStandard o) 
	{
		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			o.Albedo = 0;											 // colorBuffer[unity_InstanceID].rgb;
			o.Emission = (_WorldDirection.xyz*0.5 + 0.5) * _Emission;// * colorBuffer[unity_InstanceID].rgb;
		#endif
			o.Metallic = 0;
			o.Smoothness = 0;
			o.Alpha = 1;
	}
	ENDCG
	}
		FallBack "Diffuse"
}