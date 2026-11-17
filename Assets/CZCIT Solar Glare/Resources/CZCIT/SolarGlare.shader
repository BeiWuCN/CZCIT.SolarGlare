// 太阳眩光的绘制层，由运行时组件结合相机状态控制。
Shader "CZCIT/SolarGlare/Optical Glare"
{
    SubShader
    {
        Tags { "Queue"="Overlay-10" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "SolarGlare.hlsl"
            ENDCG
        }
    }
}
