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
}