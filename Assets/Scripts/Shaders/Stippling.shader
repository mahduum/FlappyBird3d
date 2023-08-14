Shader "Custom/StipplingSurfaceShader"
{
    Properties
    {
        _Color ("Color", Color) = (.5,.5,.4,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Alpha ("Alpha", Range(0,1)) = 0.5
        _FadeDst ("Fade Distance", Range(0,100)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Lambert //noforwardadd

        // Use shader model 3.0 target, to get nicer looking lighting
        //#pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
            float3 worldPos;
        };

        half _Alpha;
        half _FadeDst;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o)
        {
            //float3 cameraPos = _WorldSpaceCameraPos;
            //float dst = length(IN.worldPos - cameraPos);

            half alpha = IN.screenPos.w / _FadeDst;//to fit the scale?

            const float4x4 thresholdMatrix =
            {
                1, 9, 3, 11,
                13, 5, 15, 7,
                4, 12, 2, 10,
                16, 8, 14, 6
            };
        
            // multiply screen pos by width/height of screen to get pixel coord
            float2 pixelPos = IN.screenPos.xy / IN.screenPos.w * _ScreenParams.xy;
        
            // threshold of current pixel, divide by 17 to close in range 0 to 1
            const float threshold = thresholdMatrix[pixelPos.x % 4][pixelPos.y % 4] / 17;
        
            o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
            o.Albedo *= _Color;
        
            // discard pixels that have threshold greater than alpha
            clip(alpha - threshold);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
