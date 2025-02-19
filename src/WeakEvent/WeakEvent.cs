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
	/// Subscribes the specified async handler to the event.
	/// </summary>
	/// <param name="handler">
	/// The handler to subscribe. It will be invoked when the event is raised,
	/// provided that the target is still alive.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Subscribe(Func<TEvent, CancellationToken, Task> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		_handlers.Add(new WeakEventHandler<TEvent>(handler));
	}

	/// <summary>
	/// Unsubscribes the specified handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public bool Unsubscribe(Action<TEvent> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		return _handlers.RemoveAll(weh => weh.Matches(handler)) > 0;
	}

	/// <summary>
	/// Unsubscribes the specified async handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public bool Unsubscribe(Func<TEvent, Task> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		return _handlers.RemoveAll(weh => weh.Matches(handler)) > 0;
	}

	/// <summary>
	/// Unsubscribes the specified async handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public bool Unsubscribe(Func<TEvent, CancellationToken, Task> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		return _handlers.RemoveAll(weh => weh.Matches(handler)) > 0;
	}

	/// <summary>
	/// Raises the event, invoking all live subscribers with the provided event data.
	/// Dead subscribers (whose targets have been garbage-collected) are removed.
	/// </summary>
	/// <param name="eventData">The event data to send to the subscribers.</param>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	public async Task SendAsync(TEvent eventData, CancellationToken cancellationToken = default)
	{
		await _lock.WaitAsync(cancellationToken);

		_handlers.RemoveAll(x => !x.IsAlive);

		foreach (var handler in _handlers)
		{
			await handler.InvokeAsync(eventData, cancellationToken);
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
	/// Subscribes the specified async handler to the event.
	/// </summary>
	/// <param name="handler">
	/// The handler to subscribe. It will be invoked when the event is raised,
	/// provided that the target is still alive.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Subscribe(Func<CancellationToken, Task> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		_handlers.Add(new WeakEventHandler(handler));
	}

	/// <summary>
	/// Unsubscribes the specified handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public bool Unsubscribe(Action handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		return _handlers.RemoveAll(weh => weh.Matches(handler)) > 0;
	}

	/// <summary>
	/// Unsubscribes the specified async handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public bool Unsubscribe(Func<Task> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		return _handlers.RemoveAll(weh => weh.Matches(handler)) > 0;
	}

	/// <summary>
	/// Unsubscribes the specified async handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public bool Unsubscribe(Func<CancellationToken, Task> handler)
	{
		ArgumentNullException.ThrowIfNull(handler);

		return _handlers.RemoveAll(weh => weh.Matches(handler)) > 0;
	}

	/// <summary>
	/// Raises the event, invoking all live subscribers with the provided event data.
	/// Dead subscribers (whose targets have been garbage-collected) are removed.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	public async Task SendAsync(CancellationToken cancellationToken = default)
	{
		await _lock.WaitAsync(cancellationToken);

		_handlers.RemoveAll(x => !x.IsAlive);

		foreach (var handler in _handlers)
		{
			await handler.InvokeAsync(cancellationToken);
		}

		_lock.Release();
	}
}