using System.Collections.Generic;
using Godot;
using Path = System.IO.Path;

namespace JumpAndShoot.scripts
{
	public class SkinSwitcher : Sprite
	{
		[Export(PropertyHint.Dir)] private string _skinsFolder = "";
		
		private readonly List<Texture> _skins = new();
		private int _currentSkin;

		public override void _Ready()
		{
			this.LoadSkins();
			
			Node? node = this;
			do
			{
				if (node is Node2D node2D)
				{
					this.Scale /= node2D.Scale;
				}

				node = node.GetParent();
			} while (node != null && node.Name != "Player");

			this.Scale *= 4;
		}

		private void LoadSkins()
		{
			Directory skinsDirectory = new();
			Error openError = skinsDirectory.Open(this._skinsFolder);
			if (openError != Error.Ok)
			{
				GD.PrintErr($"Error opening skins directory: {openError}");
				return;
			}
			Error beginError = skinsDirectory.ListDirBegin(true, true);
			if (beginError != Error.Ok)
			{
				GD.PrintErr($"Error listing skins directory: {beginError}");
				return;
			}

			do
			{
				string? fileName = skinsDirectory.GetNext();
				if (string.IsNullOrEmpty(fileName))
				{
					break;
				}

				if (fileName.EndsWith(".png"))
				{
					string filePath = Path.Combine(this._skinsFolder, fileName);
					var texture = GD.Load(filePath) as Texture;

					if (texture is null)
					{
						GD.PrintErr($"Error loading skin: {filePath}");
						continue;
					}
					
					texture.Flags &= ~(uint)Texture.FlagsEnum.Filter;
					
					this._skins.Add(texture);
				}
			} while (true);

			this._currentSkin = this._skins.Count;
		}

		public override void _Process(float delta)
		{
			bool switchForward = Input.IsActionJustPressed("cycleSkin");

			if (switchForward)
			{
				this._currentSkin = (this._currentSkin + 1) % (this._skins.Count + 1);
				this.Texture = this._currentSkin == this._skins.Count ? null : this._skins[this._currentSkin];
			}
			
			bool switchBack = Input.IsActionJustPressed("cycleSkinBackwards");

			if (switchBack)
			{
				this._currentSkin -= 1;
				this._currentSkin += this._skins.Count + 1;
				this._currentSkin %= this._skins.Count + 1;
				this.Texture = this._currentSkin == this._skins.Count ? null : this._skins[this._currentSkin];
			}
		}
	}
}
