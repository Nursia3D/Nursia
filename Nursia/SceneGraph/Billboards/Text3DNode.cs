using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Attributes;
using System.ComponentModel;

namespace Nursia.SceneGraph.Billboards
{
	/// <summary>
	/// A billboard node that renders text in 3D space, always facing the camera.
	/// </summary>
	[EditorInfo("Billboards")]
	public class Text3DNode : BillboardNodeBase
	{
		private Texture2D _texture = null;
		private string _text;
		private SpriteFont _font = null;

		/// <summary>
		/// Gets the texture generated from the text.
		/// </summary>
		protected override Texture2D RenderTexture
		{
			get
			{
				UpdateTexture();

				return _texture;
			}
		}

		/// <summary>
		/// Gets or sets the text to display on this billboard.
		/// </summary>
		public string Text
		{
			get => _text;

			set
			{
				if (value == _text)
				{
					return;
				}

				_text = value;
				Invalidate();
			}
		}

		[Browsable(false)]
		public string FontPath { get; set; }


		[JsonIgnore]
		public SpriteFont Font
		{
			get => _font;

			set
			{
				_font = value;
				Invalidate();
			}
		}

		private void UpdateTexture()
		{
			if (_texture != null || string.IsNullOrEmpty(_text))
			{
				return;
			}

			var font = _font ?? Resources.DebugFont;
			var size = font.MeasureString(_text);

			Width = size.X / 64.0f;
			Height = size.Y / 64.0f;

			var device = Nrs.GraphicsDevice;
			var target = new RenderTarget2D(device, (int)size.X, (int)size.Y);

			var oldViewport = device.Viewport;
			try
			{
				device.SetRenderTarget(target);

				device.Clear(Color.Transparent);
				var spriteBatch = Resources.SpriteBatch;
				spriteBatch.Begin();
				spriteBatch.DrawString(font, _text, Vector2.Zero, Color.White);
				spriteBatch.End();
			}
			finally
			{
				device.SetRenderTarget(null);
				device.Viewport = oldViewport;
			}

			_texture = target;

			/*			using (var stream = File.OpenWrite(@$"D:\Temp\test_{Text}.png"))
						{
							_texture.SaveAsPng(stream, _texture.Width, _texture.Height);
						}*/
		}

		private void Invalidate()
		{
			_texture = null;
		}

		protected override SceneNode CreateInstanceCore() => new Text3DNode();

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var text = (Text3DNode)node;

			Text = text.Text;
			FontPath = text.FontPath;
			Font = text.Font;
		}

		public override void Load(AssetManager assetManager)
		{
			base.Load(assetManager);

			if (!string.IsNullOrEmpty(FontPath))
			{
				Font = assetManager.LoadSpriteFont(Nrs.GraphicsDevice, FontPath);
			}
			else
			{
				Font = Resources.DebugFont;
			}
		}
	}
}
