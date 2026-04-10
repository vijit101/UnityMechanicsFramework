using UnityEngine;

public class SimpleRotator_UMFOSS : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAxis = new Vector3(0f, 1f, 0f);
    [SerializeField] private float speed = 120f;

    private void Update()
    {
        transform.Rotate(rotationAxis, speed * Time.deltaTime, Space.Self);
    }
}
