shader_type canvas_item;

uniform vec4 shieldColor: source_color;
uniform vec4 shineColor: source_color;
uniform bool enabled = true;
uniform float flash: hint_range(0, 1) = 0.0;

void vertex() {
    if (enabled) {
        vec2 vert = VERTEX;
        vert.x += 1.0 * sin(TIME + VERTEX.y + VERTEX.x);
        vert.y += 1.0 * sin(TIME + VERTEX.x + VERTEX.y);
        
        VERTEX = vert;
    }
}

void fragment() {
    if (enabled) {
        float shineMix;
        vec4 color = texture(TEXTURE, UV);
        
        if (color.a > 0.001) {
            color = mix(color, shieldColor, 0.5);
            
            shineMix = pow(abs(0.5 * sin(5.0 * (0.3 * TIME + UV.x + UV.y))), 2.0);
            color = mix(color, shineColor, shineMix);
            shineMix = pow(abs(0.5 * sin(5.0 * (0.3 * TIME + UV.x))), 2.0);
            color = mix(color, shineColor, shineMix);
            shineMix = pow(abs(0.5 * sin(5.0 * (0.3 * TIME + UV.y))), 2.0);
            color = mix(color, shineColor, shineMix);
            
            vec4 flashColor = vec4(1.0, 1.0, 1.0, 1.0);
            color = mix(color, flashColor, flash * 0.7);
        }
        
        COLOR = color;
    }
}
