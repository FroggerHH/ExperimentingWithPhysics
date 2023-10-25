using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace ExperimentingWithPhysics.Patch;

[HarmonyPatch] public class CreatePiece
{
    private static Piece conveyor;
    private static bool done;

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))] [HarmonyPostfix] [HarmonyWrapSafe] 
    private static void Init(ZNetScene __instance)
    {
        if (done) return;
        if (SceneManager.GetActiveScene().name != "main") return;
        if (!ZNet.instance.IsServer()) return;
        var parent = new GameObject("ExperimentingWithPhysics_Parent").transform;
        parent.gameObject.SetActive(false);
        DontDestroyOnLoad(parent);

        done = true;
        var wood_floor_1x1 = __instance.GetPrefab("wood_floor_1x1").GetComponent<Piece>();
        conveyor = Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), Vector3.zero, Quaternion.identity,
            parent).AddComponent<Piece>();
        conveyor.name = "Conveyor";
        conveyor.gameObject.layer = LayerMask.NameToLayer("piece");
        conveyor.gameObject.AddComponent<Conveyor>();
        var rigidbody = conveyor.gameObject.AddComponent<Rigidbody>();
        rigidbody.isKinematic = true;
        conveyor.transform.localScale = new Vector3(2f, 0.3f, 1f);
        var zNetView = conveyor.gameObject.AddComponent<ZNetView>();
        zNetView.m_syncInitialScale = false;
        zNetView.m_ghost = false;
        zNetView.m_distant = false;
        zNetView.m_persistent = true;
        zNetView.m_type = ZDO.ObjectType.Solid;
        conveyor.enabled = true;
        conveyor.m_canBeRemoved = true;
        conveyor.m_isUpgrade = false;
        conveyor.m_clipEverything = true;
        conveyor.m_comfort = 0;
        conveyor.m_groundPiece = false;
        conveyor.m_placeEffect = wood_floor_1x1.m_placeEffect;
        conveyor.m_category = Piece.PieceCategory.Misc;
        conveyor.m_resources = new Piece.Requirement[0];
        conveyor.m_icon = Sprite.Create(new Texture2D(128, 128), new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
        var wearNTear = conveyor.gameObject.AddComponent<WearNTear>();
        wearNTear.m_health = 1000;
        wearNTear.m_new = conveyor.gameObject;
        wearNTear.m_worn = conveyor.gameObject;
        wearNTear.m_wet = conveyor.gameObject;
        wearNTear.m_broken = conveyor.gameObject;
        wearNTear.m_destroyedEffect = wood_floor_1x1.GetComponent<WearNTear>().m_destroyedEffect;
        var material = conveyor.GetComponent<Renderer>().sharedMaterial;
        material.color = new Color(0.82f, 0.62f, 0.38f);
        material.SetFloat("_EMISSION", 0);
        material.SetFloat("_METALLICGLOSSMAP", 0);

        LoadImageFromWEB(
            @"https://cdn.dribbble.com/users/47421/screenshots/3772951/conveyor-belt.png",
            sprite => conveyor.m_icon = sprite);
    }

    public static void LoadImageFromWEB(string url, Action<Sprite> callback)
    {
        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out _)) return;

        GetPlugin().StartCoroutine(_Internal_LoadImage(url, callback));
    }

    private static IEnumerator _Internal_LoadImage(string url, Action<Sprite> callback)
    {
        using var request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.result is UnityWebRequest.Result.Success)
        {
            var texture = DownloadHandlerTexture.GetContent(request);
            if (texture.width == 0 || texture.height == 0) yield break;
            var temp = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            temp.SetPixels(texture.GetPixels());
            temp.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            sprite.name = url.Split('/').Last();
            callback?.Invoke(sprite);
        }
    }

    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))] [HarmonyWrapSafe]
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.CopyOtherDB))]
    [HarmonyPostfix]
    private static void PieceManager_Patch_ObjectDBInit(ObjectDB __instance)
    {
        var pieceTable = __instance?.GetItemPrefab("Hammer")?.GetComponent<ItemDrop>()?.m_itemData.m_shared
            .m_buildPieces;
        if (pieceTable == null || conveyor == null) return;
        if (!pieceTable.m_pieces.Contains(conveyor.gameObject))
            pieceTable.m_pieces.Add(conveyor.gameObject);
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))] [HarmonyWrapSafe] [HarmonyPostfix]
    private static void Patch_ZNetSceneAwake(ZNetScene __instance)
    {
        if (!__instance.m_prefabs.Contains(conveyor.gameObject))
            __instance.m_prefabs.Add(conveyor.gameObject);
        var hashCode = conveyor.name.GetStableHashCode();
        if (!__instance.m_namedPrefabs.ContainsKey(hashCode))
            __instance.m_namedPrefabs.Add(hashCode, conveyor.gameObject);
    }
}