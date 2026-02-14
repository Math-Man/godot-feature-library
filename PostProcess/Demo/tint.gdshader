shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform float intensity : hint_range(0.0, 1.0) = 0.0;
uniform vec4 tint_color : source_color = vec4(1.0, 0.0, 0.0, 1.0);

void fragment() {
    vec4 screen = texture(screen_texture, SCREEN_UV);
    COLOR = mix(screen, vec4(tint_color.rgb, 1.0), intensity);
}
