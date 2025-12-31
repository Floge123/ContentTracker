using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using MentorRouletteCounter.Trackers;
using MentorRouletteCounter.Windows;
using System;

namespace MentorRouletteCounter
{
    public sealed class Plugin : IDalamudPlugin
    {
        private const string MainCommand = "/dutytracker";

        public string Name => "Duty Tracker";

        private IDalamudPluginInterface PluginInterface { get; init; }
        public Configuration Configuration { get; init; }
        public readonly WindowSystem WindowSystem = new("Duty Tracker");
        private MainWindow MainWindow { get; init; }
        private ITrackerManager TrackerManager { get; init; }

        public Plugin(IDalamudPluginInterface pluginInterface)
        {
            try
            {
                PluginInterface = pluginInterface;
                Service.Initialize(pluginInterface);

                MainWindow = new MainWindow(this);
                Service.Commands.AddHandler(MainCommand, new Dalamud.Game.Command.CommandInfo(OnMainCommand)
                {
                    HelpMessage = "Shows the main UI"
                });

                PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
                PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;
                PluginInterface.UiBuilder.OpenConfigUi += ToggleMainUi;

                Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
                Configuration.Initialize(PluginInterface);

                TrackerManager = new TrackerManager();
                TrackerManager.Initialize();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

        public void Dispose()
        {
            TrackerManager.Dispose();
            // Unregister all actions to not leak anything during disposal of plugin
            PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
            PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;

            WindowSystem.RemoveAllWindows();

            MainWindow.Dispose();

            Service.Commands.RemoveHandler(MainCommand);
        }

        private void ToggleMainUi() => MainWindow.Toggle();

        private void OnMainCommand(string command, string arguments) => MainWindow.Toggle();
    }
}
