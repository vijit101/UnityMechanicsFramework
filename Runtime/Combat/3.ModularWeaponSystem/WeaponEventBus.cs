// Author: Aditya Jaiswal, Atharv S. Jain
using System;
using UnityEngine;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Hit information passed alongside <see cref="WeaponHitEvent"/>.
    /// </summary>
    [System.Serializable]
    public struct HitData
    {
        public float damage;
        public Vector3 hitPoint;
        public GameObject hitObject;
    }

    /// <summary> Raised when a weapon is equipped. </summary>
    [System.Serializable]
    public struct WeaponEquippedEvent
    {
        public WeaponData data;
    }

    /// <summary> Raised when a weapon is unequipped. </summary>
    [System.Serializable]
    public struct WeaponUnequippedEvent
    {
        public WeaponData data;
    }

    /// <summary> Raised when a weapon fires. </summary>
    [System.Serializable]
    public struct WeaponFiredEvent
    {
        public WeaponData data;
    }

    /// <summary> Raised when a weapon's hitbox makes contact. </summary>
    [System.Serializable]
    public struct WeaponHitEvent
    {
        public HitData hitData;
    }

    /// <summary> Raised when a weapon begins reloading. </summary>
    [System.Serializable]
    public struct WeaponReloadStartEvent
    {
        public WeaponData data;
    }

    /// <summary> Raised when a weapon finishes reloading. </summary>
    [System.Serializable]
    public struct WeaponReloadCompleteEvent
    {
        public WeaponData data;
    }

    /// <summary> Raised when a weapon's ammo counts change. </summary>
    [System.Serializable]
    public struct AmmoChangedEvent
    {
        public int current;
        public int reserve;
    }

    /// <summary> Raised every frame a chargeable weapon's charge percent changes. </summary>
    [System.Serializable]
    public struct ChargeChangedEvent
    {
        public float percent;
    }

    /// <summary>
    /// Thin combat-domain wrapper around the generic <see cref="EventBus"/>.
    /// Defines weapon event payload structs and provides typed Raise/On
    /// helpers so weapons and listeners never type the bus generics directly.
    /// All storage is delegated to <see cref="EventBus"/>.
    /// </summary>
    public static class WeaponEventBus
    {
        // ----- Raise -----

        /// <summary> Publishes a <see cref="WeaponEquippedEvent"/>. </summary>
        public static void RaiseWeaponEquipped(WeaponData data)
        {
            EventBus.Publish(new WeaponEquippedEvent { data = data });
        }

        /// <summary> Publishes a <see cref="WeaponUnequippedEvent"/>. </summary>
        public static void RaiseWeaponUnequipped(WeaponData data)
        {
            EventBus.Publish(new WeaponUnequippedEvent { data = data });
        }

        /// <summary> Publishes a <see cref="WeaponFiredEvent"/>. </summary>
        public static void RaiseWeaponFired(WeaponData data)
        {
            EventBus.Publish(new WeaponFiredEvent { data = data });
        }

        /// <summary> Publishes a <see cref="WeaponHitEvent"/>. </summary>
        public static void RaiseWeaponHit(HitData hitData)
        {
            EventBus.Publish(new WeaponHitEvent { hitData = hitData });
        }

        /// <summary> Publishes a <see cref="WeaponReloadStartEvent"/>. </summary>
        public static void RaiseWeaponReloadStart(WeaponData data)
        {
            EventBus.Publish(new WeaponReloadStartEvent { data = data });
        }

        /// <summary> Publishes a <see cref="WeaponReloadCompleteEvent"/>. </summary>
        public static void RaiseWeaponReloadComplete(WeaponData data)
        {
            EventBus.Publish(new WeaponReloadCompleteEvent { data = data });
        }

        /// <summary> Publishes an <see cref="AmmoChangedEvent"/>. </summary>
        public static void RaiseAmmoChanged(int current, int reserve)
        {
            EventBus.Publish(new AmmoChangedEvent { current = current, reserve = reserve });
        }

        /// <summary> Publishes a <see cref="ChargeChangedEvent"/>. </summary>
        public static void RaiseChargeChanged(float percent)
        {
            EventBus.Publish(new ChargeChangedEvent { percent = percent });
        }

        // ----- Subscribe -----

        /// <summary> Subscribes to <see cref="WeaponEquippedEvent"/>. </summary>
        public static void OnWeaponEquipped(Action<WeaponEquippedEvent> handler)
        {
            EventBus.Subscribe(handler);
        }

        /// <summary> Subscribes to <see cref="WeaponUnequippedEvent"/>. </summary>
        public static void OnWeaponUnequipped(Action<WeaponUnequippedEvent> handler)
        {
            EventBus.Subscribe(handler);
        }

        /// <summary> Subscribes to <see cref="WeaponFiredEvent"/>. </summary>
        public static void OnWeaponFired(Action<WeaponFiredEvent> handler)
        {
            EventBus.Subscribe(handler);
        }

        /// <summary> Subscribes to <see cref="WeaponHitEvent"/>. </summary>
        public static void OnWeaponHit(Action<WeaponHitEvent> handler)
        {
            EventBus.Subscribe(handler);
        }

        /// <summary> Subscribes to <see cref="WeaponReloadStartEvent"/>. </summary>
        public static void OnWeaponReloadStart(Action<WeaponReloadStartEvent> handler)
        {
            EventBus.Subscribe(handler);
        }

        /// <summary> Subscribes to <see cref="WeaponReloadCompleteEvent"/>. </summary>
        public static void OnWeaponReloadComplete(Action<WeaponReloadCompleteEvent> handler)
        {
            EventBus.Subscribe(handler);
        }

        /// <summary> Subscribes to <see cref="AmmoChangedEvent"/>. </summary>
        public static void OnAmmoChanged(Action<AmmoChangedEvent> handler)
        {
            EventBus.Subscribe(handler);
        }

        /// <summary> Subscribes to <see cref="ChargeChangedEvent"/>. </summary>
        public static void OnChargeChanged(Action<ChargeChangedEvent> handler)
        {
            EventBus.Subscribe(handler);
        }
    }
}
