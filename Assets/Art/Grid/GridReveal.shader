// Grid lines that only show near the mouse. Used by GridOverlay on a sprite stretched over the board.
// Each tile's first column and row of pixels is a line; the lines are clearest at the mouse and fade
// to nothing at the radius.
Shader "Elemental/GridReveal"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Line Colour", Color) = (1, 0.93, 0.78, 1)
        _Strength ("Strength At The Mouse", Range(0, 1)) = 0.8
        _Radius ("Radius (tiles)", Float) = 2.6
        _Falloff ("Falloff", Float) = 1.25
        _PixelsPerTile ("Pixels Per Tile", Float) = 50
        _CellSize ("Tile Size (set by GridOverlay)", Float) = 1
        _Origin ("Board Corner (set by GridOverlay)", Vector) = (0, 0, 0, 0)
        _Mouse ("Mouse (set by GridOverlay)", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float _Strength;
                float _Radius;
                float _Falloff;
                float _PixelsPerTile;
                float _CellSize;
                float4 _Origin;
                float4 _Mouse;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Which pixel of its tile is this?
                float2 tiles = (input.positionWS.xy - _Origin.xy) / _CellSize;
                float2 pixel = floor(frac(tiles) * _PixelsPerTile);

                // Lines sit on each tile's first column and first row of pixels.
                if (pixel.x != 0 && pixel.y != 0) discard;

                // Clearest at the mouse, gone by the radius.
                float distance01 = distance(input.positionWS.xy, _Mouse.xy) / (_Radius * _CellSize);
                float alpha = _Strength * pow(saturate(1 - distance01), _Falloff);
                return half4(_Color.rgb, _Color.a * alpha);
            }
            ENDHLSL
        }
    }
}