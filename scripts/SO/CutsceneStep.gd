class_name CutsceneStep
extends Resource

# C# SO/CutsceneStep 的 GDScript 版（Web 导出用）

# 这一句话要显示的 CG
# 如果不填，就继续保持上一张 CG
@export var cg: Texture2D

# 这一句话
@export_multiline var text := ""
