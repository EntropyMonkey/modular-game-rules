Shader "Squid/AtlasMaker"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	Category
	{
		Tags { "Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend Off
		ColorMask RGBA
		Cull Off Lighting Off ZWrite Off ZTest Always Fog { Mode Off }

		SubShader
		{
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
			
				#include "UnityCG.cginc"

				sampler2D _MainTex;
				float4 _MainTex_ST;
			
				struct appdata_t
				{
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
				};

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					return o;
				}

				half4 frag (v2f i) : COLOR
				{
					return tex2D(_MainTex, i.texcoord);
				}
				ENDCG 
			}
		} 	
	}
}