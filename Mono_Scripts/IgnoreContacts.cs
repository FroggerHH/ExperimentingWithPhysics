using Unity.Collections;

namespace ExperimentingWithPhysics;

public class IgnoreContacts : CustomPhysics
{
    private void Start()
    {
        Physics.ContactModifyEvent += PhysicsOnContactModifyEvent;
    }

    private void PhysicsOnContactModifyEvent(PhysicsScene scene, NativeArray<ModifiableContactPair> pairs)
    {
        foreach (var modifiableContactPair in pairs)
            for (var i = 0; i < modifiableContactPair.contactCount; ++i)
                modifiableContactPair.IgnoreContact(i);
    }
}