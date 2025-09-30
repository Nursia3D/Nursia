using System;
using System.IO;
using Microsoft.Xna.Framework;
using Nursia.Utilities;

namespace Nursia.Editor
{
	public class State
	{
		public const string StateFileName = "Project.nursia";

		public static string StateFilePath
		{
			get
			{
				var result = Path.Combine(Configuration.ProjectFolder, StateFileName);
				return result;
			}
		}

		public Point Size { get; set; }
		public float TopSplitterPosition { get; set; }
		public float LeftSplitterPosition { get; set; }
		public string[] OpenFiles { get; set; }
		public bool ShowGrid { get; set; }
		public bool DrawBoundingBoxes { get; set; }
		public bool DrawLightViewFrustum { get; set; }
		public bool DrawShadowMap { get; set; }
		public bool ShowCameraPosition { get; set; }
		public Nrs.GraphicsSettingsType GraphicsSettings { get; set; }
		public Nrs.DebugSettingsType DebugSettings { get; set; }

		public State()
		{
		}

		public void Save() => JsonExtensions.SerializeToFile(StateFilePath, this);

		public static State Load()
		{
			if (!File.Exists(StateFilePath))
			{
				return null;
			}

			try
			{
				return JsonExtensions.DeserializeFromFile<State>(StateFilePath);
			}
			catch(Exception ex)
			{
				Nrs.LogError(ex.Message);
				return null;
			}
		}

		public override string ToString()
		{
			return string.Format("Size = {0}\n" +
								 "TopSplitter = {1:0.##}\n" +
								 "LeftSplitter= {2:0.##}\n" +
								 "OpenFiles = {3}",
				Size,
				TopSplitterPosition,
				LeftSplitterPosition,
				OpenFiles);
		}
	}
}