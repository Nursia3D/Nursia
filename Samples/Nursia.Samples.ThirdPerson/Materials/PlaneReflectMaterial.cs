using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Materials;
using Nursia.Rendering;
using System.ComponentModel;

namespace Nursia.Samples.ThirdPerson.Materials
{
	internal class PlaneReflectMaterial : IMaterial
	{
		private static EffectBinding _binding;

		private MaterialFlags _flags = MaterialFlags.None;
		public string Id { get; set; }

		public BlendState BlendState => null;
		public DepthStencilState DepthStencilState => null;
		public RasterizerState RasterizerState => null;

		public MaterialFlags Flags => _flags;

		[DefaultValue(true)]
		public bool CastsShadows
		{
			get => _flags.HasFlag(MaterialFlags.CastsShadows);

			set
			{
				if (value)
				{
					_flags |= MaterialFlags.CastsShadows;
				}
				else
				{
					_flags &= ~MaterialFlags.CastsShadows;
				}
			}
		}

		public Color DiffuseColor { get; set; } = Color.White;

		public IMaterial Clone()
		{
			return new PlaneReflectMaterial()
			{
				Id = Id,
				CastsShadows = CastsShadows,
				DiffuseColor = DiffuseColor,
			};
		}

		public EffectBinding GetShadowTechnique(MaterialTechnique materialTechnique)
		{
			throw new System.NotImplementedException();
		}

		public EffectBinding GetColorTechnique(MaterialTechnique materialTechnique, LightTechnique lightTechnique, bool shadow, bool translucent, bool clipPlane)
		{
			if (_binding != null)
			{
				return _binding;
			}

			var effectSource = Effects.GetEffectSource("ReflectionPlane");
			var effect = new Effect(Nrs.GraphicsDevice, effectSource);
			_binding = new EffectBinding(effect);

			_binding.AddMaterialLevelSetter<PlaneReflectMaterial>("cMatDiffColor", (m, p) => p.SetValue(m.DiffuseColor.ToVector4()));

			return _binding;
		}
	}
}
