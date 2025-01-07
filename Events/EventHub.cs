using SDL2Engine.Core.Utils;

namespace SDL2Engine.Events
{
    public static class EventHub
    {
        private static readonly Dictionary<Type, Delegate> eventHandlers = new Dictionary<Type, Delegate>();

        public static void Subscribe<TEventArgs>(EventHandler<TEventArgs> handler) where TEventArgs : EventArgs
        {
            var type = typeof(TEventArgs);
            if (!eventHandlers.ContainsKey(type))
            {
                eventHandlers[type] = null;
            }
            eventHandlers[type] = (EventHandler<TEventArgs>)eventHandlers[type] + handler;
            Debug.LogEvents($"Subscribed to event</color><color=white> {type.Name}");
        }

        public static void Unsubscribe<TEventArgs>(EventHandler<TEventArgs> handler) where TEventArgs : EventArgs
        {
            var type = typeof(TEventArgs);
            if (eventHandlers.ContainsKey(type))
            {
                eventHandlers[type] = (EventHandler<TEventArgs>)eventHandlers[type] - handler;
                Debug.LogEvents($"Unsubscribed from event</color><color=white> {type.Name}");
            }
        }

        public static void Raise<TEventArgs>(object sender, TEventArgs e) where TEventArgs : EventArgs
        {
            var type = typeof(TEventArgs);
            if (eventHandlers.ContainsKey(type))
            {
                var handler = (EventHandler<TEventArgs>)eventHandlers[type];
                handler?.Invoke(sender, e);
                Debug.LogEvents($"Invoked event</color> <color=white>{type.Name}");
            }
        }
    }
}