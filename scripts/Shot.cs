using System;
using System.Threading.Tasks;
using Godot;

namespace JumpAndShoot.scripts
{
	public class Shot : KinematicBody2D, IGravityRotatable
	{
		private const float Speed = 1000;
		private bool _destroyed;
		private Vector2 _velocity;
		private CollisionShape2D _collisionShape = default!;
		private Particles2D _particles = default!;
		private AudioStreamPlayer2D _shotSound = default!;
		private AudioStreamPlayer2D _shotImpact = default!;
		
		public override void _Ready()
		{
			this._collisionShape = this.GetComponent<CollisionShape2D>();
			this._particles = this.GetComponent<Particles2D>();
			this._particles.Visible = false;
			this._shotSound = this.GetComponent<AudioStreamPlayer2D>("ShotSound");
			this._shotImpact = this.GetComponent<AudioStreamPlayer2D>("ShotImpact");
		}
		
		public override void _PhysicsProcess(float delta)
		{
			KinematicCollision2D? collision = this.MoveAndCollide(this._velocity * delta);
			if (collision == null)
			{
				return;
			}

			if (collision.Collider is IShootable shootable)
			{
				shootable.OnShot();
			}

			this._shotImpact.Play();
			this.Destroy();
		}
		
		public void Fly()
		{
			this._shotSound.Play();
			this._velocity = new Vector2(Mathf.Cos(this.Rotation), Mathf.Sin(this.Rotation)) * Speed;
			
			RunAfterTimeout(TimeSpan.FromSeconds(2), this.Destroy);
		}

		private void Destroy()
		{
			if (this._destroyed)
			{
				return;
			}

			this._destroyed = true;
			this.SetProcess(false);
			this.SetPhysicsProcess(false);
			this._collisionShape.Visible = false;
			this._collisionShape.Disabled = true;
			this._particles.Visible = true;
			
			RunAfterTimeout(TimeSpan.FromMilliseconds(150), this.ParticlesVisible);
		}

		private static void RunAfterTimeout(TimeSpan timeout, Action action)
		{
			Task.Run(async () =>
			{
				await Task.Delay(timeout);
				action();
			});
		}

		private void ParticlesVisible()
		{
			this._particles.Visible = false;
			RunAfterTimeout(TimeSpan.FromMilliseconds(150), this.QueueFree);
		}
		
		public void ApplyRotation(float targetRotation)
		{
			this.Rotation = Mathf.LerpAngle(this.Rotation, targetRotation + Mathf.Pi / 2, 0.1f);
			this._velocity = new Vector2(Mathf.Cos(this.Rotation), Mathf.Sin(this.Rotation)) * Speed;
		}
	}
}

