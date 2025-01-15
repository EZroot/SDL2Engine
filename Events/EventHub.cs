using System;
using System.Collections.Concurrent;
using System.Threading;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Events
{
    public static class EventHub
    {
        // Use ConcurrentDictionary for thread-safe access
        private static readonly ConcurrentDictionary<Type, Delegate> eventHandlers = new ConcurrentDictionary<Type, Delegate>();

        /// <summary>
        /// Subscribes a handler to a specific event type.
        /// </summary>
        public static void Subscribe<TEventArgs>(EventHandler<TEventArgs> handler) where TEventArgs : EventArgs
        {
            var type = typeof(TEventArgs);
            eventHandlers.AddOrUpdate(
                type,
                handler,
                (key, existingHandler) => Delegate.Combine(existingHandler, handler)
            );

            Debug.LogEvents($"Subscribed to event <color=white> {type.Name}");
        }

        /// <summary>
        /// Unsubscribes a handler from a specific event type.
        /// </summary>
        public static void Unsubscribe<TEventArgs>(EventHandler<TEventArgs> handler) where TEventArgs : EventArgs
        {
            var type = typeof(TEventArgs);
            bool updated = false;

            eventHandlers.AddOrUpdate(
                type,
                null, // If the type doesn't exist, there's nothing to remove
                (key, existingHandler) =>
                {
                    var newHandler = Delegate.Remove(existingHandler, handler);
                    if (newHandler == null)
                    {
                        updated = eventHandlers.TryRemove(key, out _);
                        return null;
                    }
                    return newHandler;
                }
            );

            if (updated || eventHandlers.ContainsKey(type))
            {
                Debug.LogEvents($"Unsubscribed from event <color=white> {type.Name}");
            }
        }

        /// <summary>
        /// Raises an event, invoking all subscribed handlers.
        /// </summary>
        public static void Raise<TEventArgs>(object sender, TEventArgs e) where TEventArgs : EventArgs
        {
            var type = typeof(TEventArgs);
            if (eventHandlers.TryGetValue(type, out var handler))
            {
                // Cast to the appropriate delegate type
                var eventHandler = handler as EventHandler<TEventArgs>;
                eventHandler?.Invoke(sender, e);
                Debug.LogEvents($"Invoked event <color=white> {type.Name}");
            }
        }
    }
}
