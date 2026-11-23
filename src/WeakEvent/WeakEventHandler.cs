using System.Reflection;

namespace ByteAether.WeakEvent;

internal readonly struct WeakEventHandler
{
	private readonly Delegate _invoke;
	private readonly MethodInfo _method;

	// If _weakTarget is null, the handler is static.
	private readonly WeakReference? _weakTarget;

	private WeakEventHandler(
		Delegate handler,
		Delegate invoke)
	{
		_weakTarget = handler.Target != null ? new WeakReference(handler.Target) : null;
		_method = handler.Method;
		_invoke = invoke;
	}

	// If _weakTarget is null, the handler is static and always alive.
	public bool IsAlive => _weakTarget == null || _weakTarget.Target != null;

	public static WeakEventHandler Create(Action handler)
		=> Create<Action, object?>(
			handler,
			(invoke, _, _) =>
			{
				invoke();
				return Task.CompletedTask;
			}
		);

	public static WeakEventHandler Create<TEvent>(Action<TEvent> handler)
		=> Create<Action<TEvent>, TEvent>(
			handler,
			(invoke, eventData, _) =>
			{
				invoke((TEvent)eventData!);
				return Task.CompletedTask;
			}
		);

	public static WeakEventHandler Create(Func<Task> handler)
		=> Create<Func<Task>, object?>(
			handler,
			(invoke, _, _) => RequireTask(invoke())
		);

	public static WeakEventHandler Create<TEvent>(Func<TEvent, Task> handler)
		=> Create<Func<TEvent, Task>, TEvent>(
			handler,
			(invoke, eventData, _) => RequireTask(invoke((TEvent)eventData!))
		);

	public static WeakEventHandler Create(Func<CancellationToken, Task> handler)
		=> Create<Func<CancellationToken, Task>, object?>(
			handler,
			(invoke, _, cancellationToken) => RequireTask(invoke(cancellationToken))
		);

	public static WeakEventHandler Create<TEvent>(Func<TEvent, CancellationToken, Task> handler)
		=> Create<Func<TEvent, CancellationToken, Task>, TEvent>(
			handler,
			(invoke, eventData, cancellationToken) =>
				RequireTask(invoke((TEvent)eventData!, cancellationToken))
		);

	public Task InvokeAsync(object? eventData, CancellationToken cancellationToken = default)
		=> InvokeAsync<object?>(eventData, cancellationToken);

	public Task InvokeAsync<TEvent>(
		TEvent eventData,
		CancellationToken cancellationToken = default
	)
	{
		var target = GetTarget();

		if (target == null && _weakTarget != null)
		{
			return Task.CompletedTask;
		}

		return ((Func<object?, TEvent, CancellationToken, Task>)_invoke)(
			target,
			eventData,
			cancellationToken
		);
	}

	public bool Matches(Delegate handler)
		=> ReferenceEquals(_weakTarget?.Target, handler.Target) && _method.Equals(handler.Method);

	private static WeakEventHandler Create<TDelegate, TEvent>(
		TDelegate handler,
		Func<TDelegate, TEvent, CancellationToken, Task> invoke
	)
		where TDelegate : Delegate
	{
		ThrowIfNull(handler);

		// Delegates for static methods and open-instance methods both have no target.
		// Retaining those delegates is safe and avoids reconstructing them as static delegates.
		if (handler.Target == null)
		{
			return CreateTargetless(handler, invoke);
		}

		var method = handler.Method;

		return CreateHandler(
			handler,
			(Func<object?, TEvent, CancellationToken, Task>)(
				(target, eventData, cancellationToken) =>
					invoke(GetDelegate<TDelegate>(method, target), eventData, cancellationToken)
			)
		);
	}

	private static WeakEventHandler CreateTargetless<TDelegate, TEvent>(
		TDelegate handler,
		Func<TDelegate, TEvent, CancellationToken, Task> invoke
	)
		where TDelegate : Delegate
		=> CreateHandler(
			handler,
			(Func<object?, TEvent, CancellationToken, Task>)(
				(_, eventData, cancellationToken) =>
					invoke(handler, eventData, cancellationToken)
			)
		);

	private static WeakEventHandler CreateHandler<TEvent>(
		Delegate handler,
		Func<object?, TEvent, CancellationToken, Task> invoke
	) => new(handler, invoke);

	private object? GetTarget() => _weakTarget?.Target;

	private static TDelegate GetDelegate<TDelegate>(MethodInfo method, object? target)
		where TDelegate : Delegate
		=> (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), target!, method);

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