namespace ExperimentingWithPhysics;

public class CustomPhysics : MonoBehaviour
{
    public ZNetView m_nview;
    public float maxImpulse = 2;
    public static List<CustomPhysics> all = new();

    public Rigidbody rb;
    protected int BodyId => rb.GetInstanceID();

    public virtual void Awake()
    {
        m_nview = GetComponent<ZNetView>();
        if (!m_nview || m_nview.m_ghost) return;

        rb = GetComponent<Rigidbody>();
        foreach (var componentsInChild in GetComponentsInChildren<Collider>())
            componentsInChild.hasModifiableContacts = true;
        all.Add(this);

        maxImpulse = Plugin.maxImpulse.Value;
    }

    public virtual void OnDestroy() => all.Remove(this);
}