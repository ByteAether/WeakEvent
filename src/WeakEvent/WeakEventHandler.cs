using System.Reflection;

namespace ByteAether.WeakEvent;

internal readonly struct WeakEventHandler<TEvent>
{
	private readonly WeakReference? _target;
	private readonly MethodInfo? _method;
	private readonly Action<TEvent>? _staticHandler;

	public WeakEventHandler(Action<TEvent> handler)
	{
		// If the handler is a static method, there is no target.
		if (handler.Target == null)
		{
			_staticHandler = handler;
		}
		else
		{
			_target = new WeakReference(handler.Target);
			_method = handler.Method;
		}
	}

	public Action<TEvent>? GetHandler()
	{
		if (_staticHandler != null)
		{
			return _staticHandler;
		}

		var target = _target!.Target;
		if (target != null)
		{
			// Recreate a delegate with the target and stored method.
			return (Action<TEvent>)Delegate.CreateDelegate(
				typeof(Action<TEvent>), target, _method!);
		}

		return null;
	}

	public bool Matches(Action<TEvent> handler)
	{
		return handler.Target == null
			? _staticHandler != null && _staticHandler.Equals(handler)
			: _target?.Target == handler.Target && _method!.Equals(handler.Method);
	}
}

internal readonly struct WeakEventHandler
{
	private readonly WeakReference? _target;
	private readonly MethodInfo? _method;
	private readonly Action? _staticHandler;

	public WeakEventHandler(Action handler)
	{
		// If the handler is a static method, there is no target.
		if (handler.Target == null)
		{
			_staticHandler = handler;
		}
		else
		{
			_target = new WeakReference(handler.Target);
			_method = handler.Method;
		}
	}

	public Action? GetHandler()
	{
		if (_staticHandler != null)
		{
			return _staticHandler;
		}

		var target = _target!.Target;
		if (target != null)
		{
			// Recreate a delegate with the target and stored method.
			return (Action)Delegate.CreateDelegate(
				typeof(Action), target, _method!);
		}

		return null;
	}

	public bool Matches(Action handler)
	{
		return handler.Target == null
			? _staticHandler != null && _staticHandler.Equals(handler)
			: _target?.Target == handler.Target && _method!.Equals(handler.Method);
	}
}
