using Godot;

public partial class Player : CharacterBody2D
{
	[Export] public float speed = 300f;
	[Export] public float jumpHeight = 100f;
	[Export] public float coyoteTime = 0.1f;
	[Export] public float jumpBufferTime = 0.1f;

	public bool isInLight = false;

	private float coyoteCounter;
	private float jumpBufferCounter;

	private AnimatedSprite2D anim;

	public override void _Ready()
	{
		anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		// 跳跃只播放一次。
		anim.SpriteFrames.SetAnimationLoopMode("jump", SpriteFrames.LoopMode.None);
		anim.Play("idle");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (isInLight)
		{
			speed = Mathf.MoveToward(speed, 300f, 600f * (float)delta);
			jumpHeight = Mathf.MoveToward(jumpHeight, 100f, 200f * (float)delta);
		}
		else
		{
			speed = Mathf.MoveToward(speed, 0f, 30f * (float)delta);
			jumpHeight = Mathf.MoveToward(jumpHeight, 0f, 10f * (float)delta);
		}

		// 两项都耗尽，重开。
		if (speed == 0f && jumpHeight == 0f)
		{
			GetTree().CallDeferred(SceneTree.MethodName.ReloadCurrentScene);
			return;
		}

		Vector2 velocity = Velocity;
		Vector2 gravity = GetGravity();

		// 重力。
		if (!IsOnFloor())
			velocity += gravity * (float)delta;

		// 左右移动。
		float direction = 0;

		if (Input.IsActionPressed("move_left"))
			direction -= 1;

		if (Input.IsActionPressed("move_right"))
			direction += 1;

		velocity.X = direction * speed;

		// 原图朝左：向右移动时翻转，松开按键时保持朝向。
		if (direction != 0)
			anim.FlipH = direction > 0;

		// 计时器递减
		coyoteCounter = Mathf.Max(0f, coyoteCounter - (float)delta);
		jumpBufferCounter = Mathf.Max(0f, jumpBufferCounter - (float)delta);

		// 在地面时刷新土狼时间
		if (IsOnFloor())
			coyoteCounter = coyoteTime;

		// 按下跳跃时刷新缓冲
		if (Input.IsActionJustPressed("move_jump"))
			jumpBufferCounter = jumpBufferTime;

		// 土狼窗口内且缓冲未过期则起跳
		if (coyoteCounter > 0f && jumpBufferCounter > 0f)
		{
			velocity.Y = -Mathf.Sqrt(2f * gravity.Y * jumpHeight);
			coyoteCounter = 0f;
			jumpBufferCounter = 0f;
		}

		Velocity = velocity;
		MoveAndSlide();

		// 移动完成后，根据最新的落地状态选择动画。
		string nextAnimation;

		if (!IsOnFloor())
			nextAnimation = "jump";
		else if (Velocity.X != 0)
			nextAnimation = "walk";
		else
			nextAnimation = "idle";

		// 只有换动画时才播放，jump 播完后保持最后一帧。
		if (anim.Animation != nextAnimation)
			anim.Play(nextAnimation);
	}
}
