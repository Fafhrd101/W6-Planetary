// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FORGE3D/DamageFX/Ship PBR (Specular) DamageFX"
{
	Properties
	{
		[Header(DamageFX)]
		_DamageMap("DamageMap", 2D) = "white" {}
		_HeatMap("HeatMap", 2D) = "white" {}
		_ClipTiling("ClipTiling", Float) = 0.16
		_ClipDirtTiling("ClipDirtTiling", Float) = 0.21
		_ClipOffset("ClipOffset", Float) = -0.01
		_DirtTiling("DirtTiling", Float) = 3
		_DirtOffset("DirtOffset", Float) = 0.1
		_DirtStr("DirtStr", Range( 0 , 10000)) = 11
		_DirtSharpen("DirtSharpen", Range( 0.003 , 10)) = 1
		_HeatOffset("HeatOffset", Float) = 0.03
		_HeatStr("HeatStr", Range( 0 , 10000)) = 10000
		_HeatSharpness("HeatSharpness", Range( 0.03 , 10)) = 2.87
		_BurnOffset("BurnOffset", Float) = 0.01
		_BurnStr("BurnStr", Range( 0 , 10000)) = 655
		_BurnEdge("BurnEdge", Range( 0.03 , 10)) = 0.31
		_ClipCut("ClipCut", Range( 0.001 , 1)) = 0.081
		_VertexDamage("VertexDamage", Float) = 0
		_Cutoff( "Mask Clip Value", Float ) = 0.999
		_Tint("Tint", Color) = (0.5294118,0.5294118,0.5294118,1)
		_Albedo("Albedo", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_SpecularSmoothness("SpecularSmoothness", 2D) = "white" {}
		_Specular("Specular", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		_NormalScale("NormalScale", Range( -2 , 2)) = 0
		_Mask("Mask", 2D) = "black" {}
		_Windows("Windows", Color) = (1,0,0,1)
		_TeamColor("TeamColor", Color) = (1,0,0,1)
		_BlinkLights("BlinkLights", Color) = (1,0,0,1)
		_Exhaust("Exhaust", Color) = (1,0,0,1)
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf StandardSpecular keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float2 uv2_texcoord2;
			float3 worldPos;
		};

		uniform float _NormalScale;
		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform float4 _Tint;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _TeamColor;
		uniform sampler2D _Mask;
		uniform float4 _Mask_ST;
		uniform sampler2D _DamageMap;
		uniform float _DirtTiling;
		uniform float _DirtStr;
		uniform float4 _Points[200];
		uniform float4 _Data[200];
		uniform float _BurnOffset;
		uniform float _HeatOffset;
		uniform float _DirtOffset;
		uniform float _ClipOffset;
		uniform float _DirtSharpen;
		uniform float4 _Windows;
		uniform float4 _Exhaust;
		uniform float4 _BlinkLights;
		uniform float _BurnEdge;
		uniform sampler2D _HeatMap;
		uniform float4 _HeatMap_ST;
		uniform float _BurnStr;
		uniform float _ClipDirtTiling;
		uniform float _ClipTiling;
		uniform float _HeatSharpness;
		uniform float _HeatStr;
		uniform sampler2D _SpecularSmoothness;
		uniform float4 _SpecularSmoothness_ST;
		uniform float _Specular;
		uniform float _Smoothness;
		uniform float _ClipCut;
		uniform float _VertexDamage;
		uniform float _Cutoff = 0.999;


		float4 For1_g75( float4 PointsHack , float4 DataHack , float3 VertexPos , int MaxPoints , float BurnOffset , float HeatOffset , float DirtOffset , float ClipOffset )
		{
			float4 damageMask = 0;
			for(int i=0; i<MaxPoints;i++)
			{
			float localDistance = distance(_Points[i].xyz, VertexPos.xyz) + _Points[i].w;
			damageMask.x += (1 - saturate(localDistance + (-1 * DirtOffset))) * _Data[i].x;
			damageMask.y += (1 - saturate(localDistance + (-1 * BurnOffset))) * _Data[i].z;
			damageMask.z += (1 - saturate(localDistance + (-1 * HeatOffset))) * _Data[i].y;
			damageMask.w += (1 - saturate(localDistance + (-1 * ClipOffset))) * _Data[i].w;
			}
			return damageMask;
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float4 PointsHack1_g75 = _Points[clamp(0,0,(200 - 1))];
			float4 DataHack1_g75 = _Data[0];
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 VertexPos1_g75 = ase_vertex3Pos;
			int MaxPoints1_g75 = 200;
			float BurnOffset1_g75 = _BurnOffset;
			float HeatOffset1_g75 = _HeatOffset;
			float DirtOffset1_g75 = _DirtOffset;
			float ClipOffset1_g75 = _ClipOffset;
			float4 localFor1_g75 = For1_g75( PointsHack1_g75 , DataHack1_g75 , VertexPos1_g75 , MaxPoints1_g75 , BurnOffset1_g75 , HeatOffset1_g75 , DirtOffset1_g75 , ClipOffset1_g75 );
			float ClipMask270_g74 = localFor1_g75.w;
			float2 temp_cast_0 = (_ClipDirtTiling).xx;
			float2 uv2_TexCoord64_g74 = v.texcoord1.xy * temp_cast_0 + float2( 0,0 );
			float2 temp_cast_1 = (_ClipTiling).xx;
			float2 uv2_TexCoord41_g74 = v.texcoord1.xy * temp_cast_1 + float2( 0,0 );
			float temp_output_288_0_g74 = ( tex2Dlod( _DamageMap, float4( uv2_TexCoord64_g74, 0, 0) ).r * tex2Dlod( _DamageMap, float4( uv2_TexCoord41_g74, 0, 0) ).a );
			float temp_output_391_0_g74 = saturate( ( saturate( ( ClipMask270_g74 * 10 ) ) / temp_output_288_0_g74 ) );
			float3 ase_vertexNormal = v.normal.xyz;
			v.vertex.xyz += ( temp_output_391_0_g74 * _VertexDamage * ase_vertexNormal * temp_output_288_0_g74 );
		}

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			float3 normalizeResult16 = normalize( UnpackScaleNormal( tex2D( _Normal, uv_Normal ) ,_NormalScale ) );
			o.Normal = normalizeResult16;
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float2 uv_Mask = i.uv_texcoord * _Mask_ST.xy + _Mask_ST.zw;
			float4 tex2DNode18 = tex2D( _Mask, uv_Mask );
			float4 lerpResult40 = lerp( ( _Tint * tex2D( _Albedo, uv_Albedo ) ) , _TeamColor , tex2DNode18.g);
			float2 temp_cast_0 = (_DirtTiling).xx;
			float2 uv2_TexCoord130_g74 = i.uv2_texcoord2 * temp_cast_0 + float2( 0,0 );
			float4 PointsHack1_g75 = _Points[clamp(0,0,(200 - 1))];
			float4 DataHack1_g75 = _Data[0];
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 VertexPos1_g75 = ase_vertex3Pos;
			int MaxPoints1_g75 = 200;
			float BurnOffset1_g75 = _BurnOffset;
			float HeatOffset1_g75 = _HeatOffset;
			float DirtOffset1_g75 = _DirtOffset;
			float ClipOffset1_g75 = _ClipOffset;
			float4 localFor1_g75 = For1_g75( PointsHack1_g75 , DataHack1_g75 , VertexPos1_g75 , MaxPoints1_g75 , BurnOffset1_g75 , HeatOffset1_g75 , DirtOffset1_g75 , ClipOffset1_g75 );
			float Dirtmask30_g74 = localFor1_g75.x;
			float temp_output_139_0_g74 = pow( Dirtmask30_g74 , _DirtSharpen );
			float temp_output_71_44 = saturate( ( tex2D( _DamageMap, uv2_TexCoord130_g74 ).a * _DirtStr * temp_output_139_0_g74 ) );
			float4 lerpResult46 = lerp( lerpResult40 , float4( 0,0,0,0 ) , temp_output_71_44);
			o.Albedo = lerpResult46.rgb;
			float BurnMask268_g74 = localFor1_g75.y;
			float mulTime231_g74 = _Time.y * 1;
			float2 uv_HeatMap = i.uv2_texcoord2 * _HeatMap_ST.xy + _HeatMap_ST.zw;
			float2 panner229_g74 = ( uv_HeatMap + mulTime231_g74 * float2( 0.025,0.025 ));
			float4 tex2DNode204_g74 = tex2D( _HeatMap, panner229_g74 );
			float ClipMask270_g74 = localFor1_g75.w;
			float2 temp_cast_2 = (_ClipDirtTiling).xx;
			float2 uv2_TexCoord64_g74 = i.uv2_texcoord2 * temp_cast_2 + float2( 0,0 );
			float2 temp_cast_3 = (_ClipTiling).xx;
			float2 uv2_TexCoord41_g74 = i.uv2_texcoord2 * temp_cast_3 + float2( 0,0 );
			float temp_output_288_0_g74 = ( tex2D( _DamageMap, uv2_TexCoord64_g74 ).r * tex2D( _DamageMap, uv2_TexCoord41_g74 ).a );
			float temp_output_391_0_g74 = saturate( ( saturate( ( ClipMask270_g74 * 10 ) ) / temp_output_288_0_g74 ) );
			float BrunMaskMask355_g74 = saturate( ( temp_output_391_0_g74 * BurnMask268_g74 ) );
			float HeatMask269_g74 = localFor1_g75.z;
			o.Emission = ( ( ( tex2DNode18.a * _Windows ) + ( tex2DNode18.b * _Exhaust * 100.0 ) + ( tex2DNode18.r * _BlinkLights ) ) + max( float4( 0,0,0,0 ) , ( ( pow( BurnMask268_g74 , _BurnEdge ) * tex2DNode204_g74 * _BurnStr * BrunMaskMask355_g74 ) + ( saturate( pow( HeatMask269_g74 , _HeatSharpness ) ) * tex2DNode204_g74 * _HeatStr ) ) ) ).rgb;
			float2 uv_SpecularSmoothness = i.uv_texcoord * _SpecularSmoothness_ST.xy + _SpecularSmoothness_ST.zw;
			float4 tex2DNode11 = tex2D( _SpecularSmoothness, uv_SpecularSmoothness );
			float4 lerpResult55 = lerp( ( tex2DNode11 * _Specular ) , float4( 0,0,0,0 ) , temp_output_71_44);
			o.Specular = lerpResult55.rgb;
			float lerpResult56 = lerp( ( tex2DNode11.a * _Smoothness ) , 0 , temp_output_71_44);
			o.Smoothness = lerpResult56;
			o.Alpha = 1;
			clip( saturate( step( _ClipCut , ( 1.0 - temp_output_391_0_g74 ) ) ) - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=14501
1927;29;1906;1004;1324.984;-242.9084;1.3;True;False
Node;AmplifyShaderEditor.SamplerNode;18;-1042.599,504.3;Float;True;Property;_Mask;Mask;26;0;Create;True;0;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;42;-316.3009,1198.5;Float;False;Constant;_Float0;Float 0;12;0;Create;True;0;100;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;3;-364.5,-467;Float;False;Property;_Tint;Tint;19;0;Create;True;0;0.5294118,0.5294118,0.5294118,1;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;9;-414.5,-294;Float;True;Property;_Albedo;Albedo;20;0;Create;True;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;19;-386.8,802.7;Float;False;Property;_Windows;Windows;27;0;Create;True;0;1,0,0,1;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;32;-398.0984,1295.6;Float;False;Property;_BlinkLights;BlinkLights;29;0;Create;True;0;1,0,0,1;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;27;-543.4996,780.8998;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;23;-393.3332,1024.234;Float;False;Property;_Exhaust;Exhaust;30;0;Create;True;0;1,0,0,1;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;11;-428.5,234;Float;True;Property;_SpecularSmoothness;SpecularSmoothness;22;0;Create;True;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;5;4.5,291;Float;False;Property;_Specular;Specular;23;0;Create;True;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;30;-62.43194,-156.3675;Float;False;Property;_TeamColor;TeamColor;28;0;Create;True;0;1,0,0,1;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;41;-615.4997,-35.09984;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;27.5,-317;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-110.3334,1005.434;Float;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-132.0974,1274.2;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;6;10.1,405.8;Float;False;Property;_Smoothness;Smoothness;24;0;Create;True;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-775.5,60;Float;False;Property;_NormalScale;NormalScale;25;0;Create;True;0;0;0;-2;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-101.6,781.3;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;1;-423.5,21;Float;True;Property;_Normal;Normal;21;0;Create;True;0;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;40;248.5003,-257.4997;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;71;329.2924,2.940503;Float;False;DamageFX;0;;74;947c011340f36384dab084490b24dfb9;0;0;4;COLOR;366;FLOAT;286;FLOAT;44;FLOAT3;414
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;419.5,221;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;425.5,328;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;25;242.0983,1022.499;Float;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;47;701.4524,34.47;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalizeNode;16;-0.5,26;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;56;679.7849,368.3112;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;55;675.4761,228.8788;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;46;550.5046,-194.7796;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;946.0226,-13.16357;Float;False;True;7;Float;ASEMaterialInspector;0;0;StandardSpecular;FORGE3D/DamageFX/Ship PBR (Specular) DamageFX;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;Off;0;0;False;0;0;False;0;Custom;0.999;True;True;0;True;Opaque;;AlphaTest;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;18;-1;-1;-1;0;0;0;False;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;27;0;18;4
WireConnection;41;0;18;2
WireConnection;10;0;3;0
WireConnection;10;1;9;0
WireConnection;24;0;18;3
WireConnection;24;1;23;0
WireConnection;24;2;42;0
WireConnection;33;0;18;1
WireConnection;33;1;32;0
WireConnection;20;0;27;0
WireConnection;20;1;19;0
WireConnection;1;5;17;0
WireConnection;40;0;10;0
WireConnection;40;1;30;0
WireConnection;40;2;41;0
WireConnection;12;0;11;0
WireConnection;12;1;5;0
WireConnection;15;0;11;4
WireConnection;15;1;6;0
WireConnection;25;0;20;0
WireConnection;25;1;24;0
WireConnection;25;2;33;0
WireConnection;47;0;25;0
WireConnection;47;1;71;366
WireConnection;16;0;1;0
WireConnection;56;0;15;0
WireConnection;56;2;71;44
WireConnection;55;0;12;0
WireConnection;55;2;71;44
WireConnection;46;0;40;0
WireConnection;46;2;71;44
WireConnection;0;0;46;0
WireConnection;0;1;16;0
WireConnection;0;2;47;0
WireConnection;0;3;55;0
WireConnection;0;4;56;0
WireConnection;0;10;71;286
WireConnection;0;11;71;414
ASEEND*/
//CHKSM=ACDA10B1F50E234884B9FF73F3B97883A3DB3507