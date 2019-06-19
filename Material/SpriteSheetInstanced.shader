 Shader "Instanced/SpriteSheetInstanced" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
    }
    
    SubShader {
        Tags{
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }
        Cull Back
        Lighting Off
        ZWrite On
        Blend One OneMinusSrcAlpha
        Pass {
            CGPROGRAM
            // Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
            #pragma exclude_renderers gles

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"

            sampler2D _MainTex;

            StructuredBuffer<float4x2> matrixBuffer;

            struct v2f{
                float4 pos : SV_POSITION;
                float2 uv: TEXCOORD0;
            };

            float4x4 rotationZMatrix(float rZ){
                float angleZ = radians(rZ);
                float c = cos(angleZ);
                float s = sin(angleZ);
                float4x4 ZMatrix  = 
                    float4x4( 
                       c,  -s, 0,  0,
                       s,  c,  0,  0,
                       0,  0,  1,  0,
                       0,  0,  0,  1);
                return ZMatrix;
            }

            v2f vert (appdata_full v, uint instanceID : SV_InstanceID){
                //transform.xy = posizion x and y
                //transform.z = rotation angle
                //transform.w = scale
                float4 transform = float4(matrixBuffer[instanceID][0].x,matrixBuffer[instanceID][1].x,matrixBuffer[instanceID][2].x,matrixBuffer[instanceID][3].x);
                float4 uv = float4(matrixBuffer[instanceID][0].y,matrixBuffer[instanceID][1].y,matrixBuffer[instanceID][2].y,matrixBuffer[instanceID][3].y);
                //rotate the vertex
                v.vertex = mul(v.vertex-float4(0.5,0.5,0,0),rotationZMatrix(transform.z));
                //scale it
                float3 worldPosition = float3(transform.x,transform.y,-transform.y/10) + (v.vertex.xyz * transform.w);
                v2f o;
                o.pos = UnityObjectToClipPos(float4(worldPosition, 1.0f));
                o.uv =  v.texcoord * uv.xy + uv.zw;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target{
                fixed4 albedo = tex2D(_MainTex, i.uv);
                clip(albedo.a - 1.0 / 255.0);
                albedo.rgb *= albedo.a;
                return albedo;
            }

            ENDCG
        }
    }
}