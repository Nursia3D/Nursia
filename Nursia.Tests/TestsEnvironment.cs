using System.Runtime.CompilerServices;

namespace Nursia.Tests;

internal static class TestAssemblyInitializer
{
	[ModuleInitializer]
	public static void Initialize()
	{
		Nrs.SetGame(new TestGame(), false);
	}
}
