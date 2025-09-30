using AssetManagementBase;
using System;

namespace FuelCell
{
	class Program
	{
		static void Main(string[] args)
		{
			AMBConfiguration.Logger = Console.WriteLine;

			using (var game = new FuelCellGame())
			{
				game.Run();
			}
		}
	}
}
