using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    public class ObjectiveInstance_UMFOSS
    {
        public ObjectiveData_UMFOSS Data { get; }

        public int CurrentCount { get; private set; }

        public bool IsRevealed { get; set; }

        public ObjectiveInstance_UMFOSS(ObjectiveData_UMFOSS data)
        {
            Data = data;
        }

        public bool IsComplete() => CurrentCount >= Data.requiredCount;

        public float GetProgress()
        {
            if (Data.requiredCount <= 0) return 1f;
            return Mathf.Clamp01((float)CurrentCount / Data.requiredCount);
        }

        public void Increment(int amount = 1)
        {
            if (IsComplete()) return;
            CurrentCount = Mathf.Min(CurrentCount + amount, Data.requiredCount);
        }

        public void SetCount(int count)
        {
            CurrentCount = Mathf.Clamp(count, 0, Data.requiredCount);
        }
    }
}
