using AssetManagementBase;
using System;

namespace Nursia.ModelViewer
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				if (args.Length == 0)
				{
					Console.WriteLine("Usage: ModelViewer <filePath>");
					return;
				}

				Environment.SetEnvironmentVariable("FNA3D_FORCE_DRIVER", "D3D11");

				AMBConfiguration.Logger = Console.WriteLine;
				using (var game = new ViewerGame(args[0]))
				{
					game.Run();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}
	}
}
