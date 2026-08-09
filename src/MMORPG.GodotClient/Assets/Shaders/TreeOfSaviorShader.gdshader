shader_type canvas_item;

uniform vec4 outline_color : source_color = vec4(0.12, 0.10, 0.15, 0.45);
uniform float outline_size = 0.8;
uniform float softness = 0.15;

void fragment() {
    vec4 col = texture(TEXTURE, UV);

    if (col.a > 0.05) {
        // Soft Cell-Shading Shading adjustment
        vec3 toon_col = mix(col.rgb * 0.95, col.rgb, step(softness, (col.r + col.g + col.b) / 3.0));
        COLOR = vec4(toon_col, col.a);
    } else {
        // Subtle Outline Detection
        vec2 size = TEXTURE_PIXEL_SIZE * outline_size;
        float alpha = 0.0;
        alpha += texture(TEXTURE, UV + vec2(0.0, -size.y)).a;
        alpha += texture(TEXTURE, UV + vec2(0.0, size.y)).a;
        alpha += texture(TEXTURE, UV + vec2(-size.x, 0.0)).a;
        alpha += texture(TEXTURE, UV + vec2(size.x, 0.0)).a;

        if (alpha > 0.1) {
            COLOR = vec4(outline_color.rgb, outline_color.a * 0.6);
        } else {
            COLOR = vec4(0.0);
        }
    }
}
