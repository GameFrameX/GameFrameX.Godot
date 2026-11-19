using Godot;

// C# Web 导出微信可行性 spike：全部逻辑在 C#，无 GDScript。
// 成功标准：微信小游戏里出现 Godot 徽章场景 + 状态标签持续被 C# _Process 驱动刷新。
public partial class Main : Node3D
{
    private int _frameCount;
    private Label _statusLabel = null!;

    public override void _Ready()
    {
        var environment = new Environment();
        var sky = new Sky();
        sky.SkyMaterial = new ProceduralSkyMaterial();
        environment.BackgroundMode = Environment.BGMode.Sky;
        environment.Sky = sky;
        environment.AmbientLightSource = Environment.AmbientSource.Sky;
        environment.AmbientLightEnergy = 1.1f;
        var worldEnvironment = new WorldEnvironment();
        worldEnvironment.Environment = environment;
        AddChild(worldEnvironment);

        var camera = new Camera3D();
        camera.Position = new Vector3(0f, 1.2f, 4f);
        AddChild(camera);
        camera.Current = true;

        var light = new DirectionalLight3D();
        light.RotationDegrees = new Vector3(-35f, 25f, 0f);
        AddChild(light);

        var icon = new MeshInstance3D();
        var box = new BoxMesh();
        box.Size = new Vector3(1f, 1f, 1f);
        var material = new StandardMaterial3D();
        material.AlbedoColor = new Color(0.35f, 0.6f, 0.95f);
        icon.Mesh = box;
        icon.MaterialOverride = material;
        icon.Position = new Vector3(0f, 0.8f, 0f);
        AddChild(icon);
        _icon = icon;

        var interfaceLayer = new CanvasLayer();
        AddChild(interfaceLayer);
        _statusLabel = new Label();
        _statusLabel.Name = "status";
        _statusLabel.Position = new Vector2(24f, 24f);
        _statusLabel.AddThemeFontSizeOverride("font_size", 28);
        interfaceLayer.AddChild(_statusLabel);
    }

    private MeshInstance3D _icon = null!;

    public override void _Process(double delta)
    {
        _frameCount++;
        _icon.RotateY((float)delta * 1.2f);
        if (_frameCount % 30 == 0)
        {
            _statusLabel.Text = $"C# ALIVE frames={_frameCount} fps={Engine.GetFramesPerSecond()}";
            GD.Print($"CSHARP-SPIKE: alive frames={_frameCount}");
        }
    }
}
