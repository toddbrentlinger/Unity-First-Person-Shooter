Shader "VIS Bump Spec" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)	
	_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
	_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
	_Shininess ("Shininess", Range (0.03, 1)) = 0.078125
}
SubShader
{ 
	Tags { "RenderType"="Opaque" }
	LOD 400
	
	CGPROGRAM
	#pragma surface surf BlinnPhong
	#pragma target 3.0
	
	sampler2D _MainTex;
	sampler2D _BumpMap;
	half _Shininess;
	fixed4 _Color;

	struct Input {
		float2 uv_MainTex;
		float2 uv_BumpMap;
		float3 viewDir;
	};

	void surf (Input IN, inout SurfaceOutput o)
	{
		fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
		o.Albedo = tex.rgb * _Color.rgb;
		o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		o.Gloss = tex.a*5;
		o.Alpha = tex.a;
		o.Specular = _Shininess;
		o.Emission = 0.0;
	}
	ENDCG
}

SubShader
{ 
	Tags { "RenderType"="Opaque" }
	LOD 400
	
	CGPROGRAM
	#pragma surface surf BlinnPhong

	sampler2D _MainTex;
	sampler2D _BumpMap;
	half _Shininess;
	float4 _RimColor;
	float _RimPower;

	struct Input {
		float2 uv_MainTex;
		float2 uv_BumpMap;
		float3 viewDir;
	};

	void surf (Input IN, inout SurfaceOutput o)
	{
		fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
		o.Albedo = tex.rgb;
		o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		o.Gloss = tex.a;
		o.Alpha = tex.a;
		o.Specular = 0;
		o.Emission = 0.0;
	}
	ENDCG
}

FallBack "Bumped Specular"
}
