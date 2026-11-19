Shader "FallenShader/Painter"
{   
    Properties
    {
        [HideInInspector]_Radius ("Radius", Float) = 4
        [HideInInspector]_SampleSteps( "Sample Steps", Int ) = 1
    }

    SubShader
    {
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        ENDHLSL

        Tags { "RenderType"="Opaque" }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "Painter"

            HLSLPROGRAM
            
            #pragma vertex Vert
            #pragma fragment Frag

            CBUFFER_START(UnityPerMaterial)
            float _Radius;
            int _SampleSteps;
            CBUFFER_END

            
            inline half Luma(half3 c)
            {
                return dot(c, half3(0.2126h, 0.7152h, 0.0722h));
            }

            inline half3 SampleScene(float2 uv)
            {
                return SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv).rgb;
            }

            float4 Frag(Varyings i) : SV_Target
            {
                float2 uv = i.texcoord;
                float2 texel = _BlitTexture_TexelSize.xy;

                half lumL = Luma(SampleScene(uv - float2(texel.x, 0)));
                half lumR = Luma(SampleScene(uv + float2(texel.x, 0)));
                half lumD = Luma(SampleScene(uv - float2(0, texel.y)));
                half lumU = Luma(SampleScene(uv + float2(0, texel.y)));

                float2 g = float2((float)(lumR - lumL), (float)(lumU - lumD));

                float invLen = rsqrt(max(dot(g, g), 1e-8));
                float2 n = g * invLen;
                float2 t = float2(-n.y, n.x);

                float2 stepX = t * texel.x;
                float2 stepY = n * texel.y;

                int R = (int)round(_Radius);
                float nSamp = ((R / _SampleSteps) + 1) * ((R / _SampleSteps) + 1);

                int2 qOff[4] = { int2(-R,-R), int2(-R,0), int2(0,-R), int2(0,0) };


                half bestVar = 1e9h;
                half3 bestMean = 0;

                [unroll]
                for (int q = 0; q < 4; q++)
                {
                    half3 sumC = 0;
                    float sumL = 0.0;
                    float sumL2 = 0.0;

                    float2 base = (float)qOff[q].x * stepX + (float)qOff[q].y * stepY;

                    for (int y = 0; y <= R; y+=_SampleSteps)
                    {
                        float2 rowBase = base + (float)y * stepY;

                        for (int x = 0; x <= R; x+=_SampleSteps)
                        {
                            float2 uvp = uv + rowBase + (float)x * stepX;

                            half3 ccol = SampleScene(uvp);
                            sumC += ccol;

                            float l = (float)Luma(ccol);
                            sumL  += l;
                            sumL2 += l * l;
                        }
                    }

                    half3 meanC = sumC / (half)nSamp;

                    float meanL = sumL / nSamp;
                    float varL  = max(0.0, sumL2 / nSamp - meanL * meanL);

                    if ((half)varL < bestVar)
                    {
                        bestVar = (half)varL;
                        bestMean = meanC;
                    }
                }

                return float4(saturate(bestMean), 1.0);
            }

            ENDHLSL
            
        }
    }
}
