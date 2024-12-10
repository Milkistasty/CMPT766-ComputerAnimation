// student ID: 301586823
// student name: Gouttham Nambirajan
// email: gna23@sfu.ca

Shader "Teleport"

// teleport effect

{
    Properties
    {
        [NoScaleOffset]_Albedo("Albedo", 2D) = "white" {}  // Texture maps for surface color and detail
        [NoScaleOffset]_Normal("Normal", 2D) = "bump" {}  // Texture maps for surface color and detail
        _Metallic("Mettalic", Float) = 0.5  // Surface material properties for realistic lighting
        _Smoothness("Smoothness", Float) = 0.5  // Surface material properties for realistic lighting
        
        // Dissolve
        _DissolveAmount("Dissolve Amount", Float) = 0  // Control the dissolve effect
        _DissolveOffset("Dissolve Offset", Float) = 0  // Control the dissolve effect
        _DissolveSpread("Dissolve Spread", Float) = 1  // Control the dissolve effect
        
        _ClipValue("Clip Value", Float) = 0.5
        
        // Adds randomness with 3D noise
        _PerlinNoiseScale("Noise Scale", Float) = 10
        _NoiseStretch("Noise Stretch", Vector) = (10, 1, 10, 0)
        _NoiseIntensity("NoiseIntensity", Float) = 1
        
        // Controls vertex distortion/stretching
        _StretchScale("Stretch Scale", Float) = 2
        
        // stretch
        _VertexStretchOffset("Vertex Stretch Offset", Float) = 0
        _VertexStretchSpread("Vertex Stretch Spread", Float) = 1
        
        // Glowing effect around dissolving edges
        _EmissionOffset("Emission Offset", Float) = 0
        _EmissionDistance("Emission Distance", Float) = 1
        [HDR]_EmissionColor("Emission Color(RGB), Intensity(A)", Color) = (0,0,0,1)
        
        // Rim lighting effect controls the glow at the object's edges like the body surface
        [HDR]_RimColor("Rim Color(RGB)", Color) = (1,1,1,1)
        _RimPower("Rim Power", Float) = 10
        _RimIntensity("Rim Intensity", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
        }
        Pass
        {
            Tags
            {
                "LightMode" = "ForwardBase"
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityStandardUtils.cginc"
            #include "UnityLightingCommon.cginc"
            #include "A_Tools.cginc"
            #include "Noise/ClassicNoise3D.hlsl"

            sampler2D _Albedo;
            sampler2D _Normal;
            float _Metallic;
            float _Smoothness;

            float _DissolveAmount;
            float _DissolveOffset;
            float _DissolveSpread;

            float _ClipValue;

            float _PerlinNoiseScale;
            float3 _NoiseStretch;
            float _NoiseIntensity;
            
            float _StretchScale;

            float _VertexStretchOffset;
            float _VertexStretchSpread;

            float _EmissionOffset;
            float _EmissionDistance;
            fixed4 _EmissionColor;

            fixed3 _RimColor;
            float _RimPower;
            float _RimIntensity;
            
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv: TEXCOORD0;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float4 tangent : TEXCOORD1;
                float2 uv: TEXCOORD2;
                SHADOW_COORDS(3)
                float3 normal : TEXCOORD3;
                float3 objectPos : TEXCOORD4;
            };
            
            float3 CreateBinormal (float3 normal, float3 tangent, float binormalSign) {
	            return cross(normal, tangent.xyz) *
		            (binormalSign * unity_WorldTransformParams.w);
            }

            void IntializeFragmentNormal(inout v2f i)
            {
                float3 tangentSpaceNormal = UnpackScaleNormal(tex2D(_Normal, i.uv.xy), 1);
                float3 binormal = CreateBinormal(i.normal, i.tangent.xyz, i.tangent.w);
                i.normal = normalize(
		            tangentSpaceNormal.x * i.tangent +
		            tangentSpaceNormal.y * binormal +
		            tangentSpaceNormal.z * i.normal
	            );
            }

            v2f vert (appdata v)
            {
                v2f o;

                // Vertex Stretching
                float yObject = v.vertex.x;
                float yMask = yObject - _DissolveAmount - _VertexStretchOffset;
                yMask = yMask / _VertexStretchSpread;
                // Deforms vertices along the x-axis based on dissolve parameters
                float vertexOffsetMask = pow(max(1-yMask,0),1.5);
                v.vertex.x = v.vertex.x - vertexOffsetMask * _StretchScale;
                    
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
                o.uv = v.uv;
                TRANSFER_SHADOW(o)  // shadow mapping
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.objectPos = v.vertex;
                
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                 // Y Mask
                float yObject = i.objectPos.x;
                float yMask = yObject - _DissolveAmount - _DissolveOffset;
                yMask = yMask / _DissolveSpread;
                
                // 3D Noise: use it to creat some noises like some black points in the map
                // from https://github.com/JimmyCushnie/Noisy-Nodes/blob/master/README.md

                // Combines a dissolve mask and 3D Perlin noise for a dynamic, procedural transition
                float noise1 = PerlinNoise3D(i.worldPos * _NoiseStretch, 
                _PerlinNoiseScale);

                // noise [0, 1] to [-1, 1]
                float noise2 = noise1 * 2 - 1;
                yMask = yMask + noise2 * _NoiseIntensity;
                clip(yMask - _ClipValue);  // discards pixels outside the dissolve threshold, creating the fading effect

                // Emission Glow
                float emissionMask = 1 - distance((yObject - _EmissionOffset),
                 _DissolveAmount) /_EmissionDistance;
                emissionMask = max(emissionMask, 0) * noise1;
                fixed3 emission = emissionMask * _EmissionColor * 
                _EmissionColor.a;
                
                IntializeFragmentNormal(i);
                float oneMinusReflectivity = 1 - _Metallic;
                // lambert -1 to 1
                float3 lightDir = _WorldSpaceLightPos0;
                lightDir = normalize(lightDir);
                float lambert = dot(lightDir, i.normal);
                lambert = clamp(lambert, 0, 1);  // diffuse lighting based on the angle between the light direction and surface normal

                // blinn-phong lighting model
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 halfDir = normalize(viewDir + lightDir);
                float phong = dot(halfDir, i.normal);  // Creates specular highlights using the half-vector method
                phong = clamp(phong, 0, 1);
                phong = pow(phong, _Smoothness * 100);
                float3 ambient = ShadeSH9(half4(i.normal, 1)); // effect is not very good, uses spherical harmonics for ambient lighting

               // Rim lighting 
               // Simulates a glowing outline at the object’s edges
                float rimMask = 1 - dot(viewDir, i.normal);
                rimMask = pow(rimMask, _RimPower);
                fixed3 rim = rimMask * _RimColor * _RimIntensity;
                
                // ombines all effects
                fixed3 finalColor = ((lambert+0.1f) * tex2D(_Albedo, i.uv)) *
                 oneMinusReflectivity + phong * _LightColor0 * _Metallic + 
                 emission + rim;
                
                return OutputTestColor(finalColor);
            }
            ENDCG
        }
    }
	Fallback "Diffuse"  // fallback shader ensures compatibility with systems that cannot render advanced features
}