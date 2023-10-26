namespace ExperimentingWithPhysics;

public class Conveyor : CustomPhysics
{
    public float velocityModifier = 1f;

    public Vector3 direction => directionPoint.position.normalized;

    public Transform directionPoint;

    public override void Awake()
    {
        base.Awake();
        if (!m_nview || m_nview.m_ghost) return;
        velocityModifier = Plugin.velocityModifier.Value;
    }

    private void FixedUpdate()
    {
        if (!rb || !m_nview || m_nview.m_ghost) return;
        direction.Normalize();

        var initialPosition = rb.position;
        rb.position += direction * velocityModifier * Time.fixedDeltaTime;
        rb.MovePosition(initialPosition);
    }
}