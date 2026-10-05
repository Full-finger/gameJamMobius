using Godot;

public partial class Player : CharacterBody2D
{
    [Export] public float speed = 300f;
    [Export] public float jumpHeight = 100f;

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;
        Vector2 gravity = GetGravity();

        // 重力
        if (!IsOnFloor())
            velocity += gravity * (float)delta;

        // 左右移动，同时按下则抵消
        float direction = 0;

        if (Input.IsActionPressed("move_left"))
            direction -= 1;

        if (Input.IsActionPressed("move_right"))
            direction += 1;

        velocity.X = direction * speed;

        // 根据跳跃高度计算起跳速度
        if (IsOnFloor() && Input.IsActionJustPressed("move_jump"))
            velocity.Y = -Mathf.Sqrt(2f * gravity.Y * jumpHeight);

        Velocity = velocity;
        MoveAndSlide();
    }
}