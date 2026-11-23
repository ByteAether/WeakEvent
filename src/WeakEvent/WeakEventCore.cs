namespace ByteAether.WeakEvent;

internal sealed class WeakEventCore
{
	private readonly object _sync = new();
	private WeakEventHandler[] _handlers = [];

	public int SubscriberCount
	{
		get
		{
			// ReSharper disable once InconsistentlySynchronizedField
			var handlers = Volatile.Read(ref _handlers);
			var count = 0;

			foreach (var handler in handlers)
			{
				if (handler.IsAlive)
				{
					count++;
				}
			}

			return count;
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

		var invocations = handler.GetInvocationList();
		var handlers = new WeakEventHandler[invocations.Length];

		for (var i = 0; i < invocations.Length; i++)
		{
			handlers[i] = createHandler((TDelegate)invocations[i]);
		}

		lock (_sync)
		{
			var combined = new WeakEventHandler[_handlers.Length + handlers.Length];
			_handlers.CopyTo(combined, 0);
			handlers.CopyTo(combined, _handlers.Length);
			Volatile.Write(ref _handlers, combined);
		}
	}

	public bool Unsubscribe(Delegate handler)
	{
		if (handler is null)
		{
			throw new ArgumentNullException(nameof(handler));
		}

		var invocations = handler.GetInvocationList();

		lock (_sync)
		{
			var handlers = _handlers;
			var remaining = new WeakEventHandler[handlers.Length];
			var count = 0;

			foreach (var weakHandler in handlers)
			{
				var remove = false;

				foreach (var invocation in invocations)
				{
					if (weakHandler.Matches(invocation))
					{
						remove = true;
						break;
					}
				}

				if (!remove)
				{
					remaining[count++] = weakHandler;
				}
			}

			if (count == handlers.Length)
			{
				return false;
			}

			Array.Resize(ref remaining, count);
			Volatile.Write(ref _handlers, remaining);
			return true;
		}
	}

	public async Task PublishAsync<TEvent>(
		TEvent eventData,
		CancellationToken cancellationToken = default
	)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var handlers = GetLiveSnapshot();

		foreach (var handler in handlers)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await handler.InvokeAsync(eventData, cancellationToken);
		}
	}

	private WeakEventHandler[] GetLiveSnapshot()
	{
		// ReSharper disable once InconsistentlySynchronizedField
		var handlers = Volatile.Read(ref _handlers);
		var liveCount = 0;

		foreach (var handler in handlers)
		{
			if (handler.IsAlive)
			{
				liveCount++;
			}
		}

		if (liveCount == handlers.Length)
		{
			return handlers;
		}

		var liveHandlers = new WeakEventHandler[liveCount];
		var index = 0;

		foreach (var handler in handlers)
		{
			if (handler.IsAlive)
			{
				liveHandlers[index++] = handler;
			}
		}

		if (index < liveHandlers.Length)
		{
			Array.Resize(ref liveHandlers, index);
		}

		lock (_sync)
		{
			if (ReferenceEquals(_handlers, handlers))
			{
				Volatile.Write(ref _handlers, liveHandlers);
			}
		}

		return liveHandlers;
	}
}
