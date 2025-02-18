using System.Reflection;

namespace ByteAether.WeakEvent;

internal readonly struct WeakEventHandler<TEvent>(Delegate handler)
{
	private readonly WeakReference? _weakTarget = handler.Target != null ? new WeakReference(handler.Target) : null;
	private readonly MethodInfo _method = handler.Method;

	public bool IsAlive => _weakTarget == null || _weakTarget.Target != null;

	public Task InvokeAsync(TEvent eventData)
	{
		if (!IsAlive)
		{
			return Task.CompletedTask;
		}

		var target = _weakTarget?.Target;

		if (_method.ReturnType == typeof(Task))
		{
			return (Task)_method.Invoke(target, [eventData])!;
		}
		else
		{
			_method.Invoke(target, [eventData]);
			return Task.CompletedTask;
		}
	}

	public bool Matches(Delegate handler)
		=> _weakTarget?.Target == handler.Target && _method.Equals(handler.Method);
}

internal readonly struct WeakEventHandler(Delegate handler)
{
	private readonly WeakReference? _weakTarget = handler.Target != null ? new WeakReference(handler.Target) : null;
	private readonly MethodInfo _method = handler.Method;

	public bool IsAlive => _weakTarget == null || _weakTarget.Target != null;

	public Task InvokeAsync()
	{
		if (!IsAlive)
		{
			return Task.CompletedTask;
		}

		var target = _weakTarget?.Target;

		if (_method.ReturnType == typeof(Task))
		{
			return (Task)_method.Invoke(target, [])!;
		}
		else
		{
			_method.Invoke(target, []);
			return Task.CompletedTask;
		}
	}

	public bool Matches(Delegate handler)
		=> _weakTarget?.Target == handler.Target && _method.Equals(handler.Method);
}