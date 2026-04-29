using UnityEngine;

public class SimpleMover_UMFOSS : MonoBehaviour
{
    [SerializeField] private Vector3 pointA = new Vector3(-3f, 0.5f, 0f);
    [SerializeField] private Vector3 pointB = new Vector3(3f, 0.5f, 0f);
    [SerializeField] private float speed = 2f;

    private bool movingToB = true;

    private void Update()
    {
        Vector3 target = movingToB ? pointB : pointA;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) <= 0.01f)
        {
            movingToB = !movingToB;
        }
    }
}
