using Godot;

namespace JumpAndShoot.scripts
{
	public class Box : KinematicBody2D, IShootable
	{
		private int _life = 4;
		private const int MaxLives = 4;
		private Sprite _sprite = default!;
		private Node2D _box = default!;

		private Vector2 _velocity = Vector2.Zero;

		public override void _Ready()
		{
			this._sprite = this.GetComponent<Sprite>();
			this._box = this.GetParent<Node2D>();
		}

		public override void _PhysicsProcess(float delta)
		{
			if (!this.IsOnFloor())
			{
				this._velocity.y += 40;
			}

			this._velocity = this.MoveAndSlideWithSnap(this._velocity, Vector2.Down * 5, Vector2.Up, true, 2, 0.785398f, false);
		}

		public void OnShot()
		{
			this._life -= 1;

			if (this._life == 0)
			{
				this._box.QueueFree();
				return;
			}

			this._sprite.Frame = MaxLives - this._life;
		}
	}
}
