// Student ID: 301586596
// Student Name: Wenhe Wang
// Student Email: wwa118@sfu.ca

Shader "Final/Hologram"

{
	// parameters
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}  // The main texture applied to the hologram
        _RimMin("RimMin", Range(-1,1)) = 0.0  // The minimum value for the rim effect (controls the spread and strength of rim lighting)
        _RimMax("RimMax", Range(0,2)) = 1.0  // The maximum value for the rim effect (controls the spread and strength of rim lighting)
        _RimIntensity("Rim Intensity", Float) = 1.0  // Controls the intensity of the rim lighting (higher values result in a stronger rim effect)
        _InnerColor("Inner Color", Color) = (0.0, 0.0, 0.0, 0.0)  // The color of the inner part of the hologram (used in rim lighting)
        _RimColor("Rim Color", Color) = (1,1,1,1)  // The color of the rim lighting effect
        _FlowTilling("Flow Tilling", Vector) = (1,1,0,0)  // Tiling factor for the flow texture (controls how the scanlines repeat across the surface)
        _FlowSpeed("Flow Speed", Vector) = (1,1,0,0)  // Speed at which the flow texture moves (affects the rate of the scanline movement)
        _FlowTex("Flow Tex", 2D) = "white" {}  // The texture used for the scanning effect on the hologram (typically a line or grid pattern)
        _FlowIntensity("Flow Intensity", Float) = 0.5  // Controls the strength and movement of the scanning effect on the hologram’s texture
        _GlowIntensity ("Glow Intensity", Range(0.0, 2.0)) = 1.0  // Controls the emission of light, making the hologram brighter or dimmer
        _GlitchChance ("Glitch Chance", Range(0.0, 1.0)) = 0.1  // The probability of a glitch occurring each frame (affects the frequency of glitches)
        _GlitchSpeed ("Glitch Speed", Range(0, 50)) = 50.0  // Controls the speed of the glitch effect (higher values make the glitching more erratic)
        _GlitchIntensity ("Glitch Intensity", Range(0.0, 0.3)) = 0.05  // Controls the strength of the glitch effect (higher values make the glitch more intense)
        _FlickerSpeed ("Flicker Speed", Range(0.0, 10.0)) = 1.0  // Controls how fast the flicker effect happens (higher values make it faster)
        _FlickerIntensity ("Flicker Intensity", Range(0.0, 1.0)) = 0.5  // Controls the intensity of the flicker effect (higher values make the flicker more noticeable)
        _InnerAlpha("Inner Alpha", Range(0.0, 1.0)) = 0.0  // Controls the transparency of the inner part of the hologram
    }

    SubShader
    {	
		// render this material after opaque objects but before fully transparent ones
		Tags { "RenderType"="Transparent" "IgnoreProjector"="true" "Queue"="Transparent" "DisableBatching"="true"}
        LOD 100

		// Blending
        Pass
        {
			// closing writes depth
			ZWrite Off
			// set up blending status
			// Alpha Blend = SrcColor * SrcAlpha + DestColor * 1.0
			Blend SrcAlpha One

			// Unity CG call
            CGPROGRAM
            #pragma vertex vert  // init vectex shader
            #pragma fragment frag  // init fragment shader
			// include UnityCG.cginc lib
            #include "UnityCG.cginc"

			// get model data from CPU
            struct appdata
            {
                float4 vertex : POSITION;  
				float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
				float4 color : COLOR;
            };

			// vertex shader output
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
				float3 pos_world : TEXCOORD1;
				float3 normal_world : TEXCOORD2;
				float3 pivot_world : TEXCOORD3;
            };

			sampler2D _MainTex;
            float4 _MainTex_ST;
            float _RimMin;
            float _RimMax;
            float4 _InnerColor;
            float4 _RimColor;
            float _RimIntensity;
            float4 _FlowTilling;
            float4 _FlowSpeed;
            sampler2D _FlowTex;
            float _FlowIntensity;
            half _GlowIntensity;
            half _GlitchSpeed, _GlitchIntensity;
            half _FlickerSpeed, _FlickerIntensity;
            float _InnerAlpha;

			// vertex shader: transform the vertex coordinates from object space to clip space
            v2f vert (appdata v)
            {
				v2f o;

				// Glitch Effect: Apply sinusoidal offset for vertex displacement
				v.vertex.z += sin(_Time.y * _GlitchSpeed * 5 * v.vertex.y) * _GlitchIntensity;

                o.vertex = UnityObjectToClipPos(v.vertex);
                float3 normal_world = mul(float4(v.normal, 0.0), unity_WorldToObject).xyz;
                float3 pos_world = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal_world = normalize(normal_world);
                o.pos_world = pos_world;
                o.pivot_world = mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0)).xyz;
                o.uv = v.uv;
                return o;
            }

			// fragment shader: output final color in the screen
            fixed4 frag (v2f i) : SV_Target
            {	
				
				half3 normal_world = normalize(i.normal_world);
                half3 view_world = normalize(_WorldSpaceCameraPos.xyz - i.pos_world);
                half NdotV = saturate(dot(normal_world, view_world));
                half fresnel = 1.0 - NdotV;
                fresnel = smoothstep(_RimMin, _RimMax, fresnel);

				// Rim Lighting Effect: Highlight edges
                half emiss = tex2D(_MainTex, i.uv).r;
                emiss = pow(emiss, 5.0);
                half final_fresnel = saturate(fresnel + emiss);
                half3 final_rim_color = lerp(_InnerColor.xyz, _RimColor.xyz * _RimIntensity, final_fresnel);
                half final_rim_alpha = final_fresnel;

                // Scanline Effect
                // 1 - this -> from bottom to up, without minus -> from up to bottom 
                half2 uv_flow = 1 - (i.pos_world.xy - i.pivot_world.xy) * _FlowTilling.xy;
                uv_flow = uv_flow + _Time.y * _FlowSpeed.xy;
                float4 flow_rgba = tex2D(_FlowTex, uv_flow) * _FlowIntensity;

				// Flicker Effect: shine unstreadily
				half flicker = 1.0 + sin(_Time.y * _FlickerSpeed) * _FlickerIntensity;

				// final color: combine Rim light, Scanline, and Flicker
				float3 final_col = final_rim_color + flow_rgba.xyz;
                final_col.rgb *= flicker;  // Apply flicker to RGB
                final_col.rgb *= _GlowIntensity;  // Apply the glow intensity to the color
				float final_alpha = saturate(final_rim_alpha + flow_rgba.a + _InnerAlpha);
                return float4(final_col, final_alpha);
            }
            ENDCG
        }
    }
}
