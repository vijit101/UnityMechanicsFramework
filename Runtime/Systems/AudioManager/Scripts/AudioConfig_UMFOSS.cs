using System;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "UMFOSS/Audio/AudioConfig")]
    public class AudioConfig_UMFOSS : ScriptableObject
    {
        [Header("SFX")]
        public SFXEntry[] sfxEntries;

        [Header("Music")]
        public MusicEntry[] musicEntries;

        [Header("Ambient")]
        public AmbientEntry[] ambientEntries;
    }

    [Serializable]
    public class SFXEntry
    {
        public string key;              // e.g. "PlayerJump", "BulletHit", "CoinPickup"
        public AudioClip[] clips;       // multiple clips — manager picks random to avoid repetition
        public float baseVolume = 1f;   // 0.0 to 1.0
        public float basePitch = 1f;    // 1.0 = normal, 0.9–1.1 = random range for variety
        public float pitchVariance = 0.1f; // ± variance applied randomly per play
    }

    [Serializable]
    public class MusicEntry
    {
        public string key;              // e.g. "MainMenu", "BossTheme", "ExplorationTrack"
        public AudioClip clip;
        public float baseVolume = 1f;
        public float fadeInDuration = 1.5f;
        public float fadeOutDuration = 1.5f;
    }

    [Serializable]
    public class AmbientEntry
    {
        public string key;
        public AudioClip clip;
        public float baseVolume = 1f;
    }
}
