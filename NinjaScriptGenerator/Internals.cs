namespace NinjaScriptGenerator
{
    //Commons
    struct Errors
    {
        public static string Success = "Code Successfully Generated";
        public static string IncompatibleCompares = "Error: Incompatible Comparing Pairs. Please Select Proper Pairs to Compare";
        public static string DuplicateVariables = "Error: Cannot Contain Multiple Variables with Same Name";
        public static string InvalidVariables = "Error: Variable Value is Incompatible with the Variable Type";
        public static string DuplicateInstruments = "Error: Cannot Contain Multiple Instruments with Same Setup";
        public static string DuplicateTargetActions = "Error: Cannot Contain Multiple Target Actions with Same Parameters";
        public static string InvalidCompares = "Error: Compare Data is Invalid. Please Check if Proper Data is Supplied";
        public static string InvalidCondition = "Error: ConditionSet is Invalid. Please Check if Proper Data is Supplied";
        public static string InternalFatal = "Error: Something That Should Not be Happened Just Happened. Please Refer to the Developer";
    }
    class Helper
    {
        public static string ToStringHelperForOffset(string input, double offset, OffsetType type, ArithmeticOperator sign)
        {
            if (offset == 0) return input;
            switch (type)
            {
                case OffsetType.Arithmetic:
                default:
                    switch (sign)
                    {
                        case ArithmeticOperator.Plus:
                        default:
                            return offset < 0 ? $"({input} + ({offset}))" : $"({input} + {offset})";
                        case ArithmeticOperator.Minus:
                            return offset < 0 ? $"({input} - ({offset}))" : $"({input} - {offset})";
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
        string ToFormatString(string varName);
    }
    interface IPriceAction
    {
        string ToFormatString();
    }
    interface IOperation
    {
        TargetActionType Action { get; set; }
        double Quantity { get; set; }
    }
}
