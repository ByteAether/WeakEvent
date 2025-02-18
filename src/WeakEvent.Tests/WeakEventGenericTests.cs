namespace ByteAether.WeakEvent.Tests;

public class WeakEventGenericTests
{
	[Fact]
	public void Subscribe_NullHandler_ThrowsArgumentNullException()
	{
		var weakEvent = new WeakEvent<string>();
		Assert.Throws<ArgumentNullException>(() => weakEvent.Subscribe(null!));
	}

	[Fact]
	public void Unsubscribe_NullHandler_ThrowsArgumentNullException()
	{
		var weakEvent = new WeakEvent<string>();
		Assert.Throws<ArgumentNullException>(() => weakEvent.Unsubscribe(null!));
	}

	[Fact]
	public void Unsubscribe_NonExistentHandler_DoesNotThrow()
	{
		var weakEvent = new WeakEvent<string>();
		Action<string> handler = msg => { };
		var exception = Record.Exception(() => weakEvent.Unsubscribe(handler));
		Assert.Null(exception);
	}

	[Fact]
	public void Send_InvokesSubscribedHandler()
	{
		var weakEvent = new WeakEvent<string>();
		var received = string.Empty;
		weakEvent.Subscribe(msg => received = msg);

		weakEvent.Send("Hello");

		Assert.Equal("Hello", received);
	}

	[Fact]
	public void Unsubscribe_RemovesHandler()
	{
		var weakEvent = new WeakEvent<string>();
		var count = 0;
		Action<string> handler = msg => count++;
		weakEvent.Subscribe(handler);

		weakEvent.Unsubscribe(handler);
		weakEvent.Send("Test");

		Assert.Equal(0, count);
	}

	[Fact]
	public void MultipleHandlers_AreInvoked()
	{
		var weakEvent = new WeakEvent<int>();
		var count = 0;
		weakEvent.Subscribe(i => count += i);
		weakEvent.Subscribe(i => count += i * 2);

		weakEvent.Send(5);

		// Expected: 5 + 10 = 15
		Assert.Equal(15, count);
	}

	[Fact]
	public void DeadHandler_IsNotInvoked_AfterGarbageCollection()
	{
		var weakEvent = new WeakEvent<string>();
		var callCount = 0;
		CreateSubscriber(weakEvent, () => callCount++);

		// Force garbage collection to reclaim the subscriber instance.
		GC.Collect();
		GC.WaitForPendingFinalizers();

		weakEvent.Send("Test");

		Assert.Equal(0, callCount);
	}

	private static void CreateSubscriber(WeakEvent<string> weakEvent, Action onEvent)
	{
		var subscriber = new GenericSubscriber(onEvent);
		weakEvent.Subscribe(subscriber.Handler);
		// The subscriber goes out of scope after this method, allowing it to be GC’d.
	}

	private class GenericSubscriber(Action onEvent)
	{
		private readonly Action _onEvent = onEvent;

		public void Handler(string _)
		{
			_onEvent();
		}
	}
}
