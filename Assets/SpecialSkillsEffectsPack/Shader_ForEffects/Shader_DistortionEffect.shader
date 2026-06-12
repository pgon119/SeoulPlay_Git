Shader "GAPH Custom Shader/Distortion Effect" {
	Properties {
		_TintColor ("Tint Color", Color) = (1,1,1,1)
		_Mask ("Mask", 2D) = "white" {}
		_NormalMap ("Normalmap", 2D) = "bump" {}
		_DistortFactor ("Distortion", Float) = 10
		_InvFade ("Soft Particles Factor", Range(0,10)) = 1.0
	}

	SubShader {
		Tags {
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"RenderPipeline" = "UniversalPipeline"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		Lighting Off
		ZWrite Off

		Pass {
			Name "URPTransparentFallback"
			Tags { "LightMode" = "SRPDefaultUnlit" }

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_particles

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes {
				float4 positionOS : POSITION;
				float2 texcoord : TEXCOORD0;
				half4 color : COLOR;
			};

			struct Varyings {
				float4 positionCS : SV_POSITION;
				float2 uvMask : TEXCOORD0;
				float2 uvNormal : TEXCOORD1;
				half4 color : COLOR;
			};

			TEXTURE2D(_Mask);
			SAMPLER(sampler_Mask);
			TEXTURE2D(_NormalMap);
			SAMPLER(sampler_NormalMap);

			CBUFFER_START(UnityPerMaterial)
				half4 _TintColor;
				float4 _Mask_ST;
				float4 _NormalMap_ST;
				float _DistortFactor;
				float _InvFade;
			CBUFFER_END

			Varyings vert(Attributes input) {
				Varyings output;
				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				output.uvMask = TRANSFORM_TEX(input.texcoord, _Mask);
				output.uvNormal = TRANSFORM_TEX(input.texcoord, _NormalMap);
				output.color = input.color;
				return output;
			}

			half4 frag(Varyings input) : SV_Target {
				half4 mask = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, input.uvMask);
				half3 normal = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uvNormal));
				half normalGlow = saturate((normal.z + 0.35h) * 0.75h);

				half4 result = _TintColor * input.color;
				result.rgb *= normalGlow;
				result.a *= mask.a;
				return result;
			}
			ENDHLSL
		}
	}

	Fallback Off
}
