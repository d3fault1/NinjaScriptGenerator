﻿using System;
using System.Globalization;
using Newtonsoft.Json;
using System.IO;
using System.Threading;

namespace NinjaScriptGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Test Region
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
            //Input ip = new Input
            //{
            //    Name = "MyInput",
            //    Description = "My input description",
            //    Type = VariableType.Double,
            //    Value = "5.5",
            //    Minimum = "1"
            //};
            //ConditionSet cs = new ConditionSet
            //{
            //    Compares = new CompareData[] {
            //        new CompareData
            //        {
            //            FirstObject = new Bollinger
            //            {
            //                Period = "2",
            //                NumStdDev = "1",
            //                BarsAgo = 1,
            //                Offset = "0.5",
            //                OffsetType = OffsetType.Arithmetic,
            //                PlotOnChart = true,
            //                Price = PriceType.Weighted,
            //                ValuePlot = BandValue.Upper
            //            },
            //            //SecondObject = new CurrentDayOHL
            //            //{
            //            //    BarsAgo = 1,
            //            //    Offset = "2",
            //            //    OffsetType = OffsetType.Arithmetic,
            //            //    PlotOnChart = true,
            //            //    Price = PriceType.High,
            //            //    ValuePlot = CurrentDayOHLValue.CurrentOpen
            //            //},
            //            SecondObject = new VariableReference
            //            {
            //                Reference = "MyVar"
            //            },
            //            Operation = CompareType.Greater
            //        },
            //        new CompareData
            //        {
            //            FirstObject = new Bollinger
            //            {
            //                Period = "2",
            //                NumStdDev = "1",
            //                BarsAgo = 5,
            //                Offset = "2",
            //                OffsetType = OffsetType.Pips,
            //                PlotOnChart = true,
            //                Price = PriceType.Weighted,
            //                ValuePlot = BandValue.Upper
            //            },
            //            SecondObject = new High
            //            {
            //                BarsAgo = 8,
            //                Offset = "-5.5",
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
            //    Value = "-5"
            //};
            //strategyData.Instruments = new InstrumentData[] { id };
            //strategyData.Variables = new Variable[] { vr };
            //strategyData.Inputs = new Input[] { ip };
            //strategyData.ConditionSets = new ConditionSet[] { cs };
            //strategyData.TargetActions = new TargetAction[] { ta };
            //var output = JsonConvert.SerializeObject(strategyData, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented, Culture = CultureInfo.InvariantCulture });
            //File.WriteAllText(@"F:\sample.json", output);
            #endregion

            try
            {
                //var input = File.ReadAllText(args[0]);
                var input = File.ReadAllText(@"data.json");
                try
                {
                    var data = JsonConvert.DeserializeObject<StrategyData>(input, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented, Culture = CultureInfo.InvariantCulture });
                    CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
                    CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

                    var retval = CodeGenerator.GenerateFromStrategyData(data);
                    Console.WriteLine(retval);
                }
                catch
                {
                    Console.WriteLine("Error: Invalid JSON, Please check proper formats for the JSON");
                }
            }
            catch
            {
                //Console.WriteLine("Error: File not found");
            }
        }
    }
}
