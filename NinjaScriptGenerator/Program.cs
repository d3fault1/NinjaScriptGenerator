using System;
using Newtonsoft.Json;
using System.IO;

namespace NinjaScriptGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            //StrategyData strategyData = new StrategyData();
            //strategyData.Name = "My Strategy";
            //strategyData.Description = "This is a test strategy";
            //strategyData.Defaults = new DefaultProperties
            //{
            //    Calculate = CalculateMethod.OnBarClose,
            //    ContractsPerEntry = 1,
            //    ExitOnSessionClose = false,
            //};
            //InstrumentData id = new InstrumentData
            //{
            //    Name = Instrument.INS_FDAX_03_22,
            //    Type = InstrumentType.Minute,
            //    Value = 1
            //};
            //Variable vr = new Variable
            //{
            //    Name = "MyVar",
            //    Type = VariableType.Int32,
            //    Value = "1"
            //};
            //ConditionSet cs = new ConditionSet
            //{
            //    Compares = new CompareData[] { new CompareData
            //        {
            //            FirstObject = new Bollinger
            //            {
            //                Period = 2,
            //                NumStdDev = 1,
            //                BarsAgo = 1,
            //                Offset = 0.5,
            //                OffsetType = OffsetType.Arithmetic,
            //                PlotOnChart = true,
            //                Price = PriceType.Weighted,
            //                ValuePlot = BandValue.Upper
            //            },
            //            SecondObject = new CurrentDayOHL
            //            {
            //                BarsAgo = 1,
            //                Offset = 2,
            //                OffsetType = OffsetType.Arithmetic,
            //                PlotOnChart = true,
            //                Price = PriceType.High,
            //                ValuePlot = CurrentDayOHLValue.CurrentOpen
            //            },
            //            Operation = CompareType.Greater
            //        },
            //        new CompareData
            //        {
            //            FirstObject = new Bollinger
            //            {
            //                Period = 2,
            //                NumStdDev = 1,
            //                BarsAgo = 5,
            //                Offset = 2,
            //                OffsetType = OffsetType.Pips,
            //                PlotOnChart = true,
            //                Price = PriceType.Weighted,
            //                ValuePlot = BandValue.Upper
            //            },
            //            SecondObject = new High
            //            {
            //                BarsAgo = 8,
            //                Offset = -5.5,
            //                OffsetType = OffsetType.Percent,
            //                Operator = ArithmeticOperator.Divide
            //            },
            //            Operation = CompareType.CrossAbove
            //        }
            //    },
            //    Operations = new Long[] {new Long
            //        {
            //            Action = TargetActionType.Entry,
            //            Quantity = -1
            //        }},
            //    ConditionType = ConditionType.IfAll
            //};
            //TargetAction ta = new TargetAction
            //{
            //    TargetType = TargetType.StopLoss,
            //    Type = ProfitLossType.Price,
            //    Value = -5
            //};
            //strategyData.Instruments = new InstrumentData[] { id };
            //strategyData.Variables = new Variable[] { vr };
            //strategyData.ConditionSets = new ConditionSet[] { cs };
            //strategyData.TargetActions = new TargetAction[] { ta };
            //var output = JsonConvert.SerializeObject(strategyData, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented });
            //File.WriteAllText(@"F:\testjson.txt", output);

            //var input = File.ReadAllText(@"F:\testjson.txt");
            var input = File.ReadAllText(args[0]);
            var data = JsonConvert.DeserializeObject<StrategyData>(input, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented });
            var retval = CodeGenerator.GenerateFromStrategyData(data);
            Console.ReadLine();
            Console.WriteLine(retval);
            Console.ReadLine();
        }
    }
}
