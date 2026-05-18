namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Event delegates for the Save System.
    /// Subscribe to these events to react to save/load operations
    /// without coupling to the SaveSystem directly.
    /// </summary>
    public static class SaveSystemEvents_UMFOSS
    {
        /// <summary>Fired after a successful save operation.</summary>
        /// <param name="slotName">The slot that was saved to.</param>
        public delegate void SaveEvent(string slotName);

        /// <summary>Fired after a successful load operation.</summary>
        /// <param name="slotName">The slot that was loaded from.</param>
        public delegate void LoadEvent(string slotName);

        /// <summary>Fired after a save file is deleted.</summary>
        /// <param name="slotName">The slot that was deleted.</param>
        public delegate void DeleteEvent(string slotName);

        /// <summary>Fired when a save or load operation fails.</summary>
        /// <param name="slotName">The slot involved.</param>
        /// <param name="reason">Human-readable failure reason.</param>
        public delegate void FailEvent(string slotName, string reason);

        /// <summary>Fired when a new game is started on a slot.</summary>
        /// <param name="slotName">The slot that was reset.</param>
        public delegate void NewGameEvent(string slotName);
    }
}
