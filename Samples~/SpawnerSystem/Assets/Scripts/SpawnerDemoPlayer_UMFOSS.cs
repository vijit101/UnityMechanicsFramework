using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.SpawnerSystem
{
    public class SpawnerDemoPlayer_UMFOSS : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 8f;
        Rigidbody2D _rb;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        void FixedUpdate()
        {
            var x = Input.GetAxisRaw("Horizontal");
            var y = Input.GetAxisRaw("Vertical");
            _rb.velocity = new Vector2(x, y).normalized * moveSpeed;
        }
    }
}
