shader_type canvas_item;

uniform float progress : hint_range(0.0, 1.1, 0.01) = 0.0;
uniform sampler2D noise_texture;

uniform vec4 edge_color : source_color = vec4(1.0, 0.5, 0.0, 1.0);
uniform float edge_width : hint_range(0.0, 0.1) = 0.05;

void fragment() {
	vec4 current_color = texture(TEXTURE, UV);

	// 原图完全透明的地方不参与溶解。
	if (current_color.a <= 0.0) {
		discard;
	}

	float noise_value = texture(noise_texture, UV).r;

	if (noise_value < progress - edge_width) {
		discard;
	}
	else if (noise_value < progress) {
		// 边缘染色，但保留原图透明度，包括半透明边缘。
		COLOR = vec4(
			edge_color.rgb,
			current_color.a * edge_color.a
		);
	}
	else {
		COLOR = current_color;
	}
}