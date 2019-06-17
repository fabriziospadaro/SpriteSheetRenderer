 Shader "Instanced/SpriteSheetInstanced" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    SubShader {

        Pass {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Tags {"Queue"="Transparent"}
            CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            StructuredBuffer<float4x2> matrixBuffer;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv: TEXCOORD0;
            };

            v2f vert (appdata_full v, uint instanceID : SV_InstanceID)
            {
                //IMPLEMENTARE ROTAZIONE SI TROVA SULLA W
                float4 transform = float4(matrixBuffer[instanceID][0].x,matrixBuffer[instanceID][1].x,matrixBuffer[instanceID][2].x,matrixBuffer[instanceID][3].x);
                float4 uv = float4(matrixBuffer[instanceID][0].y,matrixBuffer[instanceID][1].y,matrixBuffer[instanceID][2].y,matrixBuffer[instanceID][3].y);
                //rotation in stored inside the z
                float3 worldPosition = float3(transform.x,transform.y,0) + (v.vertex.xyz * transform.w);
                float3 color = v.color;

                v2f o;
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPosition, 1.0f));
                o.uv =  v.texcoord * uv.xy + uv.zw;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 albedo = tex2D(_MainTex, i.uv);
                fixed4 output = fixed4(albedo.rgb, albedo.w);
                return output;
            }

            ENDCG
        }
    }
}