using Godot;

namespace JumpAndShoot.scripts
{
    public class SkinSwitcher : Sprite
    {
        public override void _Process(float delta)
        {
            bool switchForward = Input.IsActionJustPressed("cycleSkin");
            bool switchBack = Input.IsActionJustPressed("cycleSkinBackwards");

            if (switchBack)
            {
                this.Frame = (this.Hframes * this.Vframes + this.Frame - 1) % (this.Hframes * this.Vframes);
            }
            
            if (switchForward)
            {
                this.Frame = (this.Frame + 1) % (this.Hframes * this.Vframes);
            }
            
        }
    }
}
