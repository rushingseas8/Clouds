// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/*
Custom cloud shader. Supports:
- Texture offsets in the x/y axes
- Density adjustment to make clouds fade in/out
- Post-process multiply to color correct
*/
Shader "Custom/CloudShader"
{
    Properties
    {   
        // Basic shader variables
        // TODO add a second texture for the low-octave perlin noise mask
        // This will allow for better control of cloud cover than just adjusting the density.
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        // X and Y offset for the texture. Used to represent moving clouds.
        _OffsetX ("Offset X", Range(0,1)) = 0.0
        _OffsetY ("Offset Y", Range(0,1)) = 0.0
        
        // Flat add/subtract a value. We'll then normalize the resulting range.
        _CloudDensity ("Cloud Density", Range(0, 1.0)) = 0.5
        _CloudMultiply ("Cloud Multiply", Range(0.0, 3.0)) = 1.0
        
        // How stormy are these clouds? Affects color
        _Storminess ("Storminess", Range(0.0, 1.0)) = 0.0
        
        // Alpha cutoff for shadows
        _Cutoff ("Cutoff", Range(0.0, 1.0)) = 0.4
        
        // Fresnel stuff
        _Power ("Fresnel Power", Range(0.0, 1000)) = 2.0
        _Scale ("Fresnel Scale", Range(0.0, 1000)) = 0.1
        _SunAngle ("Sun angle", Range(0.0, 1.0)) = 0.0
        
        // Colors used for basic shading of clouds
        // Lerps between Clear and Full as noise in [0, 1], and after that, will
        // lerp between Full and Over as noise in [1, 2].
        // Stormy color is used for the _Storminess slider, and both Full and Over
        // will lerp to it using the _Storminess value. 
        _ClearColor ("Clear Color", Color) = (0, 0, 0, 0)
        _FullColor ("Full Color", Color) = (1, 1, 1, 1)
        _OverColor ("Over Color", Color) = (0.33, 0.33, 0.33, 1)
        _StormyColor ("Stormy Color", Color) = (0.33, 0.33, 0.33, 1.0)
        
        _StartEdgeColor ("Start Edge Color", Color) = (0.8, 0.8, 0.2, 1.0)
        _EndEdgeColor ("End Edge Color", Color) = (1, 0, 0, 1.0)
    }
    SubShader
    {
        Tags 
        { 
            //"Queue"="Transparent"
            //"RenderType"="Transparent" 
            "Queue"="Transparent"
            "RenderType"="Transparent" 
        }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        //#pragma surface surf Standard fullforwardshadows alpha:fade
        #pragma surface surf Standard fullforwardshadows addshadow alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        half _OffsetX;
        half _OffsetY;
        half _CloudDensity;
        half _CloudMultiply;
        half _Storminess;
        
        half _Power;
        half _Scale;
        half _SunAngle;
        
        fixed4 _ClearColor;
        fixed4 _FullColor;
        fixed4 _OverColor;
        fixed4 _StormyColor;
        
        fixed4 _StartEdgeColor;
        fixed4 _EndEdgeColor;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // We use a noise texture as our raw input.
            
            // Offset into the noise texture
            float2 offset = float2(_OffsetX, _OffsetY);
            
            // Because the texture is alpha-only, we use only its alpha channel
            float noise = tex2D (_MainTex, IN.uv_MainTex + offset).a;          
            
            //noise = smoothstep(0, 1, noise); // Test- maybe things look better if we smooth the noise?
            
            noise += (_CloudDensity * 2.0) - 1.0;   // Add the density modulation
            //noise = clamp(noise, 0, 1);             // Clamp to range [0, 1] to prevent extraneous values
            noise = clamp(noise, 0, 10);            // Test- Just don't allow negative values (this prevents losing information at high _CloudDensity)
            //noise /= _CloudDensity * 1.1f;          // Normalize range
            //noise *= 0.92f * 1.0f / (2.6f * pow(_CloudDensity, 2.4f));
            
            // Automatic intensity multiplier
            // This will scale the function so that the noise will on average go in the range [0.0, 1.0]
            if (_CloudDensity < 0.5f) 
            {
                noise /= 3.0f * pow(_CloudDensity, 2.4f);
            }
            else
            {
                noise /= 1.5f * pow(_CloudDensity, 1.4f);
            }
            
            noise *= _CloudMultiply + _Storminess * 0.5f;                // Post-correct color, if desired
            
            // Color adjustment due to storminess
            fixed4 FullColor = lerp(_FullColor, _StormyColor, _Storminess);
            fixed4 OverColor = lerp(_OverColor, _StormyColor, _Storminess - 0.4f);
            
            // TODO remove if statement
            fixed4 color;
            if (noise < 1.0f) 
            {
                color = lerp(_ClearColor, FullColor, noise);
            }
            else 
            {
                color = lerp(FullColor, OverColor, noise - 1.0f);
            }
            //color = lerp(lerp(_ClearColor, FullColor, noise), lerp(FullColor, OverColor, noise - 1.0f), noise);
            //fixed4 color = lerp(lerp(_ClearColor, _FullColor, noise), _OverColor, noise - 1.0f);
            
            float2 uvs = IN.uv_MainTex;
            // Fresnel effect for sunset
            float3 worldLightPos = WorldSpaceLightDir(UnityObjectToClipPos(IN.worldPos));
            //float R = _Scale * pow(1.0 + dot(normalize(IN.viewDir), o.Normal), _Power);
            //float R = _Scale * pow(1.0 + dot(normalize(worldLightPos), o.Normal), _Power);
            //float R = _Scale * pow(1.0 + abs(dot(normalize(worldLightPos), float3(0, 0.2, 1))), _Power);
            //float R = _Scale * pow(
            //    1.0 + dot(
            //        normalize(worldLightPos), 
            //        normalize(float3(
            //            cos((uvs.x - 0.5f) * 3.14159265358), 
            //            0, 
            //            //cos(uvs.y)
            //            0
            //        ))
            //    ), 
            //    _Power
            //);
            
            //float R = _Scale * pow(1.0 + sqrt(pow(cos((uvs.x - 0.5f) * 3.14159265358), 2.0) + pow(cos(uvs.y), 2.0)), _Power);
            
            // Let's use world pos instead
            float pi = 3.14159265358;
            float xContrib = cos(pi / 2.0 * clamp(IN.worldPos.x / 750.0, -1.0, 1.0));
            float yContrib = sin(pi / 2.0 * clamp(IN.worldPos.z / 750.0, -1.0, 1.0));
            
            // Restrict sunrise/set effect to the appropriate hemisphere
            if (_SunAngle < 0.25f || _SunAngle > 0.75f) 
            {
                // Restrict to negative hemisphere for sunrise
                yContrib = clamp(yContrib, -1.0, 0.0);
            }
            else 
            {
                // Restrict to positive hemisphere for sunset
                yContrib = clamp(yContrib, 0.0, 1.0);
            }
            
            float sunTime = max(0, cos(_SunAngle * 4.0 * pi));
            //float yContrib = clamp(IN.worldPos.z / 750.0, -1.0, 1.0);
            // Note: good values for _Scale and _Power are 0.034 and 3.61, respectively.
            //float R = _Scale * pow(1.0 + pow(pow(xContrib, 2.0) * pow(yContrib, 2.0), 0.2), _Power);
            float R = _Scale * pow(1.0 + (pow(xContrib, 2.0) * pow(yContrib, 2.0)), _Power);
            //R = R * sunTime;
            
            // Lerp between the clouds + sunset color.
            // Sunset color is itself lerped based on the fresnel effect strength, 
            // from _StartEdgeColor to _EndEdgeColor.
            color.rgb = lerp(color, lerp(_StartEdgeColor, _EndEdgeColor, R), R * sunTime * sunTime * sunTime).rgb;
            
            //color = lerp(_StartEdgeColor, _EndEdgeColor, R); // Debug fresnel effect
            //color = lerp(float4(0,0,0,1), lerp(_StartEdgeColor, _EndEdgeColor, R), R * sunTime * sunTime * sunTime);
            
            o.Albedo = color.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = color.a;
        }
        
        void frag() {}
        ENDCG
    }
    FallBack "Diffuse"
}
