using Godot;

[GlobalClass]
public partial class CutsceneStep : Resource
{
    // 这一句话要显示的 CG
    // 如果不填，就继续保持上一张 CG
    [Export]
    public Texture2D cg;

    // 这一句话
    [Export(PropertyHint.MultilineText)]
    public string text = "";
}