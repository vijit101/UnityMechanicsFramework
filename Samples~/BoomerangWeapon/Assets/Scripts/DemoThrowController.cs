using UnityEngine;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Combat;

namespace GameplayMechanicsUMFOSS.Samples.BoomerangWeapon
{
    /// <summary>
    /// Simple first-person controller for the RecallDemo scene.
    /// Not part of the framework -- exists purely as a demo harness.
    /// </summary>
    public class DemoThrowController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float mouseSensitivity = 2f;

        [Header("Weapons")]
        [SerializeField] private BoomerangWeapon_UMFOSS weapon;
        [SerializeField] private BoomerangWeapon_UMFOSS[] allWeapons;

        [Header("Camera")]
        [SerializeField] private Transform cameraTransform;

        [Header("UI")]
        [SerializeField] private UnityEngine.UI.Text stateText;

        private CharacterController characterController;
        private float verticalLookRotation;
        private int currentWeaponIndex;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (cameraTransform == null)
                cameraTransform = Camera.main.transform;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            EventBus.Subscribe<WeaponThrownEvent>(OnWeaponThrown);
            EventBus.Subscribe<WeaponStuckEvent>(OnWeaponStuck);
            EventBus.Subscribe<WeaponRecallStartedEvent>(OnWeaponRecallStarted);
            EventBus.Subscribe<WeaponCaughtEvent>(OnWeaponCaught);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<WeaponThrownEvent>(OnWeaponThrown);
            EventBus.Unsubscribe<WeaponStuckEvent>(OnWeaponStuck);
            EventBus.Unsubscribe<WeaponRecallStartedEvent>(OnWeaponRecallStarted);
            EventBus.Unsubscribe<WeaponCaughtEvent>(OnWeaponCaught);
        }

        private void Update()
        {
            HandleMovement();
            HandleMouseLook();
            HandleWeaponInput();
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            characterController.Move(move.normalized * moveSpeed * Time.deltaTime);

            if (!characterController.isGrounded)
                characterController.Move(Vector3.down * 9.81f * Time.deltaTime);
        }

        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            verticalLookRotation -= mouseY;
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

            cameraTransform.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }

        private void HandleWeaponInput()
        {
            if (Input.GetMouseButtonDown(0))
                weapon.Throw(cameraTransform.forward);

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.R))
                weapon.Recall();

            if (Input.GetKeyDown(KeyCode.Tab) && allWeapons != null && allWeapons.Length > 1)
            {
                if (weapon.IsEquipped)
                {
                    weapon.gameObject.SetActive(false);
                    currentWeaponIndex = (currentWeaponIndex + 1) % allWeapons.Length;
                    weapon = allWeapons[currentWeaponIndex];
                    weapon.gameObject.SetActive(true);
                    UpdateStateText("EQUIPPED (switched)");
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void OnWeaponThrown(WeaponThrownEvent e) => UpdateStateText("THROWN");
        private void OnWeaponStuck(WeaponStuckEvent e) => UpdateStateText($"EMBEDDED in {e.Surface.name}");
        private void OnWeaponRecallStarted(WeaponRecallStartedEvent e) => UpdateStateText("RECALLING...");
        private void OnWeaponCaught(WeaponCaughtEvent e) => UpdateStateText("EQUIPPED");

        private void UpdateStateText(string state)
        {
            if (stateText != null)
                stateText.text = $"Weapon: {state}\n\nLMB: Throw | RMB/R: Recall | Tab: Switch";
        }
    }
}
