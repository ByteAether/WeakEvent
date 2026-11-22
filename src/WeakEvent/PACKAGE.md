# WeakEvent
*from ByteAether*

[![License](https://img.shields.io/github/license/ByteAether/WeakEvent?logo=github&label=License)](https://github.com/ByteAether/WeakEvent/blob/main/LICENSE)
[![NuGet Version](https://img.shields.io/nuget/v/ByteAether.WeakEvent?logo=nuget&label=Version)](https://www.nuget.org/packages/ByteAether.WeakEvent/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ByteAether.WeakEvent?logo=nuget&label=Downloads)](https://www.nuget.org/packages/ByteAether.WeakEvent/)
[![GitHub Build Status](https://img.shields.io/github/actions/workflow/status/ByteAether/WeakEvent/build-and-test.yml?logo=github&label=Build%20%26%20Test)](https://github.com/ByteAether/WeakEvent/actions/workflows/build-and-test.yml)
[![GitHub Security](https://img.shields.io/github/actions/workflow/status/ByteAether/WeakEvent/codeql.yml?logo=github&label=Security%20Validation)](https://github.com/ByteAether/WeakEvent/actions/workflows/codeql.yml)

WeakEvent is a lightweight .NET implementation of the weak event pattern. A normal event can keep an inactive subscriber alive because the publisher holds a reference to its handler. WeakEvent breaks that retention by storing instance handlers with weak references, allowing subscriber objects to be reclaimed when no longer used elsewhere, even if they have not explicitly unsubscribed.

The library fits applications where long-lived publishers notify short-lived or repeatedly created subscribers, such as UI components, desktop applications, background services, and Blazor components. It supports synchronous and asynchronous handlers, notifications with or without payload data, and cancellation-aware publishing. Static handlers remain alive for the lifetime of the event source, while dead instance subscriptions are ignored and cleaned up during publication.

## Key Features

![.NET Standard 2.0](https://img.shields.io/badge/.NET_Standard-2.0-brightgreen)

- **Weak References:** Subscribers are held weakly so they can be garbage-collected when no longer in use.
- **Events With or Without Payloads:** Use `WeakEvent` for notifications or `WeakEvent<TEvent>` to pass event data.
- **Synchronous and Asynchronous Handlers:** Subscribe with `Action`, `Func<Task>`, or a cancellation-aware async delegate.
- **Automatic Cleanup:** Dead subscribers are removed during publication.
- **Simple API:** Methods for subscribing, unsubscribing, and publishing.

## Publication behavior

- Subscribers are invoked sequentially in subscription order.
- Publication uses a subscriber snapshot captured when it starts. Subscribing or unsubscribing during publication affects later publications, not the current one.
- Cancellation is checked before publication starts and before each subscriber is invoked. Cancellation-aware handlers receive the token passed to `PublishAsync`.
- Static handlers remain alive for the lifetime of the event source. Instance handlers, including capturing lambdas, are held weakly.
- Synchronous handler exceptions propagate unchanged. Asynchronous handler exceptions propagate after the returned task is awaited.
- Each invocation in a multicast delegate is subscribed independently and can be unsubscribed as a group.

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

// Create an event without payload data.
var myEvent = new WeakEvent();

// Create and subscribe a handler.
var subscriber = () => Console.WriteLine("Event received!");
myEvent.Subscribe(subscriber);

// Publish the event.
await myEvent.PublishAsync();
```

### Using the `WeakEvent<TEvent>`

```csharp
using ByteAether.WeakEvent;

// Create an event with payload data.
var myEvent = new WeakEvent<MyEventData>();

// Create and subscribe a handler.
var subscriber = (MyEventData data) => Console.WriteLine("Received: " + data.Message);
myEvent.Subscribe(subscriber);

// Publish the event.
await myEvent.PublishAsync(new MyEventData("Hello, World!"));

// Define your event data
public record MyEventData(string Message);
```

## API Overview

### `WeakEvent`
 * `SubscriberCount`\
   Number of live subscribers currently registered.
 * `Subscribe(Action handler)`\
   `Subscribe(Func<Task> handler)`\
   `Subscribe(Func<CancellationToken, Task> handler)`\
   Subscribes the handler. Each invocation in a multicast delegate is subscribed independently. Live handlers are invoked when the event is published.
 * `Unsubscribe(Action handler)`\
   `Unsubscribe(Func<Task> handler)`\
   `Unsubscribe(Func<CancellationToken, Task> handler)`\
   Unsubscribes the handler and returns whether at least one matching invocation was removed. A multicast delegate unsubscribes all matching invocations.
 * `PublishAsync(CancellationToken cancellationToken = default)`\
   Invokes live subscribers sequentially and removes dead subscribers.

### `WeakEvent<TEvent>`
 * `SubscriberCount`\
   Number of live subscribers currently registered.
 * `Subscribe(Action<TEvent> handler)`\
   `Subscribe(Func<TEvent, Task> handler)`\
   `Subscribe(Func<TEvent, CancellationToken, Task> handler)`\
   Subscribes the handler. Each invocation in a multicast delegate is subscribed independently. Live handlers are invoked when the event is published.
 * `Unsubscribe(Action<TEvent> handler)`\
   `Unsubscribe(Func<TEvent, Task> handler)`\
   `Unsubscribe(Func<TEvent, CancellationToken, Task> handler)`\
   Unsubscribes the handler and returns whether at least one matching invocation was removed. A multicast delegate unsubscribes all matching invocations.
 * `PublishAsync(TEvent eventData, CancellationToken cancellationToken = default)`\
   Invokes live subscribers sequentially with `eventData` and removes dead subscribers.
