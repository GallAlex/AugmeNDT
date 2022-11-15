Shader "Volume Rendering/RaymarchingShader/MIP"
{
    Properties
    {
        _MainTex("Texture", 3D) = "" {}
        _MAX_STEP_COUNT("MAX STEP COUNT", float) = 256
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
                #pragma multi_compile_instancing

                #include "UnityCG.cginc"

                // Maximum amount of raymarching samples
                //#define MAX_STEP_COUNT 512
                //#define MAX_STEP_COUNT 256

                struct vert_in
                {
                    float4 vertex : POSITION;
                    float4 normal : NORMAL;
                    float2 uv : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float3 vertexLocal : TEXCOORD1;
                    float2 uv : TEXCOORD0;
                    float3 normal : NORMAL;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                struct RayInfo
                {
                    float3 startPos;
                    float3 endPos;
                    float3 direction;
                    float2 aabbInters;
                };

                struct RaymarchInfo
                {
                    RayInfo ray;
                    int numSteps;
                    float numStepsRecip;
                    float stepSize;
                };

                sampler3D _MainTex;
                float _MAX_STEP_COUNT;
                float _MinVal;
                float _MaxVal;

                // Find near and far ray intersection points with axis aligned bounding box
                float2 intersectAABB(float3 rayOrigin, float3 rayDir, float3 boxMin, float3 boxMax)
                {
                    float3 tMin = (boxMin - rayOrigin) / rayDir;
                    float3 tMax = (boxMax - rayOrigin) / rayDir;
                    float3 t1 = min(tMin, tMax);
                    float3 t2 = max(tMin, tMax);
                    float tNear = max(max(t1.x, t1.y), t1.z);
                    float tFar = min(min(t2.x, t2.y), t2.z);
                    return float2(tNear, tFar);
                };

                // Get a ray for the specified fragment (back-to-front)
                RayInfo getRayBack2Front(float3 vertexLocal)
                {
                    RayInfo ray;
                    ray.direction = normalize(ObjSpaceViewDir(float4(vertexLocal, 0.0f)));
                    ray.startPos = vertexLocal + float3(0.5f, 0.5f, 0.5f);
                    // Find intersections with axis aligned boundinng box (the volume)
                    ray.aabbInters = intersectAABB(ray.startPos, ray.direction, float3(0.0, 0.0, 0.0), float3(1.0f, 1.0f, 1.0));

                    // Check if camera is inside AABB
                    const float3 farPos = ray.startPos + ray.direction * ray.aabbInters.y - float3(0.5f, 0.5f, 0.5f);
                    float4 clipPos = UnityObjectToClipPos(float4(farPos, 1.0f));
                    ray.aabbInters += min(clipPos.w, 0.0);

                    ray.endPos = ray.startPos + ray.direction * ray.aabbInters.y;
                    return ray;
                }

                RaymarchInfo initRaymarch(RayInfo ray, int maxNumSteps)
                {
                    RaymarchInfo raymarchInfo;
                    raymarchInfo.stepSize = 1.732f/*greatest distance in box*/ / maxNumSteps;
                    raymarchInfo.numSteps = (int)clamp(abs(ray.aabbInters.x - ray.aabbInters.y) / raymarchInfo.stepSize, 1, maxNumSteps);
                    raymarchInfo.numStepsRecip = 1.0 / raymarchInfo.numSteps;
                    return raymarchInfo;
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

                v2f vert(vert_in v)
                {
                    v2f output;

                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_OUTPUT(v2f, output);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                    UNITY_TRANSFER_INSTANCE_ID(v, output);

                    output.vertex = UnityObjectToClipPos(v.vertex);
                    output.vertexLocal = v.vertex;
                    output.normal = UnityObjectToWorldNormal(v.normal);
                    output.uv = v.uv;

                    return output;
                }

                //UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);

                fixed4 frag(v2f input) : SV_Target
                {
                    UNITY_SETUP_INSTANCE_ID(input);
                    //fixed4 sampledColor = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, input.uv);

                    // Start raymarching at the front surface of the object
                    //float3 rayOrigin = input.objectVertex;  
                    RayInfo ray = getRayBack2Front(input.vertexLocal);
                    RaymarchInfo raymarchInfo = initRaymarch(ray, _MAX_STEP_COUNT);

                    // Use vector from camera to object surface to get ray direction
                    //float3 rayDirection = mul(unity_WorldToObject, float4(normalize(input.vectorToSurface), 1));
                    
                    float maxDensity = 0.0f;
                    float3 maxDensityPos = ray.startPos;
                    

                    // Raymarch through object space
                    for (int step = 0; step < raymarchInfo.numSteps; step++)
                    {
                        const float t = step * raymarchInfo.numStepsRecip;
                        const float3 currPos = lerp(ray.startPos, ray.endPos, t);
                        //const float density = tex3D(_MainTex, samplePosition + float3(0.5f, 0.5f, 0.5f)); //Density
                        const float density = getDensity(currPos);

                        if (density > maxDensity && density > _MinVal && density < _MaxVal)
                        {
                            maxDensity = density;
                            maxDensityPos = currPos;
                        }
                    }
                        fixed4 sampledColor = float4(1.0f, 1.0f, 1.0f, maxDensity);
                        //color = BlendUnder(color, sampledColor);
                        return sampledColor;
                }
                ENDCG
            }
        }
}