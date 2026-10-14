using System.Collections;
using UnityEngine;

public class Knockback : MonoBehaviour
{
    [SerializeField] private float duration = 0.15f;    // cuánto dura el empuje (segundos)
    [SerializeField] private float upwardForce = 3f;    // pequeño saltito hacia arriba (0 = sin salto)

    // Mientras es true, PlayerMovement y EnemyAI no tocan la velocidad
    public bool IsKnockedBack { get; private set; }

    private Rigidbody2D rb;
    private Coroutine current;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // sourcePosition = de dónde viene el golpe; force = cuánta velocidad le damos
    public void Apply(Vector2 sourcePosition, float force)
    {
        // Nos empuja en dirección contraria al atacante
        float direction = transform.position.x >= sourcePosition.x ? 1f : -1f;

        if (current != null)
        {
            StopCoroutine(current);
        }
        current = StartCoroutine(KnockbackRoutine(direction, force));
    }

    private IEnumerator KnockbackRoutine(float direction, float force)
    {
        IsKnockedBack = true;
        rb.linearVelocity = new Vector2(direction * force, upwardForce);

        yield return new WaitForSeconds(duration);

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        IsKnockedBack = false;
        current = null;
    }
}