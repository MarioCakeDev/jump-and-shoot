using Godot;

namespace JumpAndShoot.scripts
{
	public class GravityField : Area2D
	{
		private Sprite _gravityArrow = default!;
		private GravitySignalReceiver _signalReceiver = default!;
		
		public override void _Ready()
		{
			this._gravityArrow = this.GetComponent<Sprite>();
			this._signalReceiver = this.GetComponent<GravitySignalReceiver>();
			
			this.ApplyDirection();
			this._Process(0);
		}
		
		private void ApplyDirection()
		{
			if (!this.HasMeta("Direction"))
			{
				return;
			}

			var direction = this.GetMeta("Direction").ToString();

			const float quarterTurn = Mathf.Pi / 2f;
			this._signalReceiver.GravityRotation = direction switch
			{
				"Down"  => 0f * quarterTurn,
				"Left"  => 1f * quarterTurn,
				"Up"    => 2f * quarterTurn,
				"Right" => 3f * quarterTurn,
				_ => this._signalReceiver.GravityRotation
			};

			this._gravityArrow.Rotation = this._signalReceiver.GravityRotation;
		}
		
		public override void _Process(float delta)
		{
			Rect2 gravityArrowRegionRect = this._gravityArrow.RegionRect;
			gravityArrowRegionRect.Position += Vector2.Up * 0.2f;
			
			this._gravityArrow.RegionRect = gravityArrowRegionRect;
		}
	}
}
