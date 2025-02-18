
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
	public async Task Send_InvokesSubscribedHandlerAsync()
	{
		var weakEvent = new WeakEvent<string>();
		var received = string.Empty;
		weakEvent.Subscribe(msg => received = msg);

		await weakEvent.SendAsync("Hello");

		Assert.Equal("Hello", received);
	}

	[Fact]
	public async Task Send_InvokesSubscribedAsyncHandler()
	{
		var weakEvent = new WeakEvent<string>();
		var received = string.Empty;
		weakEvent.Subscribe(async msg =>
		{
			received = msg;
			await Task.CompletedTask;
		});

		await weakEvent.SendAsync("Hello");

		Assert.Equal("Hello", received);
	}

	[Fact]
	public async Task Unsubscribe_RemovesHandler()
	{
		var weakEvent = new WeakEvent<string>();
		var count = 0;
		Action<string> handler = msg => count++;

		weakEvent.Subscribe(handler);
		weakEvent.Unsubscribe(handler);

		await weakEvent.SendAsync("Test");

		Assert.Equal(0, count);
	}

	[Fact]
	public async Task MultipleHandlers_AreInvoked()
	{
		var weakEvent = new WeakEvent<int>();
		var count = 0;
		weakEvent.Subscribe(i => count += i);
		weakEvent.Subscribe(i => count += i * 2);

		await weakEvent.SendAsync(5);

		// Expected: 5 + 10 = 15
		Assert.Equal(15, count);
	}

	[Fact]
	public async Task DeadHandler_IsNotInvoked_AfterGarbageCollection()
	{
		var weakEvent = new WeakEvent<string>();
		var callCount = 0;
		await CreateSubscriberAndInvoke(weakEvent, () => callCount++);

		// Force garbage collection to reclaim the subscriber instance.
		GC.Collect();
		GC.WaitForPendingFinalizers();

		// Invoke second time
		await weakEvent.SendAsync("Test");

		// Just one call should happen instead of 2
		Assert.Equal(1, callCount);
	}

	private static async Task CreateSubscriberAndInvoke(WeakEvent<string> weakEvent, Action onEvent)
	{
		var subscriber = new GenericSubscriber(onEvent);
		weakEvent.Subscribe(subscriber.Handler);
		await weakEvent.SendAsync("Test");
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
