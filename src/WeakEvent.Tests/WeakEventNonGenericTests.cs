namespace ByteAether.WeakEvent.Tests;
public class WeakEventNonGenericTests
{
	[Fact]
	public void Subscribe_NullHandler_ThrowsArgumentNullException()
	{
		var weakEvent = new WeakEvent();
		Assert.Throws<ArgumentNullException>(() => weakEvent.Subscribe(null!));
	}

	[Fact]
	public void Unsubscribe_NullHandler_ThrowsArgumentNullException()
	{
		var weakEvent = new WeakEvent();
		Assert.Throws<ArgumentNullException>(() => weakEvent.Unsubscribe(null!));
	}

	[Fact]
	public void Unsubscribe_NonExistentHandler_DoesNotThrow()
	{
		var weakEvent = new WeakEvent();
		Action handler = () => { };
		var exception = Record.Exception(() => weakEvent.Unsubscribe(handler));
		Assert.Null(exception);
	}

	[Fact]
	public void Send_InvokesSubscribedHandler()
	{
		var weakEvent = new WeakEvent();
		var invoked = false;
		weakEvent.Subscribe(() => invoked = true);

		weakEvent.Send();

		Assert.True(invoked);
	}

	[Fact]
	public void Unsubscribe_RemovesHandler()
	{
		var weakEvent = new WeakEvent();
		var count = 0;
		Action handler = () => count++;
		weakEvent.Subscribe(handler);

		weakEvent.Unsubscribe(handler);
		weakEvent.Send();

		Assert.Equal(0, count);
	}

	[Fact]
	public void MultipleHandlers_AreInvoked()
	{
		var weakEvent = new WeakEvent();
		var count = 0;
		weakEvent.Subscribe(() => count += 1);
		weakEvent.Subscribe(() => count += 2);

		weakEvent.Send();

		Assert.Equal(3, count);
	}

	[Fact]
	public void DeadHandler_IsNotInvoked_AfterGarbageCollection()
	{
		var weakEvent = new WeakEvent();
		var callCount = 0;
		CreateSubscriber(weakEvent, () => callCount++);

		// Force garbage collection to reclaim the subscriber instance.
		GC.Collect();
		GC.WaitForPendingFinalizers();

		weakEvent.Send();

		Assert.Equal(0, callCount);
	}

	private static void CreateSubscriber(WeakEvent weakEvent, Action onEvent)
	{
		var subscriber = new NonGenericSubscriber(onEvent);
		weakEvent.Subscribe(subscriber.Handler);
		// The subscriber goes out of scope after this method, allowing it to be GC’d.
	}

	private class NonGenericSubscriber(Action onEvent)
	{
		private readonly Action _onEvent = onEvent;

		public void Handler()
		{
			_onEvent();
		}
	}
}
