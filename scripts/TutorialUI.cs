using Godot;
using System.Collections.Generic;

public partial class TutorialUI : CanvasLayer
{
	[ExportGroup("节点")]
	[Export] public Control root;
	[Export] public RichTextLabel textLabel;
	[Export] public Control continueHint;

	[ExportGroup("打字机")]
	[Export] public float charactersPerSecond = 35f;

	private List<string> messages = new();

	private int currentIndex = 0;

	private float characterCounter = 0f;

	private bool active = false;
	private bool textFinished = false;

	// 用于恢复教程出现之前的暂停状态
	private bool previousPausedState = false;


	public override void _Ready()
	{
		// 非常重要：
		// 游戏暂停以后这个 UI 仍然必须继续运行
		ProcessMode = ProcessModeEnum.Always;

		root.Visible = false;
		continueHint.Visible = false;
	}


	// =========================================================
	// 外部调用这个函数开始教程
	// =========================================================

	public bool ShowMessages(IEnumerable<string> newMessages)
	{
		// 正在播放教程就不重复触发
		if (active)
			return false;

		messages = new List<string>(newMessages);

		if (messages.Count == 0)
			return false;

		active = true;
		currentIndex = 0;

		// 记录原本是不是暂停状态
		previousPausedState = GetTree().Paused;

		// 暂停整个游戏
		GetTree().Paused = true;

		root.Visible = true;

		StartCurrentMessage();
		return true;
	}


	// =========================================================
	// 开始显示当前这一句话
	// =========================================================

	private void StartCurrentMessage()
	{
		textLabel.Text = messages[currentIndex];

		textLabel.VisibleCharacters = 0;

		characterCounter = 0f;

		textFinished = false;

		continueHint.Visible = false;
	}


	// =========================================================
	// 打字机
	// =========================================================

	public override void _Process(double delta)
	{
		if (!active || textFinished)
			return;

		characterCounter += charactersPerSecond * (float)delta;

		int totalCharacters =
			textLabel.GetTotalCharacterCount();

		int visibleCharacters =
			Mathf.Min(
				(int)characterCounter,
				totalCharacters
			);

		textLabel.VisibleCharacters = visibleCharacters;

		// 当前文字全部打完
		if (visibleCharacters >= totalCharacters)
		{
			textFinished = true;

			continueHint.Visible = true;
		}
	}


	// =========================================================
	// 任意键继续
	// =========================================================

	public override void _Input(InputEvent @event)
	{
		if (!active)
			return;

		bool pressed = false;

		// 键盘任意键
		if (@event is InputEventKey key)
		{
			if (key.Pressed && !key.Echo)
				pressed = true;
		}

		// 鼠标任意按键
		else if (@event is InputEventMouseButton mouse)
		{
			if (mouse.Pressed)
				pressed = true;
		}

		// 手柄按钮，顺手也支持
		else if (@event is InputEventJoypadButton joypad)
		{
			if (joypad.Pressed)
				pressed = true;
		}

		if (!pressed)
			return;

		// 防止这次输入继续传给暂停菜单等其他东西
		GetViewport().SetInputAsHandled();

		// 文字还没打完，不翻页
		if (!textFinished)
			return;

		NextMessage();
	}

	// =========================================================
	// 下一句
	// =========================================================

	private void NextMessage()
	{
		currentIndex++;

		// 还有下一句
		if (currentIndex < messages.Count)
		{
			StartCurrentMessage();
			return;
		}

		// 全部结束
		FinishTutorial();
	}


	// =========================================================
	// 教程结束
	// =========================================================

	private void FinishTutorial()
	{
		active = false;

		root.Visible = false;
		continueHint.Visible = false;

		// 恢复教程出现之前的暂停状态
		GetTree().Paused = previousPausedState;
	}
}