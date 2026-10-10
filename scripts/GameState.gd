extends Node

# C# GameState 的 GDScript 版（Web 导出用）
# autoload 单例：直接用 GameState.XXX 访问

var Level1Collected := false
var Level2Collected := false
var Level3Collected := false

# 只暂存当前这次挑战捡到的道具；过关前不计入结局。
var pendingLevel := 0
var pendingScene: Node

var AllCollected: bool:
	get:
		return Level1Collected and Level2Collected and Level3Collected


func _ready() -> void:
	pass


func Collect(level: int) -> void:
	var scene := get_tree().current_scene

	if pendingScene != scene:
		DiscardPendingCollection()
		pendingScene = scene
		# 死亡重开、手动重开、返回菜单，都会离开当前场景。
		scene.tree_exiting.connect(DiscardPendingCollection)

	pendingLevel = level


# 只有成功到达本关终点才调用。
func ConfirmLevelCollection() -> void:
	match pendingLevel:
		1:
			Level1Collected = true
		2:
			Level2Collected = true
		3:
			Level3Collected = true

	pendingLevel = 0

	print("收集状态：%s, %s, %s" % [Level1Collected, Level2Collected, Level3Collected])


func DiscardPendingCollection() -> void:
	pendingLevel = 0
	pendingScene = null


func IsCollected(level: int) -> bool:
	match level:
		1:
			return Level1Collected
		2:
			return Level2Collected
		3:
			return Level3Collected
	return false


# 只有开始一局全新游戏时调用
func ResetAll() -> void:
	DiscardPendingCollection()
	Level1Collected = false
	Level2Collected = false
	Level3Collected = false
