using Godot;

public partial class Player : CharacterBody2D
{
	[Export] public float speed = 300f;
	[Export] public float jumpHeight = 100f;

	[ExportGroup("跳跃容错")]
	[Export] public float coyoteTime = 0.1f;
	[Export] public float jumpBufferTime = 0.1f;
	[Export] public int cornerCorrection = 6;

	[ExportGroup("左右移动手感")]
	[Export] public float acceleration = 2400f;
	[Export] public float deceleration = 3200f;
	[Export] public float turnAcceleration = 4800f;
	[Export] public float airAcceleration = 1800f;
	[Export] public float airDeceleration = 900f;

	[ExportGroup("跳跃手感")]
	[Export] public float riseGravityScale = 1.4f;
	[Export] public float fallGravityScale = 2.6f;
	[Export] public float apexGravityScale = 0.45f;
	[Export] public float apexSpeed = 60f;
	[Export] public float jumpCutMultiplier = 0.45f;
	[Export] public float maxFallSpeed = 900f;

	[ExportGroup("藤蔓手感")]
	[Export] public float vineSwingMultiplier = 5f; // A/D 推力、摆荡速度上限和跟随速度倍率。
	[Export] public float vineGrabImpulse = 1.25f; // 抓住时带入水平冲量；原先 0.25 的五倍。
	[Export] public float vineJumpMultiplier = 5f; // 跳离藤蔓的速度倍率。

[ExportGroup("落地回弹")]
[Export(PropertyHint.Range, "0.0,0.15,0.01")]
public float landingSquash = 0.05f; // Y轴压缩5%

[Export(PropertyHint.Range, "0.01,0.2,0.01")]
public float landingSquashTime = 0.06f;

[Export(PropertyHint.Range, "0.01,0.3,0.01")]
public float landingRecoverTime = 0.10f;

private Vector2 animNormalScale;
private Tween landingTween;
	public bool isInLight = false;

	private float normalSpeed;
	private float normalJumpHeight;
	private float coyoteCounter;
	private float jumpBufferCounter;
	private bool jumping;
	private bool jumpCut;
	private AnimatedSprite2D anim;
	private AudioStreamPlayer2D footstepAudio;
	private string currentAction = "";

	private RigidBody2D vine;
	private float grabCooldown;
	private float vineFlightTime;
	private bool canJumpFromVine;
	private Vector2 vineGrabPoint;
	private Vector2 grabCenterOffset;

	// GrabArea 调用这个方法，让玩家抓住当前这一节。
	public void GrabVine(RigidBody2D target)
	{
		if (!IsPhysicsProcessing() || vine != null || grabCooldown > 0f)
			return;

		vine = target;

		// 抓身体接触到的这一段位置，保持玩家直立，不跳到刚体中心。
		grabCenterOffset = GetNode<CollisionShape2D>("CollisionShape2D").GlobalPosition - GlobalPosition;
		vineGrabPoint = vine.ToLocal(GlobalPosition + grabCenterOffset);
		float halfLength = ((RectangleShape2D)vine.GetNode<CollisionShape2D>("CollisionShape2D").Shape).Size.Y * 0.5f;
		vineGrabPoint.X = 0f;
		vineGrabPoint.Y = Mathf.Clamp(vineGrabPoint.Y, -halfLength, halfLength);
		vine.ApplyCentralImpulse(new Vector2(Velocity.X, 0f) * vineGrabImpulse);
		Velocity = Vector2.Zero;
		vineFlightTime = 0f;

		// 如果正按着跳跃，先松开，再按才会跳离藤蔓。
		canJumpFromVine = !Input.IsActionPressed("move_jump");

		coyoteCounter = 0f;
		jumpBufferCounter = 0f;
		jumping = false;
		jumpCut = true;

		PlayAnimation("idle");
	}

	// 返回 true 表示正在处理藤蔓，跳过普通移动。
	private bool UpdateVine(float dt)
	{
		grabCooldown = Mathf.Max(0f, grabCooldown - dt);
		vineFlightTime = Mathf.Max(0f, vineFlightTime - dt);

		if (!GodotObject.IsInstanceValid(vine))
		{
			vine = null;
			return false;
		}

		float direction = 0f;
		if (Input.IsActionPressed("move_left")) direction -= 1f;
		if (Input.IsActionPressed("move_right")) direction += 1f;

		// 左右按键推动藤蔓摆动。
		if (Mathf.Abs(vine.LinearVelocity.X) < 650f * vineSwingMultiplier || direction * vine.LinearVelocity.X < 0f)
			vine.ApplyCentralForce(new Vector2(direction * 900f * vineSwingMultiplier, 0f));

		if (direction != 0f)
			anim.FlipH = direction > 0f;

		if (!Input.IsActionPressed("move_jump"))
			canJumpFromVine = true;

		if (canJumpFromVine && Input.IsActionJustPressed("move_jump"))
		{
			// 继承抓取点的部分切向速度；旋转带来的速度也算进去。
			Vector2 radius = vine.ToGlobal(vineGrabPoint) - vine.GlobalPosition;
			Vector2 pointVelocity = vine.LinearVelocity + new Vector2(-radius.Y, radius.X) * vine.AngularVelocity;
			Vector2 launch = pointVelocity.LimitLength(normalSpeed) * 0.4f;
			launch.X = Mathf.Clamp(launch.X + direction * speed * 0.5f, -normalSpeed, normalSpeed);
			float upwardCarry = Mathf.Min(launch.Y, 0f);
			Jump(ref launch, GetGravity().Y);
			launch.Y += upwardCarry;
			launch *= vineJumpMultiplier;

			vine = null;
			grabCooldown = 0.45f;
			vineFlightTime = 0.2f;

			Velocity = launch;
			MoveAndSlide();
			return true;
		}

		PlayAnimation("idle");

		// 用玩家自身的位置跟随这一节，不需要手部节点。
		Vector2 targetPosition = vine.ToGlobal(vineGrabPoint) - grabCenterOffset;
		Velocity = ((targetPosition - GlobalPosition) / dt).LimitLength(normalSpeed * 1.5f * vineSwingMultiplier);
		MoveAndSlide();
		// 被墙或地面挡住时松开，避免被藤蔓隔着地形一直拽住。
		if (GlobalPosition.DistanceTo(targetPosition) > normalSpeed * 0.25f)
		{
			vine = null;
			grabCooldown = 0.45f;
			Velocity = Vector2.Zero;
		}

		return true;
	}
	public override void _Ready()
	{
		normalSpeed = speed;
		normalJumpHeight = jumpHeight;
		// 菌盖边缘约 46～48 度，也算作可以站立的地面。
		FloorMaxAngle = Mathf.DegToRad(55f);
		FloorSnapLength = 24f;
		FloorConstantSpeed = true;

		anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	animNormalScale = anim.Scale;
footstepAudio = GetNode<AudioStreamPlayer2D>("FootstepAudio");
		// 给当前玩家一份独立材质。
		anim.Material = (ShaderMaterial)anim.Material.Duplicate();

		// 出生时恢复完整显示。
		((ShaderMaterial)anim.Material).SetShaderParameter("progress", 0f);
		anim.SpriteFrames.SetAnimationLoopMode("jump", SpriteFrames.LoopMode.None);
		anim.SpriteFrames.SetAnimationLoopMode("jumpOLD", SpriteFrames.LoopMode.None);
		PlayAnimation("idle");
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;

		// 光外十秒耗尽，光内约半秒恢复到检查器设置的初始值。
		if (isInLight)
		{
			speed = Mathf.MoveToward(speed, normalSpeed, normalSpeed * 2f * dt);
			jumpHeight = Mathf.MoveToward(jumpHeight, normalJumpHeight, normalJumpHeight * 2f * dt);
		}
		else
		{
			speed = Mathf.MoveToward(speed, 0f, normalSpeed * 0.1f * dt);
			jumpHeight = Mathf.MoveToward(jumpHeight, 0f, normalJumpHeight * 0.1f * dt);
		}

		if (speed == 0f && jumpHeight == 0f)
		{
			Die();
			return;
		}
		if (UpdateVine(dt))
			return;
		Vector2 velocity = Velocity;
		float gravity = GetGravity().Y;
		bool grounded = IsOnFloor() && velocity.Y >= 0f;
		bool jumpHeld = Input.IsActionPressed("move_jump");

		coyoteCounter = Mathf.Max(0f, coyoteCounter - dt);
		jumpBufferCounter = Mathf.Max(0f, jumpBufferCounter - dt);

		if (grounded)
		{
			coyoteCounter = coyoteTime;
			jumping = false;
			jumpCut = false;
		}

		if (Input.IsActionJustPressed("move_jump"))
			jumpBufferCounter = jumpBufferTime;

		float direction = 0f;
		if (Input.IsActionPressed("move_left")) direction -= 1f;
		if (Input.IsActionPressed("move_right")) direction += 1f;

		// 起步稍有过渡，松手刹车更快，反向输入时迅速转向。
		float moveAcceleration = grounded ? acceleration : airAcceleration;
		if (direction == 0f)
			moveAcceleration = grounded ? deceleration : airDeceleration;
		else if (velocity.X * direction < 0f)
			moveAcceleration = grounded ? turnAcceleration : airAcceleration * 2f;

		// 顶点只对主动跳跃生效；走出平台、撞头后不会突然悬浮。
		bool nearApex = jumping && !jumpCut && jumpHeld && Mathf.Abs(velocity.Y) < apexSpeed;
		if (!grounded && nearApex && direction != 0f)
			moveAcceleration *= 1.2f;

		if (vineFlightTime <= 0f || grounded)
		{
			velocity.X = Mathf.MoveToward(
				velocity.X, direction * speed, moveAcceleration * dt);
		}
		if (direction != 0f)
			anim.FlipH = direction > 0f;

		bool startedJump = false;
		if ((grounded || coyoteCounter > 0f) && jumpBufferCounter > 0f && jumpHeight > 0f)
		{
			Jump(ref velocity, gravity);
			startedJump = true;
		}

		// 上升中松开跳跃键，只削减一次向上速度，得到小跳。
		if (jumping && !jumpCut && !jumpHeld && velocity.Y < 0f)
		{
			velocity.Y *= jumpCutMultiplier;
			jumpCut = true;
		}

		if (!grounded && !startedJump)
		{
			float gravityScale = velocity.Y < 0f ? riseGravityScale : fallGravityScale;
			nearApex = jumping && !jumpCut && jumpHeld && Mathf.Abs(velocity.Y) < apexSpeed;
			if (nearApex)
				gravityScale = apexGravityScale;

			velocity.Y = Mathf.Min(velocity.Y + gravity * gravityScale * dt, maxFallSpeed);
		}

		// 头顶仅擦到平台边角时，小幅挪开，让起跳顺利通过。
		if (velocity.Y < 0f)
			CorrectCeilingCorner(velocity.Y * dt, direction);
bool wasOnFloor = IsOnFloor();

Velocity = velocity;
MoveAndSlide();

if (!wasOnFloor && IsOnFloor())
{
	PlayLandingSquash();
}

		if (IsOnCeiling())
		{
			jumping = false;
			jumpCut = true;
		}

		// 落地这一帧就消费提前按下的跳跃，不额外等一帧。
		bool bufferedJump = false;
		if (IsOnFloor() && !startedJump && jumpBufferCounter > 0f && jumpHeight > 0f)
		{
			velocity = Velocity;
			Jump(ref velocity, gravity);
			Velocity = velocity;
			bufferedJump = true;
		}

		string nextAnimation;
		if (!IsOnFloor() || bufferedJump)
			nextAnimation = "jump";
		else if (Mathf.Abs(Velocity.X) > 1f)
			nextAnimation = "walk";
		else
			nextAnimation = "idle";
bool isWalking =
    IsOnFloor() &&
    !bufferedJump &&
    Mathf.Abs(Velocity.X) > 1f;

if (isWalking)
{
    // 防止每个 PhysicsProcess 都从头播放
    if (!footstepAudio.Playing)
        footstepAudio.Play();
}
else
{
    if (footstepAudio.Playing)
        footstepAudio.Stop();
}
		PlayAnimation(nextAnimation);
	}

	// 光内用正常版，光外用 OLD 版；同一个动作切换外观时保留播放进度。
	private void PlayAnimation(string action, bool restart = false)
	{
		string animation = isInLight ? action : action + "OLD";
		if (anim.Animation == animation && !restart)
			return;

		int frame = anim.Frame;
		float progress = anim.FrameProgress;
		bool sameAction = currentAction == action;

		if (restart)
			anim.Stop();
		anim.Play(animation);
		if (sameAction && !restart)
			anim.SetFrameAndProgress(frame, progress);

		currentAction = action;
	}

	private void Jump(ref Vector2 velocity, float gravity)
	{
		velocity.Y = -Mathf.Sqrt(2f * gravity * riseGravityScale * jumpHeight);
		coyoteCounter = 0f;
		jumpBufferCounter = 0f;
		jumping = true;
		jumpCut = !Input.IsActionPressed("move_jump");

		// 提前轻点后已松开的缓冲跳跃，也按小跳处理。
		if (jumpCut)
			velocity.Y *= jumpCutMultiplier;

		PlayAnimation("jump", true);
	}

	// 蹦床等外部弹射：给一个大向上的速度，不受松键小跳削减影响。
	public void Launch(float verticalSpeed)
	{
		var v = Velocity;
		v.Y = -verticalSpeed;
		Velocity = v;
		coyoteCounter = 0f;
		jumpBufferCounter = 0f;
		jumping = true;
		jumpCut = true; // 锁死小跳削减，弹力全额生效
		PlayAnimation("jump", true);
	}
private void PlayLandingSquash()
{
	// 防止短时间连续触发导致 Tween 互相打架
	if (landingTween != null && landingTween.IsValid())
		landingTween.Kill();

	anim.Scale = animNormalScale;

	Vector2 squashScale = new Vector2(
		animNormalScale.X * (1f + landingSquash * 0.6f),
		animNormalScale.Y * (1f - landingSquash)
	);

	landingTween = CreateTween();

	// 落地：快速压扁
	landingTween.TweenProperty(
		anim,
		"scale",
		squashScale,
		landingSquashTime
	)
	.SetTrans(Tween.TransitionType.Quad)
	.SetEase(Tween.EaseType.Out);

	// 回弹：恢复原大小
	landingTween.TweenProperty(
		anim,
		"scale",
		animNormalScale,
		landingRecoverTime
	)
	.SetTrans(Tween.TransitionType.Back)
	.SetEase(Tween.EaseType.Out);
}
	private void CorrectCeilingCorner(float upwardDistance, float direction)
	{
		var collision = new KinematicCollision2D();
		Vector2 upward = new Vector2(0f, upwardDistance);
		if (!TestMove(GlobalTransform, upward, collision) || collision.GetNormal().Y < 0.5f)
			return;

		int preferredSide = direction < 0f ? -1 : 1;
		for (int distance = 1; distance <= cornerCorrection; distance++)
		{
			for (int side = 0; side < 2; side++)
			{
				Vector2 offset = new Vector2(distance * preferredSide * (side == 0 ? 1 : -1), 0f);
				Transform2D shifted = GlobalTransform;
				shifted.Origin += offset;
				if (!TestMove(GlobalTransform, offset) && !TestMove(shifted, upward))
				{
					GlobalPosition += offset;
					return;
				}
			}
		}
	}

	private async void Die()
	{
		SetPhysicsProcess(false);
		Velocity = Vector2.Zero;

		// 定格角色姿势。
		anim.Pause();

		AnimationPlayer animationPlayer =
			GetNode<AnimationPlayer>("AnimationPlayer");

		GpuParticles2D particles =
			GetNode<GpuParticles2D>("DeathParticles");

		// 溶解和粒子同时开始。
		animationPlayer.Play("Dissolve");
		particles.Restart();
		particles.Emitting = true;

		// 等一秒的溶解动画结束，再重开。
		await ToSignal(
			animationPlayer,
			AnimationPlayer.SignalName.AnimationFinished
		);

		GetTree().CallDeferred(SceneTree.MethodName.ReloadCurrentScene);
	}
	public float LifeRatio
{
    get
    {
        if (normalSpeed <= 0f || normalJumpHeight <= 0f)
            return 0f;

        float speedRatio = speed / normalSpeed;
        float jumpRatio = jumpHeight / normalJumpHeight;

        return Mathf.Clamp(
            Mathf.Min(speedRatio, jumpRatio),
            0f,
            1f
        );
    }
}
}
