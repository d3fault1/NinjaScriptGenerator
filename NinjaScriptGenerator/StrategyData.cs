namespace NinjaScriptGenerator
{
    class StrategyData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DefaultProperties Defaults { get; set; }
        public InstrumentData[] Instruments { get; set; }
        public Variable[] Variables { get; set; }
        public ConditionSet[] ConditionSets { get; set; }
        public TargetAction[] TargetActions { get; set; }
    }
}