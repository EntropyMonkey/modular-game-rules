Shader "Squid/Alpha Blended"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	Category
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest Greater 0
		Cull Off Lighting Off ZWrite On Fog { Mode Off }

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
					float4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex : POSITION;
					float4 color : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.color = v.color;
					o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					return o;
				}

				float Use8Bit = 0;

				half4 frag (v2f i) : COLOR
				{
					float4 result = i.color;

					if(Use8Bit)
					{
						result.a *= tex2D(_MainTex, i.texcoord).a;
					}
					else
					{
						result *= tex2D(_MainTex, i.texcoord);
					}

					return result;
				}
				ENDCG 
			}
		} 	
	
		SubShader
		{
			Pass
			{
				SetTexture [_MainTex]
				{
					combine texture * primary
				}
			}
		}
	}
}