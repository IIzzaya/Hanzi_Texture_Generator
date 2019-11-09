Shader "Custom/2D/GlowNTwist"
{
    Properties {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
        _Glow_Precision("_Glow_Precision", Range(2, 32)) = 8
        _Glow_Size("_Glow_Size", Range(0, 16)) = 1
        _Glow_Color("_Glow_Color", COLOR) = (1, 1, 0, 1)
        _Glow_Intensity("_Glow_Intensity", Range(0, 4)) = 0
        _Glow_Offset_X("_Glow_Offset_X", Range(-1, 1)) = 0
        _Glow_Offset_Y("_Glow_Offset_Y", Range(-1, 1)) = 0
        
        _Twist_Bend("_Twist_Bend", Range(-1, 1)) = 0
        _Twist_Offset_X("_Twist_Offset_X", Range(-1, 2)) = 0.5
        _Twist_Offset_Y("_Twist_Offset_Y", Range(-1, 2)) = 0.5
        _Twist_Radius("_Twist_Radius", Range(0, 1)) = 0.5
        _Twist_Intensity("_Twist_Intensity", Range(0, 1)) = 1
        
        _Sprite_Alpha("_Sprite_Alpha", Range(0, 1)) = 1.0
        _Alpha("_Alpha", Range(0, 1)) = 1.0
        
        // required for UI.Mask
        [HideInInspector]_StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector]_ColorMask("Color Mask", Float) = 15
    }

    SubShader {
        
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "true" "RenderType" = "Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        ZWrite Off Blend SrcAlpha OneMinusSrcAlpha Cull Off
        
        // required for UI.Mask
        Stencil {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        
        Pass {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
            
            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            
            struct v2f {
                float2 texcoord  : TEXCOORD0;
                float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
            };
            
            sampler2D _MainTex;

            float _Glow_Precision;
            float _Glow_Size;
            float4 _Glow_Color;
            float _Glow_Intensity;
            float _Glow_Offset_X;
            float _Glow_Offset_Y;

            float _Twist_Bend;
            float _Twist_Offset_X;
            float _Twist_Offset_Y;
            float _Twist_Radius;
            float _Twist_Intensity;
            
            float _Sprite_Alpha;
            float _Alpha;
            
            v2f vert(appdata_t IN) {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }
            
            float4 AddGlow(sampler2D source, float2 uv, float precision, float size, float4 color, float intensity, float posx, float posy, float alpha) {
                
                int samples = precision;
                int samples2 = samples * 0.5;
                
                float4 result = float4(0, 0, 0, 0);
                float count = 0;
                for (int iy = -samples2; iy < samples2; iy++) {
                    for (int ix = -samples2; ix < samples2; ix++) {
                        float2 uv2 = float2(ix, iy);
                        uv2 /= samples;
                        uv2 *= size * 0.1;
                        uv2 += float2(-posx, posy);
                        uv2 = saturate(uv + uv2);
                        result += tex2D(source, uv2);
                        
                        count++;
                    }
                }
                
                result.a = lerp(0, result.a / count, intensity);
                result.rgb = color.rgb ;
                
                float4 glow = result;
                float4 origin = tex2D(source, uv);
                
                if (intensity < 0.25) {
                    result = lerp (origin, lerp(glow, origin, origin.a), (intensity) * 4);
                } else {
                    result = lerp(glow, origin, origin.a);
                }
                
                result = lerp(glow, result, alpha);
                
                return result;
            }
            
            float2 TwistUV(float2 uv, float value, float posx, float posy, float radius) {
                float2 center = float2(posx, posy);
                float2 tc = uv - center;
                float dist = length(tc);
                
                if (dist < radius) {
                    float percent = (radius - dist) / radius;
                    float theta = percent * percent * 16.0 * sin(value);
                    float s = sin(theta);
                    float c = cos(theta);
                    tc = float2(dot(tc, float2(c, -s)), dot(tc, float2(s, c)));
                }
                tc += center;
                return tc;
            }
            
            float4 frag (v2f i) : COLOR {
            
                float2 twistedUV = TwistUV(i.texcoord,
                                           _Twist_Bend,
                                           _Twist_Offset_X,
                                           _Twist_Offset_Y,
                                           _Twist_Radius);
                                           
                i.texcoord = lerp(i.texcoord, twistedUV, _Twist_Intensity);
                
                float4 glowResult = AddGlow(_MainTex, 
                                            i.texcoord,
                                            _Glow_Precision,
                                            _Glow_Size,
                                            _Glow_Color,
                                            _Glow_Intensity,
                                            _Glow_Offset_X,
                                            _Glow_Offset_Y,
                                            _Sprite_Alpha);

                float4 result = glowResult;
                result.rgb *= i.color.rgb;
                result.a = result.a * _Alpha * i.color.a;
                return result;
            }
            
            ENDCG
        }
    }
    Fallback "Sprites/Default"
}
