using Godot;

public partial class Player : CharacterBody2D
{
    [Export] public float speed = 300f;
    [Export] public float jumpHeight = 100f;


    // =========================================================
    // 跳跃容错
    // =========================================================

    [ExportGroup("跳跃容错")]
    [Export] public float coyoteTime = 0.1f;
    [Export] public float jumpBufferTime = 0.1f;
    [Export] public int cornerCorrection = 6;


    // =========================================================
    // 左右移动
    // =========================================================

    [ExportGroup("左右移动手感")]
    [Export] public float acceleration = 2400f;
    [Export] public float deceleration = 3200f;
    [Export] public float turnAcceleration = 4800f;
    [Export] public float airAcceleration = 1800f;
    [Export] public float airDeceleration = 900f;


    // =========================================================
    // 跳跃
    // =========================================================

    [ExportGroup("跳跃手感")]
    [Export] public float riseGravityScale = 1.4f;
    [Export] public float fallGravityScale = 2.6f;
    [Export] public float apexGravityScale = 0.45f;
    [Export] public float apexSpeed = 60f;
    [Export] public float jumpCutMultiplier = 0.45f;
    [Export] public float maxFallSpeed = 900f;


    // =========================================================
    // 藤蔓
    // =========================================================

    [ExportGroup("藤蔓手感")]
    [Export]
    public float vineSwingMultiplier = 5f;

    [Export]
    public float vineGrabImpulse = 1.25f;

    [Export]
    public float vineJumpMultiplier = 5f;


    // =========================================================
    // 边界
    // =========================================================

    [ExportGroup("边界")]
    [Export]
    public float deathY = float.MaxValue;


    // =========================================================
    // 落地回弹
    // =========================================================

    [ExportGroup("落地回弹")]

    [Export(PropertyHint.Range, "0.0,0.15,0.01")]
    public float landingSquash = 0.05f;

    [Export(PropertyHint.Range, "0.01,0.2,0.01")]
    public float landingSquashTime = 0.06f;

    [Export(PropertyHint.Range, "0.01,0.3,0.01")]
    public float landingRecoverTime = 0.10f;


    // =========================================================
    // 音效
    // 你只需要把对应音频拖进这五个槽
    // =========================================================

    [ExportGroup("音效")]

    [Export]
    public AudioStream footstepSound;

    [Export]
    public AudioStream landingSound;

    [Export]
    public AudioStream leaveLightSound;

    [Export]
    public AudioStream enterLightSound;

    [Export]
    public AudioStream clockSound;


    // =========================================================
    // 内部变量
    // =========================================================

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

    private string currentAction = "";


    // =========================================================
    // 音频播放器
    // =========================================================

    private AudioStreamPlayer2D footstepAudio;
    private AudioStreamPlayer2D landingAudio;
    private AudioStreamPlayer2D leaveLightAudio;
    private AudioStreamPlayer2D enterLightAudio;
    private AudioStreamPlayer2D clockAudio;

    private bool lightStateInitialized = false;
    private bool lastLightState = false;

    private bool isDying = false;


    // =========================================================
    // 藤蔓
    // =========================================================

    private RigidBody2D vine;

    private float grabCooldown;
    private float vineFlightTime;

    private bool canJumpFromVine;

    private Vector2 vineGrabPoint;
    private Vector2 grabCenterOffset;


    // =========================================================
    // 藤蔓抓取
    // =========================================================

    public void GrabVine(RigidBody2D target)
    {
        if (!IsPhysicsProcessing() ||
            vine != null ||
            grabCooldown > 0f)
        {
            return;
        }

        // 抓藤蔓后停止脚步声
        StopSound(footstepAudio);

        vine = target;

        grabCenterOffset =
            GetNode<CollisionShape2D>("CollisionShape2D").GlobalPosition
            - GlobalPosition;

        vineGrabPoint =
            vine.ToLocal(GlobalPosition + grabCenterOffset);

        float halfLength =
            ((RectangleShape2D)
                vine.GetNode<CollisionShape2D>("CollisionShape2D").Shape)
            .Size.Y * 0.5f;

        vineGrabPoint.X = 0f;

        vineGrabPoint.Y =
            Mathf.Clamp(
                vineGrabPoint.Y,
                -halfLength,
                halfLength
            );

        vine.ApplyCentralImpulse(
            new Vector2(Velocity.X, 0f)
            * vineGrabImpulse
        );

        Velocity = Vector2.Zero;

        vineFlightTime = 0f;

        canJumpFromVine =
            !Input.IsActionPressed("move_jump");

        coyoteCounter = 0f;
        jumpBufferCounter = 0f;

        jumping = false;
        jumpCut = true;

        PlayAnimation("idle");
    }


    // =========================================================
    // 藤蔓更新
    // =========================================================

    private bool UpdateVine(float dt)
    {
        grabCooldown =
            Mathf.Max(0f, grabCooldown - dt);

        vineFlightTime =
            Mathf.Max(0f, vineFlightTime - dt);


        if (!GodotObject.IsInstanceValid(vine))
        {
            vine = null;
            return false;
        }


        // 抓藤蔓期间不能有脚步声
        StopSound(footstepAudio);


        float direction = 0f;

        if (Input.IsActionPressed("move_left"))
            direction -= 1f;

        if (Input.IsActionPressed("move_right"))
            direction += 1f;


        if (
            Mathf.Abs(vine.LinearVelocity.X)
            < 650f * vineSwingMultiplier
            ||
            direction * vine.LinearVelocity.X < 0f
        )
        {
            vine.ApplyCentralForce(
                new Vector2(
                    direction
                    * 900f
                    * vineSwingMultiplier,

                    0f
                )
            );
        }


        if (direction != 0f)
            anim.FlipH = direction > 0f;


        if (!Input.IsActionPressed("move_jump"))
            canJumpFromVine = true;


        if (
            canJumpFromVine &&
            Input.IsActionJustPressed("move_jump")
        )
        {
            Vector2 radius =
                vine.ToGlobal(vineGrabPoint)
                - vine.GlobalPosition;

            Vector2 pointVelocity =
                vine.LinearVelocity
                +
                new Vector2(
                    -radius.Y,
                    radius.X
                )
                * vine.AngularVelocity;


            Vector2 launch =
                pointVelocity
                .LimitLength(normalSpeed)
                * 0.4f;


            launch.X = Mathf.Clamp(
                launch.X
                + direction
                * speed
                * 0.5f,

                -normalSpeed,
                normalSpeed
            );


            float upwardCarry =
                Mathf.Min(
                    launch.Y,
                    0f
                );


            Jump(
                ref launch,
                GetGravity().Y
            );


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


        Vector2 targetPosition =
            vine.ToGlobal(vineGrabPoint)
            - grabCenterOffset;


        Velocity =
            (
                (targetPosition - GlobalPosition)
                / dt
            )
            .LimitLength(
                normalSpeed
                * 1.5f
                * vineSwingMultiplier
            );


        MoveAndSlide();


        if (
            GlobalPosition.DistanceTo(targetPosition)
            >
            normalSpeed * 0.25f
        )
        {
            vine = null;

            grabCooldown = 0.45f;

            Velocity = Vector2.Zero;
        }


        return true;
    }


    // =========================================================
    // Ready
    // =========================================================

    public override void _Ready()
    {
        normalSpeed = speed;
        normalJumpHeight = jumpHeight;


        FloorMaxAngle =
            Mathf.DegToRad(55f);

        FloorSnapLength = 24f;

        FloorConstantSpeed = true;


        anim =
            GetNode<AnimatedSprite2D>(
                "AnimatedSprite2D"
            );

        animNormalScale = anim.Scale;


        // =====================================================
        // 自动创建音频播放器
        // =====================================================

        footstepAudio =
            CreateAudioPlayer(footstepSound);

        landingAudio =
            CreateAudioPlayer(landingSound);

        leaveLightAudio =
            CreateAudioPlayer(leaveLightSound);

        enterLightAudio =
            CreateAudioPlayer(enterLightSound);

        clockAudio =
            CreateAudioPlayer(clockSound);


        // =====================================================
        // 玩家材质
        // =====================================================

        anim.Material =
            (ShaderMaterial)
            anim.Material.Duplicate();


        ((ShaderMaterial)anim.Material)
            .SetShaderParameter(
                "progress",
                0f
            );


        anim.SpriteFrames
            .SetAnimationLoopMode(
                "jump",
                SpriteFrames.LoopMode.None
            );

        anim.SpriteFrames
            .SetAnimationLoopMode(
                "jumpOLD",
                SpriteFrames.LoopMode.None
            );


        PlayAnimation("idle");
    }


    // =========================================================
    // Physics
    // =========================================================

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;


        // =====================================================
        // 光照状态音效
        // =====================================================

        UpdateLightAudioState();


        // =====================================================
        // 光照机制
        // =====================================================

        if (isInLight)
        {
            speed =
                Mathf.MoveToward(
                    speed,
                    normalSpeed,
                    normalSpeed * 2f * dt
                );

            jumpHeight =
                Mathf.MoveToward(
                    jumpHeight,
                    normalJumpHeight,
                    normalJumpHeight * 2f * dt
                );
        }
        else
        {
            speed =
                Mathf.MoveToward(
                    speed,
                    0f,
                    normalSpeed * 0.1f * dt
                );

            jumpHeight =
                Mathf.MoveToward(
                    jumpHeight,
                    0f,
                    normalJumpHeight * 0.1f * dt
                );
        }


        // =====================================================
        // 迟暮死亡
        // =====================================================

        if (
            speed == 0f &&
            jumpHeight == 0f
        )
        {
            Die();
            return;
        }


        // =====================================================
        // 藤蔓
        // =====================================================

        if (UpdateVine(dt))
            return;


        Vector2 velocity = Velocity;

        float gravity =
            GetGravity().Y;


        bool grounded =
            IsOnFloor() &&
            velocity.Y >= 0f;


        bool jumpHeld =
            Input.IsActionPressed(
                "move_jump"
            );


        coyoteCounter =
            Mathf.Max(
                0f,
                coyoteCounter - dt
            );


        jumpBufferCounter =
            Mathf.Max(
                0f,
                jumpBufferCounter - dt
            );


        if (grounded)
        {
            coyoteCounter =
                coyoteTime;

            jumping = false;
            jumpCut = false;
        }


        if (
            Input.IsActionJustPressed(
                "move_jump"
            )
        )
        {
            jumpBufferCounter =
                jumpBufferTime;
        }


        // =====================================================
        // 左右输入
        // =====================================================

        float direction = 0f;


        if (
            Input.IsActionPressed(
                "move_left"
            )
        )
        {
            direction -= 1f;
        }


        if (
            Input.IsActionPressed(
                "move_right"
            )
        )
        {
            direction += 1f;
        }


        float moveAcceleration =
            grounded
            ? acceleration
            : airAcceleration;


        if (direction == 0f)
        {
            moveAcceleration =
                grounded
                ? deceleration
                : airDeceleration;
        }
        else if (
            velocity.X * direction < 0f
        )
        {
            moveAcceleration =
                grounded
                ? turnAcceleration
                : airAcceleration * 2f;
        }


        bool nearApex =
            jumping
            &&
            !jumpCut
            &&
            jumpHeld
            &&
            Mathf.Abs(velocity.Y)
            < apexSpeed;


        if (
            !grounded &&
            nearApex &&
            direction != 0f
        )
        {
            moveAcceleration *= 1.2f;
        }


        if (
            vineFlightTime <= 0f ||
            grounded
        )
        {
            velocity.X =
                Mathf.MoveToward(
                    velocity.X,
                    direction * speed,
                    moveAcceleration * dt
                );
        }


        if (direction != 0f)
            anim.FlipH =
                direction > 0f;


        // =====================================================
        // 跳跃
        // =====================================================

        bool startedJump = false;


        if (
            (
                grounded ||
                coyoteCounter > 0f
            )
            &&
            jumpBufferCounter > 0f
            &&
            jumpHeight > 0f
        )
        {
            Jump(
                ref velocity,
                gravity
            );

            startedJump = true;
        }


        if (
            jumping &&
            !jumpCut &&
            !jumpHeld &&
            velocity.Y < 0f
        )
        {
            velocity.Y *=
                jumpCutMultiplier;

            jumpCut = true;
        }


        if (
            !grounded &&
            !startedJump
        )
        {
            float gravityScale =
                velocity.Y < 0f
                ? riseGravityScale
                : fallGravityScale;


            nearApex =
                jumping
                &&
                !jumpCut
                &&
                jumpHeld
                &&
                Mathf.Abs(velocity.Y)
                < apexSpeed;


            if (nearApex)
            {
                gravityScale =
                    apexGravityScale;
            }


            velocity.Y =
                Mathf.Min(
                    velocity.Y
                    +
                    gravity
                    * gravityScale
                    * dt,

                    maxFallSpeed
                );
        }


        // =====================================================
        // 掉出地图
        // =====================================================

        if (
            GlobalPosition.Y >
            deathY
        )
        {
            Die();
            return;
        }


        // =====================================================
        // 顶角修正
        // =====================================================

        if (velocity.Y < 0f)
        {
            CorrectCeilingCorner(
                velocity.Y * dt,
                direction
            );
        }


        // =====================================================
        // 移动
        // =====================================================

        bool wasOnFloor =
            IsOnFloor();


        Velocity = velocity;

        MoveAndSlide();


        // =====================================================
        // 落地
        // =====================================================

        if (
            !wasOnFloor &&
            IsOnFloor()
        )
        {
            PlayLandingSquash();

            // ★ 落地音效
            PlaySound(landingAudio);
        }


        if (IsOnCeiling())
        {
            jumping = false;
            jumpCut = true;
        }


        // =====================================================
        // Buffer Jump
        // =====================================================

        bool bufferedJump = false;


        if (
            IsOnFloor()
            &&
            !startedJump
            &&
            jumpBufferCounter > 0f
            &&
            jumpHeight > 0f
        )
        {
            velocity = Velocity;

            Jump(
                ref velocity,
                gravity
            );

            Velocity = velocity;

            bufferedJump = true;
        }


        // =====================================================
        // 动画
        // =====================================================

        string nextAnimation;


        if (
            !IsOnFloor() ||
            bufferedJump
        )
        {
            nextAnimation = "jump";
        }
        else if (
            Mathf.Abs(Velocity.X)
            > 1f
        )
        {
            nextAnimation = "walk";
        }
        else
        {
            nextAnimation = "idle";
        }


        // =====================================================
        // 脚步声
        // =====================================================

        bool isWalking =
            IsOnFloor()
            &&
            !bufferedJump
            &&
            Mathf.Abs(Velocity.X)
            > 1f;


        if (isWalking)
        {
            if (
                footstepAudio.Stream != null &&
                !footstepAudio.Playing
            )
            {
                footstepAudio.Play();
            }
        }
        else
        {
            StopSound(
                footstepAudio
            );
        }


        PlayAnimation(
            nextAnimation
        );
    }


    // =========================================================
    // 光照音频
    // =========================================================

    private void UpdateLightAudioState()
    {
        // 第一次运行：
        // 只同步状态，不播“进入/离开”的提示音
        if (!lightStateInitialized)
        {
            lightStateInitialized = true;

            lastLightState =
                isInLight;


            // 出生时如果就在光外，
            // 直接开始时钟声
            if (!isInLight)
            {
                PlaySound(
                    clockAudio
                );
            }

            return;
        }


        // =====================================================
        // 光照状态发生变化
        // =====================================================

        if (
            isInLight !=
            lastLightState
        )
        {
            // -------------------------
            // 进入光
            // -------------------------

            if (isInLight)
            {
                StopSound(
                    clockAudio
                );

                PlaySound(
                    enterLightAudio
                );
            }

            // -------------------------
            // 离开光
            // -------------------------

            else
            {
                PlaySound(
                    leaveLightAudio
                );

                PlaySound(
                    clockAudio
                );
            }


            lastLightState =
                isInLight;
        }


        // =====================================================
        // 保证时钟声在光外持续循环
        // =====================================================

        if (
            !isInLight &&
            !isDying
        )
        {
            if (
                clockAudio.Stream != null &&
                !clockAudio.Playing
            )
            {
                clockAudio.Play();
            }
        }
        else
        {
            StopSound(
                clockAudio
            );
        }
    }


    // =========================================================
    // 创建声音播放器
    // =========================================================

    private AudioStreamPlayer2D CreateAudioPlayer(
        AudioStream stream
    )
    {
        AudioStreamPlayer2D player =
            new AudioStreamPlayer2D();


        player.Stream = stream;

        // 你的总音量 Slider 控制的就是 Master
        player.Bus = "Master";


        AddChild(player);


        return player;
    }


    // =========================================================
    // 播放一次声音
    // =========================================================

    private void PlaySound(
        AudioStreamPlayer2D player
    )
    {
        if (
            player == null ||
            player.Stream == null
        )
        {
            return;
        }


        // 如果同一音效再次触发，
        // 直接从头播放
        player.Play();
    }


    // =========================================================
    // 停止声音
    // =========================================================

    private void StopSound(
        AudioStreamPlayer2D player
    )
    {
        if (
            player != null &&
            player.Playing
        )
        {
            player.Stop();
        }
    }


    // =========================================================
    // 动画
    // =========================================================

    private void PlayAnimation(
        string action,
        bool restart = false
    )
    {
        string animation =
            isInLight
            ? action
            : action + "OLD";


        if (
            anim.Animation == animation &&
            !restart
        )
        {
            return;
        }


        int frame =
            anim.Frame;

        float progress =
            anim.FrameProgress;


        bool sameAction =
            currentAction == action;


        if (restart)
            anim.Stop();


        anim.Play(animation);


        if (
            sameAction &&
            !restart
        )
        {
            anim.SetFrameAndProgress(
                frame,
                progress
            );
        }


        currentAction = action;
    }


    // =========================================================
    // 正常跳跃
    // =========================================================

    private void Jump(
        ref Vector2 velocity,
        float gravity
    )
    {
        velocity.Y =
            -Mathf.Sqrt(
                2f
                * gravity
                * riseGravityScale
                * jumpHeight
            );


        coyoteCounter = 0f;
        jumpBufferCounter = 0f;


        jumping = true;


        jumpCut =
            !Input.IsActionPressed(
                "move_jump"
            );


        if (jumpCut)
        {
            velocity.Y *=
                jumpCutMultiplier;
        }


        PlayAnimation(
            "jump",
            true
        );
    }


    // =========================================================
    // 外部弹射
    // =========================================================

    public void Launch(
        float verticalSpeed
    )
    {
        var v = Velocity;

        v.Y =
            -verticalSpeed;

        Velocity = v;


        coyoteCounter = 0f;
        jumpBufferCounter = 0f;


        jumping = true;

        jumpCut = true;


        PlayAnimation(
            "jump",
            true
        );
    }


    // =========================================================
    // 落地压缩
    // =========================================================

    private void PlayLandingSquash()
    {
        if (
            landingTween != null &&
            landingTween.IsValid()
        )
        {
            landingTween.Kill();
        }


        anim.Scale =
            animNormalScale;


        Vector2 squashScale =
            new Vector2(
                animNormalScale.X
                *
                (
                    1f
                    +
                    landingSquash
                    * 0.6f
                ),

                animNormalScale.Y
                *
                (
                    1f
                    -
                    landingSquash
                )
            );


        landingTween =
            CreateTween();


        landingTween
            .TweenProperty(
                anim,
                "scale",
                squashScale,
                landingSquashTime
            )
            .SetTrans(
                Tween.TransitionType.Quad
            )
            .SetEase(
                Tween.EaseType.Out
            );


        landingTween
            .TweenProperty(
                anim,
                "scale",
                animNormalScale,
                landingRecoverTime
            )
            .SetTrans(
                Tween.TransitionType.Back
            )
            .SetEase(
                Tween.EaseType.Out
            );
    }


    // =========================================================
    // 顶角修正
    // =========================================================

    private void CorrectCeilingCorner(
        float upwardDistance,
        float direction
    )
    {
        var collision =
            new KinematicCollision2D();


        Vector2 upward =
            new Vector2(
                0f,
                upwardDistance
            );


        if (
            !TestMove(
                GlobalTransform,
                upward,
                collision
            )
            ||
            collision
                .GetNormal()
                .Y < 0.5f
        )
        {
            return;
        }


        int preferredSide =
            direction < 0f
            ? -1
            : 1;


        for (
            int distance = 1;
            distance <= cornerCorrection;
            distance++
        )
        {
            for (
                int side = 0;
                side < 2;
                side++
            )
            {
                Vector2 offset =
                    new Vector2(
                        distance
                        * preferredSide
                        *
                        (
                            side == 0
                            ? 1
                            : -1
                        ),

                        0f
                    );


                Transform2D shifted =
                    GlobalTransform;


                shifted.Origin +=
                    offset;


                if (
                    !TestMove(
                        GlobalTransform,
                        offset
                    )
                    &&
                    !TestMove(
                        shifted,
                        upward
                    )
                )
                {
                    GlobalPosition +=
                        offset;

                    return;
                }
            }
        }
    }


    // =========================================================
    // 死亡
    // =========================================================

    private async void Die()
    {
        if (isDying)
            return;


        isDying = true;


        SetPhysicsProcess(false);

        Velocity =
            Vector2.Zero;


        // =====================================================
        // 死亡后停止所有玩家环境音
        // =====================================================

        StopSound(
            footstepAudio
        );

        StopSound(
            clockAudio
        );

        StopSound(
            leaveLightAudio
        );

        StopSound(
            enterLightAudio
        );

        StopSound(
            landingAudio
        );


        // =====================================================
        // 死亡动画
        // =====================================================

        anim.Pause();


        AnimationPlayer animationPlayer =
            GetNode<AnimationPlayer>(
                "AnimationPlayer"
            );


        GpuParticles2D particles =
            GetNode<GpuParticles2D>(
                "DeathParticles"
            );


        animationPlayer.Play(
            "Dissolve"
        );


        particles.Restart();

        particles.Emitting =
            true;


        await ToSignal(
            animationPlayer,
            AnimationPlayer.SignalName
                .AnimationFinished
        );


        TransitionManager
            .Instance
            .ReloadCurrentScene();
    }


    // =========================================================
    // HUD生命比例
    // =========================================================

    public float LifeRatio
    {
        get
        {
            if (
                normalSpeed <= 0f ||
                normalJumpHeight <= 0f
            )
            {
                return 0f;
            }


            float speedRatio =
                speed
                /
                normalSpeed;


            float jumpRatio =
                jumpHeight
                /
                normalJumpHeight;


            return Mathf.Clamp(
                Mathf.Min(
                    speedRatio,
                    jumpRatio
                ),

                0f,
                1f
            );
        }
    }
}