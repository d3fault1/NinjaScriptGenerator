using Newtonsoft.Json;

namespace NinjaScriptGenerator
{
    class StrategyData
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Description { get; set; }
        [JsonProperty(Required = Required.Always)]
        public DefaultProperties Defaults { get; set; }
        [JsonProperty(Required = Required.Always)]
        public InstrumentData[] Instruments { get; set; }
        [JsonProperty(Required = Required.Always)]
        public Input[] Inputs { get; set; }
        [JsonProperty(Required = Required.Always)]
        public Variable[] Variables { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ConditionSet[] ConditionSets { get; set; }
        [JsonProperty(Required = Required.Always)]
        public TargetAction[] TargetActions { get; set; }
    }
}