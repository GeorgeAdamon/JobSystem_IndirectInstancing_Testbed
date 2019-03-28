// Author: George Adamopoulos
// Summary: Simple variant of Unity's Surface shader, that supports per-instance properties 
//and is designed to work with the DrawMeshInstancedIndirect() command.
// Its instanced properties are:
// 1) positionBuffer
// 2) colorBuffer
Shader "Adamon Shaders/Instanced Surface-Positional" {
	Properties
	{
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
			// 2) The scaling of the instances will bew uniform
			// 3) The instances will not have lightmaps
			// 4) The instances will not be affected by light probes 
			#pragma instancing_options procedural:setup assumeuniformscaling nolightmap nolightmap

			struct Input {
				float2 uv_MainTex;
			};
			
		//========== UNIFORMS ==========//
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			StructuredBuffer<float3> _positionBuffer;
			StructuredBuffer<float4> _colorBuffer;
			#endif
			//half _Glossiness;
			//half _Metallic;
			float _Emission;
			half _Scale;
			half3 _EmissionColor;

		//========== PER INSTANCE SETUP ==========//
			void setup()
			{
				#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
					float3 data = _positionBuffer[unity_InstanceID];

					// Set Uniform Scale
					unity_ObjectToWorld._11_21_31_41 = float4(_Scale, 0, 0, 0);
					unity_ObjectToWorld._12_22_32_42 = float4(0, _Scale, 0, 0);
					unity_ObjectToWorld._13_23_33_43 = float4(0, 0, _Scale, 0);

					// Set Position
					unity_ObjectToWorld._14_24_34_44 = float4(data.xyz, 1);
					unity_WorldToObject = unity_ObjectToWorld;
					//unity_WorldToObject._14_24_34 *= -1;
					//unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
				#endif
			}

		//========== PER-INSTANCE SURFACE SHADING ==========//
			void surf(Input IN, inout SurfaceOutputStandard o) 
			{
				#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
					o.Albedo = 0;				// _colorBuffer[unity_InstanceID].rgb;
					o.Emission = _EmissionColor; // * _colorBuffer[unity_InstanceID].rgb;
				#endif

				o.Metallic = 0;
				o.Smoothness = 0;
				o.Alpha = 1;
			}
		ENDCG
	}
		FallBack "Diffuse"
}