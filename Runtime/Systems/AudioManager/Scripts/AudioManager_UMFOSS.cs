using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Systems
{
    public class AudioManager_UMFOSS : MonoSingletonGeneric<AudioManager_UMFOSS>
    {
        public AudioConfig_UMFOSS audioConfig;

        [Header("SFX Pool")]
        [SerializeField] private int sfxPoolSize = 10;

        public float MasterVolume { get; private set; } = 1f;
        public float SFXVolume { get; private set; } = 1f;
        public float MusicVolume { get; private set; } = 1f;
        public float AmbientVolume { get; private set; } = 1f;

        private List<SFXSourceData> sfxPool;
        private AudioSource musicSource;
        private AudioSource ambientSource;

        private Coroutine musicCrossfadeCoroutine;
        private MusicEntry currentMusicEntry;
        private AmbientEntry currentAmbientEntry;

        private class SFXSourceData
        {
            public AudioSource Source;
            public float BaseVolume;
            public float StartTime;
            public float BusyUntilTime;
            public bool IsIdle => Time.time >= BusyUntilTime && !Source.isPlaying;
        }

        protected override void Awake()
        {
            base.Awake();

            LoadVolumes();

            sfxPool = new List<SFXSourceData>(sfxPoolSize);
            for (int i = 0; i < sfxPoolSize; i++)
            {
                GameObject go = new GameObject($"SFXSource_{i}");
                go.transform.SetParent(transform);
                AudioSource source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                sfxPool.Add(new SFXSourceData { Source = source });
            }

            GameObject musicGo = new GameObject("MusicSource");
            musicGo.transform.SetParent(transform);
            musicSource = musicGo.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;

            GameObject ambientGo = new GameObject("AmbientSource");
            ambientGo.transform.SetParent(transform);
            ambientSource = ambientGo.AddComponent<AudioSource>();
            ambientSource.playOnAwake = false;
            ambientSource.loop = true;
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlaySFXEvent>(HandlePlaySFXEvent);
            EventBus.Subscribe<PlayMusicEvent>(HandlePlayMusicEvent);
            EventBus.Subscribe<StopMusicEvent>(HandleStopMusicEvent);
            EventBus.Subscribe<SetVolumeEvent>(HandleSetVolumeEvent);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlaySFXEvent>(HandlePlaySFXEvent);
            EventBus.Unsubscribe<PlayMusicEvent>(HandlePlayMusicEvent);
            EventBus.Unsubscribe<StopMusicEvent>(HandleStopMusicEvent);
            EventBus.Unsubscribe<SetVolumeEvent>(HandleSetVolumeEvent);
        }

        private void LoadVolumes()
        {
            MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            AmbientVolume = PlayerPrefs.GetFloat("AmbientVolume", 1f);
        }

        public void PlaySFX(string key)
        {
            PlaySFX(key, Vector3.zero, false);
        }

        public void PlaySFX(string key, Vector3 worldPosition)
        {
            PlaySFX(key, worldPosition, true);
        }

        private void PlaySFX(string key, Vector3 position, bool use3D)
        {
            if (audioConfig == null) return;
            SFXEntry entry = null;
            foreach (var sfx in audioConfig.sfxEntries)
            {
                if (sfx.key == key)
                {
                    entry = sfx;
                    break;
                }
            }

            if (entry == null)
            {
                Debug.LogWarning($"[AudioManager] SFX key '{key}' not found!");
                return;
            }

            if (entry.clips == null || entry.clips.Length == 0)
            {
                Debug.LogWarning($"[AudioManager] SFX entry '{key}' has no clips!");
                return;
            }

            SFXSourceData data = GetAvailableSFXSource();
            if (data == null) return;

            AudioClip clip = entry.clips[Random.Range(0, entry.clips.Length)];
            
            data.Source.transform.position = position;
            data.Source.spatialBlend = use3D ? 1f : 0f;
            data.Source.clip = clip;
            
            float variance = Mathf.Clamp(entry.pitchVariance, 0f, 1f);
            float randomPitch = entry.basePitch + Random.Range(-variance, variance);
            data.Source.pitch = Mathf.Clamp(randomPitch, 0.5f, 2.0f);
            
            data.Source.volume = entry.baseVolume * SFXVolume * MasterVolume;
            data.BaseVolume = entry.baseVolume;
            
            data.StartTime = Time.time;
            data.BusyUntilTime = Time.time + clip.length;

            data.Source.Play();
        }

        public void StopAllSFX()
        {
            foreach (var data in sfxPool)
            {
                data.Source.Stop();
                data.BusyUntilTime = 0;
            }
        }

        private SFXSourceData GetAvailableSFXSource()
        {
            foreach (var data in sfxPool)
            {
                if (data.IsIdle) return data;
            }

            SFXSourceData oldest = sfxPool[0];
            foreach (var data in sfxPool)
            {
                if (data.StartTime < oldest.StartTime) oldest = data;
            }

            oldest.Source.Stop();
            return oldest;
        }

        public void PlayMusic(string key, bool fadeIn = true)
        {
            if (audioConfig == null) return;
            MusicEntry newEntry = null;
            foreach (var m in audioConfig.musicEntries)
            {
                if (m.key == key)
                {
                    newEntry = m;
                    break;
                }
            }

            if (newEntry == null)
            {
                Debug.LogWarning($"[AudioManager] Music key '{key}' not found!");
                return;
            }

            if (currentMusicEntry != null && currentMusicEntry.key == key && musicSource.isPlaying)
                return;

            if (musicCrossfadeCoroutine != null)
                StopCoroutine(musicCrossfadeCoroutine);

            musicCrossfadeCoroutine = StartCoroutine(MusicCrossfadeCoroutine(newEntry, fadeIn));
            currentMusicEntry = newEntry;

            EventBus.Publish(new OnMusicStarted { key = newEntry.key });
        }

        public void StopMusic(bool fadeOut = true)
        {
            if (musicSource.isPlaying)
            {
                if (fadeOut)
                {
                    if (musicCrossfadeCoroutine != null) StopCoroutine(musicCrossfadeCoroutine);
                    float duration = currentMusicEntry != null ? currentMusicEntry.fadeOutDuration : 1.5f;
                    musicCrossfadeCoroutine = StartCoroutine(MusicFadeOutCoroutine(duration));
                }
                else
                {
                    musicSource.Stop();
                }
                
                string lastKey = currentMusicEntry?.key ?? "";
                currentMusicEntry = null;
                EventBus.Publish(new OnMusicStopped { key = lastKey });
            }
        }

        public void PauseMusic()
        {
            musicSource.Pause();
        }

        public void ResumeMusic()
        {
            musicSource.UnPause();
        }

        private IEnumerator MusicCrossfadeCoroutine(MusicEntry newEntry, bool fadeIn)
        {
            float fadeOutDuration = currentMusicEntry != null ? currentMusicEntry.fadeOutDuration : 1.5f;
            float startVolume = musicSource.isPlaying ? musicSource.volume : 0;
            float elapsed = 0;

            if (startVolume > 0)
            {
                while (elapsed < fadeOutDuration)
                {
                    elapsed += Time.deltaTime;
                    musicSource.volume = Mathf.Lerp(startVolume, 0, elapsed / fadeOutDuration);
                    yield return null;
                }
            }

            musicSource.clip = newEntry.clip;
            musicSource.Play();

            elapsed = 0;
            float targetVolume = newEntry.baseVolume * MusicVolume * MasterVolume;
            float fadeInDur = fadeIn ? newEntry.fadeInDuration : 0.1f;
            
            while (elapsed < fadeInDur)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0, targetVolume, elapsed / fadeInDur);
                yield return null;
            }
            
            musicSource.volume = targetVolume;
            musicCrossfadeCoroutine = null;
        }

        private IEnumerator MusicFadeOutCoroutine(float duration)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0, elapsed / duration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = 0;
            musicCrossfadeCoroutine = null;
        }

        public void PlayAmbient(string key)
        {
            if (audioConfig == null) return;
            AmbientEntry entry = null;
            foreach (var a in audioConfig.ambientEntries)
            {
                if (a.key == key)
                {
                    entry = a;
                    break;
                }
            }

            if (entry == null)
            {
                Debug.LogWarning($"[AudioManager] Ambient key '{key}' not found!");
                return;
            }

            if (currentAmbientEntry != null && currentAmbientEntry.key == key && ambientSource.isPlaying)
                return;

            ambientSource.clip = entry.clip;
            ambientSource.volume = entry.baseVolume * AmbientVolume * MasterVolume;
            ambientSource.Play();
            currentAmbientEntry = entry;
        }

        public void StopAmbient()
        {
            ambientSource.Stop();
            currentAmbientEntry = null;
        }

        public void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
            UpdateActiveVolumes();
            EventBus.Publish(new OnVolumeChanged { category = AudioCategory.Master, newVolume = MasterVolume });
        }

        public void SetSFXVolume(float volume)
        {
            SFXVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("SFXVolume", SFXVolume);
            UpdateActiveVolumes();
            EventBus.Publish(new OnVolumeChanged { category = AudioCategory.SFX, newVolume = SFXVolume });
        }

        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
            UpdateActiveVolumes();
            EventBus.Publish(new OnVolumeChanged { category = AudioCategory.Music, newVolume = MusicVolume });
        }

        public void SetAmbientVolume(float volume)
        {
            AmbientVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("AmbientVolume", AmbientVolume);
            UpdateActiveVolumes();
            EventBus.Publish(new OnVolumeChanged { category = AudioCategory.Ambient, newVolume = AmbientVolume });
        }
        
        private void UpdateActiveVolumes()
        {
            if (currentMusicEntry != null && musicCrossfadeCoroutine == null)
            {
                musicSource.volume = currentMusicEntry.baseVolume * MusicVolume * MasterVolume;
            }
            if (currentAmbientEntry != null)
            {
                ambientSource.volume = currentAmbientEntry.baseVolume * AmbientVolume * MasterVolume;
            }
            foreach (var data in sfxPool)
            {
                if (!data.IsIdle)
                {
                    data.Source.volume = data.BaseVolume * SFXVolume * MasterVolume;
                }
            }
        }

        public void MuteAll()
        {
            AudioListener.volume = 0f;
            EventBus.Publish(new OnAllAudioMuted());
        }

        public void UnmuteAll()
        {
            AudioListener.volume = 1f;
            EventBus.Publish(new OnAllAudioUnmuted());
        }

        public void PauseAll()
        {
            AudioListener.pause = true;
        }

        public void ResumeAll()
        {
            AudioListener.pause = false;
        }

        private void HandlePlaySFXEvent(PlaySFXEvent e)
        {
            if (e.position != Vector3.zero)
                PlaySFX(e.key, e.position);
            else
                PlaySFX(e.key);
        }

        private void HandlePlayMusicEvent(PlayMusicEvent e)
        {
            PlayMusic(e.key, e.fadeIn);
        }

        private void HandleStopMusicEvent(StopMusicEvent e)
        {
            StopMusic(e.fadeOut);
        }

        private void HandleSetVolumeEvent(SetVolumeEvent e)
        {
            switch (e.category)
            {
                case AudioCategory.Master: SetMasterVolume(e.volume); break;
                case AudioCategory.SFX: SetSFXVolume(e.volume); break;
                case AudioCategory.Music: SetMusicVolume(e.volume); break;
                case AudioCategory.Ambient: SetAmbientVolume(e.volume); break;
            }
        }
    }

    public struct OnMusicStarted { public string key; }
    public struct OnMusicStopped { public string key; }
    public struct OnVolumeChanged { public AudioCategory category; public float newVolume; }
    public struct OnAllAudioMuted { }
    public struct OnAllAudioUnmuted { }
}
