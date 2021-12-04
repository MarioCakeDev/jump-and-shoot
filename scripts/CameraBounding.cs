using Godot;

namespace JumpAndShoot.scripts
{
	public class CameraBounding : KinematicBody2D
	{
		private Camera2D _camera = default!;

		public override void _Ready()
		{
			this._camera = this.GetParent<Camera2D>();
		}

		public override void _PhysicsProcess(float delta)
		{
			KinematicCollision2D? collision = this.MoveAndCollide(Vector2.Zero);
			if (collision != null)
			{
				this._camera.Position += this.Position;
				this.Position = Vector2.Zero;
			}
		}
	}
}
