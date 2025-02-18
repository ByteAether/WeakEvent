namespace ByteAether.WeakEvent.Tests;
public class WeakEventHandlerGenericTests
{
	[Fact]
	public void Constructor_NullDelegate_ThrowsException()
	{
		// Arrange
		Action<string> handler = null!;

		// Act & Assert
		Assert.Throws<NullReferenceException>(() => new WeakEventHandler<string>(handler));
	}

	[Fact]
	public async Task StaticHandler_InvokeAsync_Void()
	{
		// Arrange
		_staticResult = string.Empty;
		var weakHandler = new WeakEventHandler<string>(StaticHandler);

		// Act
		await weakHandler.InvokeAsync("TestStatic");

		// Assert
		Assert.Equal("TestStatic", _staticResult);
		Assert.True(weakHandler.IsAlive);
	}

	private static string _staticResult = string.Empty;
	private static void StaticHandler(string data)
	{
		_staticResult = data;
	}

	[Fact]
	public async Task InstanceHandler_InvokeAsync_Void()
	{
		// Arrange
		var subscriber = new Subscriber();
		var weakHandler = new WeakEventHandler<string>(subscriber.InstanceHandler);

		// Act
		await weakHandler.InvokeAsync("InstanceTest");

		// Assert
		Assert.Equal("InstanceTest", subscriber.LastReceived);
		Assert.True(weakHandler.IsAlive);
	}

	[Fact]
	public async Task InstanceHandler_InvokeAsync_Async()
	{
		// Arrange
		var subscriber = new Subscriber();
		var weakHandler = new WeakEventHandler<string>(subscriber.AsyncHandler);

		// Act
		await weakHandler.InvokeAsync("AsyncTest");

		// Assert
		Assert.Equal("AsyncTest", subscriber.LastReceived);
		Assert.True(weakHandler.IsAlive);
	}

	[Fact]
	public void Matches_ReturnsTrueForSameDelegate()
	{
		// Arrange
		var subscriber = new Subscriber();
		var weakHandler = new WeakEventHandler<string>(subscriber.InstanceHandler);

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
		var weakHandler = new WeakEventHandler<string>(subscriber1.InstanceHandler);

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
		await weakHandler.InvokeAsync("Should not be received");

		// Assert
		Assert.False(isAliveBefore);
	}

	private static WeakEventHandler<string> CreateWeakSubscriber()
	{
		var subscriber = new Subscriber
		{
			ThrowIfInvoked = true
		};
		return new WeakEventHandler<string>(subscriber.InstanceHandler);
		// 'subscriber' goes out of scope after this method.
	}

	private class Subscriber
	{
		public string LastReceived { get; private set; } = string.Empty;
		public bool ThrowIfInvoked { get; set; }

		public void InstanceHandler(string data)
		{
			if (ThrowIfInvoked)
			{
				throw new InvalidOperationException("This should not happen!");
			}

			LastReceived = data;
		}

		public async Task AsyncHandler(string data)
		{
			if (ThrowIfInvoked)
			{
				throw new InvalidOperationException("This should not happen!");
			}

			LastReceived = data;
			await Task.CompletedTask;
		}
	}
}