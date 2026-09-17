// FairyGUI 扫光效果 Shader（对应 Unity 侧 FairyGUI-ImageSweepLight.shader）
// 基于 canvas_item 默认绘制扩展扫光效果：按角度生成扫光带，周期由 light_time + next_time 决定。
// 注意：Godot 移植的 NImage 网格直接使用图集子区域 UV（非 0-1），
// 因此通过 uv_offset/uv_scale 把采样 UV 归一化到 0-1 后再做扫光运算。
shader_type canvas_item;

uniform float light_time = 0.6;      // 扫光时间（秒）
uniform float light_thick = 0.3;     // 扫光带厚度（0-1）
uniform float next_time = 2.0;       // 两次扫光间隔（秒）
uniform float light_angle = 45.0;    // 扫光角度（0-360）
uniform float light_intensity = 1.4; // 扫光强度（0-3）
uniform bool sweep_light_enabled = true;
uniform vec2 uv_offset = vec2(0.0, 0.0); // 精灵在图集内的 UV 原点
uniform vec2 uv_scale = vec2(1.0, 1.0);  // 精灵在图集内的 UV 尺寸

void fragment() {
    vec4 col = texture(TEXTURE, UV) * COLOR;

    if (sweep_light_enabled && uv_scale.x > 0.0 && uv_scale.y > 0.0) {
        // 归一化到精灵本地 0-1 坐标
        vec2 suv = (UV - uv_offset) / uv_scale;
        float cycle = light_time + next_time;
        float t = mod(TIME, cycle);
        if (t <= light_time) {
            // 扫光进度 (0-1)
            float progress = t / light_time;
            float rad = mod(light_angle, 360.0) * 0.01745329251;
            vec2 light_dir = vec2(cos(rad), sin(rad));
            // 以中心为原点的坐标（-0.5 ~ 0.5）
            vec2 centered_uv = suv - vec2(0.5);
            // 对角线长度的一半作为扫光范围，保证覆盖整张纹理
            float diagonal_length = sqrt(2.0) * 0.5;
            float sweep_distance = (progress * 2.0 - 1.0) * diagonal_length;
            float pixel_distance = dot(centered_uv, light_dir) - sweep_distance;
            float half_thick = light_thick * 0.5;

            if (abs(pixel_distance) < half_thick) {
                // 中心最亮、边缘渐暗
                float normalized_distance = abs(pixel_distance) / half_thick;
                float light_strength = smoothstep(0.0, 1.0, 1.0 - normalized_distance);
                col.rgb = clamp(col.rgb + col.rgb * (light_intensity * light_strength), vec3(0.0), vec3(1.0));
            }
        }
    }

    COLOR = col;
}
