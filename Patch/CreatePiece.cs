using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace ExperimentingWithPhysics.Patch;

[HarmonyPatch] public class CreatePiece
{
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))] [HarmonyPostfix] [HarmonyWrapSafe]
    [HarmonyPriority(-999)]
    private static void Init(ZNetScene __instance)
    {
        if (SceneManager.GetActiveScene().name != "main") return;

        var wearNTear = conveyor.gameObject.AddComponent<WearNTear>();
        var wood_floor_1x1 = __instance.GetPrefab("wood_floor_1x1").GetComponent<Piece>();
        var wood_floor_1x1_WearNTear = wood_floor_1x1.GetComponent<WearNTear>();

        conveyor.m_placeEffect = wood_floor_1x1.m_placeEffect;
        wearNTear.m_hitEffect = wood_floor_1x1_WearNTear.m_hitEffect;
        wearNTear.m_destroyedEffect = wood_floor_1x1_WearNTear.m_destroyedEffect;
    }
}