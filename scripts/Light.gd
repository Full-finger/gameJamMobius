extends Area2D

# C# Light 的 GDScript 版（Web 导出用）

# Compatibility 渲染器（Web 平台）无 2D 泛光（glow），超亮光照直接叠加成死白。
# 实测光心：Forward+ e=4.43 时 RGB≈(0.85,0.78,0.29) 亮而有层次；
# Compatibility 同参数时 ≈(0.75,0.94,0.52) 过曝发白；e=2.0 时 ≈(0.42,0.52,0.29) 清晰可见。
# 因此只把过亮的光源压到上限，正常亮度的光源（如冬关 e=1.0）不动。
# 桌面（Forward+）不受影响。
const COMPAT_ENERGY_MAX := 2.0

@onready var pointLight: PointLight2D = get_parent() as PointLight2D


func _ready() -> void:
	if RenderingServer.get_current_rendering_method() == "gl_compatibility":
		pointLight.energy = minf(pointLight.energy, COMPAT_ENERGY_MAX)

	body_entered.connect(OnBodyEntered)
	body_exited.connect(OnBodyExited)


func OnBodyEntered(body: Node2D) -> void:
	if body is Player:
		(body as Player).isInLight = true


func OnBodyExited(body: Node2D) -> void:
	if body is Player:
		(body as Player).isInLight = false
