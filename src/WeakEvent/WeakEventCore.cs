namespace ByteAether.WeakEvent;

internal sealed class WeakEventCore
{
	private readonly List<WeakEventHandler> _handlers = [];
	private readonly ReaderWriterLockSlim _rwLock = new();

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

	public void Subscribe<TDelegate>(
		TDelegate handler,
		Func<TDelegate, WeakEventHandler> createHandler
	)
		where TDelegate : Delegate
	{
		if (handler is null)
		{
			throw new ArgumentNullException(nameof(handler));
		}

		var handlers = handler
			.GetInvocationList()
			.Select(invocation => createHandler((TDelegate)invocation))
			.ToArray();

		_rwLock.EnterWriteLock();
		try
		{
			_handlers.AddRange(handlers);
		}
		finally
		{
			_rwLock.ExitWriteLock();
		}
	}

	public bool Unsubscribe(Delegate handler)
	{
		if (handler is null)
		{
			throw new ArgumentNullException(nameof(handler));
		}

		var handlers = handler.GetInvocationList();

		_rwLock.EnterWriteLock();
		try
		{
			return _handlers.RemoveAll(
				weakHandler => handlers.Any(weakHandler.Matches)
			) > 0;
		}
		finally
		{
			_rwLock.ExitWriteLock();
		}
	}

	public async Task PublishAsync<TEvent>(
		TEvent eventData,
		CancellationToken cancellationToken = default
	)
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

		foreach (var handler in handlers)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await handler.InvokeAsync(eventData, cancellationToken);
		}
	}
}