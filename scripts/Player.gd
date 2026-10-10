class_name Player
extends CharacterBody2D

# C# Player 的 GDScript 版（Web 导出用）
# export 变量名、方法名与 C# 版保持一致，场景数据无缝兼容
# 注意：Godot 内置属性 velocity 在 GDScript 里是小写

@export var speed := 300.0
@export var jumpHeight := 100.0


# =========================================================
# 跳跃容错
# =========================================================

@export_group("跳跃容错")
@export var coyoteTime := 0.1
@export var jumpBufferTime := 0.1
@export var cornerCorrection := 6


# =========================================================
# 左右移动
# =========================================================

@export_group("左右移动手感")
@export var acceleration := 2400.0
@export var deceleration := 3200.0
@export var turnAcceleration := 4800.0
@export var airAcceleration := 1800.0
@export var airDeceleration := 900.0


# =========================================================
# 跳跃
# =========================================================

@export_group("跳跃手感")
@export var riseGravityScale := 1.4
@export var fallGravityScale := 2.6
@export var apexGravityScale := 0.45
@export var apexSpeed := 60.0
@export var jumpCutMultiplier := 0.45
@export var maxFallSpeed := 900.0


# =========================================================
# 藤蔓
# =========================================================

@export_group("藤蔓手感")
@export var vineSwingMultiplier := 5.0
@export var vineGrabImpulse := 1.25
@export var vineJumpMultiplier := 5.0


# =========================================================
# 边界
# =========================================================

@export_group("边界")
@export var deathY := INF


# =========================================================
# 落地回弹
# =========================================================

@export_group("落地回弹")

@export_range(0.0, 0.15, 0.01)
var landingSquash := 0.05

@export_range(0.01, 0.2, 0.01)
var landingSquashTime := 0.06

@export_range(0.01, 0.3, 0.01)
var landingRecoverTime := 0.10


# =========================================================
# 音效
# 你只需要把对应音频拖进这五个槽
# =========================================================

@export_group("音效")

@export var footstepSound: AudioStream
@export var landingSound: AudioStream
@export var leaveLightSound: AudioStream
@export var enterLightSound: AudioStream
@export var clockSound: AudioStream


# =========================================================
# 内部变量
# =========================================================

var animNormalScale: Vector2
var landingTween: Tween

var isInLight := false

var normalSpeed: float
var normalJumpHeight: float

var coyoteCounter := 0.0
var jumpBufferCounter := 0.0

var jumping := false
var jumpCut := false

var anim: AnimatedSprite2D

var currentAction := ""


# =========================================================
# 音频播放器
# =========================================================

var footstepAudio: AudioStreamPlayer2D
var landingAudio: AudioStreamPlayer2D
var leaveLightAudio: AudioStreamPlayer2D
var enterLightAudio: AudioStreamPlayer2D
var clockAudio: AudioStreamPlayer2D

var lightStateInitialized := false
var lastLightState := false

var isDying := false


# =========================================================
# 藤蔓
# =========================================================

var vine: RigidBody2D

var grabCooldown := 0.0
var vineFlightTime := 0.0

var canJumpFromVine := false

var vineGrabPoint: Vector2
var grabCenterOffset: Vector2


# =========================================================
# HUD生命比例
# =========================================================

var LifeRatio: float:
	get:
		if normalSpeed <= 0.0 or normalJumpHeight <= 0.0:
			return 0.0

		var speedRatio := speed / normalSpeed
		var jumpRatio := jumpHeight / normalJumpHeight

		return clampf(minf(speedRatio, jumpRatio), 0.0, 1.0)


# =========================================================
# 藤蔓抓取
# =========================================================

func GrabVine(target: RigidBody2D) -> void:
	if not is_physics_processing() or vine != null or grabCooldown > 0.0:
		return

	# 抓藤蔓后停止脚步声
	StopSound(footstepAudio)

	vine = target

	grabCenterOffset = (
		($CollisionShape2D as CollisionShape2D).global_position
		- global_position
	)

	vineGrabPoint = vine.to_local(global_position + grabCenterOffset)

	var halfLength: float = (
		(vine.get_node("CollisionShape2D") as CollisionShape2D).shape
		as RectangleShape2D
	).size.y * 0.5

	vineGrabPoint.x = 0.0
	vineGrabPoint.y = clampf(vineGrabPoint.y, -halfLength, halfLength)

	vine.apply_central_impulse(Vector2(velocity.x, 0.0) * vineGrabImpulse)

	velocity = Vector2.ZERO

	vineFlightTime = 0.0

	canJumpFromVine = not Input.is_action_pressed("move_jump")

	coyoteCounter = 0.0
	jumpBufferCounter = 0.0

	jumping = false
	jumpCut = true

	PlayAnimation("idle")


# =========================================================
# 藤蔓更新
# =========================================================

func UpdateVine(dt: float) -> bool:
	grabCooldown = maxf(0.0, grabCooldown - dt)
	vineFlightTime = maxf(0.0, vineFlightTime - dt)

	if not is_instance_valid(vine):
		vine = null
		return false

	# 抓藤蔓期间不能有脚步声
	StopSound(footstepAudio)

	var direction := 0.0

	if Input.is_action_pressed("move_left"):
		direction -= 1.0

	if Input.is_action_pressed("move_right"):
		direction += 1.0

	if (
		absf(vine.linear_velocity.x) < 650.0 * vineSwingMultiplier
		or direction * vine.linear_velocity.x < 0.0
	):
		vine.apply_central_force(
			Vector2(direction * 900.0 * vineSwingMultiplier, 0.0)
		)

	if direction != 0.0:
		anim.flip_h = direction > 0.0

	if not Input.is_action_pressed("move_jump"):
		canJumpFromVine = true

	if canJumpFromVine and Input.is_action_just_pressed("move_jump"):
		var radius: Vector2 = vine.to_global(vineGrabPoint) - vine.global_position

		var pointVelocity: Vector2 = (
			vine.linear_velocity
			+ Vector2(-radius.y, radius.x) * vine.angular_velocity
		)

		var launch: Vector2 = pointVelocity.limit_length(normalSpeed) * 0.4

		launch.x = clampf(
			launch.x + direction * speed * 0.5,
			-normalSpeed,
			normalSpeed
		)

		var upwardCarry := minf(launch.y, 0.0)

		launch = Jump(launch, get_gravity().y)

		launch.y += upwardCarry
		launch *= vineJumpMultiplier

		vine = null

		grabCooldown = 0.85
		vineFlightTime = 0.2

		velocity = launch

		move_and_slide()

		return true

	PlayAnimation("idle")

	var targetPosition: Vector2 = vine.to_global(vineGrabPoint) - grabCenterOffset

	velocity = (
		(targetPosition - global_position) / dt
	).limit_length(normalSpeed * 1.5 * vineSwingMultiplier)

	move_and_slide()

	if global_position.distance_to(targetPosition) > normalSpeed * 0.25:
		vine = null

		grabCooldown = 0.45

		velocity = Vector2.ZERO

	return true


# =========================================================
# Ready
# =========================================================

func _ready() -> void:
	normalSpeed = speed
	normalJumpHeight = jumpHeight

	floor_max_angle = deg_to_rad(55.0)
	floor_snap_length = 24.0
	floor_constant_speed = true

	anim = $AnimatedSprite2D as AnimatedSprite2D

	animNormalScale = anim.scale

	# =====================================================
	# 自动创建音频播放器
	# =====================================================

	footstepAudio = CreateAudioPlayer(footstepSound)
	landingAudio = CreateAudioPlayer(landingSound)
	leaveLightAudio = CreateAudioPlayer(leaveLightSound)
	enterLightAudio = CreateAudioPlayer(enterLightSound)
	clockAudio = CreateAudioPlayer(clockSound)

	# =====================================================
	# 玩家材质
	# =====================================================

	anim.material = anim.material.duplicate() as ShaderMaterial

	(anim.material as ShaderMaterial).set_shader_parameter("progress", 0.0)

	anim.sprite_frames.set_animation_loop_mode("jump", SpriteFrames.LOOP_NONE)
	anim.sprite_frames.set_animation_loop_mode("jumpOLD", SpriteFrames.LOOP_NONE)

	PlayAnimation("idle")


func _unhandled_input(event: InputEvent) -> void:
	if isDying or get_tree().paused or not event is InputEventKey:
		return

	var keyEvent := event as InputEventKey

	if not keyEvent.pressed or keyEvent.echo:
		return

	if keyEvent.keycode != KEY_R and keyEvent.physical_keycode != KEY_R:
		return

	get_viewport().set_input_as_handled()
	TransitionManager.ReloadCurrentScene()


# =========================================================
# Physics
# =========================================================

func _physics_process(delta: float) -> void:
	var dt := delta

	# =====================================================
	# 光照状态音效
	# =====================================================

	UpdateLightAudioState()

	# =====================================================
	# 光照机制
	# =====================================================

	if isInLight:
		speed = move_toward(speed, normalSpeed, normalSpeed * 2.0 * dt)
		jumpHeight = move_toward(jumpHeight, normalJumpHeight, normalJumpHeight * 2.0 * dt)
	else:
		speed = move_toward(speed, 0.0, normalSpeed * 0.1 * dt)
		jumpHeight = move_toward(jumpHeight, 0.0, normalJumpHeight * 0.1 * dt)

	# =====================================================
	# 迟暮死亡
	# =====================================================

	if speed == 0.0 and jumpHeight == 0.0:
		Die()
		return

	# =====================================================
	# 藤蔓
	# =====================================================

	if UpdateVine(dt):
		return

	var vel := velocity

	var gravity: float = get_gravity().y

	var grounded := is_on_floor() and vel.y >= 0.0

	var jumpHeld := Input.is_action_pressed("move_jump")

	coyoteCounter = maxf(0.0, coyoteCounter - dt)
	jumpBufferCounter = maxf(0.0, jumpBufferCounter - dt)

	if grounded:
		coyoteCounter = coyoteTime
		jumping = false
		jumpCut = false

	if Input.is_action_just_pressed("move_jump"):
		jumpBufferCounter = jumpBufferTime

	# =====================================================
	# 左右输入
	# =====================================================

	var direction := 0.0

	if Input.is_action_pressed("move_left"):
		direction -= 1.0

	if Input.is_action_pressed("move_right"):
		direction += 1.0

	var moveAcceleration := acceleration if grounded else airAcceleration

	if direction == 0.0:
		moveAcceleration = deceleration if grounded else airDeceleration
	elif vel.x * direction < 0.0:
		moveAcceleration = turnAcceleration if grounded else airAcceleration * 2.0

	var nearApex: bool = (
		jumping and not jumpCut and jumpHeld and absf(vel.y) < apexSpeed
	)

	if not grounded and nearApex and direction != 0.0:
		moveAcceleration *= 1.2

	if vineFlightTime <= 0.0 or grounded:
		vel.x = move_toward(vel.x, direction * speed, moveAcceleration * dt)

	if direction != 0.0:
		anim.flip_h = direction > 0.0

	# =====================================================
	# 跳跃
	# =====================================================

	var startedJump := false

	if (grounded or coyoteCounter > 0.0) and jumpBufferCounter > 0.0 and jumpHeight > 0.0:
		vel = Jump(vel, gravity)
		startedJump = true

	if jumping and not jumpCut and not jumpHeld and vel.y < 0.0:
		vel.y *= jumpCutMultiplier
		jumpCut = true

	if not grounded and not startedJump:
		var gravityScale: float = riseGravityScale if vel.y < 0.0 else fallGravityScale

		nearApex = (
			jumping and not jumpCut and jumpHeld and absf(vel.y) < apexSpeed
		)

		if nearApex:
			gravityScale = apexGravityScale

		vel.y = minf(vel.y + gravity * gravityScale * dt, maxFallSpeed)

	# =====================================================
	# 掉出地图
	# =====================================================

	if global_position.y > deathY:
		Die()
		return

	# =====================================================
	# 顶角修正
	# =====================================================

	if vel.y < 0.0:
		CorrectCeilingCorner(vel.y * dt, direction)

	# =====================================================
	# 移动
	# =====================================================

	var wasOnFloor := is_on_floor()

	velocity = vel

	move_and_slide()

	# =====================================================
	# 落地
	# =====================================================

	if not wasOnFloor and is_on_floor():
		PlayLandingSquash()

		# ★ 落地音效
		PlaySound(landingAudio)

	if is_on_ceiling():
		jumping = false
		jumpCut = true

	# =====================================================
	# Buffer Jump
	# =====================================================

	var bufferedJump := false

	if is_on_floor() and not startedJump and jumpBufferCounter > 0.0 and jumpHeight > 0.0:
		vel = velocity

		vel = Jump(vel, gravity)

		velocity = vel

		bufferedJump = true

	# =====================================================
	# 动画
	# =====================================================

	var nextAnimation: String

	if not is_on_floor() or bufferedJump:
		nextAnimation = "jump"
	elif absf(velocity.x) > 1.0:
		nextAnimation = "walk"
	else:
		nextAnimation = "idle"

	# =====================================================
	# 脚步声
	# =====================================================

	var isWalking: bool = (
		is_on_floor() and not bufferedJump and absf(velocity.x) > 1.0
	)

	if isWalking:
		if footstepAudio.stream != null and not footstepAudio.playing:
			footstepAudio.play()
	else:
		StopSound(footstepAudio)

	PlayAnimation(nextAnimation)


# =========================================================
# 光照音频
# =========================================================

func UpdateLightAudioState() -> void:
	# 第一次运行：
	# 只同步状态，不播“进入/离开”的提示音
	if not lightStateInitialized:
		lightStateInitialized = true

		lastLightState = isInLight

		# 出生时如果就在光外，直接开始时钟声
		if not isInLight:
			PlaySound(clockAudio)

		return

	# =====================================================
	# 光照状态发生变化
	# =====================================================

	if isInLight != lastLightState:
		# -------------------------
		# 进入光
		# -------------------------

		if isInLight:
			StopSound(clockAudio)
			PlaySound(enterLightAudio)
		# -------------------------
		# 离开光
		# -------------------------

		else:
			PlaySound(leaveLightAudio)
			PlaySound(clockAudio)

		lastLightState = isInLight

	# =====================================================
	# 保证时钟声在光外持续循环
	# =====================================================

	if not isInLight and not isDying:
		if clockAudio.stream != null and not clockAudio.playing:
			clockAudio.play()
	else:
		StopSound(clockAudio)


# =========================================================
# 创建声音播放器
# =========================================================

func CreateAudioPlayer(stream: AudioStream) -> AudioStreamPlayer2D:
	var player := AudioStreamPlayer2D.new()

	player.stream = stream

	# 你的总音量 Slider 控制的就是 Master
	player.bus = "Master"

	add_child(player)

	return player


# =========================================================
# 播放一次声音
# =========================================================

func PlaySound(player: AudioStreamPlayer2D) -> void:
	if player == null or player.stream == null:
		return

	# 如果同一音效再次触发，直接从头播放
	player.play()


# =========================================================
# 停止声音
# =========================================================

func StopSound(player: AudioStreamPlayer2D) -> void:
	if player != null and player.playing:
		player.stop()


# =========================================================
# 动画
# =========================================================

func PlayAnimation(action: String, restart := false) -> void:
	var animation: String = action if isInLight else action + "OLD"

	if anim.animation == animation and not restart:
		return

	var frame := anim.frame
	var progress: float = anim.frame_progress

	var sameAction := currentAction == action

	if restart:
		anim.stop()

	anim.play(animation)

	if sameAction and not restart:
		anim.set_frame_and_progress(frame, progress)

	currentAction = action


# =========================================================
# 正常跳跃
# （C# 用 ref 传参，这里改成返回更新后的速度）
# =========================================================

func Jump(vel: Vector2, gravity: float) -> Vector2:
	var result := vel

	result.y = -sqrt(2.0 * gravity * riseGravityScale * jumpHeight)

	coyoteCounter = 0.0
	jumpBufferCounter = 0.0

	jumping = true

	jumpCut = not Input.is_action_pressed("move_jump")

	if jumpCut:
		result.y *= jumpCutMultiplier

	PlayAnimation("jump", true)

	return result


# =========================================================
# 外部弹射
# =========================================================

func Launch(verticalSpeed: float) -> void:
	velocity.y = -verticalSpeed

	coyoteCounter = 0.0
	jumpBufferCounter = 0.0

	jumping = true
	jumpCut = true

	PlayAnimation("jump", true)


# =========================================================
# 落地压缩
# =========================================================

func PlayLandingSquash() -> void:
	if landingTween != null and landingTween.is_valid():
		landingTween.kill()

	anim.scale = animNormalScale

	var squashScale := Vector2(
		animNormalScale.x * (1.0 + landingSquash * 0.6),
		animNormalScale.y * (1.0 - landingSquash)
	)

	landingTween = create_tween()

	landingTween.tween_property(anim, "scale", squashScale, landingSquashTime
	).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)

	landingTween.tween_property(anim, "scale", animNormalScale, landingRecoverTime
	).set_trans(Tween.TRANS_BACK).set_ease(Tween.EASE_OUT)


# =========================================================
# 顶角修正
# =========================================================

func CorrectCeilingCorner(upwardDistance: float, direction: float) -> void:
	var collision := KinematicCollision2D.new()

	var upward := Vector2(0.0, upwardDistance)

	if not test_move(global_transform, upward, collision) or collision.get_normal().y < 0.5:
		return

	var preferredSide := -1 if direction < 0.0 else 1

	for distance in range(1, cornerCorrection + 1):
		for side in range(0, 2):
			var offset := Vector2(
				distance * preferredSide * (1 if side == 0 else -1),
				0.0
			)

			var shifted := global_transform
			shifted.origin += offset

			if not test_move(global_transform, offset) and not test_move(shifted, upward):
				global_position += offset
				return


# =========================================================
# 死亡
# =========================================================

func Die() -> void:
	if isDying:
		return

	isDying = true

	set_physics_process(false)

	velocity = Vector2.ZERO

	# =====================================================
	# 死亡后停止所有玩家环境音
	# =====================================================

	StopSound(footstepAudio)
	StopSound(clockAudio)
	StopSound(leaveLightAudio)
	StopSound(enterLightAudio)
	StopSound(landingAudio)

	# =====================================================
	# 死亡动画
	# =====================================================

	anim.pause()

	var animationPlayer := $AnimationPlayer as AnimationPlayer

	var particles := $DeathParticles as GPUParticles2D

	animationPlayer.play("Dissolve")

	particles.restart()
	particles.emitting = true

	await animationPlayer.animation_finished

	TransitionManager.ReloadCurrentScene()
