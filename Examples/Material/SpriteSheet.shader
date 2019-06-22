 Shader "Instanced/SpriteSheet" {
    Properties {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_PivotOffset ("Pivot Offset", Float ) = 0.5
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
			float _PivotOffset;

            StructuredBuffer<float4x2> transformBuffer;
			StructuredBuffer<float4> colorsBuffer;
			StructuredBuffer<float2> uvBuffer;
			StructuredBuffer<int> uvCellsBuffer;


			struct appdata_t {
				float4 vertex : POSITION;
			};

            struct v2f{
                float4 pos : SV_POSITION;
                float2 uv: TEXCOORD0;
				fixed4 color : COLOR0;
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

            v2f vert (appdata_t i, uint instanceID : SV_InstanceID, uint vertexID : SV_VertexID){

                //transform.xy = posizion x and y
                //transform.z = rotation angle
                //transform.w = scale
  
				float4x2 t = transformBuffer[instanceID];
				float4 transform = float4(t[0].x, t[1].x, t[2].x, t[3].x);
				float4 vert = i.vertex;

				float pivotOffset = _PivotOffset;
				vert.xy -= pivotOffset;
                vert = mul(vert,rotationZMatrix(transform.z));
				vert.xy += pivotOffset;

                //scale it	
				vert.xy = vert.xy * transform.w;
				vert.z = 1;
				
				v2f o;
				o.pos = UnityObjectToClipPos(float4(vert.xyz + transform.xyz, 1.0f));
				o.uv = uvBuffer[uvCellsBuffer[instanceID] * 4 + vertexID];
				o.color = colorsBuffer[instanceID];
                return o;
            }

            fixed4 frag (v2f i) : SV_Target{
				fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                col.rgb *= col.a;

				clip(col.a - 1.0 / 255.0);
				return col;
            }

            ENDCG
        }
    }
}