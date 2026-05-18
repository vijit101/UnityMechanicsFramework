// Author: Aditya Jaiswal, Atharv S. Jain
using UnityEngine;
using UnityEngine.UI;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Drives a filled UI <see cref="Image"/> from <see cref="ChargeChangedEvent"/>.
    /// Drop on a Canvas object, point at a child Image set to Filled type, and
    /// the bar will track any chargeable weapon's charge percent through the
    /// event bus — no direct reference to the weapon needed.
    /// </summary>
    public class ChargeBarUI_UMFOSS : MonoBehaviour
    {
        [Tooltip("UI Image with Image Type set to Filled. Its fillAmount is driven by charge percent.")]
        [SerializeField] private Image fillImage;

        private void OnEnable()
        {
            WeaponEventBus.OnChargeChanged(HandleChargeChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ChargeChangedEvent>(HandleChargeChanged);
        }

        private void HandleChargeChanged(ChargeChangedEvent evt)
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = evt.percent;
            }
        }
    }
}
