// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "PDT Shaders/TestGrid" {
	Properties {
		_LineColor ("Line Color", Color) = (1,1,1,1)
		_CellColor ("Cell Color", Color) = (0,0,0,0)
		_SelectedColor ("Selected Color", Color) = (1,0,0,1)
		[PerRendererData] _MainTex ("Albedo (RGB)", 2D) = "white" {}
		[IntRange] _GridSizeX("Grid Size X", Range(1,100)) = 10
		[IntRange] _GridSizeY("Grid Size Y", Range(1,100)) = 10
		_LineSize("Line Size", Range(0,1)) = 0.15
		[IntRange] _SelectCell("Select Cell Toggle ( 0 = False , 1 = True )", Range(0,1)) = 0.0
		[IntRange] _SelectedCellX("Selected Cell X", Range(0,100)) = 0.0
		[IntRange] _SelectedCellY("Selected Cell Y", Range(0,100)) = 0.0
	}
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 200
	

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard alpha:blend

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness = 0.0;
		half _Metallic = 0.0;
		float4 _LineColor;
		float4 _CellColor;
		float4 _SelectedColor;

		float _GridSizeX;
		float _GridSizeY;
		float _LineSize;

		float _SelectCell;
		float _SelectedCellX;
		float _SelectedCellY;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color

			float2 uv = IN.uv_MainTex;

			_SelectedCellX = floor(_SelectedCellX);
			_SelectedCellY = floor(_SelectedCellY);

			fixed4 c = float4(0.0,0.0,0.0,0.0);

			float alph = 1.;

			float gsizeX = floor(_GridSizeX);
			float gsizeY = floor(_GridSizeY);



			gsizeX += _LineSize;
			gsizeY += _LineSize;

			float2 id;

			id.x = floor(uv.x/(1.0/gsizeX));
			id.y = floor(uv.y/(1.0/gsizeY));

			float4 color = _CellColor;
			alph = _CellColor.w;

			//This checks that the cell is currently selected if the Select Cell slider is set to 1 ( True )
			if (round(_SelectCell) == 1.0 && id.x == _SelectedCellX && id.y == _SelectedCellY)
			{
				alph = _SelectedColor.w;
				color = _SelectedColor;
			}

			if (frac(uv.x*gsizeX) <= _LineSize || frac(uv.y*gsizeY) <= _LineSize)
			{
				alph = _LineColor.w;
				color = _LineColor;
			}
			

			//Clip transparent spots using alpha cutout
			if (alph == 0.0) {
				clip(c.a - 1.0);
			}
			

			//return half4(color.x * alph, color.y * alph, color.z * alph, alph);

			o.Albedo = float3( color.x,color.y,color.z);
			// Metallic and smoothness come from slider variables
			o.Metallic = 0.0;
			o.Smoothness = 0.0;
			o.Alpha = alph;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
