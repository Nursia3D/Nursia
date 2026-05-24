using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace Nursia
{
	partial class Nrs
	{
		/// <summary>
		/// Provides editor-related settings for the Nursia engine.
		/// </summary>
		public class EditorSettingsType
		{
			/// <summary>
			/// Gets or sets the radius of the terrain marker used in the editor.
			/// </summary>
			public float TerrainMarkerRadius { get; set; } = 10.0f;

			/// <summary>
			/// Gets or sets the position of the terrain marker in the editor.
			/// </summary>
			[JsonIgnore]
			[Browsable(false)]
			public Vector3? TerrainMarkerPosition { get; set; }

			/// <summary>
			/// Initializes a new instance of the <see cref="EditorSettingsType"/> class.
			/// </summary>
			internal EditorSettingsType()
			{
			}
		}

		private static EditorSettingsType _EditorSettingsInstance = new EditorSettingsType();

		/// <summary>
		/// Gets or sets the current editor settings for the engine.
		/// </summary>
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
