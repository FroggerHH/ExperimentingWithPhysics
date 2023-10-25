using Unity.Collections;

namespace ExperimentingWithPhysics;

public class SoftBody : CustomPhysics
{
    private Collider[] colliders;
    private bool IsInitialized;

    public override void Awake()
    {
        base.Awake();
        Init();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        all.Remove(this);
    }

    public void Init()
    {
        if (IsInitialized) return;

        #region Rigidbody

        try
        {
            rb ??= GetComponentsInChildren<Rigidbody>().SingleOrDefault();
        }
        catch (InvalidOperationException)
        {
            DebugError($"'{this.GetPrefabName()}' have more than one rigidbody, witch is unacceptable");
        }

        if (rb == null)
        {
            DebugError($"'{this.GetPrefabName()}' doesn't have a rigidbody, but it needs it");
            return;
        }

        #endregion

        colliders ??= GetComponentsInChildren<Collider>();
        for (var i = 0; i < colliders.Length; i++) colliders[i].hasModifiableContacts = true;

        rb.angularDrag = angularDrag.Value;
        Physics.ContactModifyEvent += ContactModify;
        IsInitialized = true;
    }

    private void ContactModify(PhysicsScene physicsScene, NativeArray<ModifiableContactPair> pairs)
    {
        foreach (var pair in pairs)
        {
            if (pair.bodyInstanceID != BodyId && pair.otherBodyInstanceID != BodyId) continue;
            for (var i = 0; i < pair.contactCount; i++)
            {
                var separation = -pair.GetSeparation(i);
                if (separation < 0) separation = 0;
                pair.SetMaxImpulse(i, maxImpulse * separation);
            }
        }
    }
}