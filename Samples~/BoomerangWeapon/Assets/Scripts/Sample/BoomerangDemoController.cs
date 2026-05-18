using UnityEngine;
using TMPro;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Combat;
#if UMF_NEW_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GameplayMechanicsUMFOSS.Samples.BoomerangWeapon
{
    /// <summary>
    /// Input controller for the Boomerang Weapon demo scene.
    /// Handles throw/recall input, crosshair aiming, and HUD state display.
    /// Works alongside PlayerController (movement) and WeaponFeedbackHandler (juice).
    ///
    /// Input bindings are exposed as Inspector fields so the user can rebind keys
    /// without modifying code. The demo supports both the Legacy Input Manager
    /// and the new Input System package.
    /// </summary>
    public class BoomerangDemoController : MonoBehaviour
    {
        [Header("Weapons")]
        [SerializeField] private BoomerangWeapon_UMFOSS[] weapons;

        [Header("Aiming")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float aimAssistRadius = 0.5f;
        [SerializeField] private float maxAimDistance = 50f;
        [SerializeField] private LayerMask aimLayers;

        [Header("Input Bindings")]
        [Tooltip("Mouse button used to throw the weapon. 0 = Left, 1 = Right, 2 = Middle.")]
        [SerializeField] private int throwMouseButton = 0;
        [Tooltip("Keyboard key used to recall the weapon back to the player.")]
        [SerializeField] private KeyCode recallKey = KeyCode.R;
        [Tooltip("Keyboard key used to switch between equipped weapons.")]
        [SerializeField] private KeyCode switchWeaponKey = KeyCode.Q;

        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI stateLabel;
        [SerializeField] private TextMeshProUGUI controlsLabel;
        [SerializeField] private RectTransform crosshair;

        [Header("Crosshair Colors")]
        [SerializeField] private Color defaultCrosshairColor = Color.white;
        [SerializeField] private Color targetLockedColor = Color.red;
        [SerializeField] private Color recallReadyColor = Color.cyan;

        private UnityEngine.UI.Image crosshairImage;
        private int currentWeaponIndex;
        private BoomerangWeapon_UMFOSS weapon;

        private void Awake()
        {
            if (crosshair != null)
                crosshairImage = crosshair.GetComponent<UnityEngine.UI.Image>();
        }

        private void Start()
        {
            if (weapons != null && weapons.Length > 0)
            {
                currentWeaponIndex = 0;
                ActivateWeapon(currentWeaponIndex);
            }

            if (controlsLabel != null)
                controlsLabel.text = $"{MouseLabel(throwMouseButton)}: Throw  |  {recallKey}: Recall  |  {switchWeaponKey}: Switch  |  WASD: Move  |  Shift: Sprint  |  Space: Jump  |  ESC: Unlock";
        }

        private void Update()
        {
            if (weapon == null) return;

            HandleInput();
            UpdateHUD();
        }

        private void HandleInput()
        {
            if (GetThrowPressed() && weapon.IsEquipped && Cursor.lockState == CursorLockMode.Locked)
            {
                weapon.Throw(GetAimDirection());
            }

            if (GetRecallPressed() && !weapon.IsEquipped)
            {
                weapon.Recall();
            }

            if (GetSwitchPressed() && weapon.IsEquipped && weapons.Length > 1)
            {
                SwitchWeapon();
            }
        }

        private void SwitchWeapon()
        {
            weapons[currentWeaponIndex].gameObject.SetActive(false);
            currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
            ActivateWeapon(currentWeaponIndex);
        }

        private void ActivateWeapon(int index)
        {
            weapon = weapons[index];
            weapon.gameObject.SetActive(true);
        }

        private Vector3 GetAimDirection()
        {
            if (playerCamera == null)
                return transform.forward;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            // Aim assist: if a SphereCast hits a target, throw toward that point
            if (UnityEngine.Physics.SphereCast(ray, aimAssistRadius, out RaycastHit hit, maxAimDistance, aimLayers))
            {
                return (hit.point - weapon.transform.position).normalized;
            }

            // No target found: throw toward the point maxAimDistance away on the ray
            return (ray.GetPoint(maxAimDistance) - weapon.transform.position).normalized;
        }

        private void UpdateHUD()
        {
            if (stateLabel != null)
            {
                stateLabel.text = $"{weapon.gameObject.name} — {weapon.CurrentState}";
            }

            UpdateCrosshair();
        }

        private void UpdateCrosshair()
        {
            if (crosshairImage == null || playerCamera == null) return;

            switch (weapon.CurrentState)
            {
                case WeaponStateKey.Equipped:
                    Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                    bool aimingAtTarget = UnityEngine.Physics.SphereCast(ray, aimAssistRadius, maxAimDistance, aimLayers);
                    crosshairImage.color = aimingAtTarget ? targetLockedColor : defaultCrosshairColor;
                    break;

                case WeaponStateKey.Thrown:
                case WeaponStateKey.Embedded:
                    crosshairImage.color = recallReadyColor;
                    break;

                case WeaponStateKey.Recalling:
                    crosshairImage.color = defaultCrosshairColor;
                    break;
            }
        }

        // ───────────────────────────────────────────────
        // Input Abstraction (Legacy + New Input System)
        // ───────────────────────────────────────────────

        private bool GetThrowPressed()
        {
#if UMF_NEW_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                switch (throwMouseButton)
                {
                    case 0: return Mouse.current.leftButton.wasPressedThisFrame;
                    case 1: return Mouse.current.rightButton.wasPressedThisFrame;
                    case 2: return Mouse.current.middleButton.wasPressedThisFrame;
                }
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(throwMouseButton);
#endif
            return false;
        }

        private bool GetRecallPressed()
        {
#if UMF_NEW_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                Key key = KeyCodeToKey(recallKey);
                if (key != Key.None)
                    return Keyboard.current[key].wasPressedThisFrame;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(recallKey);
#endif
            return false;
        }

        private bool GetSwitchPressed()
        {
#if UMF_NEW_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                Key key = KeyCodeToKey(switchWeaponKey);
                if (key != Key.None)
                    return Keyboard.current[key].wasPressedThisFrame;
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(switchWeaponKey);
#endif
            return false;
        }

        private static string MouseLabel(int button)
        {
            switch (button)
            {
                case 0: return "LMB";
                case 1: return "RMB";
                case 2: return "MMB";
                default: return $"Mouse{button}";
            }
        }

#if UMF_NEW_INPUT_SYSTEM
        // Maps the most common KeyCode values to their new-Input-System equivalents.
        // Returns Key.None for unmapped keys; callers must handle that case.
        private static Key KeyCodeToKey(KeyCode k)
        {
            switch (k)
            {
                case KeyCode.A: return Key.A; case KeyCode.B: return Key.B; case KeyCode.C: return Key.C;
                case KeyCode.D: return Key.D; case KeyCode.E: return Key.E; case KeyCode.F: return Key.F;
                case KeyCode.G: return Key.G; case KeyCode.H: return Key.H; case KeyCode.I: return Key.I;
                case KeyCode.J: return Key.J; case KeyCode.K: return Key.K; case KeyCode.L: return Key.L;
                case KeyCode.M: return Key.M; case KeyCode.N: return Key.N; case KeyCode.O: return Key.O;
                case KeyCode.P: return Key.P; case KeyCode.Q: return Key.Q; case KeyCode.R: return Key.R;
                case KeyCode.S: return Key.S; case KeyCode.T: return Key.T; case KeyCode.U: return Key.U;
                case KeyCode.V: return Key.V; case KeyCode.W: return Key.W; case KeyCode.X: return Key.X;
                case KeyCode.Y: return Key.Y; case KeyCode.Z: return Key.Z;
                case KeyCode.Alpha0: return Key.Digit0; case KeyCode.Alpha1: return Key.Digit1;
                case KeyCode.Alpha2: return Key.Digit2; case KeyCode.Alpha3: return Key.Digit3;
                case KeyCode.Alpha4: return Key.Digit4; case KeyCode.Alpha5: return Key.Digit5;
                case KeyCode.Alpha6: return Key.Digit6; case KeyCode.Alpha7: return Key.Digit7;
                case KeyCode.Alpha8: return Key.Digit8; case KeyCode.Alpha9: return Key.Digit9;
                case KeyCode.Space: return Key.Space; case KeyCode.Tab: return Key.Tab;
                case KeyCode.Return: return Key.Enter; case KeyCode.Escape: return Key.Escape;
                case KeyCode.LeftShift: return Key.LeftShift; case KeyCode.RightShift: return Key.RightShift;
                case KeyCode.LeftControl: return Key.LeftCtrl; case KeyCode.RightControl: return Key.RightCtrl;
                case KeyCode.LeftAlt: return Key.LeftAlt; case KeyCode.RightAlt: return Key.RightAlt;
                case KeyCode.F1: return Key.F1; case KeyCode.F2: return Key.F2; case KeyCode.F3: return Key.F3;
                case KeyCode.F4: return Key.F4; case KeyCode.F5: return Key.F5; case KeyCode.F6: return Key.F6;
                case KeyCode.F7: return Key.F7; case KeyCode.F8: return Key.F8; case KeyCode.F9: return Key.F9;
                case KeyCode.F10: return Key.F10; case KeyCode.F11: return Key.F11; case KeyCode.F12: return Key.F12;
                default: return Key.None;
            }
        }
#endif
    }
}
