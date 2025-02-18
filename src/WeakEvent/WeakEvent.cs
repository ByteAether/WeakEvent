namespace ByteAether.WeakEvent;

/// <summary>
/// Represents a weak event that stores its subscribers using weak references.
/// Subscribers register an <see cref="Action{TEvent}"/> or <see cref="Func{TEvent, Task}"/> and when an event is raised, only those whose target is still alive will be invoked.
/// </summary>
/// <typeparam name="TEvent">The type of the event arguments.</typeparam>
public class WeakEvent<TEvent>
{
	private readonly List<WeakEventHandler<TEvent>> _handlers = [];

	private readonly SemaphoreSlim _lock = new(1, 1);

	/// <summary>
	/// Subscribes the specified handler to the event.
	/// </summary>
	/// <param name="handler">
	/// The handler to subscribe. It will be invoked when the event is raised,
	/// provided that the target is still alive.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Subscribe(Action<TEvent> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		_handlers.Add(new WeakEventHandler<TEvent>(handler));
	}

	/// <summary>
	/// Subscribes the specified async handler to the event.
	/// </summary>
	/// <param name="handler">
	/// The handler to subscribe. It will be invoked when the event is raised,
	/// provided that the target is still alive.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Subscribe(Func<TEvent, Task> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		_handlers.Add(new WeakEventHandler<TEvent>(handler));
	}

	/// <summary>
	/// Unsubscribes the specified handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Unsubscribe(Action<TEvent> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		_handlers.RemoveAll(weh => weh.Matches(handler));
	}

	/// <summary>
	/// Unsubscribes the specified async handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Unsubscribe(Func<TEvent, Task> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		_handlers.RemoveAll(weh => weh.Matches(handler));
	}

	/// <summary>
	/// Raises the event, invoking all live subscribers with the provided event data.
	/// Dead subscribers (whose targets have been garbage-collected) are removed.
	/// </summary>
	/// <param name="eventData">The event data to send to the subscribers.</param>
	public async Task SendAsync(TEvent eventData)
	{
		await _lock.WaitAsync();

		_handlers.RemoveAll(x => !x.IsAlive);

		foreach (var handler in _handlers)
		{
			await handler.InvokeAsync(eventData);
		}

		_lock.Release();
	}
}

/// <summary>
/// Represents a weak event that stores its subscribers using weak references.
/// Subscribers register an <see cref="Action"/> or <see cref="Func{Task}"/> and when an event is raised, only those whose target is still alive will be invoked.
/// </summary>
public class WeakEvent
{
	private readonly List<WeakEventHandler> _handlers = [];

	private readonly SemaphoreSlim _lock = new(1, 1);

	/// <summary>
	/// Subscribes the specified handler to the event.
	/// </summary>
	/// <param name="handler">
	/// The handler to subscribe. It will be invoked when the event is raised,
	/// provided that the target is still alive.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Subscribe(Action handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		_handlers.Add(new WeakEventHandler(handler));
	}

	/// <summary>
	/// Subscribes the specified async handler to the event.
	/// </summary>
	/// <param name="handler">
	/// The handler to subscribe. It will be invoked when the event is raised,
	/// provided that the target is still alive.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Subscribe(Func<Task> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		_handlers.Add(new WeakEventHandler(handler));
	}

	/// <summary>
	/// Unsubscribes the specified handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Unsubscribe(Action handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		_handlers.RemoveAll(weh => weh.Matches(handler));
	}

	/// <summary>
	/// Unsubscribes the specified async handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Unsubscribe(Func<Task> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		_handlers.RemoveAll(weh => weh.Matches(handler));
	}

	/// <summary>
	/// Raises the event, invoking all live subscribers with the provided event data.
	/// Dead subscribers (whose targets have been garbage-collected) are removed.
	/// </summary>
	public async Task SendAsync()
	{
		await _lock.WaitAsync();

		_handlers.RemoveAll(x => !x.IsAlive);

		foreach (var handler in _handlers)
		{
			await handler.InvokeAsync();
		}

		_lock.Release();
	}
}