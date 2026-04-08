using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    public class QuestInstance_UMFOSS
    {
        public QuestData_UMFOSS Data { get; }

        public QuestState State { get; set; }

        public float StartTime { get; set; }

        public float CompletionTime { get; set; }

        public List<ObjectiveInstance_UMFOSS> Objectives { get; }

        public QuestInstance_UMFOSS(QuestData_UMFOSS data, IEnumerable<ObjectiveInstance_UMFOSS> objectives)
        {
            Data = data;
            Objectives = objectives.ToList();
        }

        public bool IsComplete()
        {
            return Objectives.Where(o => !o.Data.isOptional).All(o => o.IsComplete());
        }

        public bool IsFailed() => State == QuestState.Failed;

        public float GetProgress()
        {
            var required = Objectives.Where(o => !o.Data.isOptional).ToList();
            if (required.Count == 0) return 1f;
            return required.Average(o => o.GetProgress());
        }
    }
}
