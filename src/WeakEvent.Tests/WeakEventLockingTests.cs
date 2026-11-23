using Xunit.Abstractions;

namespace ByteAether.WeakEvent.Tests;

public class WeakEventLockingTests(ITestOutputHelper output)
{
	private readonly ITestOutputHelper _output = output;

	[Fact]
	public async Task Handler_CanUnsubscribeItself()
	{
		WeakEvent e = new();
		Action? handler = null;
		handler = () =>
		{
			_output.WriteLine("Handler executing...");
			e.Unsubscribe(handler!);
		};

		e.Subscribe(handler);
		await e.PublishAsync();
	}

	[Fact]
	public async Task SubscriptionChanges_DuringPublication_AreAppliedToNextPublication()
	{
		WeakEvent e = new();
		TaskCompletionSource entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
		TaskCompletionSource release = new(TaskCreationOptions.RunContinuationsAsynchronously);
		var callCount = 0;

		async Task firstHandler()
		{
			entered.TrySetResult();
			await release.Task;
			callCount++;
		}

		void secondHandler() => callCount++;

		e.Subscribe(firstHandler);
		var firstPublication = e.PublishAsync();
		await entered.Task;

		e.Subscribe(secondHandler);
		release.SetResult();
		await firstPublication;

		Assert.Equal(1, callCount);

		await e.PublishAsync();
		Assert.Equal(3, callCount);
	}

}