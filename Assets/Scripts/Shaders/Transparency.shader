Shader "Custom/Transparency"
{
    Properties
    {
        _Color ("Color", Color) = (.5,.5,.4,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Alpha ("Alpha", float) = 0.5
        _FadeDist ("Fade Distance", float) = 100.0
        
    }
    SubShader
    {
        Tags { "RenderType"="AlphaTest" }

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            //float4 screenPos;
            float3 worldPos;
        };
        //
        half _Alpha;
        half _FadeDst;
        // //
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        //UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        //UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutput o)
        {
            float3 cameraPos = _WorldSpaceCameraPos;
            float dst = length(IN.worldPos - cameraPos) - 30.0;
        
            half fade = saturate((dst/_FadeDst));
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo =  c.rgb * _Color.rgb;
            o.Alpha = _Alpha;
            o.Alpha *= fade;

            //stripes
            //clip (frac((IN.worldPos.y + _Time.x + IN.worldPos.z * 0.1) * 5) - 0.5);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
