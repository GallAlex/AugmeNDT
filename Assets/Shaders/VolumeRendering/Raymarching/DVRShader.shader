Shader "Volume Rendering/RaymarchingShader/DVR"
{
    Properties
    {
        _MainTex("Texture", 3D) = "white" {}
        _StepSize("Step Size", float) = 0.01
        _MinVal("Min val", Range(0.0, 1.0)) = 0.0
        _MaxVal("Max val", Range(0.0, 1.0)) = 1.0
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            LOD 100
            Cull Off
            ZTest LEqual
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                // Maximum amount of raymarching samples
                #define MAX_STEP_COUNT 128

                // Allowed floating point inaccuracy
                #define EPSILON 0.00001f

                struct appdata
                {
                    float4 vertex : POSITION;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float3 objectVertex : TEXCOORD0;
                    float3 vectorToSurface : TEXCOORD1;
                };

                sampler3D _MainTex;
                float4 _MainTex_ST;
                float _StepSize;
                float _MinVal;
                float _MaxVal;

                v2f vert(appdata v)
                {
                    v2f output;

                    // Vertex in object space this will be the starting point of raymarching
                    output.objectVertex = v.vertex;

                    // Calculate vector from camera to vertex in world space
                    float3 worldVertex = mul(unity_ObjectToWorld, v.vertex).xyz;
                    output.vectorToSurface = worldVertex - _WorldSpaceCameraPos;

                    output.vertex = UnityObjectToClipPos(v.vertex);
                    return output;
                }

                float4 BlendUnder(float4 color, float4 newColor)
                {
                    color.rgb += (1.0 - color.a) * newColor.a * newColor.rgb;
                    color.a += (1.0 - color.a) * newColor.a;
                    return color;
                }

                // Returns the density at the specified position
                float getDensity(float3 pos)
                {
                    return tex3Dlod(_MainTex, float4(pos.x, pos.y, pos.z, 0.0f));
                }

                fixed4 frag(v2f input) : SV_Target
                {
                    // Start raymarching at the front surface of the object
                    float3 rayOrigin = input.objectVertex;

                    // Use vector from camera to object surface to get ray direction
                    float3 rayDirection = mul(unity_WorldToObject, float4(normalize(input.vectorToSurface), 1));

                    float4 color = float4(0, 0, 0, 0);
                    float3 samplePosition = rayOrigin;

                    // Raymarch through object space
                    for (int input = 0; input < MAX_STEP_COUNT; input++)
                    {
                        // Accumulate color only within unit cube bounds
                        if (max(abs(samplePosition.x), max(abs(samplePosition.y), abs(samplePosition.z))) < 0.5f + EPSILON)
                        {
                            const float density = tex3D(_MainTex, samplePosition + float3(0.5f, 0.5f, 0.5f)); //Density
                            float4 sampledColor = float4(density, density, density, 0.02);
            
                            color = BlendUnder(color, sampledColor);

                            if (color.a > 1.0f)
                                break;

                            samplePosition += rayDirection * _StepSize;
                        }
                    }

                    return color;
                }
                ENDCG
            }
        }
}