namespace ExperimentingWithPhysics;

public class Conveyor : CustomPhysics
{
    [SerializeField] private Vector3 movingVelocity = Vector3.forward;

    private Vector3 worldMovingVelocity;
    public float velocityModifier = 1f;

    public override void Awake()
    {
        base.Awake();
        if (!m_nview || m_nview.m_ghost) return;
        worldMovingVelocity = transform.TransformDirection(movingVelocity);
        velocityModifier = Plugin.velocityModifier.Value;
    }

    private void OnEnable()
    {
        base.Awake();
        if (!m_nview || m_nview.m_ghost) return;

        ContactsListener.RegisterModifier(BodyId, ContactModifier);
    }

    private void OnDisable()
    {
        base.OnDestroy();
        if (!m_nview || m_nview.m_ghost) return;

        ContactsListener.UnregisterModifier(BodyId);
    }

    private void ContactModifier(int body1, int body2, ModifiableContactPair pair)
    {
        if (pair.bodyInstanceID != BodyId && pair.otherBodyInstanceID != BodyId) return;
        for (var i = 0; i < pair.contactCount; i++) pair.SetTargetVelocity(i, worldMovingVelocity * velocityModifier);
    }
}