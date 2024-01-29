using Godot;

public partial class AimLook : Node
{
	[ExportGroup("Nodes")] 
	[Export] public CharacterBody3D Character { get; set; }
	[Export] public Node3D Head { get; set; }
	
	[ExportGroup("Settings")]
	[ExportSubgroup("Mouse Settings")]
	[Export(PropertyHint.Range, "1, 100, 1")] public int MouseSensitivity { get; set; } = 50;
	
	[ExportSubgroup("Clamp Settings")]
	[Export] public float MaxPitch { get; set; } = 89f;
	[Export] public float MinPitch { get; set; } = -89f;

	private float _pitch;
	private float _pitchRad;
	private Vector3 _headRotation;
	private bool _isProperlySetup;

	public override void _Ready()
	{
		_isProperlySetup = true;
		
		if (Character == null)
		{
			GD.PrintErr("Character is null! The CharacterBody3D Node is not assigned on the AimLook Node");
			_isProperlySetup = false;
		}
		if (Head == null)
		{
			GD.PrintErr("Head is null! The Node3D Node is not assigned on the AimLook Node");
			_isProperlySetup = false;
		}

		if (!_isProperlySetup) return;
		_isProperlySetup = true;
		Input.UseAccumulatedInput = false;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (!_isProperlySetup) return;
		
		if (@event is InputEventKey inputEventKey)
		{
			if (inputEventKey.IsActionPressed("ui_cancel"))
			{
				if (Input.MouseMode == Input.MouseModeEnum.Captured)
				{
					Input.MouseMode = Input.MouseModeEnum.Visible;
				}
				else
				{
					GetTree().Quit();
				}
			}
		}
		else if (Input.MouseMode != Input.MouseModeEnum.Captured)
		{
			if (@event is InputEventMouseButton inputEventMouseButton)
			{
				if (inputEventMouseButton.ButtonIndex == MouseButton.Left && inputEventMouseButton.Pressed)
				{
					Input.MouseMode = Input.MouseModeEnum.Captured;
				}
			}
		}
    
		if (@event is InputEventMouseMotion inputEventMouseMotion)
		{
			HandleMouseMotion(inputEventMouseMotion);
		}
	}
	
	private void HandleMouseMotion(InputEventMouseMotion inputEventMouseMotion)
	{
		Transform2D viewportTransform = GetTree().Root.GetFinalTransform();
		Vector2 mouseMotion = ((InputEventMouseMotion)inputEventMouseMotion.XformedBy(viewportTransform)).Relative;
		const float degreesPerUnit = 0.001f;

		mouseMotion *= MouseSensitivity;
		mouseMotion *= degreesPerUnit;
		
		AddYaw(mouseMotion.X);
		AddPitch(mouseMotion.Y);
	}
	
	private void AddYaw(float amount)
	{
		if(Mathf.IsZeroApprox(amount)) return;
		Character.RotateObjectLocal(Vector3.Down, Mathf.DegToRad(amount));
		Character.Orthonormalize();
	}
	
	private void AddPitch(float amount)
	{
		if(Mathf.IsZeroApprox(amount)) return;
		_pitch += amount;
		_pitch = Mathf.Clamp(_pitch, MinPitch, MaxPitch);
		_pitchRad = Mathf.DegToRad(_pitch);
		_headRotation.X = -_pitchRad;
		Head.Rotation = _headRotation;
		Head.Orthonormalize();
	}
}