using AssetManagementBase;
using System;
using System.IO;

namespace Nursia.Editor
{
	class Program
	{
		static void Main(string[] args)
		{
			Environment.SetEnvironmentVariable("FNA3D_FORCE_DRIVER", "D3D11");

			AMBConfiguration.Logger = Console.WriteLine;
			var path = string.Empty;
			foreach(var arg in args)
			{
				if (arg == "/nf")
				{
					Configuration.NoFixedStep = true;
				} else if (!arg.StartsWith("/"))
				{
					path = arg;
				}
			}

			if (string.IsNullOrEmpty(path))
			{
				Console.WriteLine("Usage: nrs-editor <folder>");
				return;
			}

			try
			{
				if (path == ".")
				{
					path = Directory.GetCurrentDirectory();
				}

				using (var game = new StudioGame(path))
				{
					game.Run();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}
