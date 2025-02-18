namespace ByteAether.WeakEvent.Tests;
public class WeakEventHandlerNonGenericTests
{
	[Fact]
	public void StaticHandler_GetHandler_ReturnsHandlerAndMatches()
	{
		Action handler = StaticHandler;
		var weh = new WeakEventHandler(handler);

		var retrieved = weh.GetHandler();
		Assert.NotNull(retrieved);
		retrieved();
		Assert.True(_staticInvoked);

		Assert.True(weh.Matches(handler));

		Action newDelegate = StaticHandler;
		Assert.True(weh.Matches(newDelegate));
	}

	private static bool _staticInvoked = false;

	private static void StaticHandler()
	{
		_staticInvoked = true;
	}

	[Fact]
	public void InstanceHandler_GetHandler_ReturnsHandlerAndMatches()
	{
		var instance = new InstanceTest();
		Action handler = instance.Handler;
		var weh = new WeakEventHandler(handler);

		var retrieved = weh.GetHandler();
		Assert.NotNull(retrieved);
		retrieved();
		Assert.True(instance.Invoked);

		Assert.True(weh.Matches(handler));

		Action newDelegate = instance.Handler;
		Assert.True(weh.Matches(newDelegate));
	}

	[Fact]
	public void InstanceHandler_GetHandler_ReturnsNullAfterGC()
	{
		CreateWeakInstance(out var weh);

		GC.Collect();
		GC.WaitForPendingFinalizers();
		Thread.Sleep(100);

		var handler = weh.GetHandler();
		Assert.Null(handler);
	}

	private static void CreateWeakInstance(out WeakEventHandler weh)
	{
		var instance = new InstanceTest();
		Action handler = instance.Handler;
		weh = new WeakEventHandler(handler);
		// instance goes out of scope after this method.
	}

	private class InstanceTest
	{
		public bool Invoked { get; private set; }
		public void Handler()
		{
			Invoked = true;
		}
	}
}