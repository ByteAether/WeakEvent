namespace ByteAether.WeakEvent.Tests;
public class WeakEventNonGenericTests
{
	[Fact]
	public void Subscribe_NullHandler_ThrowsArgumentNullException()
	{
		var weakEvent = new WeakEvent();
		Assert.Throws<ArgumentNullException>(() => weakEvent.Subscribe((Action)null!));
	}

	[Fact]
	public void Unsubscribe_NullHandler_ThrowsArgumentNullException()
	{
		var weakEvent = new WeakEvent();
		Assert.Throws<ArgumentNullException>(() => weakEvent.Unsubscribe((Action)null!));
	}

	[Fact]
	public void Unsubscribe_NonExistentHandler_DoesNotThrow()
	{
		var weakEvent = new WeakEvent();
		Action handler = () => { };
		var exception = Record.Exception(() => Assert.False(weakEvent.Unsubscribe(handler)));
		Assert.Null(exception);
	}

	[Fact]
	public async Task SendAsync_WithCancelledToken_ThrowsOperationCanceledExceptionHandlerNotInvoked()
	{
		var weakEvent = new WeakEvent();
		var handlerInvoked = false;
		// Subscribe a handler that would set the flag to true.
		weakEvent.Subscribe(() => handlerInvoked = true);
		// Cancel the token immediately so that any wait operation observes the cancellation
		using var cts = new CancellationTokenSource();
		cts.Cancel();

		// Assert that sending an event with the cancelled token throws the expected exception.
		await Assert.ThrowsAnyAsync<OperationCanceledException>(
			() => weakEvent.SendAsync(cts.Token)
		);
		Assert.False(handlerInvoked, "The subscribed handler should not be invoked when cancellation is requested.");
	}

	[Fact]
	public async Task Unsubscribe_RemovesHandler()
	{
		var count = 0;

		void syncHandler()
		{
			count += 1;
		}
		async Task asyncHandler()
		{
			count += 2;
			await Task.CompletedTask;
		}
		async Task asyncHandlerCT(CancellationToken ct)
		{
			count += 4;
			await Task.CompletedTask;
		}

		var weakEvent = new WeakEvent();

		weakEvent.Subscribe(syncHandler);
		weakEvent.Subscribe(asyncHandler);
		weakEvent.Subscribe(asyncHandlerCT);

		weakEvent.Unsubscribe(syncHandler);
		weakEvent.Unsubscribe(asyncHandler);
		weakEvent.Unsubscribe(asyncHandlerCT);

		await weakEvent.SendAsync();

		Assert.Equal(0, count);
	}

	[Fact]
	public async Task MultipleHandlers_AreInvoked()
	{
		var count = 0;

		void syncHandler()
		{
			count += 1;
		}
		async Task asyncHandler()
		{
			count += 2;
			await Task.CompletedTask;
		}
		async Task asyncHandlerCT(CancellationToken ct)
		{
			count += 4;
			await Task.CompletedTask;
		}

		var weakEvent = new WeakEvent();

		weakEvent.Subscribe(syncHandler);
		weakEvent.Subscribe(asyncHandler);
		weakEvent.Subscribe(asyncHandlerCT);

		await weakEvent.SendAsync();

		Assert.Equal(7, count);
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
