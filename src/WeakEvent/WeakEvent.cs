﻿namespace ByteAether.WeakEvent;

/// <summary>
/// Represents a weak event that stores its subscribers using weak references.
/// Subscribers register an <see cref="Action{TEvent}"/>, <see cref="Func{TEvent, Task}"/> or <see cref="Func{TEvent, CancellationToken, Task}"/> and when an event is raised, only those whose target is still alive will be invoked.
/// </summary>
/// <remarks>
/// Weakly referenced event subscribers. Keep your .NET events lean and memory-safe.
/// For more information, visit <see href="https://github.com/ByteAether/WeakEvent">the GitHub repository</see>.
/// </remarks>
/// <typeparam name="TEvent">The type of the event argument.</typeparam>
public class WeakEvent<TEvent> : WeakEventBase
{
	/// <summary>
	/// Subscribes the specified handler to the event.
	/// </summary>
	/// <param name="handler">
	/// The handler to subscribe. It will be invoked when the event is raised,
	/// provided that the target is still alive.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Subscribe(Action<TEvent> handler) => base.Subscribe(handler);

	/// <summary>
	/// Subscribes the specified async handler to the event.
	/// </summary>
	/// <param name="handler">
	/// The handler to subscribe. It will be invoked when the event is raised,
	/// provided that the target is still alive.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Subscribe(Func<TEvent, Task> handler) => base.Subscribe(handler);

	/// <summary>
	/// Subscribes the specified async handler with a cancellation token to the event.
	/// </summary>
	/// <param name="handler">
	/// The handler to subscribe. It will be invoked when the event is raised,
	/// provided that the target is still alive.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Subscribe(Func<TEvent, CancellationToken, Task> handler) => base.Subscribe(handler);

	/// <summary>
	/// Unsubscribes the specified handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public bool Unsubscribe(Action<TEvent> handler) => base.Unsubscribe(handler);

	/// <summary>
	/// Unsubscribes the specified async handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public bool Unsubscribe(Func<TEvent, Task> handler) => base.Unsubscribe(handler);

	/// <summary>
	/// Unsubscribes the specified async handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public bool Unsubscribe(Func<TEvent, CancellationToken, Task> handler) => base.Unsubscribe(handler);

	/// <summary>
	/// Raises the event, invoking all live subscribers with the provided event data.
	/// Dead subscribers (whose targets have been garbage-collected) are removed.
	/// </summary>
	/// <param name="eventData">The event data to publish to the subscribers.</param>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	public Task PublishAsync(TEvent eventData, CancellationToken cancellationToken = default)
		=> base.PublishAsync([eventData], cancellationToken);
}

/// <summary>
/// Represents a weak event that stores its subscribers using weak references.
/// Subscribers register an <see cref="Action"/>, <see cref="Func{Task}"/> or <see cref="Func{CancellationToken, Task}"/> and when an event is raised, only those whose target is still alive will be invoked.
/// </summary>
/// <remarks>
/// Weakly referenced event subscribers. Keep your .NET events lean and memory-safe.
/// For more information, visit <see href="https://github.com/ByteAether/WeakEvent">the GitHub repository</see>.
/// </remarks>
public class WeakEvent : WeakEventBase
{
	/// <summary>
	/// Subscribes the specified handler to the event.
	/// </summary>
	/// <param name="handler">
	/// The handler to subscribe. It will be invoked when the event is raised,
	/// provided that the target is still alive.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Subscribe(Action handler) => base.Subscribe(handler);

	/// <summary>
	/// Subscribes the specified async handler to the event.
	/// </summary>
	/// <param name="handler">
	/// The handler to subscribe. It will be invoked when the event is raised,
	/// provided that the target is still alive.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Subscribe(Func<Task> handler) => base.Subscribe(handler);

	/// <summary>
	/// Subscribes the specified async handler with a cancellation token to the event.
	/// </summary>
	/// <param name="handler">
	/// The handler to subscribe. It will be invoked when the event is raised,
	/// provided that the target is still alive.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public void Subscribe(Func<CancellationToken, Task> handler) => base.Subscribe(handler);

	/// <summary>
	/// Unsubscribes the specified handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public bool Unsubscribe(Action handler) => base.Unsubscribe(handler);

	/// <summary>
	/// Unsubscribes the specified async handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public bool Unsubscribe(Func<Task> handler) => base.Unsubscribe(handler);

	/// <summary>
	/// Unsubscribes the specified async handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	public bool Unsubscribe(Func<CancellationToken, Task> handler) => base.Unsubscribe(handler);

	/// <summary>
	/// Raises the event, invoking all live subscribers with the provided event data.
	/// Dead subscribers (whose targets have been garbage-collected) are removed.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	public Task PublishAsync(CancellationToken cancellationToken = default) => base.PublishAsync([], cancellationToken);
}

/// <summary>
/// Represents a weak event that stores its subscribers using weak references.
/// Subscribers register a <see cref="Delegate"/> and when an event is raised, only those whose target is still alive will be invoked.
/// </summary>
public abstract class WeakEventBase
{
	private readonly List<WeakEventHandler> _handlers = [];
	private readonly ReaderWriterLockSlim _rwLock = new();

	/// <summary>
	/// Number of living subscribers currently registered to the event.
	/// </summary>
	public int SubscriberCount
	{
		get
		{
			_rwLock.EnterReadLock();
			try
			{
				return _handlers.Count(x => x.IsAlive);
			}
			finally
			{
				_rwLock.ExitReadLock();
			}
		}
	}

	/// <summary>
	/// Subscribes the specified delegate handler to the event.
	/// </summary>
	/// <param name="handler">
	/// The handler to subscribe. It will be invoked when the event is raised,
	/// provided that the target is still alive.
	/// </param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	protected void Subscribe(Delegate handler)
	{
		if (handler is null)
		{
			throw new ArgumentNullException(nameof(handler));
		}

		_rwLock.EnterWriteLock();
		try
		{
			_handlers.Add(new WeakEventHandler(handler));
		}
		finally
		{
			_rwLock.ExitWriteLock();
		}
	}

	/// <summary>
	/// Unsubscribes the specified delegate handler from the event.
	/// </summary>
	/// <param name="handler">The handler to unsubscribe.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="handler"/> is null.</exception>
	protected bool Unsubscribe(Delegate handler)
	{
		if (handler is null)
		{
			throw new ArgumentNullException(nameof(handler));
		}

		_rwLock.EnterWriteLock();
		try
		{
			return _handlers.RemoveAll(weh => weh.Matches(handler)) > 0;
		}
		finally
		{
			_rwLock.ExitWriteLock();
		}
	}

	/// <summary>
	/// Raises the event, invoking all live subscribers with the provided event data.
	/// Dead subscribers (whose targets have been garbage-collected) are removed.
	/// </summary>
	/// <param name="args">Invocation arguments for each delegate</param>
	/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
	protected async Task PublishAsync(List<object?> args, CancellationToken cancellationToken = default)
	{
		List<WeakEventHandler> handlers;

		_rwLock.EnterUpgradeableReadLock();

		try
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (_handlers.Any(x => !x.IsAlive))
			{
				_rwLock.EnterWriteLock();
				try
				{
					_handlers.RemoveAll(x => !x.IsAlive);
				}
				finally
				{
					_rwLock.ExitWriteLock();
				}
			}

			handlers = [.. _handlers];
		}
		finally
		{
			_rwLock.ExitUpgradeableReadLock();
		}

		cancellationToken.ThrowIfCancellationRequested();

		foreach (var handler in handlers)
		{
			await handler.InvokeAsync(args, cancellationToken);
		}
	}
}