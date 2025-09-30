namespace Nursia.Tests
{
	[TestClass]
	public class TestsEnvironment
	{
		[AssemblyInitialize]
		public static void SetUp(TestContext testContext)
		{
			Nrs.SetGame(new TestGame(), false);
		}
	}
}
