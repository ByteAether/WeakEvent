namespace ByteAether.WeakEvent.Tests;
public class WeakEventHandlerGenericTests
{
	[Fact]
	public void StaticHandler_GetHandler_ReturnsHandlerAndMatches()
	{
		Action<string> handler = StaticHandler;
		var weh = new WeakEventHandler<string>(handler);

		var retrieved = weh.GetHandler();
		Assert.NotNull(retrieved);
		retrieved("Hello");
		Assert.Equal("Hello", _staticResult);

		Assert.True(weh.Matches(handler));

		// Create a new delegate instance for the same static method.
		Action<string> newDelegate = StaticHandler;
		Assert.True(weh.Matches(newDelegate));
	}

	private static string _staticResult = string.Empty;

	private static void StaticHandler(string msg)
	{
		_staticResult = msg;
	}

	[Fact]
	public void InstanceHandler_GetHandler_ReturnsHandlerAndMatches()
	{
		var instance = new InstanceTest();
		Action<string> handler = instance.Handler;
		var weh = new WeakEventHandler<string>(handler);

		var retrieved = weh.GetHandler();
		Assert.NotNull(retrieved);
		retrieved("World");
		Assert.Equal("World", instance.Result);

		Assert.True(weh.Matches(handler));

		// Create a new delegate instance from the same instance method.
		Action<string> newDelegate = instance.Handler;
		Assert.True(weh.Matches(newDelegate));
	}

	[Fact]
	public void InstanceHandler_GetHandler_ReturnsNullAfterGC()
	{
		CreateWeakInstance(out var weh);
		GC.Collect();
		GC.WaitForPendingFinalizers();
		// Give GC a moment to reclaim the instance.
		Thread.Sleep(100);
		var handler = weh.GetHandler();
		Assert.Null(handler);
	}

	private static void CreateWeakInstance(out WeakEventHandler<string> weh)
	{
		var instance = new InstanceTest();
		Action<string> handler = instance.Handler;
		weh = new WeakEventHandler<string>(handler);
		// instance goes out of scope after this method, allowing GC to reclaim it.
	}

	private class InstanceTest
	{
		public string? Result { get; private set; }
		public void Handler(string msg)
		{
			Result = msg;
		}
	}
}