float4 _SunDirection, _GlareRight, _GlareUp;
float _Intensity;
struct A
{
    float4 vertex : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct V
{
    float4 position : SV_POSITION;
    float3 world : TEXCOORD0;
    UNITY_VERTEX_OUTPUT_STEREO
};
V vert(A v)
{
    V o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_OUTPUT(V, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.position = UnityObjectToClipPos(v.vertex);
    o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
    return o;
}
float4 frag(V i) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    float3 ray = normalize(i.world - _WorldSpaceCameraPos);
    float3 sun = normalize(_SunDirection.xyz);
    float facing = dot(ray, sun);
    float radial = length(cross(ray, sun));
    float2 p = float2(dot(ray, _GlareRight.xyz), dot(ray, _GlareUp.xyz));
    float2 center = float2(dot(sun, _GlareRight.xyz), dot(sun, _GlareUp.xyz));
    float2 delta = p - center;
    float theta = atan2(delta.y, delta.x);
    // Unequal, fixed angular lobes: no regular lens-aperture polygon or animation.
    float pattern =
        max(0, .52 * sin(theta * 11 + .9) + .30 * sin(theta * 19 + 1.7) + .18 * sin(theta * 31 + .4));
    float angularAA = max(fwidth(theta), .001);
    float rays = pow(pattern, 3) / (1 + angularAA * 20);
    float reach = .035 + .025 * (.5 + .5 * sin(theta * 7 + 1.3));
    float streaks = .85 * rays * exp(-radial / max(reach, .01)) * smoothstep(.003, .009, radial);
    float core = .72 * exp(-radial * radial / .00028);
    float halo = .10 * exp(-radial * radial / .006);
    // White optical veil lowers local contrast over sky AND scene geometry.
    float veil = .12 * exp(-radial * radial / .035);
    float fade = (1 - smoothstep(.45, .65, radial)) * step(0, facing);
    float alpha = saturate((core + halo + veil + streaks) * _Intensity * fade);
    // Dither the LDR blend to avoid visible rings on a nearly black sky.
    float noise = frac(52.9829189 * frac(dot(floor(i.position.xy), float2(.06711056, .00583715))));
    alpha = max(0, alpha + (noise - .5) / 255 * smoothstep(0, .003, alpha));
    return float4(1, .985, .96, alpha);
}
