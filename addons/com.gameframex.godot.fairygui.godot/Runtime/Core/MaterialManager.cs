using Godot;
using System.Collections.Generic;

public enum MaterialType
{
    StandardMaterial,
}

public class MaterialManager
{
    static MaterialManager _inst;

    public static MaterialManager inst
    {
        get
        {
            if (_inst == null)
                _inst = new MaterialManager();
            return _inst;
        }
    }

    Dictionary<string, Material> _matDic = new Dictionary<string, Material>();

    public CanvasItemMaterial GetStandardMaterial(CanvasItemMaterial.BlendModeEnum blendMode)
    {
        string key = $"{blendMode}";
        Material mat;
        if (_matDic.TryGetValue(key, out mat))
        {
            return mat as CanvasItemMaterial;
        }
        CanvasItemMaterial newMat = new CanvasItemMaterial();
        newMat.BlendMode = blendMode;
        _matDic.Add(key, newMat);
        return newMat;
    }

}