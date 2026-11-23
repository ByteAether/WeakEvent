namespace ByteAether.WeakEvent;

/// <summary>
/// Represents a weak event that stores its subscribers using weak references.
/// Subscribers register an <see cref="Action{TEvent}"/>, <see cref="Func{TEvent, Task}"/> or <see cref="Func{TEvent, CancellationToken, Task}"/> and when an event is raised, only those whose target is still alive will be invoked.
/// </summary>
/// <remarks>
/// Weakly referenced event subscribers. Keep your .NET events lean and memory-safe.
/// For more information, visit <see href="https://github.com/ByteAether/WeakEvent">the GitHub repository</see>.
/// </remarks>
/// <typeparam name="TEvent">The type of the event argument.</typeparam>
public class WeakEvent<TEvent>
{
	private readonly WeakEventCore _core = new();

	/// <summary>
	/// Number of living subscribers currently registered to the event.
	/// </summary>
	public int SubscriberCount => _core.SubscriberCount;

	/// <summary>
	/// Subscribes the specified handler to the event.
	/// </summary>
	/// <param name="handler">
	/// The handler to subscribe. It will be invoked when the event is raised,
	/// provided that the target is still alive.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Subscribe(Action<TEvent> handler) => _core.Subscribe(handler, WeakEventHandler.Create);

	/// <summary>
	/// Subscribes the specified async handler to the event.
	/// </summary>
	/// <param name="handler">
	/// The handler to subscribe. It will be invoked when the event is raised,
	/// provided that the target is still alive.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Subscribe(Func<TEvent, Task> handler) => _core.Subscribe(handler, WeakEventHandler.Create);

	/// <summary>
	/// Subscribes the specified async handler with a cancellation token to the event.
	/// </summary>
	/// <param name="handler">
	/// The handler to subscribe. It will be invoked when the event is raised,
	/// provided that the target is still alive.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Subscribe(Func<TEvent, CancellationToken, Task> handler)
		=> _core.Subscribe(handler, WeakEventHandler.Create);

	/// <summary>
	/// Unsubscribes the specified handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public bool Unsubscribe(Action<TEvent> handler) => _core.Unsubscribe(handler);

	/// <summary>
	/// Unsubscribes the specified async handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public bool Unsubscribe(Func<TEvent, Task> handler) => _core.Unsubscribe(handler);

	/// <summary>
	/// Unsubscribes the specified async handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public bool Unsubscribe(Func<TEvent, CancellationToken, Task> handler) => _core.Unsubscribe(handler);

	/// <summary>
	/// Raises the event, invoking all live subscribers with the provided event data.
	/// Dead subscribers (whose targets have been garbage-collected) are removed.
	/// Subscribers are invoked sequentially from a snapshot captured when publication starts.
	/// </summary>
	/// <param name="eventData">The event data to publish to the subscribers.</param>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	public Task PublishAsync(TEvent eventData, CancellationToken cancellationToken = default)
		=> _core.PublishAsync(eventData, cancellationToken);
}

/// <summary>
/// Represents a weak event that stores its subscribers using weak references.
/// Subscribers register an <see cref="Action"/>, <see cref="Func{Task}"/> or <see cref="Func{CancellationToken, Task}"/> and when an event is raised, only those whose target is still alive will be invoked.
/// </summary>
/// <remarks>
/// Weakly referenced event subscribers. Keep your .NET events lean and memory-safe.
/// For more information, visit <see href="https://github.com/ByteAether/WeakEvent">the GitHub repository</see>.
/// </remarks>
public class WeakEvent
{
	private readonly WeakEventCore _core = new();

	/// <summary>
	/// Number of living subscribers currently registered to the event.
	/// </summary>
	public int SubscriberCount => _core.SubscriberCount;

	/// <summary>
	/// Subscribes the specified handler to the event.
	/// </summary>
	/// <param name="handler">
	/// The handler to subscribe. It will be invoked when the event is raised,
	/// provided that the target is still alive.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Subscribe(Action handler) => _core.Subscribe(handler, WeakEventHandler.Create);

	/// <summary>
	/// Subscribes the specified async handler to the event.
	/// </summary>
	/// <param name="handler">
	/// The handler to subscribe. It will be invoked when the event is raised,
	/// provided that the target is still alive.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Subscribe(Func<Task> handler) => _core.Subscribe(handler, WeakEventHandler.Create);

	/// <summary>
	/// Subscribes the specified async handler with a cancellation token to the event.
	/// </summary>
	/// <param name="handler">
	/// The handler to subscribe. It will be invoked when the event is raised,
	/// provided that the target is still alive.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Subscribe(Func<CancellationToken, Task> handler)
		=> _core.Subscribe(handler, WeakEventHandler.Create);

	/// <summary>
	/// Unsubscribes the specified handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public bool Unsubscribe(Action handler) => _core.Unsubscribe(handler);

	/// <summary>
	/// Unsubscribes the specified async handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public bool Unsubscribe(Func<Task> handler) => _core.Unsubscribe(handler);

	/// <summary>
	/// Unsubscribes the specified async handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public bool Unsubscribe(Func<CancellationToken, Task> handler) => _core.Unsubscribe(handler);

	/// <summary>
	/// Raises the event, invoking all live subscribers with the provided event data.
	/// Dead subscribers (whose targets have been garbage-collected) are removed.
	/// Subscribers are invoked sequentially from a snapshot captured when publication starts.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	public Task PublishAsync(CancellationToken cancellationToken = default)
		=> _core.PublishAsync<object?>(null, cancellationToken);
}