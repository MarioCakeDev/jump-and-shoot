using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;

namespace JumpAndShoot.scripts
{
	public class Player : KinematicBody2D, IGravityRotatable
	{
		private static readonly PackedScene ShotScene = ResourceLoader.Load<PackedScene>("res://prefabs/Shot.tscn");
		
		[Export] private float _speed = 750;
		
		private Vector2 _startPosition;
		private Camera2D _camera = default!;
		private Node2D _rotationSprite = default!;
		private Vector2 _viewportSize;
		private Viewport _viewport = default!;
		private AudioStreamPlayer2D _jumpSound = default!;
		private Vector2 _velocity;
		
		private Sprite _mainSprite = default!;
		private Sprite _skinSprite = default!;
		
		private readonly Stopwatch _backupJumpTimer = new(150);
		private readonly Stopwatch _jumpTimer = new(200);
		private readonly Stopwatch _shootTimer = new(175);
		
		private bool _isOnGround;
		private bool _isOnCeiling;
		
		private Vector2 _snapVector = Vector2.Down * 10;
		
		private bool _previouslyOnGround;

		private const float JumpStrength = 900;
		private const float HalfJumpStrength = 300;

		private float _gravityRotation;
		
		private bool _canRun = true;
		private bool _canJump = true;
		private bool _hasFullJump = true;
		private bool _canShoot = true;
		private bool _canPropel = true;
		
		private Vector2 _upVector = Vector2.Up;

		private readonly List<object> _actionsPressed = new();
		private readonly Tween _tween = new();
		
		private bool _moving;
		private bool _keepMoveDirection;
		private bool _movedBefore;

		public override void _Ready()
		{
			this._startPosition = this.Position;
			this._camera = this.GetComponent<Camera2D>();
			this._rotationSprite = this.GetComponent<Node2D>("SpriteParent");
			this._viewportSize = this.GetViewportRect().Size;
			this._viewport = this.GetViewport();
			this._jumpSound = this.GetComponent<AudioStreamPlayer2D>("JumpSound");
			
			this._mainSprite = this.GetComponent<Sprite>("Sprite");
			this._skinSprite = this.GetComponent<Sprite>("Skin");
			
			this.AddChild(this._tween);
		}

		public override void _PhysicsProcess(float _)
		{
			this.RotateVelocityAsDown();

			this.ActOnReleased(this.OnLeft);
			this.ActOnReleased(this.OnRight);

			this._velocity.x *= 0.8f;

			this._moving = false;
			
			this.ActOnPressed(this.OnLeft, this.OnRight);
			this.ActOnPressed(this.OnRight, this.OnLeft);

			this._movedBefore = this._moving;
			this._keepMoveDirection &= this._movedBefore;

			this.RotateVelocityToDirection();

			this.MoveAndSlideWithSnap(this._velocity, this._snapVector, this._upVector, true, 20, infiniteInertia: false);

			this._isOnGround = this.IsOnFloor();
			this._isOnCeiling = this.IsOnCeiling();
			
			this.RotateVelocityAsDown();
			
			this.HandleDebugInput();
			
			this.HandleJumping();
			this.HandleShooting();

			this._velocity.y = CapAbsolute(this._velocity.y, this._hasFullJump ? -1500 : -300, 1500);
			
			Vector2 mousePositionRelativeToCenter = (this.GetViewport().GetMousePosition() - this._viewportSize / 2).Rotated(this._gravityRotation);
			this._camera.Position = this._camera.Position.LinearInterpolate(mousePositionRelativeToCenter / 3, 0.05f);
			this._rotationSprite.RotationDegrees = Map(this._velocity.x, -this._speed, this._speed, -20, 20) + Mathf.Rad2Deg(this._gravityRotation);

			this.RotateVelocityToDirection();
		}

		private void ActOnPressed(Action action, Action reverseAction)
		{
			string? actionName = GetActionName(action);
			if (!Input.IsActionPressed(actionName))
			{
				return;
			}

			if (!this._actionsPressed.Contains(action))
			{
				this._actionsPressed.Add(action);
				action();
				return;
			}

			int againstActionPosition = this._actionsPressed.IndexOf(reverseAction);
			int actionPosition = this._actionsPressed.IndexOf(action);
			if (againstActionPosition == -1 || actionPosition > againstActionPosition)
			{
				action();
			}
		}

		private void ActOnReleased(Action action)
		{
			string? actionName = GetActionName(action);
			if (string.IsNullOrEmpty(actionName) || !Input.IsActionJustReleased(actionName))
			{
				return;
			}

			if (this._actionsPressed.Contains(action))
			{
				this._actionsPressed.Remove(action);
			}
		}

		private static string? GetActionName(Action action)
		{
			var actionAttribute = action.Method.GetCustomAttribute(typeof(InputActionAttribute)) as InputActionAttribute;
			return actionAttribute?.ActionName;
		}

		[InputAction("left")]
		private void OnLeft()
		{
			this.Run(-this.GetRunSpeed());
		}

		[InputAction("right")]
		private void OnRight()
		{
			this.Run(this.GetRunSpeed());
		}

		private void Run(float runSpeed)
		{
			this._mainSprite.FlipH = runSpeed < 0;
			this._skinSprite.FlipH = runSpeed < 0;
			
			this._moving = true;

			if (this._keepMoveDirection && this._movedBefore)
			{
				runSpeed = -runSpeed;
			}

			this._velocity.x = Mathf.Lerp(this._velocity.x / 0.8f, runSpeed, 0.3f);
		}

		private float GetRunSpeed()
		{
			return this._canRun ? this._speed : this._speed / 5.0f;
		}

		private static float CapAbsolute(float value, float lower, float upper)
		{
			return Mathf.Max(lower, Mathf.Min(value, upper));
		}

		private static float Map(float currentValue, float lowerOriginal, float upperOriginal, float targetLower, float targetUpper)
		{
			float maxDistance = upperOriginal - lowerOriginal;
			float currentDistance = currentValue - lowerOriginal;
			float percentileDistance = currentDistance / maxDistance;
			float resultDistance = targetUpper - targetLower;
			return percentileDistance * resultDistance + targetLower;
		}

		private void RotateVelocityAsDown()
		{
			if (this._velocity.LengthSquared() != 0)
			{
				this._velocity = this._velocity.Rotated(-this._gravityRotation);
			}
		}

		private void RotateVelocityToDirection()
		{
			if (this._velocity.LengthSquared() != 0)
			{
				this._velocity = this._velocity.Rotated(this._gravityRotation);
			}
		}

		private void Reset()
		{
			this.Position = this._startPosition;
			this._velocity = Vector2.Zero;
		}

		private void HandleJumping()
		{
			if (this._isOnGround)
			{
				this._velocity.y = 0;
			}

			if (this._isOnCeiling && this._velocity.y < 0)
			{
				this._velocity.y = 0;
			}

			bool isJumping = Input.IsActionPressed("jump");

			bool isOnGroundOrBackupJump = this._isOnGround || !this._backupJumpTimer.IsRunCooldown();

			if (isOnGroundOrBackupJump && isJumping && this._canJump && this._jumpTimer.IsRunCooldown())
			{
				this._jumpTimer.Reset();
				this._backupJumpTimer.Reset();
				this._previouslyOnGround = false;
				this._isOnGround = false;
				this._snapVector = Vector2.Zero;
				this._velocity.y = this._hasFullJump ? -JumpStrength : HalfJumpStrength;
				this._jumpSound.Play();
			}
			else if (isJumping && this._velocity.y < 0)
			{
				this._velocity.y += 25;
			}
			else
			{
				this._velocity.y += 45;
			}

			if (this._previouslyOnGround && !this._isOnGround)
			{
				this._backupJumpTimer.Reset();
			}

			this._previouslyOnGround = this._isOnGround;
		}

		private void HandleDebugInput()
		{
			if (Input.IsActionJustPressed("rotate"))
			{
				this.ApplyRotation(this._gravityRotation + Mathf.Pi / 2);
			}
			else if(Input.IsActionJustPressed("toggleJump"))
			{
				this._canJump = !this._canJump;
			}

			if (Input.IsActionJustPressed("toggleJumpHeight"))
			{
				this._hasFullJump = !this._hasFullJump;
			}

			if (Input.IsActionJustPressed("toggleShoot"))
			{
				this._canShoot = !this._canShoot;
			}

			if (Input.IsActionJustPressed("togglePropell"))
			{
				this._canPropel = !this._canPropel;
			}

			if (Input.IsActionJustPressed("toggleRunSpeed"))
			{
				this._canRun = !this._canRun;
			}

			if (Input.IsActionJustPressed("reset"))
			{
				this.Reset();
			}
		}

		public void ApplyRotation(float targetRotation)
		{
			float rotationDifference = Mathf.Abs(this._gravityRotation - targetRotation);
			if (rotationDifference < 0.001f)
			{
				return;
			}

			if (Mathf.Abs(rotationDifference - Mathf.Pi) <= 0.01f)
			{
				this._keepMoveDirection = !this._keepMoveDirection;
			}

			this._gravityRotation = targetRotation;
			if(this._gravityRotation >= Mathf.Tau)
			{
				this._gravityRotation -= Mathf.Tau;
			}

			this._upVector = Vector2.Up.Rotated(this._gravityRotation);

			const float tweenDuration = 0.25f;
			this._tween.InterpolateProperty(this._camera, "rotation", this._camera.Rotation, this._gravityRotation, tweenDuration, Tween.TransitionType.Circ);
			this._tween.Start();
		}

		private void HandleShooting()
		{
			if (Input.IsMouseButtonPressed((int) ButtonList.Left))
			{
				this.Shoot(this._viewport.GetMousePosition());
			}
		}

		private void Shoot(Vector2 shootPosition)
		{
			if(!this._canShoot)
			{
				return;
			}

			if (!this._shootTimer.IsRunCooldown())
			{
				return;
			}

			this._shootTimer.Reset();

			Vector2 shootDirection = shootPosition - this.GetGlobalTransformWithCanvas().origin;
			shootDirection = shootDirection.Normalized();

			var shot = ShotScene.Instance<Shot>();

			shot.Rotation = shootDirection.Angle() + this._gravityRotation;
			shot.Position = this.Position;
			
			if(this._canPropel)
			{
				this._velocity -= shootDirection * new Vector2(100, this._velocity.y > 0 ? 225 : 100);
			}
			
			this.GetParent().AddChild(shot);
			shot.Fly();
		}
	}
}
