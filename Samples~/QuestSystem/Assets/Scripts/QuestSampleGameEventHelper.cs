using System.Collections.Generic;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Samples.QuestSystem
{
    /// <summary>
    /// Publishes <see cref="GameEventPayload"/> for quest matching — gameplay stays decoupled from quests.
    /// </summary>
    public static class QuestSampleGameEventHelper
    {
        public static void Publish(string eventType, Dictionary<string, string> properties)
        {
            EventBus.Publish(new GameEventPayload(eventType, properties));
        }

        public static void Publish(string eventType, string key, string value)
        {
            var dict = new Dictionary<string, string> { { key, value } };
            Publish(eventType, dict);
        }
    }
}
