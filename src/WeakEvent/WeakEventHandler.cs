using System.Reflection;

namespace ByteAether.WeakEvent;

internal readonly struct WeakEventHandler(Delegate handler)
{
	// If _weakTarget is null, the handler is static.
	private readonly WeakReference? _weakTarget = handler.Target != null ? new WeakReference(handler.Target) : null;
	private readonly MethodInfo _method = handler.Method;
	private readonly bool _hasCtParam = handler.Method.GetParameters().LastOrDefault()?.ParameterType == typeof(CancellationToken);

	// If _weakTarget is null, the handler is static and always alive.
	public bool IsAlive => _weakTarget == null || _weakTarget.Target != null;

	public Task InvokeAsync(List<object?> args, CancellationToken cancellationToken = default)
	{
		// Get target first so it would not be GCed before the method is invoked.
		var target = _weakTarget?.Target;

		if (!IsAlive)
		{
			return Task.CompletedTask;
		}

		if (_hasCtParam)
		{
			args.Add(cancellationToken);
		}

		return _method.Invoke(target, [.. args]) as Task ?? Task.CompletedTask;
	}

	public bool Matches(Delegate handler)
		=> _weakTarget?.Target == handler.Target && _method.Equals(handler.Method);
}