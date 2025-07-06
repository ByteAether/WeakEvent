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
		var handler = () => { };
		var exception = Record.Exception(() => Assert.False(weakEvent.Unsubscribe(handler)));
		Assert.Null(exception);
	}

	[Fact]
	public async Task PublishAsync_WithCancelledToken_ThrowsOperationCanceledExceptionHandlerNotInvoked()
	{
		var weakEvent = new WeakEvent();
		var handlerInvoked = false;
		// Subscribe a handler that would set the flag to true.
		weakEvent.Subscribe(() => handlerInvoked = true);
		// Cancel the token immediately so that any wait operation observes the cancellation
		using var cts = new CancellationTokenSource();
		await cts.CancelAsync();

		// Assert that publishing an event with the canceled token throws the expected exception.
		await Assert.ThrowsAnyAsync<OperationCanceledException>(
			() => weakEvent.PublishAsync(cts.Token)
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

		async Task asyncHandlerCt(CancellationToken ct)
		{
			count += 4;
			await Task.CompletedTask;
		}

		var weakEvent = new WeakEvent();

		weakEvent.Subscribe(syncHandler);
		weakEvent.Subscribe(asyncHandler);
		weakEvent.Subscribe(asyncHandlerCt);

		weakEvent.Unsubscribe(syncHandler);
		weakEvent.Unsubscribe(asyncHandler);
		weakEvent.Unsubscribe(asyncHandlerCt);

		await weakEvent.PublishAsync();

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

		async Task asyncHandlerCt(CancellationToken ct)
		{
			count += 4;
			await Task.CompletedTask;
		}

		var weakEvent = new WeakEvent();

		weakEvent.Subscribe(syncHandler);
		weakEvent.Subscribe(asyncHandler);
		weakEvent.Subscribe(asyncHandlerCt);

		await weakEvent.PublishAsync();

		Assert.Equal(7, count);
	}

	[Fact]
	public async Task DeadHandler_IsNotInvoked_AfterGarbageCollection()
	{
		var weakEvent = new WeakEvent();
		var callCount = 0;

		static async Task createSubscriberAndInvoke(WeakEvent weakEvent, Action onEvent)
		{
			var subscriber = new NonGenericSubscriber(onEvent);
			weakEvent.Subscribe(subscriber.Handler);
			await weakEvent.PublishAsync();
			// The subscriber goes out of scope after this method, allowing it to be GC’d.
		}

		await createSubscriberAndInvoke(weakEvent, () => callCount++);

		// Force garbage collection to reclaim the subscriber instance.
		GC.Collect();
		GC.WaitForPendingFinalizers();

		// Invoke a second time
		await weakEvent.PublishAsync();

		// Just one call should happen instead of 2
		Assert.Equal(1, callCount);
	}

	[Fact]
	public void MultipleHandlers_CorrectCount()
	{
		// Stage 1 - No subscribers yet
		var weakEvent = new WeakEvent();
		Assert.Equal(0, weakEvent.SubscriberCount);

		// Stage 2 - Add local handler
		void syncHandler()
		{ }

		weakEvent.Subscribe(syncHandler);
		Assert.Equal(1, weakEvent.SubscriberCount);

		// Stage 3 - Add a garbage-collected handler
		void createSubscriberAndAssert()
		{
			var subscriber = new NonGenericSubscriber(() => { });
			weakEvent.Subscribe(subscriber.Handler);
			Assert.Equal(2, weakEvent.SubscriberCount);
		}

		createSubscriberAndAssert();

		// Stage 4 - Force garbage collection to reclaim the garbage-collectable subscriber instance.
		GC.Collect();
		GC.WaitForPendingFinalizers();
		Assert.Equal(1, weakEvent.SubscriberCount);

		// Stage 5 - Remove local handler
		weakEvent.Unsubscribe(syncHandler);
		Assert.Equal(0, weakEvent.SubscriberCount);
	}

	private class NonGenericSubscriber(Action onEvent)
	{
		private readonly Action _onEvent = onEvent;

		public void Handler() => _onEvent();
	}
}