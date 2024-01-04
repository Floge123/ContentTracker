using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Dalamud.Plugin;

namespace MentorRouletteCounter
{
    public class Service
    {
        public static void Initialize(DalamudPluginInterface pluginInterface)
            => pluginInterface.Create<Service>();

        // @formatter:off
        [PluginService][RequiredVersion("1.0")] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static ICommandManager Commands { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static ISigScanner SigScanner { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static IDataManager GameData { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static IChatGui Chat { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static IPluginLog Log { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static IGameInteropProvider Interop { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static IClientState Client { get; private set; } = null!;
        [PluginService][RequiredVersion("1.0")] public static IDutyState Duty { get; private set; } = null!;

        // @formatter:on
    }
}
