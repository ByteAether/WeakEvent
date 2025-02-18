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
	public async Task Send_InvokesSubscribedHandler()
	{
		var weakEvent = new WeakEvent();
		var invoked = false;
		weakEvent.Subscribe(() => invoked = true);

		await weakEvent.SendAsync();

		Assert.True(invoked);
	}

	[Fact]
	public async Task Send_InvokesSubscribedAsyncHandler()
	{
		var weakEvent = new WeakEvent();
		var invoked = false;
		weakEvent.Subscribe(async () =>
		{
			invoked = true;
			await Task.CompletedTask;
		});

		await weakEvent.SendAsync();

		Assert.True(invoked);
	}

	[Fact]
	public async Task Unsubscribe_RemovesHandler()
	{
		var weakEvent = new WeakEvent();
		var count = 0;
		Action handler = () => count++;
		weakEvent.Subscribe(handler);

		weakEvent.Unsubscribe(handler);
		await weakEvent.SendAsync();

		Assert.Equal(0, count);
	}

	[Fact]
	public async Task MultipleHandlers_AreInvoked()
	{
		var weakEvent = new WeakEvent();
		var count = 0;
		weakEvent.Subscribe(() => count += 1);
		weakEvent.Subscribe(() => count += 2);

		await weakEvent.SendAsync();

		Assert.Equal(3, count);
	}

	[Fact]
	public async Task DeadHandler_IsNotInvoked_AfterGarbageCollection()
	{
		var weakEvent = new WeakEvent();
		var callCount = 0;

		await CreateSubscriberAndInvoke(weakEvent, () => callCount++);

		// Force garbage collection to reclaim the subscriber instance.
		GC.Collect();
		GC.WaitForPendingFinalizers();

		// Invoke second time
		await weakEvent.SendAsync();

		// Just one call should happen instead of 2
		Assert.Equal(1, callCount);
	}

	private static async Task CreateSubscriberAndInvoke(WeakEvent weakEvent, Action onEvent)
	{
		var subscriber = new NonGenericSubscriber(onEvent);
		weakEvent.Subscribe(subscriber.Handler);
		await weakEvent.SendAsync();
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
