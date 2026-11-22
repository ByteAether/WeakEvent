using System.Reflection;

namespace ByteAether.WeakEvent;

internal readonly struct WeakEventHandler
{
	private readonly Func<object?, object?, CancellationToken, Task> _invoke;
	private readonly MethodInfo _method;

	// If _weakTarget is null, the handler is static.
	private readonly WeakReference? _weakTarget;

	private WeakEventHandler(
		Delegate handler,
		Func<object?, object?, CancellationToken, Task> invoke)
	{
		_weakTarget = handler.Target != null ? new WeakReference(handler.Target) : null;
		_method = handler.Method;
		_invoke = invoke;
	}

	// If _weakTarget is null, the handler is static and always alive.
	public bool IsAlive => _weakTarget == null || _weakTarget.Target != null;

	public static WeakEventHandler Create(Action handler)
		=> Create(
			handler,
			(invoke, _, _) =>
			{
				invoke();
				return Task.CompletedTask;
			}
		);

	public static WeakEventHandler Create<TEvent>(Action<TEvent> handler)
		=> Create(
			handler,
			(invoke, eventData, _) =>
			{
				invoke((TEvent)eventData!);
				return Task.CompletedTask;
			}
		);

	public static WeakEventHandler Create(Func<Task> handler)
		=> Create(
			handler,
			(invoke, _, _) => RequireTask(invoke())
		);

	public static WeakEventHandler Create<TEvent>(Func<TEvent, Task> handler)
		=> Create(
			handler,
			(invoke, eventData, _) => RequireTask(invoke((TEvent)eventData!))
		);

	public static WeakEventHandler Create(Func<CancellationToken, Task> handler)
		=> Create(
			handler,
			(invoke, _, cancellationToken) => RequireTask(invoke(cancellationToken))
		);

	public static WeakEventHandler Create<TEvent>(Func<TEvent, CancellationToken, Task> handler)
		=> Create(
			handler,
			(invoke, eventData, cancellationToken) =>
				RequireTask(invoke((TEvent)eventData!, cancellationToken))
		);

	public Task InvokeAsync(object? eventData, CancellationToken cancellationToken = default)
	{
		// Get target first so it would not be GCed before the method is invoked.
		var target = _weakTarget?.Target;

		if (_weakTarget != null && target == null)
		{
			return Task.CompletedTask;
		}

		return _invoke(target, eventData, cancellationToken);
	}

	public bool Matches(Delegate handler)
		=> _weakTarget?.Target == handler.Target && _method.Equals(handler.Method);

	private static WeakEventHandler Create<TDelegate>(
		TDelegate handler,
		Func<TDelegate, object?, CancellationToken, Task> invoke
	)
		where TDelegate : Delegate
	{
		ThrowIfNull(handler);
		var method = handler.Method;

		return new WeakEventHandler(
			handler,
			(target, eventData, cancellationToken) =>
				invoke(GetDelegate<TDelegate>(method, target), eventData, cancellationToken)
		);
	}

	private static TDelegate GetDelegate<TDelegate>(MethodInfo method, object? target)
		where TDelegate : Delegate
		=> (TDelegate)(
			target == null
				? Delegate.CreateDelegate(typeof(TDelegate), method)
				: Delegate.CreateDelegate(typeof(TDelegate), target, method)
		);

	private static Task RequireTask(Task? task)
		=> task ?? throw new InvalidOperationException("An asynchronous event handler returned a null Task.");

	private static void ThrowIfNull(Delegate? handler)
	{
		if (handler == null)
		{
			throw new ArgumentNullException(nameof(handler));
		}
	}
}