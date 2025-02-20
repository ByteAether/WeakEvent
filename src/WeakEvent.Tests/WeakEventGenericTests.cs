
namespace ByteAether.WeakEvent.Tests;

public class WeakEventGenericTests
{
	[Fact]
	public void Subscribe_NullHandler_ThrowsArgumentNullException()
	{
		var weakEvent = new WeakEvent<string>();
		Assert.Throws<ArgumentNullException>(() => weakEvent.Subscribe((Action<string>)null!));
	}

	[Fact]
	public void Unsubscribe_NullHandler_ThrowsArgumentNullException()
	{
		var weakEvent = new WeakEvent<string>();
		Assert.Throws<ArgumentNullException>(() => weakEvent.Unsubscribe((Action<string>)null!));
	}

	[Fact]
	public void Unsubscribe_NonExistentHandler_DoesNotThrow()
	{
		var weakEvent = new WeakEvent<string>();
		Action<string> handler = msg => { };
		var exception = Record.Exception(() => Assert.False(weakEvent.Unsubscribe(handler)));
		Assert.Null(exception);
	}

	[Fact]
	public async Task PublishAsync_WithCancelledToken_ThrowsOperationCanceledExceptionHandlerNotInvoked()
	{
		var weakEvent = new WeakEvent<string>();
		var handlerInvoked = false;
		// Subscribe a handler that would set the flag to true.
		weakEvent.Subscribe(eventData => handlerInvoked = true);
		// Cancel the token immediately so that any wait operation observes the cancellation
		using var cts = new CancellationTokenSource();
		cts.Cancel();

		// Assert that publishing an event with the cancelled token throws the expected exception.
		await Assert.ThrowsAnyAsync<OperationCanceledException>(
			() => weakEvent.PublishAsync("Test event", cts.Token)
		);
		Assert.False(handlerInvoked, "The subscribed handler should not be invoked when cancellation is requested.");
	}

	[Fact]
	public async Task Unsubscribe_RemovesHandler()
	{
		var count = 0;

		void syncHandler(int x)
		{
			count += 1 * x;
		}
		async Task asyncHandler(int x)
		{
			count += 2 * x;
			await Task.CompletedTask;
		}
		async Task asyncHandlerCT(int x, CancellationToken ct)
		{
			count += 4 * x;
			await Task.CompletedTask;
		}

		var weakEvent = new WeakEvent<int>();

		weakEvent.Subscribe(syncHandler);
		weakEvent.Subscribe(asyncHandler);
		weakEvent.Subscribe(asyncHandlerCT);

		weakEvent.Unsubscribe(syncHandler);
		weakEvent.Unsubscribe(asyncHandler);
		weakEvent.Unsubscribe(asyncHandlerCT);

		await weakEvent.PublishAsync(2);

		Assert.Equal(0, count);
	}

	[Fact]
	public async Task MultipleHandlers_AreInvoked()
	{
		var count = 0;

		void syncHandler(int x)
		{
			count += 1 * x;
		}
		async Task asyncHandler(int x)
		{
			count += 2 * x;
			await Task.CompletedTask;
		}
		async Task asyncHandlerCT(int x, CancellationToken ct)
		{
			count += 4 * x;
			await Task.CompletedTask;
		}

		var weakEvent = new WeakEvent<int>();

		weakEvent.Subscribe(syncHandler);
		weakEvent.Subscribe(asyncHandler);
		weakEvent.Subscribe(asyncHandlerCT);

		await weakEvent.PublishAsync(2);

		Assert.Equal(14, count);
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
		await weakEvent.PublishAsync("Test");

		// Just one call should happen instead of 2
		Assert.Equal(1, callCount);
	}

	private static async Task CreateSubscriberAndInvoke(WeakEvent<string> weakEvent, Action onEvent)
	{
		var subscriber = new GenericSubscriber(onEvent);
		weakEvent.Subscribe(subscriber.Handler);
		await weakEvent.PublishAsync("Test");
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
