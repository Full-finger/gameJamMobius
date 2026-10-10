extends Node

# C# GameState 的 GDScript 版（Web 导出用）
# autoload 单例：直接用 GameState.XXX 访问

var Level1Collected := false
var Level2Collected := false
var Level3Collected := false

var AllCollected: bool:
	get:
		return Level1Collected and Level2Collected and Level3Collected


func _ready() -> void:
	pass


func Collect(level: int) -> void:
	match level:
		1:
			Level1Collected = true
		2:
			Level2Collected = true
		3:
			Level3Collected = true

	print("收集状态：%s, %s, %s" % [Level1Collected, Level2Collected, Level3Collected])


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
	Level1Collected = false
	Level2Collected = false
	Level3Collected = false
