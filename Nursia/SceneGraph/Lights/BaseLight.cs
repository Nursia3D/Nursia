using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Materials;
using System.ComponentModel;

namespace Nursia.SceneGraph.Lights
{
	/// <summary>
	/// Base class for all light types in a Nursia scene.
	/// </summary>
	public abstract class BaseLight : SceneNode
	{
		private bool _castsShadow = true;

		/// <summary>
		/// Gets or sets a value indicating whether this light casts shadows.
		/// </summary>
		[Category("Shadows")]
		[DefaultValue(true)]
		public bool CastsShadow
		{
			get => _castsShadow;

			set
			{
				if (value == _castsShadow)
				{
					return;
				}

				_castsShadow = value;
				OnCastsShadowChanged();
			}
		}

		/// <summary>
		/// Gets or sets the color of this light.
		/// </summary>
		[Category("Light")]
		public Color Color { get; set; } = Color.White;

		/// <summary>
		/// Gets or sets a value indicating whether this light is rendered as translucent.
		/// </summary>
		[Category("Light")]
		[DefaultValue(false)]
		public bool Translucent { get; set; } = false;

		/// <summary>
		/// Gets or sets the shadow map index used by the renderer.
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		internal int? ShadowMapIndex { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BaseLight"/> class.
		/// </summary>
		protected BaseLight()
		{
		}

		/// <summary>
		/// Called when the CastsShadow property changes.
		/// </summary>
		protected virtual void OnCastsShadowChanged()
		{
		}

		/// <summary>
		/// Copies light properties from another node.
		/// </summary>
		/// <param name="node">The source node to copy from.</param>
		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var light = (BaseLight)node;
			CastsShadow = light.CastsShadow;
			Color = light.Color;
			Translucent = light.Translucent;
		}

		/// <summary>
		/// Determines whether this light affects the specified bounding box in world coordinates.
		/// </summary>
		/// <param name="boundingBox">The bounding box to test.</param>
		/// <returns><c>true</c> if this light affects the object; otherwise, <c>false</c>.</returns>
		public abstract bool AffectsObject(BoundingBox boundingBox);
	}

	/// <summary>
	/// Extension methods for BaseLight.
	/// </summary>
	public static class BaseLightExtensions
	{
		/// <summary>
		/// Gets the light technique used by this light.
		/// </summary>
		/// <param name="light">The light to get the technique for.</param>
		/// <returns>The appropriate LightTechnique for this light type.</returns>
		public static LightTechnique GetTechnique(this BaseLight light)
		{
			var asDirect = light as DirectLight;
			if (asDirect != null)
			{
				return LightTechnique.Direct;
			}

			var asPoint = light as PointLight;
			if (asPoint != null)
			{
				return LightTechnique.Point;
			}

			return LightTechnique.Spot;
		}
	}
}
