using Godot;

namespace JumpAndShoot.scripts
{
	public class Enemy : KinematicBody2D, IShootable
	{
		private bool _walkLeft = true;
		private int _walkSpeed = 100;
		private float _moved = 250;

		private AudioStreamPlayer2D _enemyDeathSound = default!;
		private CollisionShape2D _collisionShape = default!;
		private Sprite _enemySprite = default!;

		public override void _Ready()
		{
			this._enemyDeathSound = this.GetComponent<AudioStreamPlayer2D>();
			this._collisionShape = this.GetComponent<CollisionShape2D>();
			this._enemySprite = this._collisionShape.GetComponent<Sprite>();
		}

		public override void _PhysicsProcess(float delta)
		{
			this._enemySprite.FlipH = this._walkLeft;
			
			this._collisionShape.RotationDegrees = this._walkLeft ? 180 : 0;
			
			this.MoveAndSlideWithSnap(new Vector2(this._walkSpeed, 50), new Vector2(0, -100));
			
			this._moved += Mathf.Abs(this._walkSpeed * delta);
			
			if (this._moved > 500)
			{
				this._moved = 0;
				this._walkLeft = !this._walkLeft;
				this._walkSpeed = -this._walkSpeed;
			}
		}

		public void OnShot()
		{
			this._enemyDeathSound.Play();
			this.QueueFree();
		}
	}
}
