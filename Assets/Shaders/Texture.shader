Shader "Custom/Texture"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" { }
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex Vertex
			#pragma fragment Fragment
			#pragma target 2.0

			#include "UnityCG.cginc"

			struct Attributes
			{
				float4 positionOS : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			Varyings Vertex( Attributes input )
			{
				Varyings output;
				output.positionCS = UnityObjectToClipPos(input.positionOS);
				output.uv = TRANSFORM_TEX(input.uv, _MainTex);
				return output;
			}

			half4 Fragment( Varyings input ) : SV_Target
			{
				half4 color = tex2D(_MainTex, input.uv);
				return color;
			}
			ENDCG
		}
	}
}