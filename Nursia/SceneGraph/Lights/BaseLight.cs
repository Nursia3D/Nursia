using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Nursia.Materials;
using System.ComponentModel;

namespace Nursia.SceneGraph.Lights
{
	public enum ShadowType
	{
		None,
		Simple,
		PCF
	}

	/// <summary>
	/// Base Light
	/// </summary>
	public abstract class BaseLight : SceneNode
	{
		private bool _castsShadow = true;

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

		[Category("Light")]
		public Color Color { get; set; } = Color.White;

		[Category("Light")]
		[DefaultValue(false)]
		public bool Translucent { get; set; } = false;

		/// <summary>
		/// Used by renderer
		/// </summary>
		[Browsable(false)]
		[JsonIgnore]
		internal int? ShadowMapIndex { get; set; }

		protected BaseLight()
		{
		}

		protected virtual void OnCastsShadowChanged()
		{
		}

		protected override void CopyFrom(SceneNode node)
		{
			base.CopyFrom(node);

			var light = (BaseLight)node;
			CastsShadow = light.CastsShadow;
			Color = light.Color;
			Translucent = light.Translucent;
		}

		/// <summary>
		/// Determines whether the light affects specified bounding box in the world coordinates
		/// </summary>
		/// <param name="boundingBox"></param>
		/// <returns></returns>
		public abstract bool AffectsObject(BoundingBox boundingBox);
	}

	public static class BaseLightExtensions
	{
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
