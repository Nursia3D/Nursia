using AssetManagementBase;
using System;

namespace DigitalRiseModel.Samples.Character
{
	class Program
	{
		static void Main(string[] args)
		{
			foreach (var arg in args)
			{
				if (arg == "/nf")
				{
					Configuration.NoFixedStep = true;
				}
			}

			AMBConfiguration.Logger = Console.WriteLine;
			using (var game = new ViewerGame())
			{
				game.Run();
			}
		}
	}
}
