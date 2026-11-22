using System.Diagnostics.CodeAnalysis;

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
		Action<string> handler = _ => { };
		var exception = Record.Exception(() => Assert.False(weakEvent.Unsubscribe(handler)));
		Assert.Null(exception);
	}

	[Fact]
	public async Task PublishAsync_WithCancelledToken_ThrowsOperationCanceledExceptionHandlerNotInvoked()
	{
		var weakEvent = new WeakEvent<string>();
		var handlerInvoked = false;
		// Subscribe a handler that would set the flag to true.
		weakEvent.Subscribe(_ => handlerInvoked = true);
		// Cancel the token immediately so that any wait operation observes the cancellation
		using var cts = new CancellationTokenSource();
		await cts.CancelAsync();

		// Assert that publishing an event with the canceled token throws the expected exception.
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => weakEvent.PublishAsync("Test event", cts.Token)
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

		async Task asyncHandlerCt(int x, CancellationToken ct)
		{
			count += 4 * x;
			await Task.CompletedTask;
		}

		var weakEvent = new WeakEvent<int>();

		weakEvent.Subscribe(syncHandler);
		weakEvent.Subscribe(asyncHandler);
		weakEvent.Subscribe(asyncHandlerCt);

		weakEvent.Unsubscribe(syncHandler);
		weakEvent.Unsubscribe(asyncHandler);
		weakEvent.Unsubscribe(asyncHandlerCt);

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

		async Task asyncHandlerCt(int x, CancellationToken ct)
		{
			count += 4 * x;
			await Task.CompletedTask;
		}

		var weakEvent = new WeakEvent<int>();

		weakEvent.Subscribe(syncHandler);
		weakEvent.Subscribe(asyncHandler);
		weakEvent.Subscribe(asyncHandlerCt);

		await weakEvent.PublishAsync(2);

		Assert.Equal(14, count);
	}

	[Fact]
	public async Task CancellationTokenHandler_DoesNotAlterArgumentsForFollowingHandler()
	{
		var weakEvent = new WeakEvent<string>();
		var tokenHandlerCalled = false;
		var syncHandlerCalled = false;

		weakEvent.Subscribe((_, _) =>
			{
				tokenHandlerCalled = true;
				return Task.CompletedTask;
			}
		);
		weakEvent.Subscribe(_ => syncHandlerCalled = true);

		await weakEvent.PublishAsync("Test event");

		Assert.True(tokenHandlerCalled);
		Assert.True(syncHandlerCalled);
	}

	[Fact]
	[SuppressMessage("ReSharper", "AccessToDisposedClosure")]
	public async Task PublishAsync_CancellationBetweenHandlers_StopsDispatch()
	{
		var weakEvent = new WeakEvent<string>();
		using var cancellationTokenSource = new CancellationTokenSource();
		var secondHandlerCalled = false;

		weakEvent.Subscribe(_ => cancellationTokenSource.Cancel());
		weakEvent.Subscribe(_ => secondHandlerCalled = true);

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
			weakEvent.PublishAsync("Test event", cancellationTokenSource.Token)
		);
		Assert.False(secondHandlerCalled);
	}

	[Fact]
	public async Task Subscribe_MulticastHandler_SubscribesAndUnsubscribesAllInvocations()
	{
		var weakEvent = new WeakEvent<string>();
		var invocationCount = 0;
		Action<string> firstHandler = _ => invocationCount++;
		Action<string> secondHandler = _ => invocationCount++;
		var combinedHandler = firstHandler + secondHandler;

		weakEvent.Subscribe(combinedHandler);
		Assert.Equal(2, weakEvent.SubscriberCount);

		await weakEvent.PublishAsync("Test event");
		Assert.Equal(2, invocationCount);

		Assert.True(weakEvent.Unsubscribe(combinedHandler));
		await weakEvent.PublishAsync("Test event");
		Assert.Equal(2, invocationCount);
	}

	[Fact]
	public async Task PublishAsync_SynchronousHandlerException_IsNotWrapped()
	{
		var weakEvent = new WeakEvent<string>();
		weakEvent.Subscribe((Action<string>)(_ => throw new TestException()));

		await Assert.ThrowsAsync<TestException>(() => weakEvent.PublishAsync("Test event"));
	}

	[Fact]
	public async Task PublishAsync_AsyncHandlerReturningNull_ThrowsInvalidOperationException()
	{
		var weakEvent = new WeakEvent<string>();
		weakEvent.Subscribe((Func<string, Task>)(_ => null!));

		await Assert.ThrowsAsync<InvalidOperationException>(() => weakEvent.PublishAsync("Test event"));
	}

	[Fact]
	public async Task DeadHandler_IsNotInvoked_AfterGarbageCollection()
	{
		var weakEvent = new WeakEvent<string>();
		var callCount = 0;

		static async Task createSubscriberAndInvoke(WeakEvent<string> weakEvent, Action onEvent)
		{
			var subscriber = new GenericSubscriber(onEvent);
			weakEvent.Subscribe(subscriber.Handler);
			await weakEvent.PublishAsync("Test");
			// The subscriber goes out of scope after this method, allowing it to be GC’d.
		}

		await createSubscriberAndInvoke(weakEvent, () => callCount++);

		// Force garbage collection to reclaim the subscriber instance.
		GC.Collect();
		GC.WaitForPendingFinalizers();

		// Invoke a second time
		await weakEvent.PublishAsync("Test");

		// Just one call should happen instead of 2
		Assert.Equal(1, callCount);
	}

	[Fact]
	public void MultipleHandlers_CorrectCount()
	{
		// Stage 1 - No subscribers yet
		var weakEvent = new WeakEvent<string>();
		Assert.Equal(0, weakEvent.SubscriberCount);

		// Stage 2 - Add local handler
		void syncHandler(string _)
		{
		}

		weakEvent.Subscribe(syncHandler);

		// Stage 3 - Add a garbage-collected handler
		void createSubscriberAndAssert()
		{
			var subscriber = new GenericSubscriber(() => { });
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

	private class GenericSubscriber(Action onEvent)
	{
		private readonly Action _onEvent = onEvent;

		public void Handler(string _) => _onEvent();
	}

	private sealed class TestException : Exception;
}