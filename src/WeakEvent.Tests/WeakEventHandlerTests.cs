namespace ByteAether.WeakEvent.Tests;

public class WeakEventHandlerTests
{
	[Fact]
	public void Constructor_NullDelegate_ThrowsException()
	{
		// Arrange
		Action handler = null!;

		// Act & Assert
		Assert.Throws<NullReferenceException>(() => new WeakEventHandler(handler));
	}

	[Fact]
	public async Task StaticHandler_InvokeAsync_Void()
	{
		// Arrange
		_staticInvoked = false;
		var weakHandler = new WeakEventHandler(StaticHandler);

		// Act
		await weakHandler.InvokeAsync([]);

		// Assert
		Assert.True(_staticInvoked);
		Assert.True(weakHandler.IsAlive);
	}

	private static bool _staticInvoked;
	private static void StaticHandler() => _staticInvoked = true;

	[Fact]
	public async Task InstanceHandler_InvokeAsync_Void()
	{
		// Arrange
		var subscriber = new Subscriber();
		var weakHandler = new WeakEventHandler(subscriber.InstanceHandler);

		// Act
		await weakHandler.InvokeAsync([]);

		// Assert
		Assert.True(subscriber.Invoked);
		Assert.True(weakHandler.IsAlive);
	}

	[Fact]
	public async Task InstanceHandler_InvokeAsync_Async()
	{
		// Arrange
		var subscriber = new Subscriber();
		var weakHandler = new WeakEventHandler(subscriber.AsyncHandler);

		// Act
		await weakHandler.InvokeAsync([]);

		// Assert
		Assert.True(subscriber.Invoked);
		Assert.True(weakHandler.IsAlive);
	}

	[Fact]
	public void Matches_ReturnsTrueForSameDelegate()
	{
		// Arrange
		var subscriber = new Subscriber();
		var weakHandler = new WeakEventHandler(subscriber.InstanceHandler);

		// Act
		var result = weakHandler.Matches(subscriber.InstanceHandler);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void Matches_ReturnsFalseForDifferentDelegate()
	{
		// Arrange
		var subscriber1 = new Subscriber();
		var subscriber2 = new Subscriber();
		var weakHandler = new WeakEventHandler(subscriber1.InstanceHandler);

		// Act
		var result = weakHandler.Matches(subscriber2.InstanceHandler);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public async Task InvokeAsync_ForDeadInstance_DoesNotInvoke()
	{
		// Arrange
		var weakHandler = CreateWeakSubscriber();

		// Act
		GC.Collect();
		GC.WaitForPendingFinalizers();

		var isAliveBefore = weakHandler.IsAlive;
		await weakHandler.InvokeAsync([]);

		// Assert
		Assert.False(isAliveBefore);
	}

	private static WeakEventHandler CreateWeakSubscriber()
	{
		var subscriber = new Subscriber
		{
			ThrowIfInvoked = true
		};
		return new WeakEventHandler(subscriber.InstanceHandler);
		// 'subscriber' goes out of scope after this method.
	}

	private class Subscriber
	{
		public bool Invoked { get; private set; }
		public bool ThrowIfInvoked { get; init; }

		public void InstanceHandler()
		{
			if (ThrowIfInvoked)
			{
				throw new InvalidOperationException("This should not happen!");
			}

			Invoked = true;
		}

		public async Task AsyncHandler()
		{
			if (ThrowIfInvoked)
			{
				throw new InvalidOperationException("This should not happen!");
			}

			Invoked = true;
			await Task.CompletedTask;
		}
	}
}