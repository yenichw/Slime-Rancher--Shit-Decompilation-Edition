Shader "Hidden/SSAO Pro V2" {
	Properties {
		_MainTex ("", 2D) = "white" {}
		_SSAOTex ("", 2D) = "white" {}
		_NoiseTex ("", 2D) = "white" {}
	}
	SubShader {
		Pass {
			ZTest Always
			ZWrite Off
			Cull Off
			Fog {
				Mode 0
			}
			GpuProgramID 20444
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			// $Globals ConstantBuffers for Fragment Shader
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                o.texcoord.xy = v.texcoord.xy;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                o.sv_target = float4(1.0, 1.0, 1.0, 1.0);
                return o;
			}
			ENDCG
		}
		Pass {
			ZTest Always
			ZWrite Off
			Cull Off
			Fog {
				Mode 0
			}
			GpuProgramID 120282
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _MainTex_TexelSize;
			// $Globals ConstantBuffers for Fragment Shader
			float4x4 _InverseViewProject;
			float4x4 _CameraModelView;
			float4 _Params1;
			float4 _Params2;
			float4 _OcclusionColor;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _CameraDepthTexture;
			sampler2D _CameraDepthNormalsTexture;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                tmp0.x = _MainTex_TexelSize.y < 0.0;
                tmp0.y = 1.0 - v.texcoord.y;
                o.texcoord.y = tmp0.x ? tmp0.y : v.texcoord.y;
                o.texcoord1 = v.texcoord.xxy;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                float4 tmp5;
                tmp0 = tex2D(_CameraDepthTexture, inp.texcoord.xy);
                tmp0.y = _ZBufferParams.z * tmp0.x + _ZBufferParams.w;
                tmp0.x = 1.0 - tmp0.x;
                tmp0.y = 1.0 / tmp0.y;
                tmp0.z = _Params2.z - tmp0.y;
                tmp0.z = tmp0.z < 0.0;
                if (tmp0.z) {
                    discard;
                }
                tmp0.zw = inp.texcoord.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp1 = tmp0.wwww * _InverseViewProject._m01_m11_m21_m31;
                tmp1 = _InverseViewProject._m00_m10_m20_m30 * tmp0.zzzz + tmp1;
                tmp1 = _InverseViewProject._m02_m12_m22_m32 * tmp0.xxxx + tmp1;
                tmp1 = tmp1 + _InverseViewProject._m03_m13_m23_m33;
                tmp0.xzw = tmp1.xyz / tmp1.www;
                tmp1.x = _Params1.y / tmp0.y;
                tmp1.x = max(tmp1.x, 0.005);
                tmp2 = tmp1.xxxx * float4(0.5, 0.0, -0.5, 0.0) + inp.texcoord.xyxy;
                tmp1 = tmp1.xxxx * float4(0.0, 0.5, 0.0, -0.5) + inp.texcoord.xyxy;
                tmp3 = tex2D(_CameraDepthTexture, tmp2.zw);
                tmp3.x = 1.0 - tmp3.x;
                tmp4 = tmp2 * float4(2.0, 2.0, 2.0, 2.0) + float4(-1.0, -1.0, -1.0, -1.0);
                tmp2 = tex2D(_CameraDepthTexture, tmp2.xy);
                tmp2.x = 1.0 - tmp2.x;
                tmp5 = tmp4.wwww * _InverseViewProject._m01_m11_m21_m31;
                tmp5 = _InverseViewProject._m00_m10_m20_m30 * tmp4.zzzz + tmp5;
                tmp3 = _InverseViewProject._m02_m12_m22_m32 * tmp3.xxxx + tmp5;
                tmp3 = tmp3 + _InverseViewProject._m03_m13_m23_m33;
                tmp2.yzw = tmp3.xyz / tmp3.www;
                tmp2.yzw = tmp2.yzw - tmp0.xzw;
                tmp3.x = dot(tmp2.xyz, tmp2.xyz);
                tmp3.y = rsqrt(tmp3.x);
                tmp3.x = sqrt(tmp3.x);
                tmp3.x = tmp3.x * _Params1.w + 1.0;
                tmp3.x = 1.0 / tmp3.x;
                tmp2.yzw = tmp2.yzw * tmp3.yyy;
                tmp5 = tex2D(_CameraDepthNormalsTexture, inp.texcoord.xy);
                tmp3.yzw = tmp5.xyz * float3(3.5554, 3.5554, 0.0) + float3(-1.7777, -1.7777, 1.0);
                tmp3.w = dot(tmp3.xyz, tmp3.xyz);
                tmp3.w = 2.0 / tmp3.w;
                tmp3.yz = tmp3.yz * tmp3.ww;
                tmp3.w = tmp3.w - 1.0;
                tmp5.xyz = tmp3.zzz * _CameraModelView._m01_m11_m21;
                tmp5.xyz = _CameraModelView._m00_m10_m20 * tmp3.yyy + tmp5.xyz;
                tmp3.yzw = _CameraModelView._m02_m12_m22 * tmp3.www + tmp5.xyz;
                tmp2.y = dot(tmp3.xyz, tmp2.xyz);
                tmp2.y = tmp2.y - _Params2.x;
                tmp2.y = max(tmp2.y, 0.0);
                tmp2.y = tmp3.x * tmp2.y;
                tmp2.y = tmp2.y * _Params1.z;
                tmp5 = tmp4.yyyy * _InverseViewProject._m01_m11_m21_m31;
                tmp4 = _InverseViewProject._m00_m10_m20_m30 * tmp4.xxxx + tmp5;
                tmp4 = _InverseViewProject._m02_m12_m22_m32 * tmp2.xxxx + tmp4;
                tmp4 = tmp4 + _InverseViewProject._m03_m13_m23_m33;
                tmp2.xzw = tmp4.xyz / tmp4.www;
                tmp2.xzw = tmp2.xzw - tmp0.xzw;
                tmp3.x = dot(tmp2.xyz, tmp2.xyz);
                tmp4.x = rsqrt(tmp3.x);
                tmp3.x = sqrt(tmp3.x);
                tmp3.x = tmp3.x * _Params1.w + 1.0;
                tmp3.x = 1.0 / tmp3.x;
                tmp2.xzw = tmp2.xzw * tmp4.xxx;
                tmp2.x = dot(tmp3.xyz, tmp2.xyz);
                tmp2.x = tmp2.x - _Params2.x;
                tmp2.x = max(tmp2.x, 0.0);
                tmp2.x = tmp3.x * tmp2.x;
                tmp2.x = tmp2.x * _Params1.z + tmp2.y;
                tmp4 = tex2D(_CameraDepthTexture, tmp1.xy);
                tmp2.y = 1.0 - tmp4.x;
                tmp4 = tmp1 * float4(2.0, 2.0, 2.0, 2.0) + float4(-1.0, -1.0, -1.0, -1.0);
                tmp1 = tex2D(_CameraDepthTexture, tmp1.zw);
                tmp1.x = 1.0 - tmp1.x;
                tmp5 = tmp4.yyyy * _InverseViewProject._m01_m11_m21_m31;
                tmp5 = _InverseViewProject._m00_m10_m20_m30 * tmp4.xxxx + tmp5;
                tmp5 = _InverseViewProject._m02_m12_m22_m32 * tmp2.yyyy + tmp5;
                tmp5 = tmp5 + _InverseViewProject._m03_m13_m23_m33;
                tmp1.yzw = tmp5.xyz / tmp5.www;
                tmp1.yzw = tmp1.yzw - tmp0.xzw;
                tmp2.y = dot(tmp1.xyz, tmp1.xyz);
                tmp2.z = rsqrt(tmp2.y);
                tmp2.y = sqrt(tmp2.y);
                tmp2.y = tmp2.y * _Params1.w + 1.0;
                tmp2.y = 1.0 / tmp2.y;
                tmp1.yzw = tmp1.yzw * tmp2.zzz;
                tmp1.y = dot(tmp3.xyz, tmp1.xyz);
                tmp1.y = tmp1.y - _Params2.x;
                tmp1.y = max(tmp1.y, 0.0);
                tmp1.y = tmp2.y * tmp1.y;
                tmp1.y = tmp1.y * _Params1.z + tmp2.x;
                tmp2 = tmp4.wwww * _InverseViewProject._m01_m11_m21_m31;
                tmp2 = _InverseViewProject._m00_m10_m20_m30 * tmp4.zzzz + tmp2;
                tmp2 = _InverseViewProject._m02_m12_m22_m32 * tmp1.xxxx + tmp2;
                tmp2 = tmp2 + _InverseViewProject._m03_m13_m23_m33;
                tmp1.xzw = tmp2.xyz / tmp2.www;
                tmp0.xzw = tmp1.xzw - tmp0.xzw;
                tmp1.x = dot(tmp0.xyz, tmp0.xyz);
                tmp1.z = rsqrt(tmp1.x);
                tmp1.x = sqrt(tmp1.x);
                tmp1.x = tmp1.x * _Params1.w + 1.0;
                tmp1.x = 1.0 / tmp1.x;
                tmp0.xzw = tmp0.xzw * tmp1.zzz;
                tmp0.x = dot(tmp3.xyz, tmp0.xyz);
                tmp0.x = tmp0.x - _Params2.x;
                tmp0.x = max(tmp0.x, 0.0);
                tmp0.x = tmp1.x * tmp0.x;
                tmp0.x = tmp0.x * _Params1.z + tmp1.y;
                tmp0.x = -tmp0.x * 0.25 + 1.0;
                tmp0.z = 1.0 - tmp0.x;
                tmp0.w = _Params2.z - _Params2.w;
                tmp0.y = tmp0.y - tmp0.w;
                tmp0.w = _Params2.z - tmp0.w;
                tmp0.y = saturate(tmp0.y / tmp0.w);
                tmp0.xyz = tmp0.yyy * tmp0.zzz + tmp0.xxx;
                tmp0.w = 1.0;
                o.sv_target = saturate(tmp0 + _OcclusionColor);
                return o;
			}
			ENDCG
		}
		Pass {
			ZTest Always
			ZWrite Off
			Cull Off
			Fog {
				Mode 0
			}
			GpuProgramID 131318
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _MainTex_TexelSize;
			// $Globals ConstantBuffers for Fragment Shader
			float4x4 _InverseViewProject;
			float4x4 _CameraModelView;
			float4 _Params1;
			float4 _Params2;
			float4 _OcclusionColor;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _CameraDepthTexture;
			sampler2D _CameraDepthNormalsTexture;
			sampler2D _NoiseTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                tmp0.x = _MainTex_TexelSize.y < 0.0;
                tmp0.y = 1.0 - v.texcoord.y;
                o.texcoord.y = tmp0.x ? tmp0.y : v.texcoord.y;
                o.texcoord1 = v.texcoord.xxy;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                float4 tmp5;
                float4 tmp6;
                tmp0 = tex2D(_CameraDepthTexture, inp.texcoord.xy);
                tmp0.y = _ZBufferParams.z * tmp0.x + _ZBufferParams.w;
                tmp0.x = 1.0 - tmp0.x;
                tmp0.y = 1.0 / tmp0.y;
                tmp0.z = _Params2.z - tmp0.y;
                tmp0.z = tmp0.z < 0.0;
                if (tmp0.z) {
                    discard;
                }
                tmp0.zw = inp.texcoord.xy * _ScreenParams.xy;
                tmp0.zw = tmp0.zw / _Params1.xx;
                tmp1 = tex2D(_NoiseTex, tmp0.zw);
                tmp1 = tmp1.xyxy * float4(2.0, 2.0, 2.0, 2.0) + float4(-1.0, -1.0, -1.0, -1.0);
                tmp0.z = dot(tmp1.xy, tmp1.xy);
                tmp0.z = rsqrt(tmp0.z);
                tmp1 = tmp0.zzzz * tmp1;
                tmp2 = tmp1.zzww * float4(-2.0, -2.0, -2.0, -2.0);
                tmp2 = tmp1 * -tmp2 + float4(-1.0, 0.0, 0.0, -1.0);
                tmp0.z = _Params1.y / tmp0.y;
                tmp0.z = max(tmp0.z, 0.005);
                tmp2 = tmp0.zzzz * tmp2;
                tmp2 = tmp2 * float4(0.5, 0.5, 0.5, 0.5) + inp.texcoord.xyxy;
                tmp3 = tex2D(_CameraDepthTexture, tmp2.xy);
                tmp0.w = 1.0 - tmp3.x;
                tmp3 = tmp2 * float4(2.0, 2.0, 2.0, 2.0) + float4(-1.0, -1.0, -1.0, -1.0);
                tmp2 = tex2D(_CameraDepthTexture, tmp2.zw);
                tmp1.x = 1.0 - tmp2.x;
                tmp2 = tmp3.yyyy * _InverseViewProject._m01_m11_m21_m31;
                tmp2 = _InverseViewProject._m00_m10_m20_m30 * tmp3.xxxx + tmp2;
                tmp2 = _InverseViewProject._m02_m12_m22_m32 * tmp0.wwww + tmp2;
                tmp2 = tmp2 + _InverseViewProject._m03_m13_m23_m33;
                tmp2.xyz = tmp2.xyz / tmp2.www;
                tmp3.xy = inp.texcoord.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp4 = tmp3.yyyy * _InverseViewProject._m01_m11_m21_m31;
                tmp4 = _InverseViewProject._m00_m10_m20_m30 * tmp3.xxxx + tmp4;
                tmp4 = _InverseViewProject._m02_m12_m22_m32 * tmp0.xxxx + tmp4;
                tmp4 = tmp4 + _InverseViewProject._m03_m13_m23_m33;
                tmp4.xyz = tmp4.xyz / tmp4.www;
                tmp2.xyz = tmp2.xyz - tmp4.xyz;
                tmp0.x = dot(tmp2.xyz, tmp2.xyz);
                tmp0.w = rsqrt(tmp0.x);
                tmp0.x = sqrt(tmp0.x);
                tmp0.x = tmp0.x * _Params1.w + 1.0;
                tmp0.x = 1.0 / tmp0.x;
                tmp2.xyz = tmp0.www * tmp2.xyz;
                tmp5 = tex2D(_CameraDepthNormalsTexture, inp.texcoord.xy);
                tmp5.xyz = tmp5.xyz * float3(3.5554, 3.5554, 0.0) + float3(-1.7777, -1.7777, 1.0);
                tmp0.w = dot(tmp5.xyz, tmp5.xyz);
                tmp0.w = 2.0 / tmp0.w;
                tmp3.xy = tmp5.xy * tmp0.ww;
                tmp0.w = tmp0.w - 1.0;
                tmp5.xyz = tmp3.yyy * _CameraModelView._m01_m11_m21;
                tmp5.xyz = _CameraModelView._m00_m10_m20 * tmp3.xxx + tmp5.xyz;
                tmp5.xyz = _CameraModelView._m02_m12_m22 * tmp0.www + tmp5.xyz;
                tmp0.w = dot(tmp5.xyz, tmp2.xyz);
                tmp0.w = tmp0.w - _Params2.x;
                tmp0.w = max(tmp0.w, 0.0);
                tmp0.x = tmp0.x * tmp0.w;
                tmp0.x = tmp0.x * _Params1.z;
                tmp2 = tmp1.zzww + tmp1.zzww;
                tmp2 = tmp1.zwzw * -tmp2 + float4(1.0, 0.0, 0.0, 1.0);
                tmp2 = tmp0.zzzz * tmp2;
                tmp2 = tmp2 * float4(0.5, 0.5, 0.5, 0.5) + inp.texcoord.xyxy;
                tmp6 = tex2D(_CameraDepthTexture, tmp2.xy);
                tmp0.z = 1.0 - tmp6.x;
                tmp6 = tmp2 * float4(2.0, 2.0, 2.0, 2.0) + float4(-1.0, -1.0, -1.0, -1.0);
                tmp2 = tex2D(_CameraDepthTexture, tmp2.zw);
                tmp0.w = 1.0 - tmp2.x;
                tmp2 = tmp6.yyyy * _InverseViewProject._m01_m11_m21_m31;
                tmp2 = _InverseViewProject._m00_m10_m20_m30 * tmp6.xxxx + tmp2;
                tmp2 = _InverseViewProject._m02_m12_m22_m32 * tmp0.zzzz + tmp2;
                tmp2 = tmp2 + _InverseViewProject._m03_m13_m23_m33;
                tmp1.yzw = tmp2.xyz / tmp2.www;
                tmp1.yzw = tmp1.yzw - tmp4.xyz;
                tmp0.z = dot(tmp1.xyz, tmp1.xyz);
                tmp2.x = rsqrt(tmp0.z);
                tmp0.z = sqrt(tmp0.z);
                tmp0.z = tmp0.z * _Params1.w + 1.0;
                tmp0.z = 1.0 / tmp0.z;
                tmp1.yzw = tmp1.yzw * tmp2.xxx;
                tmp1.y = dot(tmp5.xyz, tmp1.xyz);
                tmp1.y = tmp1.y - _Params2.x;
                tmp1.y = max(tmp1.y, 0.0);
                tmp0.z = tmp0.z * tmp1.y;
                tmp0.x = tmp0.z * _Params1.z + tmp0.x;
                tmp2 = tmp6.wwww * _InverseViewProject._m01_m11_m21_m31;
                tmp2 = _InverseViewProject._m00_m10_m20_m30 * tmp6.zzzz + tmp2;
                tmp2 = _InverseViewProject._m02_m12_m22_m32 * tmp0.wwww + tmp2;
                tmp2 = tmp2 + _InverseViewProject._m03_m13_m23_m33;
                tmp1.yzw = tmp2.xyz / tmp2.www;
                tmp1.yzw = tmp1.yzw - tmp4.xyz;
                tmp0.z = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.z);
                tmp0.z = sqrt(tmp0.z);
                tmp0.z = tmp0.z * _Params1.w + 1.0;
                tmp0.z = 1.0 / tmp0.z;
                tmp1.yzw = tmp0.www * tmp1.yzw;
                tmp0.w = dot(tmp5.xyz, tmp1.xyz);
                tmp0.w = tmp0.w - _Params2.x;
                tmp0.w = max(tmp0.w, 0.0);
                tmp0.z = tmp0.z * tmp0.w;
                tmp0.x = tmp0.z * _Params1.z + tmp0.x;
                tmp2 = tmp3.wwww * _InverseViewProject._m01_m11_m21_m31;
                tmp2 = _InverseViewProject._m00_m10_m20_m30 * tmp3.zzzz + tmp2;
                tmp1 = _InverseViewProject._m02_m12_m22_m32 * tmp1.xxxx + tmp2;
                tmp1 = tmp1 + _InverseViewProject._m03_m13_m23_m33;
                tmp1.xyz = tmp1.xyz / tmp1.www;
                tmp1.xyz = tmp1.xyz - tmp4.xyz;
                tmp0.z = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.z);
                tmp0.z = sqrt(tmp0.z);
                tmp0.z = tmp0.z * _Params1.w + 1.0;
                tmp0.z = 1.0 / tmp0.z;
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp0.w = dot(tmp5.xyz, tmp1.xyz);
                tmp0.w = tmp0.w - _Params2.x;
                tmp0.w = max(tmp0.w, 0.0);
                tmp0.z = tmp0.z * tmp0.w;
                tmp0.x = tmp0.z * _Params1.z + tmp0.x;
                tmp0.x = -tmp0.x * 0.25 + 1.0;
                tmp0.z = 1.0 - tmp0.x;
                tmp0.w = _Params2.z - _Params2.w;
                tmp0.y = tmp0.y - tmp0.w;
                tmp0.w = _Params2.z - tmp0.w;
                tmp0.y = saturate(tmp0.y / tmp0.w);
                tmp0.xyz = tmp0.yyy * tmp0.zzz + tmp0.xxx;
                tmp0.w = 1.0;
                o.sv_target = saturate(tmp0 + _OcclusionColor);
                return o;
			}
			ENDCG
		}
		Pass {
			ZTest Always
			ZWrite Off
			Cull Off
			Fog {
				Mode 0
			}
			GpuProgramID 231021
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _MainTex_TexelSize;
			// $Globals ConstantBuffers for Fragment Shader
			float4x4 _InverseViewProject;
			float4x4 _CameraModelView;
			float4 _Params1;
			float4 _Params2;
			float4 _OcclusionColor;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _CameraDepthTexture;
			sampler2D _CameraDepthNormalsTexture;
			sampler2D _MainTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                tmp0.x = _MainTex_TexelSize.y < 0.0;
                tmp0.y = 1.0 - v.texcoord.y;
                o.texcoord.y = tmp0.x ? tmp0.y : v.texcoord.y;
                o.texcoord1 = v.texcoord.xxy;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                float4 tmp5;
                tmp0 = tex2D(_CameraDepthTexture, inp.texcoord.xy);
                tmp0.y = _ZBufferParams.z * tmp0.x + _ZBufferParams.w;
                tmp0.x = 1.0 - tmp0.x;
                tmp0.y = 1.0 / tmp0.y;
                tmp0.z = _Params2.z - tmp0.y;
                tmp0.z = tmp0.z < 0.0;
                if (tmp0.z) {
                    discard;
                }
                tmp0.zw = inp.texcoord.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp1 = tmp0.wwww * _InverseViewProject._m01_m11_m21_m31;
                tmp1 = _InverseViewProject._m00_m10_m20_m30 * tmp0.zzzz + tmp1;
                tmp1 = _InverseViewProject._m02_m12_m22_m32 * tmp0.xxxx + tmp1;
                tmp1 = tmp1 + _InverseViewProject._m03_m13_m23_m33;
                tmp0.xzw = tmp1.xyz / tmp1.www;
                tmp1.x = _Params1.y / tmp0.y;
                tmp1.x = max(tmp1.x, 0.005);
                tmp2 = tmp1.xxxx * float4(0.5, 0.0, -0.5, 0.0) + inp.texcoord.xyxy;
                tmp1 = tmp1.xxxx * float4(0.0, 0.5, 0.0, -0.5) + inp.texcoord.xyxy;
                tmp3 = tex2D(_CameraDepthTexture, tmp2.zw);
                tmp3.x = 1.0 - tmp3.x;
                tmp4 = tmp2 * float4(2.0, 2.0, 2.0, 2.0) + float4(-1.0, -1.0, -1.0, -1.0);
                tmp2 = tex2D(_CameraDepthTexture, tmp2.xy);
                tmp2.x = 1.0 - tmp2.x;
                tmp5 = tmp4.wwww * _InverseViewProject._m01_m11_m21_m31;
                tmp5 = _InverseViewProject._m00_m10_m20_m30 * tmp4.zzzz + tmp5;
                tmp3 = _InverseViewProject._m02_m12_m22_m32 * tmp3.xxxx + tmp5;
                tmp3 = tmp3 + _InverseViewProject._m03_m13_m23_m33;
                tmp2.yzw = tmp3.xyz / tmp3.www;
                tmp2.yzw = tmp2.yzw - tmp0.xzw;
                tmp3.x = dot(tmp2.xyz, tmp2.xyz);
                tmp3.y = rsqrt(tmp3.x);
                tmp3.x = sqrt(tmp3.x);
                tmp3.x = tmp3.x * _Params1.w + 1.0;
                tmp3.x = 1.0 / tmp3.x;
                tmp2.yzw = tmp2.yzw * tmp3.yyy;
                tmp5 = tex2D(_CameraDepthNormalsTexture, inp.texcoord.xy);
                tmp3.yzw = tmp5.xyz * float3(3.5554, 3.5554, 0.0) + float3(-1.7777, -1.7777, 1.0);
                tmp3.w = dot(tmp3.xyz, tmp3.xyz);
                tmp3.w = 2.0 / tmp3.w;
                tmp3.yz = tmp3.yz * tmp3.ww;
                tmp3.w = tmp3.w - 1.0;
                tmp5.xyz = tmp3.zzz * _CameraModelView._m01_m11_m21;
                tmp5.xyz = _CameraModelView._m00_m10_m20 * tmp3.yyy + tmp5.xyz;
                tmp3.yzw = _CameraModelView._m02_m12_m22 * tmp3.www + tmp5.xyz;
                tmp2.y = dot(tmp3.xyz, tmp2.xyz);
                tmp2.y = tmp2.y - _Params2.x;
                tmp2.y = max(tmp2.y, 0.0);
                tmp2.y = tmp3.x * tmp2.y;
                tmp2.y = tmp2.y * _Params1.z;
                tmp5 = tmp4.yyyy * _InverseViewProject._m01_m11_m21_m31;
                tmp4 = _InverseViewProject._m00_m10_m20_m30 * tmp4.xxxx + tmp5;
                tmp4 = _InverseViewProject._m02_m12_m22_m32 * tmp2.xxxx + tmp4;
                tmp4 = tmp4 + _InverseViewProject._m03_m13_m23_m33;
                tmp2.xzw = tmp4.xyz / tmp4.www;
                tmp2.xzw = tmp2.xzw - tmp0.xzw;
                tmp3.x = dot(tmp2.xyz, tmp2.xyz);
                tmp4.x = rsqrt(tmp3.x);
                tmp3.x = sqrt(tmp3.x);
                tmp3.x = tmp3.x * _Params1.w + 1.0;
                tmp3.x = 1.0 / tmp3.x;
                tmp2.xzw = tmp2.xzw * tmp4.xxx;
                tmp2.x = dot(tmp3.xyz, tmp2.xyz);
                tmp2.x = tmp2.x - _Params2.x;
                tmp2.x = max(tmp2.x, 0.0);
                tmp2.x = tmp3.x * tmp2.x;
                tmp2.x = tmp2.x * _Params1.z + tmp2.y;
                tmp4 = tex2D(_CameraDepthTexture, tmp1.xy);
                tmp2.y = 1.0 - tmp4.x;
                tmp4 = tmp1 * float4(2.0, 2.0, 2.0, 2.0) + float4(-1.0, -1.0, -1.0, -1.0);
                tmp1 = tex2D(_CameraDepthTexture, tmp1.zw);
                tmp1.x = 1.0 - tmp1.x;
                tmp5 = tmp4.yyyy * _InverseViewProject._m01_m11_m21_m31;
                tmp5 = _InverseViewProject._m00_m10_m20_m30 * tmp4.xxxx + tmp5;
                tmp5 = _InverseViewProject._m02_m12_m22_m32 * tmp2.yyyy + tmp5;
                tmp5 = tmp5 + _InverseViewProject._m03_m13_m23_m33;
                tmp1.yzw = tmp5.xyz / tmp5.www;
                tmp1.yzw = tmp1.yzw - tmp0.xzw;
                tmp2.y = dot(tmp1.xyz, tmp1.xyz);
                tmp2.z = rsqrt(tmp2.y);
                tmp2.y = sqrt(tmp2.y);
                tmp2.y = tmp2.y * _Params1.w + 1.0;
                tmp2.y = 1.0 / tmp2.y;
                tmp1.yzw = tmp1.yzw * tmp2.zzz;
                tmp1.y = dot(tmp3.xyz, tmp1.xyz);
                tmp1.y = tmp1.y - _Params2.x;
                tmp1.y = max(tmp1.y, 0.0);
                tmp1.y = tmp2.y * tmp1.y;
                tmp1.y = tmp1.y * _Params1.z + tmp2.x;
                tmp2 = tmp4.wwww * _InverseViewProject._m01_m11_m21_m31;
                tmp2 = _InverseViewProject._m00_m10_m20_m30 * tmp4.zzzz + tmp2;
                tmp2 = _InverseViewProject._m02_m12_m22_m32 * tmp1.xxxx + tmp2;
                tmp2 = tmp2 + _InverseViewProject._m03_m13_m23_m33;
                tmp1.xzw = tmp2.xyz / tmp2.www;
                tmp0.xzw = tmp1.xzw - tmp0.xzw;
                tmp1.x = dot(tmp0.xyz, tmp0.xyz);
                tmp1.z = rsqrt(tmp1.x);
                tmp1.x = sqrt(tmp1.x);
                tmp1.x = tmp1.x * _Params1.w + 1.0;
                tmp1.x = 1.0 / tmp1.x;
                tmp0.xzw = tmp0.xzw * tmp1.zzz;
                tmp0.x = dot(tmp3.xyz, tmp0.xyz);
                tmp0.x = tmp0.x - _Params2.x;
                tmp0.x = max(tmp0.x, 0.0);
                tmp0.x = tmp1.x * tmp0.x;
                tmp0.x = tmp0.x * _Params1.z + tmp1.y;
                tmp0.x = -tmp0.x * 0.25 + 1.0;
                tmp0.z = 1.0 - tmp0.x;
                tmp0.w = _Params2.z - _Params2.w;
                tmp0.y = tmp0.y - tmp0.w;
                tmp0.w = _Params2.z - tmp0.w;
                tmp0.y = saturate(tmp0.y / tmp0.w);
                tmp0.x = tmp0.y * tmp0.z + tmp0.x;
                tmp0.y = 1.0 - tmp0.x;
                tmp1 = tex2D(_MainTex, inp.texcoord1.xy);
                tmp0.z = dot(tmp1.xyz, float3(0.2126, 0.7152, 0.0722));
                tmp0.z = tmp0.z * _Params2.y;
                tmp0.xyz = tmp0.zzz * tmp0.yyy + tmp0.xxx;
                tmp0.w = 1.0;
                o.sv_target = saturate(tmp0 + _OcclusionColor);
                return o;
			}
			ENDCG
		}
		Pass {
			ZTest Always
			ZWrite Off
			Cull Off
			Fog {
				Mode 0
			}
			GpuProgramID 273598
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _MainTex_TexelSize;
			// $Globals ConstantBuffers for Fragment Shader
			float4x4 _InverseViewProject;
			float4x4 _CameraModelView;
			float4 _Params1;
			float4 _Params2;
			float4 _OcclusionColor;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _CameraDepthTexture;
			sampler2D _CameraDepthNormalsTexture;
			sampler2D _NoiseTex;
			sampler2D _MainTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                tmp0.x = _MainTex_TexelSize.y < 0.0;
                tmp0.y = 1.0 - v.texcoord.y;
                o.texcoord.y = tmp0.x ? tmp0.y : v.texcoord.y;
                o.texcoord1 = v.texcoord.xxy;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                float4 tmp4;
                float4 tmp5;
                float4 tmp6;
                tmp0 = tex2D(_CameraDepthTexture, inp.texcoord.xy);
                tmp0.y = _ZBufferParams.z * tmp0.x + _ZBufferParams.w;
                tmp0.x = 1.0 - tmp0.x;
                tmp0.y = 1.0 / tmp0.y;
                tmp0.z = _Params2.z - tmp0.y;
                tmp0.z = tmp0.z < 0.0;
                if (tmp0.z) {
                    discard;
                }
                tmp0.zw = inp.texcoord.xy * _ScreenParams.xy;
                tmp0.zw = tmp0.zw / _Params1.xx;
                tmp1 = tex2D(_NoiseTex, tmp0.zw);
                tmp1 = tmp1.xyxy * float4(2.0, 2.0, 2.0, 2.0) + float4(-1.0, -1.0, -1.0, -1.0);
                tmp0.z = dot(tmp1.xy, tmp1.xy);
                tmp0.z = rsqrt(tmp0.z);
                tmp1 = tmp0.zzzz * tmp1;
                tmp2 = tmp1.zzww * float4(-2.0, -2.0, -2.0, -2.0);
                tmp2 = tmp1 * -tmp2 + float4(-1.0, 0.0, 0.0, -1.0);
                tmp0.z = _Params1.y / tmp0.y;
                tmp0.z = max(tmp0.z, 0.005);
                tmp2 = tmp0.zzzz * tmp2;
                tmp2 = tmp2 * float4(0.5, 0.5, 0.5, 0.5) + inp.texcoord.xyxy;
                tmp3 = tex2D(_CameraDepthTexture, tmp2.xy);
                tmp0.w = 1.0 - tmp3.x;
                tmp3 = tmp2 * float4(2.0, 2.0, 2.0, 2.0) + float4(-1.0, -1.0, -1.0, -1.0);
                tmp2 = tex2D(_CameraDepthTexture, tmp2.zw);
                tmp1.x = 1.0 - tmp2.x;
                tmp2 = tmp3.yyyy * _InverseViewProject._m01_m11_m21_m31;
                tmp2 = _InverseViewProject._m00_m10_m20_m30 * tmp3.xxxx + tmp2;
                tmp2 = _InverseViewProject._m02_m12_m22_m32 * tmp0.wwww + tmp2;
                tmp2 = tmp2 + _InverseViewProject._m03_m13_m23_m33;
                tmp2.xyz = tmp2.xyz / tmp2.www;
                tmp3.xy = inp.texcoord.xy * float2(2.0, 2.0) + float2(-1.0, -1.0);
                tmp4 = tmp3.yyyy * _InverseViewProject._m01_m11_m21_m31;
                tmp4 = _InverseViewProject._m00_m10_m20_m30 * tmp3.xxxx + tmp4;
                tmp4 = _InverseViewProject._m02_m12_m22_m32 * tmp0.xxxx + tmp4;
                tmp4 = tmp4 + _InverseViewProject._m03_m13_m23_m33;
                tmp4.xyz = tmp4.xyz / tmp4.www;
                tmp2.xyz = tmp2.xyz - tmp4.xyz;
                tmp0.x = dot(tmp2.xyz, tmp2.xyz);
                tmp0.w = rsqrt(tmp0.x);
                tmp0.x = sqrt(tmp0.x);
                tmp0.x = tmp0.x * _Params1.w + 1.0;
                tmp0.x = 1.0 / tmp0.x;
                tmp2.xyz = tmp0.www * tmp2.xyz;
                tmp5 = tex2D(_CameraDepthNormalsTexture, inp.texcoord.xy);
                tmp5.xyz = tmp5.xyz * float3(3.5554, 3.5554, 0.0) + float3(-1.7777, -1.7777, 1.0);
                tmp0.w = dot(tmp5.xyz, tmp5.xyz);
                tmp0.w = 2.0 / tmp0.w;
                tmp3.xy = tmp5.xy * tmp0.ww;
                tmp0.w = tmp0.w - 1.0;
                tmp5.xyz = tmp3.yyy * _CameraModelView._m01_m11_m21;
                tmp5.xyz = _CameraModelView._m00_m10_m20 * tmp3.xxx + tmp5.xyz;
                tmp5.xyz = _CameraModelView._m02_m12_m22 * tmp0.www + tmp5.xyz;
                tmp0.w = dot(tmp5.xyz, tmp2.xyz);
                tmp0.w = tmp0.w - _Params2.x;
                tmp0.w = max(tmp0.w, 0.0);
                tmp0.x = tmp0.x * tmp0.w;
                tmp0.x = tmp0.x * _Params1.z;
                tmp2 = tmp1.zzww + tmp1.zzww;
                tmp2 = tmp1.zwzw * -tmp2 + float4(1.0, 0.0, 0.0, 1.0);
                tmp2 = tmp0.zzzz * tmp2;
                tmp2 = tmp2 * float4(0.5, 0.5, 0.5, 0.5) + inp.texcoord.xyxy;
                tmp6 = tex2D(_CameraDepthTexture, tmp2.xy);
                tmp0.z = 1.0 - tmp6.x;
                tmp6 = tmp2 * float4(2.0, 2.0, 2.0, 2.0) + float4(-1.0, -1.0, -1.0, -1.0);
                tmp2 = tex2D(_CameraDepthTexture, tmp2.zw);
                tmp0.w = 1.0 - tmp2.x;
                tmp2 = tmp6.yyyy * _InverseViewProject._m01_m11_m21_m31;
                tmp2 = _InverseViewProject._m00_m10_m20_m30 * tmp6.xxxx + tmp2;
                tmp2 = _InverseViewProject._m02_m12_m22_m32 * tmp0.zzzz + tmp2;
                tmp2 = tmp2 + _InverseViewProject._m03_m13_m23_m33;
                tmp1.yzw = tmp2.xyz / tmp2.www;
                tmp1.yzw = tmp1.yzw - tmp4.xyz;
                tmp0.z = dot(tmp1.xyz, tmp1.xyz);
                tmp2.x = rsqrt(tmp0.z);
                tmp0.z = sqrt(tmp0.z);
                tmp0.z = tmp0.z * _Params1.w + 1.0;
                tmp0.z = 1.0 / tmp0.z;
                tmp1.yzw = tmp1.yzw * tmp2.xxx;
                tmp1.y = dot(tmp5.xyz, tmp1.xyz);
                tmp1.y = tmp1.y - _Params2.x;
                tmp1.y = max(tmp1.y, 0.0);
                tmp0.z = tmp0.z * tmp1.y;
                tmp0.x = tmp0.z * _Params1.z + tmp0.x;
                tmp2 = tmp6.wwww * _InverseViewProject._m01_m11_m21_m31;
                tmp2 = _InverseViewProject._m00_m10_m20_m30 * tmp6.zzzz + tmp2;
                tmp2 = _InverseViewProject._m02_m12_m22_m32 * tmp0.wwww + tmp2;
                tmp2 = tmp2 + _InverseViewProject._m03_m13_m23_m33;
                tmp1.yzw = tmp2.xyz / tmp2.www;
                tmp1.yzw = tmp1.yzw - tmp4.xyz;
                tmp0.z = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.z);
                tmp0.z = sqrt(tmp0.z);
                tmp0.z = tmp0.z * _Params1.w + 1.0;
                tmp0.z = 1.0 / tmp0.z;
                tmp1.yzw = tmp0.www * tmp1.yzw;
                tmp0.w = dot(tmp5.xyz, tmp1.xyz);
                tmp0.w = tmp0.w - _Params2.x;
                tmp0.w = max(tmp0.w, 0.0);
                tmp0.z = tmp0.z * tmp0.w;
                tmp0.x = tmp0.z * _Params1.z + tmp0.x;
                tmp2 = tmp3.wwww * _InverseViewProject._m01_m11_m21_m31;
                tmp2 = _InverseViewProject._m00_m10_m20_m30 * tmp3.zzzz + tmp2;
                tmp1 = _InverseViewProject._m02_m12_m22_m32 * tmp1.xxxx + tmp2;
                tmp1 = tmp1 + _InverseViewProject._m03_m13_m23_m33;
                tmp1.xyz = tmp1.xyz / tmp1.www;
                tmp1.xyz = tmp1.xyz - tmp4.xyz;
                tmp0.z = dot(tmp1.xyz, tmp1.xyz);
                tmp0.w = rsqrt(tmp0.z);
                tmp0.z = sqrt(tmp0.z);
                tmp0.z = tmp0.z * _Params1.w + 1.0;
                tmp0.z = 1.0 / tmp0.z;
                tmp1.xyz = tmp0.www * tmp1.xyz;
                tmp0.w = dot(tmp5.xyz, tmp1.xyz);
                tmp0.w = tmp0.w - _Params2.x;
                tmp0.w = max(tmp0.w, 0.0);
                tmp0.z = tmp0.z * tmp0.w;
                tmp0.x = tmp0.z * _Params1.z + tmp0.x;
                tmp0.x = -tmp0.x * 0.25 + 1.0;
                tmp0.z = 1.0 - tmp0.x;
                tmp0.w = _Params2.z - _Params2.w;
                tmp0.y = tmp0.y - tmp0.w;
                tmp0.w = _Params2.z - tmp0.w;
                tmp0.y = saturate(tmp0.y / tmp0.w);
                tmp0.x = tmp0.y * tmp0.z + tmp0.x;
                tmp0.y = 1.0 - tmp0.x;
                tmp1 = tex2D(_MainTex, inp.texcoord1.xy);
                tmp0.z = dot(tmp1.xyz, float3(0.2126, 0.7152, 0.0722));
                tmp0.z = tmp0.z * _Params2.y;
                tmp0.xyz = tmp0.zzz * tmp0.yyy + tmp0.xxx;
                tmp0.w = 1.0;
                o.sv_target = saturate(tmp0 + _OcclusionColor);
                return o;
			}
			ENDCG
		}
		Pass {
			ZTest Always
			ZWrite Off
			Cull Off
			Fog {
				Mode 0
			}
			GpuProgramID 334557
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float2 _Direction;
			// $Globals ConstantBuffers for Fragment Shader
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _MainTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                o.texcoord.xy = v.texcoord.xy;
                o.texcoord1.xy = _Direction * float2(1.384615, 1.384615) + v.texcoord.xy;
                o.texcoord1.zw = -_Direction * float2(1.384615, 1.384615) + v.texcoord.xy;
                o.texcoord2.xy = _Direction * float2(3.230769, 3.230769) + v.texcoord.xy;
                o.texcoord2.zw = -_Direction * float2(3.230769, 3.230769) + v.texcoord.xy;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = tex2D(_MainTex, inp.texcoord1.xy);
                tmp0.xyz = tmp0.xyz * float3(0.3162162, 0.3162162, 0.3162162);
                tmp1 = tex2D(_MainTex, inp.texcoord.xy);
                tmp0.xyz = tmp1.xyz * float3(0.227027, 0.227027, 0.227027) + tmp0.xyz;
                tmp1 = tex2D(_MainTex, inp.texcoord1.zw);
                tmp0.xyz = tmp1.xyz * float3(0.3162162, 0.3162162, 0.3162162) + tmp0.xyz;
                tmp1 = tex2D(_MainTex, inp.texcoord2.xy);
                tmp0.xyz = tmp1.xyz * float3(0.0702703, 0.0702703, 0.0702703) + tmp0.xyz;
                tmp1 = tex2D(_MainTex, inp.texcoord2.zw);
                o.sv_target.xyz = tmp1.xyz * float3(0.0702703, 0.0702703, 0.0702703) + tmp0.xyz;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			ZTest Always
			ZWrite Off
			Cull Off
			Fog {
				Mode 0
			}
			GpuProgramID 406947
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float2 _Direction;
			// $Globals ConstantBuffers for Fragment Shader
			float _BilateralThreshold;
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _CameraDepthNormalsTexture;
			sampler2D _MainTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                o.texcoord.xy = v.texcoord.xy;
                o.texcoord1.xy = _Direction * float2(1.384615, 1.384615) + v.texcoord.xy;
                o.texcoord1.zw = -_Direction * float2(1.384615, 1.384615) + v.texcoord.xy;
                o.texcoord2.xy = _Direction * float2(3.230769, 3.230769) + v.texcoord.xy;
                o.texcoord2.zw = -_Direction * float2(3.230769, 3.230769) + v.texcoord.xy;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                float4 tmp2;
                float4 tmp3;
                tmp0 = tex2D(_CameraDepthNormalsTexture, inp.texcoord1.zw);
                tmp0.xyz = tmp0.xyz * float3(3.5554, 3.5554, 0.0) + float3(-1.7777, -1.7777, 1.0);
                tmp0.z = dot(tmp0.xyz, tmp0.xyz);
                tmp0.z = 2.0 / tmp0.z;
                tmp1.xy = tmp0.xy * tmp0.zz;
                tmp1.z = tmp0.z - 1.0;
                tmp0 = tex2D(_CameraDepthNormalsTexture, inp.texcoord.xy);
                tmp0.xyz = tmp0.xyz * float3(3.5554, 3.5554, 0.0) + float3(-1.7777, -1.7777, 1.0);
                tmp0.z = dot(tmp0.xyz, tmp0.xyz);
                tmp0.z = 2.0 / tmp0.z;
                tmp2.xy = tmp0.xy * tmp0.zz;
                tmp2.z = tmp0.z - 1.0;
                tmp0.x = dot(tmp2.xyz, tmp1.xyz);
                tmp0.x = tmp0.x + 1.0;
                tmp0.x = tmp0.x * 0.5;
                tmp0.x = log(tmp0.x);
                tmp0.x = tmp0.x * _BilateralThreshold;
                tmp0.x = exp(tmp0.x);
                tmp0.y = tmp0.x * 0.3162162;
                tmp0.x = tmp0.x * 0.3162162 + 0.227027;
                tmp1 = tex2D(_MainTex, inp.texcoord1.zw);
                tmp0.yzw = tmp0.yyy * tmp1.xyz;
                tmp1 = tex2D(_MainTex, inp.texcoord.xy);
                tmp0.yzw = tmp1.xyz * float3(0.227027, 0.227027, 0.227027) + tmp0.yzw;
                tmp1 = tex2D(_CameraDepthNormalsTexture, inp.texcoord1.xy);
                tmp1.xyz = tmp1.xyz * float3(3.5554, 3.5554, 0.0) + float3(-1.7777, -1.7777, 1.0);
                tmp1.z = dot(tmp1.xyz, tmp1.xyz);
                tmp1.z = 2.0 / tmp1.z;
                tmp3.xy = tmp1.xy * tmp1.zz;
                tmp3.z = tmp1.z - 1.0;
                tmp1.x = dot(tmp2.xyz, tmp3.xyz);
                tmp1.x = tmp1.x + 1.0;
                tmp1.x = tmp1.x * 0.5;
                tmp1.x = log(tmp1.x);
                tmp1.x = tmp1.x * _BilateralThreshold;
                tmp1.x = exp(tmp1.x);
                tmp1.y = tmp1.x * 0.3162162;
                tmp0.x = tmp1.x * 0.3162162 + tmp0.x;
                tmp3 = tex2D(_MainTex, inp.texcoord1.xy);
                tmp0.yzw = tmp3.xyz * tmp1.yyy + tmp0.yzw;
                tmp1 = tex2D(_CameraDepthNormalsTexture, inp.texcoord2.zw);
                tmp1.xyz = tmp1.xyz * float3(3.5554, 3.5554, 0.0) + float3(-1.7777, -1.7777, 1.0);
                tmp1.z = dot(tmp1.xyz, tmp1.xyz);
                tmp1.z = 2.0 / tmp1.z;
                tmp3.xy = tmp1.xy * tmp1.zz;
                tmp3.z = tmp1.z - 1.0;
                tmp1.x = dot(tmp2.xyz, tmp3.xyz);
                tmp1.x = tmp1.x + 1.0;
                tmp1.x = tmp1.x * 0.5;
                tmp1.x = log(tmp1.x);
                tmp1.x = tmp1.x * _BilateralThreshold;
                tmp1.x = exp(tmp1.x);
                tmp1.y = tmp1.x * 0.0702703;
                tmp0.x = tmp1.x * 0.0702703 + tmp0.x;
                tmp3 = tex2D(_MainTex, inp.texcoord2.zw);
                tmp0.yzw = tmp3.xyz * tmp1.yyy + tmp0.yzw;
                tmp1 = tex2D(_CameraDepthNormalsTexture, inp.texcoord2.xy);
                tmp1.xyz = tmp1.xyz * float3(3.5554, 3.5554, 0.0) + float3(-1.7777, -1.7777, 1.0);
                tmp1.z = dot(tmp1.xyz, tmp1.xyz);
                tmp1.z = 2.0 / tmp1.z;
                tmp3.xy = tmp1.xy * tmp1.zz;
                tmp3.z = tmp1.z - 1.0;
                tmp1.x = dot(tmp2.xyz, tmp3.xyz);
                tmp1.x = tmp1.x + 1.0;
                tmp1.x = tmp1.x * 0.5;
                tmp1.x = log(tmp1.x);
                tmp1.x = tmp1.x * _BilateralThreshold;
                tmp1.x = exp(tmp1.x);
                tmp1.y = tmp1.x * 0.0702703;
                tmp0.x = tmp1.x * 0.0702703 + tmp0.x;
                tmp2 = tex2D(_MainTex, inp.texcoord2.xy);
                tmp0.yzw = tmp2.xyz * tmp1.yyy + tmp0.yzw;
                o.sv_target.xyz = tmp0.yzw / tmp0.xxx;
                o.sv_target.w = 1.0;
                return o;
			}
			ENDCG
		}
		Pass {
			ZTest Always
			ZWrite Off
			Cull Off
			Fog {
				Mode 0
			}
			GpuProgramID 491001
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 position : SV_POSITION0;
				float2 texcoord : TEXCOORD0;
				float2 texcoord1 : TEXCOORD1;
			};
			struct fout
			{
				float4 sv_target : SV_Target0;
			};
			// $Globals ConstantBuffers for Vertex Shader
			float4 _MainTex_TexelSize;
			// $Globals ConstantBuffers for Fragment Shader
			// Custom ConstantBuffers for Vertex Shader
			// Custom ConstantBuffers for Fragment Shader
			// Texture params for Vertex Shader
			// Texture params for Fragment Shader
			sampler2D _MainTex;
			sampler2D _SSAOTex;
			
			// Keywords: 
			v2f vert(appdata_full v)
			{
                v2f o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = v.vertex.yyyy * unity_ObjectToWorld._m01_m11_m21_m31;
                tmp0 = unity_ObjectToWorld._m00_m10_m20_m30 * v.vertex.xxxx + tmp0;
                tmp0 = unity_ObjectToWorld._m02_m12_m22_m32 * v.vertex.zzzz + tmp0;
                tmp0 = tmp0 + unity_ObjectToWorld._m03_m13_m23_m33;
                tmp1 = tmp0.yyyy * unity_MatrixVP._m01_m11_m21_m31;
                tmp1 = unity_MatrixVP._m00_m10_m20_m30 * tmp0.xxxx + tmp1;
                tmp1 = unity_MatrixVP._m02_m12_m22_m32 * tmp0.zzzz + tmp1;
                o.position = unity_MatrixVP._m03_m13_m23_m33 * tmp0.wwww + tmp1;
                tmp0.x = _MainTex_TexelSize.y < 0.0;
                tmp0.y = 1.0 - v.texcoord.y;
                o.texcoord.y = tmp0.x ? tmp0.y : v.texcoord.y;
                o.texcoord1 = v.texcoord.xxy;
                return o;
			}
			// Keywords: 
			fout frag(v2f inp)
			{
                fout o;
                float4 tmp0;
                float4 tmp1;
                tmp0 = tex2D(_SSAOTex, inp.texcoord.xy);
                tmp1 = tex2D(_MainTex, inp.texcoord1.xy);
                o.sv_target.xyz = tmp0.xyz * tmp1.xyz;
                o.sv_target.w = tmp1.w;
                return o;
			}
			ENDCG
		}
	}
}