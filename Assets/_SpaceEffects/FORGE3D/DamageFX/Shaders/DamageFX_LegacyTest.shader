Shader "FORGE3D/DamageFX/Legacy Test"
{
	Properties
	{	
		_Albedo("Albedo", 2D) = "white" {}
		_Metallic("Metallic", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_DamageAlpha("Damage Alpha", 2D) = "white" {}

		_MaskClipValue("Mask Clip Value", Range(0, 0.999)) = 0.999

		_DirtOffset("Dirt Offset", Range(-3, 3)) = 0
		_BurnOffset("Burn Offset", Range(-3, 3)) = 0
		_BurnMetallic("Burn Metallic", Range(0, 1)) = 0
		_BurnSmoothness("Burn Smoothness", Range(0, 2)) = 0
	
			
			//_DistancePow("Distance Pow", Range(0.01 , 50)) = 1


		//_AlphaMaskStep("Alpha Mask Step", Range( 0.01 , 10)) = 0.01
			_ClipOffset("Clip Offset", Range( -1 , 1)) = 0
		
		_HeatColor("Heat Color", Color) = (0,0,0,0)
		_HeatOffset("Heat Offset", Range(-3, 3)) = 0
		_HeatAmount("Heat Amount", Range(0 , 400)) = 0	
			_HeatTile("Heat Tile", Range(0, 1)) = 0

		

		_DistanceMaskPow("Distance Mask Pow", Range(0.01 , 56)) = 0.01
		_VertexOffset("Vertex Offset", Range(-13 , 13)) = 0
		_VertexMaskDistance("Vertex Mask Distance", Range(0 , 555)) = 0
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry" }
		Cull Off
		CGPROGRAM
		#define MAX_POINTS 200
		#pragma target 5.0
		//#pragma multi_compile_instancing
		#pragma surface surf Standard   fullforwardshadows vertex:vertexDataFunc
		struct Input
		{
			float3 localVertexPos;
			float distanceMask;
			float2 uv_Normal;
			float2 uv_Albedo;			
			float2 uv_Metallic;
			float2 uv2_DamageAlpha;
			float3 viewDir;
		};

		uniform sampler2D _Albedo;
		uniform sampler2D _Metallic;
		uniform sampler2D _Normal;
		
		uniform sampler2D _DamageAlpha;

		uniform float _MaskClipValue = 0.5;

		
		uniform float _BurnOffset;
		uniform float _BurnMetallic;
		uniform float _BurnSmoothness;
		uniform float _DirtOffset;
		
		uniform float _DistancePow;
		
		uniform float _AlphaMaskStep;
		uniform float _ClipOffset;

		uniform float4 _HeatColor;
		uniform float _HeatOffset;
		uniform float _HeatAmount;
		uniform float _HeatTile;

		
		
		float _DistanceMaskPow;
		float _VertexOffset;
		float _VertexMaskDistance;
		
		uniform int _PointsLength = 0;
		const int _MaxPoints = MAX_POINTS;
		uniform float4 _Points[MAX_POINTS];
		uniform float4 _Data[MAX_POINTS];
		
		// VERTEX
		void vertexDataFunc(inout appdata_full vertexData, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.localVertexPos = vertexData.vertex.xyz ;
			vertexData.normal = vertexData.normal;
		//	float4 posWorld = mul(unity_ObjectToWorld, vertexData.vertex);
		//	float4 posObj = mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0));

			//vMod = (posWorld - posObj)

			//float3 vMod = (posWorld - posObj) ;
			
			for (int i = 0; i < _PointsLength; i++)
			{
				float localDistance = pow(saturate(distance(_Points[i].xyz, vertexData.vertex.xyz)) , _VertexMaskDistance);
				o.distanceMask += 1 - saturate(localDistance);
				
				

				// transform back into local space
			
				//vertexData.vertex.xyz += 0.1f;//vertexData.vertex.xyz + o.distanceMask * vertexData.normal * 0.1f * _Data[i].x;
			}

			o.distanceMask = pow(saturate(o.distanceMask), _DistanceMaskPow);
			vertexData.vertex.xyz += vertexData.normal * _VertexOffset * saturate(o.distanceMask);
		}

		
		// FRAGMENT
		void surf( Input input , inout SurfaceOutputStandard output )
		{
			
			float dirtMask = 0;
			float burnMask = 0;
			float heatMask = 0;
			float clipMask = 0;

			float temp = 0;
			
			for(int i = 0; i < _PointsLength; i++)
			{
				float localDistance = distance(_Points[i].xyz, input.localVertexPos.xyz) +  _Points[i].w;
				dirtMask += 1 - saturate(localDistance + (-1 * _DirtOffset));
				burnMask += 1 - saturate(localDistance + (-1 * _BurnOffset));
				heatMask += (1 - saturate(localDistance + (-1 * _HeatOffset))) * _Data[i].y;
				clipMask += (1 - saturate(localDistance + (-1 * _ClipOffset)));
			}

		

			float4 DamageAlphaSampler = tex2D(_DamageAlpha,input.uv2_DamageAlpha);
			float4 DamageClipSampler = tex2D(_DamageAlpha, input.uv2_DamageAlpha * _HeatTile.xx);

			
			float3 heat = saturate(saturate(heatMask) - DamageClipSampler.r * DamageClipSampler.a);// saturate(saturate(distanceMask) * DamageAlphaSampler.a);// +saturate(dirtMask));
			float3 clip_ = saturate(saturate(clipMask) - DamageClipSampler.r * DamageClipSampler.a);// saturate(saturate(distanceMask) * DamageAlphaSampler.a);// +saturate(dirtMask));

			

			float4 albedo = tex2D(_Albedo, input.uv_Albedo);
			float4 albedoDirtMask = saturate(DamageAlphaSampler.a * saturate(dirtMask));
			float4 albedoDirtMix = lerp(albedo, saturate(albedo - albedoDirtMask), saturate(dirtMask));
			float4 albedoBurnMix = lerp(albedoDirtMix, saturate(albedoDirtMix - saturate(burnMask)), saturate(burnMask));



			float3 albedoMix_ = saturate(saturate(albedo.xyz - albedoDirtMask) - saturate(burnMask));

			////////////
			output.Albedo = dot(input.viewDir, float3(0, 0, 1)) > 0 ? albedoMix_ : 0.0f;

			float3 normalSampler = UnpackNormal(tex2D(_Normal, input.uv_Normal));

			output.Normal = dot(input.viewDir, float3(0, 0, 1))>0 ? normalSampler : -normalSampler;
		//	output.Emission = dot(input.viewDir, float3(0, 0, 1));
			output.Emission = heat * _HeatColor * _HeatAmount;

			float4 metallicSampler = tex2D(_Metallic, input.uv_Metallic);
			output.Metallic = saturate(lerp(saturate(metallicSampler.r - albedoDirtMask),  _BurnMetallic * DamageAlphaSampler.a, saturate(burnMask) * 5 ));
			output.Smoothness = lerp(saturate(metallicSampler.a - albedoDirtMask), _BurnSmoothness * DamageAlphaSampler.a, saturate(burnMask)  );
			////////////

			clip( 1 - saturate(clip_) - _MaskClipValue );
			
		}

		ENDCG
	}
	Fallback "Diffuse"
}