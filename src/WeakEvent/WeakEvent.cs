namespace ByteAether.WeakEvent;

/// <summary>
/// Represents a weak event that stores its subscribers using weak references.
/// Subscribers register an <see cref="Action{TEvent}"/> and when an event is raised,
/// only those whose target is still alive will be invoked.
/// </summary>
/// <typeparam name="TEvent">The type of the event arguments.</typeparam>
public class WeakEvent<TEvent>
{
	private readonly List<WeakEventHandler<TEvent>> _handlers = [];

#if NET9_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif

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
	/// Raises the event, invoking all live subscribers with the provided event data.
	/// Dead subscribers (whose targets have been garbage-collected) are removed.
	/// </summary>
	/// <param name="eventData">The event data to send to the subscribers.</param>
	public void Send(TEvent eventData)
	{
		List<WeakEventHandler<TEvent>>? deadHandlers = null;

		lock (_lock)
		{
			foreach (var weakHandler in _handlers)
			{
				var handler = weakHandler.GetHandler();
				if (handler != null)
				{
					// Invoke the live handler.
					handler(eventData);
				}
				else
				{
					// Remember handlers whose target has been garbage-collected.
					deadHandlers ??= [];
					deadHandlers.Add(weakHandler);
				}
			}

			// Remove dead handlers from the list.
			if (deadHandlers != null)
			{
				foreach (var dead in deadHandlers)
				{
					_handlers.Remove(dead);
				}
			}
		}
	}
}

/// <summary>
/// Represents a weak event that stores its subscribers using weak references.
/// Subscribers register an <see cref="Action"/> and when an event is raised,
/// only those whose target is still alive will be invoked.
/// </summary>
public class WeakEvent
{
	private readonly List<WeakEventHandler> _handlers = [];
#if NET9_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif

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
	/// Raises the event, invoking all live subscribers.
	/// Dead subscribers (whose targets have been garbage-collected) are removed.
	/// </summary>
	public void Send()
	{
		List<WeakEventHandler>? deadHandlers = null;

		lock (_lock)
		{
			foreach (var weakHandler in _handlers)
			{
				var handler = weakHandler.GetHandler();
				if (handler != null)
				{
					// Invoke the live handler.
					handler();
				}
				else
				{
					// Remember handlers whose target has been garbage-collected.
					deadHandlers ??= [];
					deadHandlers.Add(weakHandler);
				}
			}

			// Remove dead handlers from the list.
			if (deadHandlers != null)
			{
				foreach (var dead in deadHandlers)
				{
					_handlers.Remove(dead);
				}
			}
		}
	}
}