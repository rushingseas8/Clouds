/*
This shader handles simple cutout shadows for clouds, as well as the full suite
of features that the "CloudShader" provides (density, offset, etc.)
*/
Shader "Custom/CloudShadowShader"
{
    Properties
    {
        // Basic shader variables
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        
        // X and Y offset for the texture. Used to represent moving clouds.
        _OffsetX ("Offset X", Range(0,1)) = 0.0
        _OffsetY ("Offset Y", Range(0,1)) = 0.0
        
        // Flat add/subtract a value. We'll then normalize the resulting range.
        _CloudDensity ("Cloud Density", Range(0, 1.0)) = 0.5
        _CloudMultiply ("Cloud Multiply", Range(0.0, 3.0)) = 1.0
        
        _Cutoff("Cutoff", Range(0.0, 1.0)) = 0.4
    }
    SubShader
    {
        Tags 
        { 
            //"Queue"="Transparent"
            //"RenderType"="Transparent" 
            "Queue"="Transparent"
            "RenderType"="Transparent" 
            "IgnoreProjector"="True"
        }
        LOD 200
        
        Blend SrcAlpha OneMinusSrcAlpha
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        //#pragma surface surf Standard fullforwardshadows alpha:fade
        #pragma surface surf Standard fullforwardshadows addshadow alphatest:_Cutoff
        //#pragma surface surf Standard addshadow alpha:fade
        
        // NOTE: Standard shader has dithering support for transparent shadows, but needs
        // to be modified to support the additional values we have like offset and density.
        // For now, this is a basic shader that only supports cutoff shadows.

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        half _OffsetX;
        half _OffsetY;
        half _CloudDensity;
        half _CloudMultiply;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            //// We use a noise texture as our raw input.
            //fixed4 noise = tex2D (_MainTex, IN.uv_MainTex + (_OffsetX, _OffsetY));          
            
            
            //noise += (_CloudDensity * 2.0) - 1.0;   // Add the density modulation
            //noise = clamp(noise, 0, 1);             // Clamp
            //noise /= _CloudDensity * 2.0f;          // Normalize range
            
            
            ////if (_CloudDensity < 0.5f)
            ////{
            ////    //noise *= 1.0f + (2.0f * (0.5f - _CloudDensity));
            ////    //noise.x = 1.0f;
            ////}
            //noise *= _CloudMultiply;                // Post-correct color, if desired
            
            float2 offset = float2(_OffsetX, _OffsetY);
            
            // Because the texture is alpha-only, we use only its alpha channel
            float noise = tex2D (_MainTex, IN.uv_MainTex + offset).a;          
            
            noise += (_CloudDensity * 2.0) - 1.0;   // Add the density modulation
            noise = clamp(noise, 0, 10);            // Just don't allow negative values (this prevents losing information at high _CloudDensity)
            
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
            
            noise *= _CloudMultiply;                // Post-correct color, if desired
            
            
            //fixed4 color = noise * _Color;
            fixed4 color = lerp(0.0f, 1.0f, noise);
            
            
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
