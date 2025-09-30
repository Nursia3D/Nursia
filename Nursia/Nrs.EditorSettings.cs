using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace Nursia
{
	partial class Nrs
	{
		public class EditorSettingsType
		{
			public float TerrainMarkerRadius { get; set; } = 10.0f;

			[JsonIgnore]
			[Browsable(false)]
			public Vector3? TerrainMarkerPosition { get; set; }


			internal EditorSettingsType()
			{
			}
		}

		private static EditorSettingsType _EditorSettingsInstance = new EditorSettingsType();

		public static EditorSettingsType EditorSettings
		{
			get => _EditorSettingsInstance;

			set
			{
				_EditorSettingsInstance = value ?? throw new ArgumentNullException(nameof(value));
			}
		}
	}
}
