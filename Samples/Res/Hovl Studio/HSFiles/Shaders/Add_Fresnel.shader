Shader "Hovl/Particles/Add_Fresnel"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_Noise("Noise", 2D) = "white" {}
		_Color("Color", Color) = (0.5,0.5,0.5,1)
		_Emission("Emission", Float) = 2
		_SpeedMainTexUVNoiseZW("Speed MainTex U/V + Noise Z/W", Vector) = (0,0,0,0)
		_Flow("Flow", 2D) = "white" {}
		_Mask("Mask", 2D) = "white" {}
		_Distortionpower("Distortion power", Float) = 0.2
		_Fresnelscale("Fresnel scale", Float) = 3
		_Fresnelpower("Fresnel power", Float) = 3
		_Depthpower("Depth power", Float) = 0.2
		[Toggle]_Useonlycolor("Use only color", Float) = 0
		[Toggle]_Addnoise("Add noise?", Float) = 0
		_Texturesopacity("Textures opacity", Range( 0 , 1)) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"
				#define ASE_NEEDS_FRAG_COLOR


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					float3 ase_normal : NORMAL;
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					float4 ase_texcoord3 : TEXCOORD3;
					float4 ase_texcoord4 : TEXCOORD4;
					float4 ase_texcoord5 : TEXCOORD5;
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform float4 _MainTex_ST;
				uniform float _Useonlycolor;
				uniform float4 _SpeedMainTexUVNoiseZW;
				uniform sampler2D _Mask;
				uniform float4 _Mask_ST;
				uniform sampler2D _Flow;
				uniform float4 _Flow_ST;
				uniform float _Distortionpower;
				uniform sampler2D _Noise;
				uniform float4 _Noise_ST;
				uniform float _Fresnelscale;
				uniform float _Fresnelpower;
				uniform float4 _CameraDepthTexture_TexelSize;
				uniform float _Depthpower;
				uniform float4 _Color;
				uniform float _Emission;
				uniform float _Addnoise;
				uniform float _Texturesopacity;


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					float3 ase_worldPos = mul(unity_ObjectToWorld, float4( (v.vertex).xyz, 1 )).xyz;
					o.ase_texcoord3.xyz = ase_worldPos;
					float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
					o.ase_texcoord4.xyz = ase_worldNormal;
					float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
					float4 screenPos = ComputeScreenPos(ase_clipPos);
					o.ase_texcoord5 = screenPos;				
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord3.w = 0;
					o.ase_texcoord4.w = 0;

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i , bool ase_vface : SV_IsFrontFace ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					float fade = 1;
					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						fade = saturate (_Depthpower * (sceneZ-partZ));
					#endif

					float2 appendResult186 = (float2(_SpeedMainTexUVNoiseZW.x , _SpeedMainTexUVNoiseZW.y));
					float2 uv_MainTex = i.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
					float2 uv_Mask = i.texcoord.xy * _Mask_ST.xy + _Mask_ST.zw;
					float2 appendResult177 = (float2(_SpeedMainTexUVNoiseZW.z , _SpeedMainTexUVNoiseZW.w));
					float3 uvs3_Flow = i.texcoord.xyz;
					uvs3_Flow.xy = i.texcoord.xyz.xy * _Flow_ST.xy + _Flow_ST.zw;
					float4 tex2DNode203 = tex2D( _MainTex, ( ( ( appendResult186 * _Time.y ) + uv_MainTex ) - ( (( tex2D( _Mask, uv_Mask ) * tex2D( _Flow, ( ( _Time.y * appendResult177 ) + (uvs3_Flow).xy ) ) )).rg * _Distortionpower ) ) );
					float2 uv_Noise = i.texcoord.xy * _Noise_ST.xy + _Noise_ST.zw;
					float4 tex2DNode211 = tex2D( _Noise, uv_Noise );
					float3 ase_worldPos = i.ase_texcoord3.xyz;
					float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
					ase_worldViewDir = normalize(ase_worldViewDir);
					float3 ase_worldNormal = i.ase_texcoord4.xyz;
					float fresnelNdotV187 = dot( ase_worldNormal, ase_worldViewDir );
					float fresnelNode187 = ( 0.0 + _Fresnelscale * pow( 1.0 - fresnelNdotV187, _Fresnelpower ) );
					float clampResult193 = clamp( fresnelNode187 , 0.0 , 1.0 );
					float lerpResult227 = lerp( 0.0 , clampResult193 , ase_vface);	
					float clampResult202 = clamp( fade , 0.0 , 1.0 );
					float clampResult214 = clamp( ( (1.0 + (clampResult202 - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) - lerpResult227 ) , 0.0 , 1.0 );
					float temp_output_218_0 = ( lerpResult227 + clampResult214 );
					float4 temp_cast_0 = (temp_output_218_0).xxxx;
					float w199 = (1.0 + (uvs3_Flow.z - 0.0) * (128.0 - 1.0) / (1.0 - 0.0));
					float4 temp_cast_4 = (tex2DNode203.a).xxxx;
					float div207=256.0/float((int)w199);
					float4 posterize207 = ( floor( temp_cast_4 * div207 ) / div207 );
					float opac215 = (posterize207).a;
					float temp_output_234_0 = ( opac215 * tex2DNode211.a * _Texturesopacity );
					float4 appendResult224 = (float4(( (( _Useonlycolor )?( _Color ):( float4( (( max( ( tex2DNode203 * tex2DNode211 ) , temp_cast_0 ) * _Color * i.color )).rgb , 0.0 ) )) * _Emission ).rgb , ( (( _Addnoise )?( ( temp_output_234_0 + temp_output_218_0 ) ):( temp_output_234_0 )) * _Color.a * i.color.a * (( _Addnoise )?( 1.0 ):( temp_output_218_0 )) )));		
					fixed4 col = appendResult224;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	Fallback Off
}
/*ASEBEGIN
Version=19108
Node;AmplifyShaderEditor.Vector4Node;175;-3403.278,-187.8897;Float;False;Property;_SpeedMainTexUVNoiseZW;Speed MainTex U/V + Noise Z/W;4;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;179;-3128.091,133.8754;Inherit;False;0;182;3;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TimeNode;176;-3071.756,-159.3273;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;177;-3039.331,-27.67758;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;178;-2822.756,18.16158;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;222;-2854.699,123.9574;Inherit;False;True;True;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;180;-2590.137,56.08243;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;182;-2450.287,36.03077;Inherit;True;Property;_Flow;Flow;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;181;-2436.15,-164.1162;Inherit;True;Property;_Mask;Mask;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;186;-3041.873,-249.9572;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;185;-2094.413,-6.293035;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;189;-2825.2,-248.616;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;191;-1970.021,94.15653;Float;False;Property;_Distortionpower;Distortion power;7;0;Create;True;0;0;0;False;0;False;0.2;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;190;-1942.127,-12.67462;Inherit;False;True;True;False;False;1;0;COLOR;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;188;-2824.302,-160.4125;Inherit;False;0;203;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;184;-1832.816,612.7309;Float;False;Property;_Fresnelscale;Fresnel scale;8;0;Create;True;0;0;0;False;0;False;3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;183;-1837.439,702.0196;Float;False;Property;_Fresnelpower;Fresnel power;9;0;Create;True;0;0;0;False;0;False;3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;195;-2558.478,-247.3405;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;194;-1721.198,-13.66589;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;192;-1905.483,966.1779;Float;False;Property;_Depthpower;Depth power;10;0;Create;True;0;0;0;False;0;False;0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;196;-2576.335,238.1291;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;128;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;187;-1603.648,590.0786;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;200;-1558.277,-232.7691;Inherit;True;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;199;-2264.895,294.6912;Float;False;w;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;197;-1700.775,950.5247;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;193;-1322.607,587.9484;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;202;-1413.214,950.8426;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;203;-1242.024,-223.369;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;201;-1052.624,-464.7238;Inherit;False;199;w;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;208;-1157.103,177.5223;Float;False;Property;_Color;Color;2;0;Create;True;0;0;0;False;0;False;0.5,0.5,0.5,1;0.5,0.5,0.5,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;210;-1115.114,347.522;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;211;-1230.057,-22.91238;Inherit;True;Property;_Noise;Noise;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;205;-1243.115,941.5295;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosterizeNode;207;-836.0663,-495.9696;Inherit;False;1;2;1;COLOR;0,0,0,0;False;0;INT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;212;-803.8646,594.939;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;209;-648.4307,-495.6043;Inherit;False;False;False;False;True;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;214;-634.4662,573.429;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;215;-336.3258,-489.0232;Float;False;opac;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FaceVariableNode;226;-1370.157,747.5427;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;227;-1068.726,567.8794;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;213;-544.4083,-107.5888;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;223;-385.9049,-98.63786;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ToggleSwitchNode;225;-159.333,-54.26447;Float;False;Property;_Useonlycolor;Use only color;11;0;Create;True;0;0;0;False;0;False;0;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;220;-96.40157,51.90979;Float;False;Property;_Emission;Emission;3;0;Create;True;0;0;0;False;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;219;100.9934,30.3417;Inherit;False;2;2;0;COLOR;1,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;232;-893.6516,-142.556;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;231;-708.6796,-144.8908;Inherit;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;224;788.909,81.67456;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;123;993.5782,68.37696;Float;False;True;-1;2;;0;11;Hovl/Particles/Add_Fresnel;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;False;True;2;5;False;;10;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;True;True;True;True;False;0;False;_InvFade;False;False;False;False;False;False;False;False;False;True;2;False;;True;3;False;;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;0;;0;0;Standard;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.GetLocalVarNode;216;-205.4285,127.0099;Inherit;False;215;opac;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;235;-316.6371,218.5384;Inherit;False;Property;_Texturesopacity;Textures opacity;13;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;229;302.9749,135.0907;Inherit;False;Property;_Addnoise;Add noise?;12;0;Create;False;0;0;0;False;0;False;0;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;234;4.46259,134.0384;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;228;154.3971,190.2379;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;221;554.8892,248.5282;Inherit;False;4;4;0;FLOAT;1;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;230;154.2694,438.1252;Inherit;False;Property;_Addnoise;Add noise?;12;0;Create;True;0;0;0;False;0;False;0;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;218;-460.4196,451.8848;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
WireConnection;177;0;175;3
WireConnection;177;1;175;4
WireConnection;178;0;176;2
WireConnection;178;1;177;0
WireConnection;222;0;179;0
WireConnection;180;0;178;0
WireConnection;180;1;222;0
WireConnection;182;1;180;0
WireConnection;186;0;175;1
WireConnection;186;1;175;2
WireConnection;185;0;181;0
WireConnection;185;1;182;0
WireConnection;189;0;186;0
WireConnection;189;1;176;2
WireConnection;190;0;185;0
WireConnection;195;0;189;0
WireConnection;195;1;188;0
WireConnection;194;0;190;0
WireConnection;194;1;191;0
WireConnection;196;0;179;3
WireConnection;187;2;184;0
WireConnection;187;3;183;0
WireConnection;200;0;195;0
WireConnection;200;1;194;0
WireConnection;199;0;196;0
WireConnection;197;0;192;0
WireConnection;193;0;187;0
WireConnection;202;0;197;0
WireConnection;203;1;200;0
WireConnection;205;0;202;0
WireConnection;207;1;203;4
WireConnection;207;0;201;0
WireConnection;212;0;205;0
WireConnection;212;1;227;0
WireConnection;209;0;207;0
WireConnection;214;0;212;0
WireConnection;215;0;209;0
WireConnection;227;1;193;0
WireConnection;227;2;226;0
WireConnection;213;0;231;0
WireConnection;213;1;208;0
WireConnection;213;2;210;0
WireConnection;223;0;213;0
WireConnection;225;0;223;0
WireConnection;225;1;208;0
WireConnection;219;0;225;0
WireConnection;219;1;220;0
WireConnection;232;0;203;0
WireConnection;232;1;211;0
WireConnection;231;0;232;0
WireConnection;231;1;218;0
WireConnection;224;0;219;0
WireConnection;224;3;221;0
WireConnection;123;0;224;0
WireConnection;229;0;234;0
WireConnection;229;1;228;0
WireConnection;234;0;216;0
WireConnection;234;1;211;4
WireConnection;234;2;235;0
WireConnection;228;0;234;0
WireConnection;228;1;218;0
WireConnection;221;0;229;0
WireConnection;221;1;208;4
WireConnection;221;2;210;4
WireConnection;221;3;230;0
WireConnection;230;0;218;0
WireConnection;218;0;227;0
WireConnection;218;1;214;0
ASEEND*/
//CHKSM=8B3CB8D1F848916AC0092B1EC9B101B85BFDBDAF