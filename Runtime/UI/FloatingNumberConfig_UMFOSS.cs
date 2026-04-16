using System;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.UI
{
    [CreateAssetMenu(fileName = "FloatingNumberConfig", menuName = "UMFOSS/UI/FloatingNumberConfig")]
    public class FloatingNumberConfig_UMFOSS : ScriptableObject
    {
        [Serializable]
        public class NumberStyle
        {
            public NumberType type;
            public Color colour = Color.white;
            public float fontSize = 36f;
            public float fontSizeCrit = 52f;
            public float critThreshold = 0f;
        }

        [Serializable]
        public class AnimationStyle
        {
            public NumberType type;
            public float floatHeight = 1.5f;
            public float floatDuration = 0.8f;
            [Range(0f, 1f)] public float fadeStartAt = 0.5f;
            public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            public AnimationCurve fadeCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
            public Vector2 randomOffset = new Vector2(0.3f, 0f);
        }

        [Header("Visual Styles")]
        public NumberStyle[] styles;

        [Header("Animation Styles")]
        public AnimationStyle[] animations;

        [Header("Defaults")]
        public int poolSize = 20;
        public Vector3 spawnOffset = new Vector3(0f, 1f, 0f);

        /// <summary>
        /// Creates a sensible default configuration for runtime-generated demos and tests.
        /// </summary>
        public static FloatingNumberConfig_UMFOSS CreateDefault()
        {
            FloatingNumberConfig_UMFOSS config = CreateInstance<FloatingNumberConfig_UMFOSS>();
            config.styles = new[]
            {
                CreateStyle(NumberType.Damage, Color.white, 36f, 52f, 75f),
                CreateStyle(NumberType.CriticalHit, Color.yellow, 52f, 60f, 0f),
                CreateStyle(NumberType.DamageTaken, new Color(1f, 0.35f, 0.35f), 36f, 52f, 75f),
                CreateStyle(NumberType.Heal, new Color(0.35f, 1f, 0.45f), 36f, 48f, 50f),
                CreateStyle(NumberType.PoisonDamage, new Color(0.66f, 0.32f, 0.88f), 36f, 48f, 40f),
                CreateStyle(NumberType.ShieldBlock, new Color(0.3f, 0.65f, 1f), 34f, 46f, 60f),
                CreateStyle(NumberType.Miss, new Color(0.7f, 0.7f, 0.7f), 34f, 34f, 0f),
                CreateStyle(NumberType.Experience, new Color(1f, 0.83f, 0.18f), 34f, 46f, 80f)
            };

            config.animations = new[]
            {
                CreateAnimation(NumberType.Damage, 1.4f, 0.85f, 0.5f, new Vector2(0.3f, 0f)),
                CreateAnimation(NumberType.CriticalHit, 1.75f, 1f, 0.55f, new Vector2(0.35f, 0.1f)),
                CreateAnimation(NumberType.DamageTaken, 1.5f, 0.9f, 0.5f, new Vector2(0.3f, 0f)),
                CreateAnimation(NumberType.Heal, 1.3f, 0.8f, 0.45f, new Vector2(0.25f, 0.05f)),
                CreateAnimation(NumberType.PoisonDamage, 1.25f, 0.95f, 0.45f, new Vector2(0.2f, 0.1f)),
                CreateAnimation(NumberType.ShieldBlock, 1.1f, 0.7f, 0.45f, new Vector2(0.2f, 0f)),
                CreateAnimation(NumberType.Miss, 0.9f, 0.7f, 0.4f, new Vector2(0.2f, 0f)),
                CreateAnimation(NumberType.Experience, 1.6f, 1f, 0.55f, new Vector2(0.25f, 0.15f))
            };

            config.poolSize = 20;
            config.spawnOffset = new Vector3(0f, 1f, 0f);
            return config;
        }

        /// <summary>
        /// Returns the configured visual style for a number type.
        /// </summary>
        public NumberStyle GetStyle(NumberType type)
        {
            if (styles != null)
            {
                for (int index = 0; index < styles.Length; index++)
                {
                    if (styles[index] != null && styles[index].type == type)
                    {
                        return styles[index];
                    }
                }
            }

            return CreateStyle(type, Color.white, 36f, 52f, 0f);
        }

        /// <summary>
        /// Returns the configured animation style for a number type.
        /// </summary>
        public AnimationStyle GetAnimation(NumberType type)
        {
            if (animations != null)
            {
                for (int index = 0; index < animations.Length; index++)
                {
                    if (animations[index] != null && animations[index].type == type)
                    {
                        return animations[index];
                    }
                }
            }

            return CreateAnimation(type, 1.5f, 0.8f, 0.5f, new Vector2(0.3f, 0f));
        }

        private static NumberStyle CreateStyle(NumberType type, Color colour, float fontSize, float fontSizeCrit, float critThreshold)
        {
            return new NumberStyle
            {
                type = type,
                colour = colour,
                fontSize = fontSize,
                fontSizeCrit = fontSizeCrit,
                critThreshold = critThreshold
            };
        }

        private static AnimationStyle CreateAnimation(NumberType type, float floatHeight, float floatDuration, float fadeStartAt, Vector2 randomOffset)
        {
            return new AnimationStyle
            {
                type = type,
                floatHeight = floatHeight,
                floatDuration = floatDuration,
                fadeStartAt = fadeStartAt,
                movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f),
                fadeCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f),
                randomOffset = randomOffset
            };
        }
    }
}
