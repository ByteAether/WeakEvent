# WeakEvent

[![License](https://img.shields.io/github/license/ByteAether/WeakEvent?logo=github&label=License)](https://github.com/ByteAether/WeakEvent/blob/main/LICENSE)
[![NuGet Version](https://img.shields.io/nuget/v/ByteAether.WeakEvent?logo=nuget&label=Version)](https://www.nuget.org/packages/ByteAether.WeakEvent/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ByteAether.WeakEvent?logo=nuget&label=Downloads)](https://www.nuget.org/packages/ByteAether.WeakEvent/)
[![GitHub Build Status](https://img.shields.io/github/actions/workflow/status/ByteAether/WeakEvent/build-and-test.yml?logo=github&label=Build%20%26%20Test)](https://github.com/ByteAether/WeakEvent/actions/workflows/build-and-test.yml)
[![GitHub Security](https://img.shields.io/github/actions/workflow/status/ByteAether/WeakEvent/codeql.yml?logo=github&label=Security%20Validation)](https://github.com/ByteAether/WeakEvent/actions/workflows/codeql.yml)

WeakEvent is a lightweight .NET library that implements the weak event pattern using weak references. It ensures that event subscribers don't prevent garbage collection, avoiding memory leaks.

## Key Features

![.NET Standard 2.0](https://img.shields.io/badge/.NET_Standard-2.0-brightgreen)

- **Weak References:** Subscribers are held weakly so they can be garbage-collected when no longer in use.
- **Events With or Without Data:** Choose between `WeakEvent` for notifications without data and `WeakEvent<TEvent>` for events that pass data.
- **Automatic Cleanup:** Dead subscribers are removed automatically during event publishing.
- **Simple API:** Intuitive methods for subscribing, unsubscribing, and publishing events.

## Installation

Install via NuGet:

```sh
dotnet add package ByteAether.WeakEvent
```

Use the `--version` option to specify a [preview version](https://www.nuget.org/packages/ByteAether.WeakEvent/absoluteLatest) to install.

## Usage

### Using the `WeakEvent`

```csharp
using ByteAether.WeakEvent;

// Create an instance of the weak event without event data
var myEvent = new WeakEvent();

// Create a subscriber and subscribe
var subscriber = () => Console.WriteLine("Event received!");
myEvent.Subscribe(subscriber);

// Raise the event
await myEvent.PublishAsync();
```

### Using the `WeakEvent<TEvent>`

```csharp
using ByteAether.WeakEvent;

// Create an instance of the weak event with event data
var myEvent = new WeakEvent<MyEventData>();

// Create a subscriber and subscribe
var subscriber = (MyEventData data) => Console.WriteLine("Received: " + data.Message);
myEvent.Subscribe(subscriber);

// Raise the event
await myEvent.PublishAsync(new MyEventData("Hello, World!"));

// Define your event data
public record MyEventData(string Message);
```

## API Overview

### `WeakEvent`
 * `Subscribe(Action handler)`\
   `Subscribe(Func<Task> handler)`\
   `Subscribe(Func<CancellationToken, Task> handler)`\
   Subscribes the specified handler to the event. The handler will be invoked when the event is raised, provided that its target is still alive.
 * `Unsubscribe(Action handler)`\
   `Unsubscribe(Func<Task> handler)`\
   `Unsubscribe(Func<CancellationToken, Task> handler)`\
   Unsubscribes the specified handler from the event.
 * `PublishAsync(CancellationToken cancellationToken = default)`\
   Raises the event by invoking all live subscribers. Dead subscribers (whose targets have been garbage-collected) are removed.

### `WeakEvent<TEvent>`
 * `Subscribe(Action<TEvent> handler)`\
   `Subscribe(Func<TEvent, Task> handler)`\
   `Subscribe(Func<TEvent, CancellationToken, Task> handler)`\
   Subscribes the specified handler to the event. The handler will be invoked when the event is raised, provided that its target is still alive.
 * `Unsubscribe(Action<TEvent> handler)`\
   `Unsubscribe(Func<TEvent, Task> handler)`\
   `Unsubscribe(Func<TEvent, CancellationToken, Task> handler)`\
   Unsubscribes the specified handler from the event.
 * `PublishAsync(TEvent eventData, CancellationToken cancellationToken = default)`\
   Raises the event by invoking all live subscribers with the provided event data. Dead subscribers (whose targets have been garbage-collected) are removed.