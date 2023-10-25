using BepInEx;
using BepInEx.Configuration;

namespace ExperimentingWithPhysics;

[BepInPlugin(ModGUID, ModName, ModVersion)]
internal class Plugin : BaseUnityPlugin
{
    internal const string ModName = "ExperimentingWithPhysics",
        ModVersion = "1.0.0",
        ModAuthor = "JustAFrogger",
        ModGUID = $"com.{ModAuthor}.{ModName}";

    public static ConfigEntry<float> maxImpulse;
    public static ConfigEntry<float> angularDrag;
    public static ConfigEntry<float> velocityModifier;

    private void Awake()
    {
        CreateMod(this, ModName, ModAuthor, ModVersion, ModGUID);
        OnConfigurationChanged += () =>
        {
            foreach (var body in CustomPhysics.all)
            {
                body.maxImpulse = maxImpulse.Value;
                body.rb.angularDrag = angularDrag.Value;
                if (body is Conveyor conveyor) conveyor.velocityModifier = velocityModifier.Value;
            }
        };
        maxImpulse = config("General", "Max Impulse", 2f,
            new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 10f)));
        angularDrag = config("General", "Angular Drag", 5f,
            new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 100f)));
        velocityModifier = config("General", "Conveyor Velocity Modifier", 1f,
            new ConfigDescription("", new AcceptableValueRange<float>(1f, 10f)));
    }
}