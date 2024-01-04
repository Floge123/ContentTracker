using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using SamplePlugin.Windows;
using Lumina.Excel.GeneratedSheets;
using System;

namespace SamplePlugin
{
    public sealed class Plugin : IDalamudPlugin
    {
        private readonly DutyTracker _dutyTracker;
        private const string LogPath = "D:\\Bibliothek\\Dokumente\\MentorRoulette\\Log.txt";
        private const string ExportPath = "D:\\Bibliothek\\Dokumente\\MentorRoulette\\Export.csv";

        public string Name => "Mentor Roulette Tracker";
        private const string CommandName = "/pmycommand";

        private DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("SamplePlugin");

        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            Service.Initialize(pluginInterface);

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
            var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);

            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this, goatImage);

            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "A useful message to display in /xlhelp"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            // Check if the directory exists, create it if it doesn't.
            string directoryPath = Path.GetDirectoryName(LogPath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Check if the file exists, create it if it doesn't.
            if (!File.Exists(LogPath))
            {
                using (File.Create(LogPath)) { } // Create the file and immediately close it.
            }

            ContentRepository.Initialize();
            _dutyTracker = new DutyTracker();
            Service.Duty.DutyStarted += Duty_DutyStarted;
            Service.Duty.DutyCompleted += Duty_DutyCompleted;
        }

        private void Duty_DutyStarted(object? sender, ushort e)
        {
            _dutyTracker.Start();
        }

        private void Duty_DutyCompleted(object? sender, ushort e)
        {
            using (StreamWriter w = File.AppendText(LogPath))
            {
                try
                {
                    w.WriteLine($"Duty Detected. TerritoryType: {e}");
                    var territory = Service.GameData.Excel.GetSheet<TerritoryType>()?.GetRow(e);
                    _dutyTracker.End(territory.ContentFinderCondition.Value);
                    _dutyTracker.ExportAsCsv(ExportPath);
                }
                catch (Exception ex)
                {
                    w.WriteLine(ex.ToString());
                }
            }
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            Service.Duty.DutyCompleted -= Duty_DutyCompleted;

            ConfigWindow.Dispose();
            MainWindow.Dispose();

            this.CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            MainWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }
    }
}
