using System.Collections.Generic;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// String-based bridge so the quest system can match objectives without referencing concrete event types.
    /// Publish alongside typed events: <c>EventBus.Publish(new GameEventPayload { ... })</c>.
    /// </summary>
    public struct GameEventPayload
    {
        /// <summary>Original event type name, e.g. <c>EnemyDiedEvent</c>.</summary>
        public string EventType;

        /// <summary>Serialized property bag for filtering (e.g. enemyType → Goblin).</summary>
        public Dictionary<string, string> Properties;

        public GameEventPayload(string eventType, Dictionary<string, string> properties)
        {
            EventType = eventType;
            Properties = properties ?? new Dictionary<string, string>();
        }
    }
}
