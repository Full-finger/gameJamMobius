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

		// 两项都耗尽，延后到本次物理处理结束后重开
		if (speed == 0f && jumpHeight == 0f)
		{
			GetTree().CallDeferred(SceneTree.MethodName.ReloadCurrentScene);
			return;
		}

		Vector2 velocity = Velocity;
		Vector2 gravity = GetGravity();

		// 重力
		if (!IsOnFloor())
			velocity += gravity * (float)delta;

		// 左右移动
		float direction = 0;

		if (Input.IsActionPressed("move_left"))
			direction -= 1;

		if (Input.IsActionPressed("move_right"))
			direction += 1;

		velocity.X = direction * speed;

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
	}
}
