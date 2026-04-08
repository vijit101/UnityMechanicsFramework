using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameplayMechanicsUMFOSS.Systems;

namespace GameplayMechanicsUMFOSS.Samples.GameFeel
{
    /// <summary>
    /// Demo UI panel with individual toggle buttons for each Game Feel effect
    /// and a master ALL ON / ALL OFF toggle. Demonstrates real-time enable/disable
    /// with zero runtime overhead when effects are off.
    ///
    /// Setup: Attach to a Canvas. Assign the GameFeel_UMFOSS target reference.
    /// Buttons are auto-created if no manual references are provided.
    /// </summary>
    public class GameFeelDemoUI : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private GameFeel_UMFOSS gameFeel;

        [Header("UI References (auto-created if null)")]
        [SerializeField] private Toggle hitpauseToggle;
        [SerializeField] private Toggle squashStretchToggle;
        [SerializeField] private Toggle afterimageToggle;
        [SerializeField] private Toggle screenFlashToggle;
        [SerializeField] private Toggle ghostTrailToggle;
        [SerializeField] private Toggle masterToggle;
        [SerializeField] private TextMeshProUGUI controlsLabel;

        private void Start()
        {
            if (gameFeel == null)
            {
                gameFeel = FindFirstObjectByType<GameFeel_UMFOSS>();
            }

            SetupToggleListeners();
            DisplayControls();
        }

        private void SetupToggleListeners()
        {
            if (hitpauseToggle != null)
            {
                hitpauseToggle.isOn = true;
                hitpauseToggle.onValueChanged.AddListener(gameFeel.SetHitpauseEnabled);
            }

            if (squashStretchToggle != null)
            {
                squashStretchToggle.isOn = true;
                squashStretchToggle.onValueChanged.AddListener(gameFeel.SetSquashStretchEnabled);
            }

            if (afterimageToggle != null)
            {
                afterimageToggle.isOn = true;
                afterimageToggle.onValueChanged.AddListener(gameFeel.SetAfterimageEnabled);
            }

            if (screenFlashToggle != null)
            {
                screenFlashToggle.isOn = true;
                screenFlashToggle.onValueChanged.AddListener(gameFeel.SetScreenFlashEnabled);
            }

            if (ghostTrailToggle != null)
            {
                ghostTrailToggle.isOn = true;
                ghostTrailToggle.onValueChanged.AddListener(gameFeel.SetGhostTrailEnabled);
            }

            if (masterToggle != null)
            {
                masterToggle.isOn = true;
                masterToggle.onValueChanged.AddListener(OnMasterToggleChanged);
            }
        }

        private void OnMasterToggleChanged(bool enabled)
        {
            gameFeel.SetAllEffectsEnabled(enabled);

            if (hitpauseToggle != null) hitpauseToggle.SetIsOnWithoutNotify(enabled);
            if (squashStretchToggle != null) squashStretchToggle.SetIsOnWithoutNotify(enabled);
            if (afterimageToggle != null) afterimageToggle.SetIsOnWithoutNotify(enabled);
            if (screenFlashToggle != null) screenFlashToggle.SetIsOnWithoutNotify(enabled);
            if (ghostTrailToggle != null) ghostTrailToggle.SetIsOnWithoutNotify(enabled);
        }

        private void DisplayControls()
        {
            if (controlsLabel == null) return;

            controlsLabel.text =
                "<b>CONTROLS</b>\n" +
                "WASD / Arrows  -  Move\n" +
                "Space  -  Jump\n" +
                "Left Shift  -  Dash\n" +
                "Left Click  -  Attack\n" +
                "F  -  Take Damage\n" +
                "G  -  Simulate Death\n" +
                "E  -  Pick Up Item";
        }
    }
}
