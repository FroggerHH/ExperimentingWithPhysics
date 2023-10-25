using Unity.Collections;

namespace ExperimentingWithPhysics;

public static class ContactsListener
{
    private static readonly Dictionary<int, Action<int, int, ModifiableContactPair>> modifiers = new();

    public static void RegisterModifier(int bodyId, Action<int, int, ModifiableContactPair> modifier)
    {
        if (!modifiers.TryAdd(bodyId, modifier) || modifiers.Count > 1) return;
        Physics.ContactModifyEvent += PhysicsOnContactModifyEvent;
    }

    public static void UnregisterModifier(int bodyId)
    {
        if (!modifiers.Remove(bodyId) || modifiers.Count > 0) return;
        Physics.ContactModifyEvent -= PhysicsOnContactModifyEvent;
    }

    private static void PhysicsOnContactModifyEvent(PhysicsScene scene, NativeArray<ModifiableContactPair> pairs)
    {
        foreach (var modifiableContactPair in pairs)
            if (modifiers.TryGetValue(modifiableContactPair.bodyInstanceID, out var m))
                m(modifiableContactPair.bodyInstanceID, modifiableContactPair.otherBodyInstanceID,
                    modifiableContactPair);
            else if (modifiers.TryGetValue(modifiableContactPair.otherBodyInstanceID, out m))
                m(modifiableContactPair.otherBodyInstanceID, modifiableContactPair.bodyInstanceID,
                    modifiableContactPair);
    }
}

public static class DictionaryExtensions
{
    public static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
    {
        if (!dict.ContainsKey(key))
        {
            dict.Add(key, value);
            return true;
        }

        return false;
    }
}