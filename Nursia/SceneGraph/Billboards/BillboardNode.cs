using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Attributes;
using System.ComponentModel;

namespace Nursia.SceneGraph.Billboards
{
	/// <summary>
	/// A billboard node that renders a texture as a quad facing the camera.
	/// </summary>
	[EditorInfo("Billboards")]
	public class BillboardNode : BillboardNodeBase
	{
		/// <summary>
		/// Gets or sets the texture to render on this billboard.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		public Texture2D Texture { get; set; }

		/// <summary>
		/// Gets the texture to render.
		/// </summary>
		protected override Texture2D RenderTexture => Texture;
	}
}