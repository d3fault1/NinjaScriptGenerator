using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace NinjaScriptGenerator
{
    //Commons
    struct Errors
    {
        public static string Success = "Code Successfully Generated";
        public static string IncompatibleCompares = "Error: Incompatible Comparing Pairs. Please Select Proper Pairs to Compare";
        public static string DuplicateVariables = "Error: Cannot Contain Multiple Variables with Same Name";
        public static string DuplicateInputs = "Error: Cannot Contain Multiple Input Parameters with Same Name";
        public static string InvalidVariableName = "Error: Variable Name is Invalid";
        public static string InvalidVariable = "Error: Variable Value is Incompatible with the Variable Type";
        public static string InvalidVariableRef = "Error: Invalid Variable Reference. Variable Definition does not Exist";
        public static string InvalidParam = "Error: Data Parameter is Incompatible with the Data Type";
        public static string InvalidEnum = "Error: Parameter Type is Invalid";
        public static string DuplicateInstruments = "Error: Cannot Contain Multiple Instruments with Same Setup";
        public static string DuplicateTargetActions = "Error: Cannot Contain Multiple Target Actions with Same Parameters";
        public static string InvalidCompares = "Error: Compare Data is Invalid. Please Check if Proper Data is Supplied";
        public static string InvalidCondition = "Error: ConditionSet is Invalid. Please Check if Proper Data is Supplied";
        public static string InternalFatal = "Error: Something That Should Not be Happened Just Happened. Please Refer to the Developer";
    }

    class Helper
    {
        public static string ToStringHelperForOffset(string input, string offset, OffsetType type, ArithmeticOperator sign)
        {
            double val;
            bool isRaw = Double.TryParse(offset, out val);
            if (isRaw && val == 0) return input;
            switch (type)
            {
                case OffsetType.Arithmetic:
                default:
                    switch (sign)
                    {
                        case ArithmeticOperator.Plus:
                        default:
                            return (isRaw && val >= 0) ? $"({input} + {offset})" : $"({input} + ({offset}))";
                        case ArithmeticOperator.Minus:
                            return (isRaw && val >= 0) ? $"({input} - {offset})" : $"({input} - ({offset}))";
                        case ArithmeticOperator.Multiply:
                            return $"({input} * {offset})";
                        case ArithmeticOperator.Divide:
                            return $"({input} / {offset})";
                    }
                case OffsetType.Percent:
                    return $"({input} * {offset})";
                case OffsetType.Pips:
                    return $"({input} + ({offset} * (TickSize * 10)))";
                case OffsetType.Ticks:
                    return $"({input} + ({offset} * TickSize))";
            }
        }

        public static bool FormatComponents(StrategyData data, out string error)
        {
            if (data.Defaults == null || data.Name == null)
            {
                error = Errors.InternalFatal;
                return false;
            }
            if (data.Description == null) data.Description = "";
            if (data.Instruments == null) data.Instruments = new InstrumentData[0];
            if (data.Variables == null) data.Variables = new Variable[0];
            if (data.Inputs == null) data.Inputs = new Input[0];
            if (data.ConditionSets == null) data.ConditionSets = new ConditionSet[0];
            if (data.TargetActions == null) data.TargetActions = new TargetAction[0];
            if (data.Name == "")
            {
                error = Errors.InternalFatal;
                return false;
            }

            data.Name = data.Name.Replace(" ", "");


            error = Errors.InvalidVariable;
            foreach (var variable in data.Variables)
            {
                if (!Regex.IsMatch(variable.Name, "^[a-zA-Z_@][a-zA-Z_0-9]*$"))
                {
                    error = Errors.InvalidVariableName;
                    return false;
                }

                try
                {
                    var proper = Convert.ChangeType(variable.Value, (TypeCode)variable.Type);
                    if (variable.Type != VariableType.Time && variable.Type != VariableType.Double && variable.Type != VariableType.Int32) variable.Value = proper.ToString();
                }
                catch
                {
                    if (variable.Value == "")
                    {
                        switch (variable.Type)
                        {
                            case VariableType.Boolean:
                                variable.Value = DefaultValue.BooleanDefault;
                                break;
                            case VariableType.Int32:
                                variable.Value = DefaultValue.IntegerDefault;
                                break;
                            case VariableType.Double:
                                variable.Value = DefaultValue.DoubleDefault;
                                break;
                            case VariableType.Time:
                                variable.Value = DefaultValue.TimeDefault;
                                break;
                            case VariableType.String:
                                variable.Value = "";
                                break;
                            default:
                                return false;
                        }
                    }
                    return false;
                }
            }

            foreach (var input in data.Inputs)
            {
                if (!Regex.IsMatch(input.Name, "^[a-zA-Z@][a-zA-Z0-9]*$"))
                {
                    error = Errors.InvalidVariableName;
                    return false;
                }
                input.Name = input.Name[0].ToString().ToUpperInvariant() + input.Name.Substring(1);

                if (input.Type != VariableType.Int32 && input.Type != VariableType.Double && input.Minimum != "") return false;

                try
                {
                    var proper = Convert.ChangeType(input.Value, (TypeCode)input.Type);
                    if (input.Type != VariableType.Time && input.Type != VariableType.Double && input.Type != VariableType.Int32) input.Value = proper.ToString();
                }
                catch
                {
                    if (input.Value != "") return false;
                }

                try
                {
                    var proper = Convert.ChangeType(input.Minimum, (TypeCode)input.Type);
                    input.Minimum = proper.ToString();
                }
                catch
                {
                    if (input.Minimum != "") return false;
                }
            }

            error = Errors.InvalidParam;
            foreach (var cnd_set in data.ConditionSets)
            {
                foreach (var cmp in cnd_set.Compares)
                {
                    var fprops = cmp.FirstObject.GetType().GetProperties().Where(obj => obj.Name == "Offset" || obj.Name == "Period" || obj.Name == "NumStdDev" || obj.Name == "Smooth" || obj.Name == "Fast" || obj.Name == "Slow" || obj.Name == "MAPeriod" || obj.Name == "ChangeRatePeriod" || obj.Name == "OffsetMultiplier" || obj.Name == "PeriodD" || obj.Name == "PeriodK" || obj.Name == "Strength" || obj.Name == "Intermediate" || obj.Name == "DeviationValue");
                    var sprops = cmp.SecondObject.GetType().GetProperties().Where(obj => obj.Name == "Offset" || obj.Name == "Period" || obj.Name == "NumStdDev" || obj.Name == "Smooth" || obj.Name == "Fast" || obj.Name == "Slow" || obj.Name == "MAPeriod" || obj.Name == "ChangeRatePeriod" || obj.Name == "OffsetMultiplier" || obj.Name == "PeriodD" || obj.Name == "PeriodK" || obj.Name == "Strength" || obj.Name == "Intermediate" || obj.Name == "DeviationValue");
                    foreach (var fprop in fprops)
                    {
                        Type type = typeof(int);
                        if (fprop.Name == "Offset" || fprop.Name == "OffsetMultiplier" || fprop.Name == "DeviationValue") type = typeof(double);
                        if (!TypeHandle(cmp.FirstObject, fprop.Name, type, data)) return false;
                    }
                    foreach (var sprop in sprops)
                    {
                        Type type = typeof(int);
                        if (sprop.Name == "Offset" || sprop.Name == "OffsetMultiplier" || sprop.Name == "DeviationValue") type = typeof(double);
                        if (!TypeHandle(cmp.FirstObject, sprop.Name, type, data)) return false;
                    }
                }
            }

            foreach (var tgt in data.TargetActions)
            {
                double num;
                if (!Double.TryParse(tgt.Value, out num))
                {
                    if (tgt.Value == "") tgt.Value = "0";
                    else return false;
                }
                else tgt.Value = num.ToString();

                if (!Enum.IsDefined(typeof(ProfitLossType), tgt.Type))
                {
                    error = Errors.InvalidEnum;
                    return false;
                }
                if (!Enum.IsDefined(typeof(TargetType), tgt.TargetType))
                {
                    error = Errors.InvalidEnum;
                    return false;
                }
            }

            foreach (var ins in data.Instruments)
            {
                if (!Enum.IsDefined(typeof(Instrument), ins.Name))
                {
                    error = Errors.InvalidEnum;
                    return false;
                }
                if (!Enum.IsDefined(typeof(InstrumentType), ins.Type))
                {
                    error = Errors.InvalidEnum;
                    return false;
                }
            }

            if (!Enum.IsDefined(typeof(CalculateMethod), data.Defaults.Calculate))
            {
                error = Errors.InvalidEnum;
                return false;
            }

            error = "";
            return true;
        }

        private static bool TypeHandle(ICompareData obj, string property, Type type, StrategyData data)
        {
            if (obj.GetType().GetProperty(property) != null)
            {
                string def;
                switch (type.Name)
                {
                    case "Double":
                        def = DefaultValue.DoubleDefault;
                        break;
                    case "Int32":
                        def = DefaultValue.IntegerDefault;
                        break;
                    case "Boolean":
                        def = DefaultValue.BooleanDefault;
                        break;
                    case "DateTime":
                        def = DefaultValue.TimeDefault;
                        break;
                    default:
                        def = "";
                        break;
                }
                var f = (string)obj.GetType().GetProperty(property).GetValue(obj);
                try
                {
                    var prop = Convert.ChangeType(f, type);
                    obj.GetType().GetProperty(property).SetValue(obj, prop.ToString());
                }
                catch
                {
                    if (f == "") obj.GetType().GetProperty(property).SetValue(obj, def);
                    else
                    {
                        var mv = data.Variables.FirstOrDefault(a => a.Name == f);
                        var mi = data.Inputs.FirstOrDefault(a => a.Name == f);
                        if (mv != null)
                        {
                            var tp = mv.Type == VariableType.Time ? Type.GetType("System.DateTime") : Type.GetType($"System.{mv.Type}");
                            if (tp == type) return true;
                            else return false;
                        }
                        if (mi != null)
                        {
                            var tp = mi.Type == VariableType.Time ? Type.GetType("System.DateTime") : Type.GetType($"System.{mi.Type}");
                            if (tp == type) return true;
                            else return false;
                        }
                        return false;
                    }
                }
            }
            return true;
        }

        private struct DefaultValue
        {
            public static string IntegerDefault = "0";
            public static string DoubleDefault = "0";
            public static string BooleanDefault = "false";
            public static string TimeDefault = "00:00";
        }
    }


    //Enums
    enum CompareType
    {
        Equals = 7,
        GreaterEqual = 16,
        Greater = 15,
        LessEqual = 14,
        Less = 13,
        NotEqual = 6,
        CrossAbove = 22,
        CrossBelow = 23
    }
    enum ConditionType
    {
        IfAll = 12,
        IfAny = 11
    }
    enum Instrument
    {
        INS_6B_03_22,
        INS_6E_03_22,
        INS_CL_03_22,
        INS_ES_03_22,
        INS_FDAX_03_22,
        INS_GC_03_22,
        INS_NQ_03_22,
        INS_RTY_03_22,
        INS_YM_03_22,
        INS_ZB_03_22,
        INS_ZN_03_22,
        INS_ZS_03_22,
        INS_ZW_03_22
    }
    enum InstrumentType
    {
        Day,
        Minute,
        Second,
        Ticks,
        Range
    }
    enum CalculateMethod
    {
        OnBarClose,
        OnEachTick
    }
    enum ProfitLossType
    {
        Price,
        Currency,
        Ticks,
        Disabled
    }
    enum PivotRange
    {
        Daily,
        Weekly,
        Monthly
    }
    enum HLCCalculationMode
    {
        CalcFromIntradayData,
        DailyBars
    }
    enum DeviationType
    {
        Points,
        Percent
    }
    enum OffsetType
    {
        Arithmetic,
        Percent,
        Pips,
        Ticks
    }
    enum PriceType
    {
        Close,
        High,
        Low,
        Median,
        Open,
        Typical,
        Weighted
    }
    enum BandValue
    {
        Upper,
        Middle,
        Lower
    }
    enum ChannelValue
    {
        Upper,
        Midline,
        Lower
    }
    enum CurrentDayOHLValue
    {
        CurrentOpen,
        CurrentHigh,
        CurrentLow
    }
    enum PriorDayOHLCValue
    {
        PriorOpen,
        PriorClose,
        PriorHigh,
        PriorLow
    }
    enum FibonacciPivotValue
    {
        Pp,
        R1,
        R2,
        R3,
        S1,
        S2,
        S3
    }
    enum MACDValue
    {
        Avg,
        MACD,
        Diff
    }
    enum RSIValue
    {
        Avg,
        RSI
    }
    enum StochasticsValue
    {
        D,
        K
    }
    enum SwingValue
    {
        SwingHigh,
        SwingLow
    }
    enum ZigZagValue
    {
        ZigZagHigh,
        ZigZagLow
    }
    enum VariableType
    {
        Int32 = 9,
        Double = 14,
        Boolean = 3,
        String = 18,
        Time = 16
    }
    enum ArithmeticOperator
    {
        Plus,
        Minus,
        Multiply,
        Divide
    }
    enum TargetType
    {
        TakeProfit,
        StopLoss
    }
    enum TargetActionType
    {
        Entry,
        Exit
    }


    //Interfaces
    interface ICompareData
    {
    }
    interface IIndicator
    {
        bool PlotOnChart { get; set; }
        string ToCtorString();
        string ToFormatString(string varName, bool includeBarsAgo = true);
    }
    interface IPriceAction
    {
        string ToFormatString(bool includeBarsAgo = true);
    }
    interface IOperation
    {
        TargetActionType Action { get; set; }
        double Quantity { get; set; }
    }
}
