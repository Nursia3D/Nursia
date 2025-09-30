using System;

namespace Nursia.Samples.Primives
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				using (var game = new WaterGame())
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
