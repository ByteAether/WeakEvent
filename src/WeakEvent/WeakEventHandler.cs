using System.Reflection;

namespace ByteAether.WeakEvent;

internal readonly struct WeakEventHandler<TEvent>(Delegate handler)
{
	private readonly WeakReference? _weakTarget = handler.Target != null ? new WeakReference(handler.Target) : null;
	private readonly MethodInfo _method = handler.Method;

	public bool IsAlive => _weakTarget == null || _weakTarget.Target != null;

	public Task InvokeAsync(TEvent eventData, CancellationToken cancellationToken = default)
	{
		var target = _weakTarget?.Target;

		if (target == null && _weakTarget != null)
		{
			return Task.CompletedTask;
		}

		object?[] arguments = _method.GetParameters().Length == 2
			? [eventData, cancellationToken]
			: [eventData];

		var invokeReturn = _method.Invoke(target, arguments);

		return _method.ReturnType == typeof(Task)
			? (Task)invokeReturn!
			: Task.CompletedTask;
	}

	public bool Matches(Delegate handler)
		=> _weakTarget?.Target == handler.Target && _method.Equals(handler.Method);
}

internal readonly struct WeakEventHandler(Delegate handler)
{
	private readonly WeakReference? _weakTarget = handler.Target != null ? new WeakReference(handler.Target) : null;
	private readonly MethodInfo _method = handler.Method;

	public bool IsAlive => _weakTarget == null || _weakTarget.Target != null;

	public Task InvokeAsync(CancellationToken cancellationToken = default)
	{
		var target = _weakTarget?.Target;

		if (target == null && _weakTarget != null)
		{
			return Task.CompletedTask;
		}

		object?[] arguments = _method.GetParameters().Length == 1
			? [cancellationToken]
			: [];

		var invokeReturn = _method.Invoke(target, arguments);

		return _method.ReturnType == typeof(Task)
			? (Task)invokeReturn!
			: Task.CompletedTask;
	}

	public bool Matches(Delegate handler)
		=> _weakTarget?.Target == handler.Target && _method.Equals(handler.Method);
}