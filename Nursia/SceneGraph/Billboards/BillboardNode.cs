using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Attributes;
using System.ComponentModel;

namespace Nursia.SceneGraph.Billboards
{
	[EditorInfo("Billboards")]
	public class BillboardNode : BillboardNodeBase
	{
		[Browsable(false)]
		[JsonIgnore]
		public Texture2D Texture { get; set; }

		protected override Texture2D RenderTexture => Texture;
	}
}