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

    public bool isInLight = false;

    private float normalSpeed;
    private float normalJumpHeight;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool jumping;
    private bool jumpCut;
    private AnimatedSprite2D anim;

    public override void _Ready()
    {
        normalSpeed = speed;
        normalJumpHeight = jumpHeight;
        FloorSnapLength = 6f;
        FloorConstantSpeed = true;

        anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        anim.SpriteFrames.SetAnimationLoopMode("jump", SpriteFrames.LoopMode.None);
        anim.Play("idle");
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
            GetTree().CallDeferred(SceneTree.MethodName.ReloadCurrentScene);
            return;
        }

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

        velocity.X = Mathf.MoveToward(velocity.X, direction * speed, moveAcceleration * dt);
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

        Velocity = velocity;
        MoveAndSlide();

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

        if (anim.Animation != nextAnimation)
            anim.Play(nextAnimation);
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

        anim.Stop();
        anim.Play("jump");
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
}
