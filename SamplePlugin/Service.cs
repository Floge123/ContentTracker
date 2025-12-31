using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace MentorRouletteCounter
{
    public class Service
    {
        public static void Initialize(IDalamudPluginInterface pluginInterface)
            => pluginInterface.Create<Service>();

        // @formatter:off
        [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static ICommandManager Commands { get; private set; } = null!;
        [PluginService] public static ISigScanner SigScanner { get; private set; } = null!;
        [PluginService] public static IDataManager GameData { get; private set; } = null!;
        [PluginService] public static IChatGui Chat { get; private set; } = null!;
        [PluginService] public static IPluginLog Log { get; private set; } = null!;
        [PluginService] public static IGameInteropProvider Interop { get; private set; } = null!;
        [PluginService] public static IClientState Client { get; private set; } = null!;
        [PluginService] public static IDutyState Duty { get; private set; } = null!;
        [PluginService] public static IFramework Framework { get; private set; } = null!;
        [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
        [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;

        public static readonly WindowSystem WindowSystem = new("Duty Tracker");
        // @formatter:on
    }
}
