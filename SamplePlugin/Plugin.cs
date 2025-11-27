using Dalamud.IoC;
using Dalamud.Plugin;
using Lumina.Excel.Sheets;
using MentorRouletteCounter.DutyTracking;
using MentorRouletteCounter.PeopleTracking;
using System;

namespace MentorRouletteCounter
{
    public sealed class Plugin : IDalamudPlugin
    {
        private readonly DutyTracker _dutyTracker;
        private readonly PeopleTracker _peopleTracker;

        public string Name => "Mentor Roulette Tracker";

        private IDalamudPluginInterface PluginInterface { get; init; }
        public Configuration Configuration { get; init; }

        public Plugin(IDalamudPluginInterface pluginInterface)
        {
            try
            {
                PluginInterface = pluginInterface;
                Service.Initialize(pluginInterface);

                Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
                Configuration.Initialize(PluginInterface);
                ContentRepository.Initialize();

                _dutyTracker = new DutyTracker();               
                Service.Duty.DutyStarted += Duty_DutyStarted;
                Service.Duty.DutyCompleted += Duty_DutyCompleted;

                _peopleTracker = new PeopleTracker();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
        }


        private void Duty_DutyStarted(object? sender, ushort e)
        {
            var territory = Service.GameData.Excel.GetSheet<TerritoryType>()?.GetRow(e);
            var content = territory?.ContentFinderCondition.Value;
            _dutyTracker.Start(content);
        }

        private void Duty_DutyCompleted(object? sender, ushort e)
        {
            try
            {
                var territory = Service.GameData.Excel.GetSheet<TerritoryType>()?.GetRow(e);
                var content = territory?.ContentFinderCondition.Value;
                if (content is null)
                    return;

                _dutyTracker.End(content.Value);
                _dutyTracker.ExportAsCsv();

                _peopleTracker.Track(content.Value);
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
