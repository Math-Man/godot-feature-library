shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform float intensity : hint_range(0.0, 1.0) = 0.0;
uniform float radius : hint_range(0.0, 1.0) = 0.45;
uniform float softness : hint_range(0.0, 1.0) = 0.4;

void fragment() {
    vec4 screen = texture(screen_texture, SCREEN_UV);
    vec2 uv = SCREEN_UV - 0.5;
    float dist = length(uv);
    float vignette = smoothstep(radius, radius - softness, dist);
    COLOR = mix(vec4(0.0, 0.0, 0.0, 1.0), screen, mix(1.0, vignette, intensity));
}
