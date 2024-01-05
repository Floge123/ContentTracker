using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Linq;

namespace MentorRouletteCounter
{
    public sealed class Plugin : IDalamudPlugin
    {
        private readonly DutyTracker _dutyTracker;

        public string Name => "Mentor Roulette Tracker";

        private DalamudPluginInterface PluginInterface { get; init; }
        public Configuration Configuration { get; init; }

        public Plugin([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
        {
            PluginInterface = pluginInterface;
            Service.Initialize(pluginInterface);

            Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            Configuration.Initialize(PluginInterface);
            ContentRepository.Initialize();

            try
            {
                _dutyTracker = new DutyTracker();            
                Service.Duty.DutyStarted += Duty_DutyStarted;
                Service.Duty.DutyCompleted += Duty_DutyCompleted;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

        private void Duty_DutyStarted(object? sender, ushort e)
        {
            _dutyTracker.Start();
        }

        private void Duty_DutyCompleted(object? sender, ushort e)
        {
            try
            {
                var territory = Service.GameData.Excel.GetSheet<TerritoryType>()?.GetRow(e);
                _dutyTracker.End(territory.ContentFinderCondition.Value);
                _dutyTracker.ExportAsCsv();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }

        public void Dispose()
        {
            Service.Duty.DutyStarted -= Duty_DutyStarted;
            Service.Duty.DutyCompleted -= Duty_DutyCompleted;
        }
    }
}
