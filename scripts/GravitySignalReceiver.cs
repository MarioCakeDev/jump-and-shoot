using System.Collections.Generic;
using Godot;

namespace JumpAndShoot.scripts
{
	public class GravitySignalReceiver : Node
	{
		public float GravityRotation { get; set; }
		private readonly List<IGravityRotatable> _bodies;
		private readonly List<IGravityRotatable> _bodiesToRemove;

		public GravitySignalReceiver()
		{
			this._bodies = new List<IGravityRotatable>();
			this._bodiesToRemove = new List<IGravityRotatable>();
		}

		public override void _Ready()
		{
			Node gravityField = this.GetParent();
			gravityField.Connect("body_entered", this, nameof(this.OnBodyEntered));
			gravityField.Connect("body_exited", this, nameof(this.OnBodyExited));
		}

		public override void _Process(float delta)
		{
			this._bodiesToRemove.Clear();
			foreach (IGravityRotatable gravityRotatable in this._bodies)
			{
				if (IsInstanceValid((Node) gravityRotatable))
				{
					gravityRotatable.ApplyRotation(this.GravityRotation);
				}
				else
				{
					this._bodiesToRemove.Add(gravityRotatable);
				}
			}

			foreach (IGravityRotatable body in this._bodiesToRemove)
			{
				this._bodies.Remove(body);
			}
		}

		public void OnBodyEntered(Node body)
		{
			if (body is IGravityRotatable gravityRotatable)
			{
				this._bodies.Add(gravityRotatable);
			}
		}

		public void OnBodyExited(Node body)
		{
			if (body is IGravityRotatable gravityRotatable)
			{
				gravityRotatable.ApplyRotation(0);
				this._bodies.Remove(gravityRotatable);
			}
		}
	}
}
