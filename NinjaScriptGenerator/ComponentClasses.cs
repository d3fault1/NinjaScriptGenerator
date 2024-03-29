﻿using Newtonsoft.Json;
using System;

namespace NinjaScriptGenerator
{
    class CompareData
    {
        [JsonProperty(Required = Required.Always)]
        public ICompareData FirstObject { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ICompareData SecondObject { get; set; }
        [JsonProperty(Required = Required.Always)]
        public CompareType Operation { get; set; }
    }

    class ConditionSet
    {
        [JsonProperty(Required = Required.Always)]
        public CompareData[] Compares { get; set; }
        [JsonProperty(Required = Required.Always)]
        public IOperation[] Operations { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ConditionType ConditionType { get; set; }
    }

    //Default Properties
    class DefaultProperties
    {
        [JsonProperty(Required = Required.Always)]
        public CalculateMethod Calculate { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool ExitOnSessionClose { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int ContractsPerEntry { get; set; }
    }

    //Stops And Targets
    class TargetAction
    {
        [JsonProperty(Required = Required.Always)]
        public ProfitLossType Type { get; set; }
        [JsonProperty(Required = Required.Always)]
        public TargetType TargetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Value { get; set; }

        //Overrides
        public override int GetHashCode()
        {
            return (Type.GetHashCode() * 3 + TargetType.GetHashCode()) * 3 + Value.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is TargetAction other)
            {
                if (Type == other.Type && TargetType == other.TargetType && Value == other.Value) return true;
                else return false;
            }
            else return false;
        }
    }

    //Instrument
    class InstrumentData
    {
        [JsonProperty(Required = Required.Always)]
        public Instrument Name { get; set; }
        [JsonProperty(Required = Required.Always)]
        public InstrumentType Type { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int Value { get; set; }

        //Overrides
        public override int GetHashCode()
        {
            return (Name.GetHashCode() * 3 + Type.GetHashCode()) * 3 + Value.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is InstrumentData other)
            {
                if (Name == other.Name && Type == other.Type && Value == other.Value) return true;
                else return false;
            }
            else return false;
        }
    }

    //Indicators
    class ADL : ICompareData, IIndicator
    {
        //Params

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"ADL({Price})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is ADL other)
            {
                if (Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class ADX : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string Period { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"ADX({Price}, {Period})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return Period.GetHashCode() * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is ADX other)
            {
                if (Period == other.Period && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class Bollinger : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string Period { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string NumStdDev { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }
        [JsonProperty(Required = Required.Always)]
        public BandValue ValuePlot { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"Bollinger({Price}, {NumStdDev}, {Period})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return (Period.GetHashCode() * 3 + NumStdDev.GetHashCode()) * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Bollinger other)
            {
                if (Period == other.Period && NumStdDev == other.NumStdDev && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class BOP : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string Smooth { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"BOP({Price}, {Smooth})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return Smooth.GetHashCode() * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is BOP other)
            {
                if (Smooth == other.Smooth && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class ChaikinOscillator : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string Fast { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Slow { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"ChaikinOscillator({Price}, {Fast}, {Slow})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return (Fast.GetHashCode() * 3 + Slow.GetHashCode()) * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is ChaikinOscillator other)
            {
                if (Fast == other.Fast && Slow == other.Slow && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class ChaikinVolatility : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string MAPeriod { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string ChangeRatePeriod { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"ChaikinVolatility({Price}, {MAPeriod}, {ChangeRatePeriod})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return (MAPeriod.GetHashCode() * 3 + ChangeRatePeriod.GetHashCode()) * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is ChaikinVolatility other)
            {
                if (MAPeriod == other.MAPeriod && ChangeRatePeriod == other.ChangeRatePeriod && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class CurrentDayOHL : ICompareData, IIndicator
    {
        //Params

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }
        [JsonProperty(Required = Required.Always)]
        public CurrentDayOHLValue ValuePlot { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"CurrentDayOHL({Price})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is CurrentDayOHL other)
            {
                if (Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class DEMA : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string Period { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"DEMA({Price}, {Period})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return Period.GetHashCode() * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is DEMA other)
            {
                if (Period == other.Period && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class DoubleStochastics : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string Period { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"DoubleStochastics({Price}, {Period})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return Period.GetHashCode() * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is DoubleStochastics other)
            {
                if (Period == other.Period && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class EMA : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string Period { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"EMA({Price}, {Period})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return Period.GetHashCode() * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is EMA other)
            {
                if (Period == other.Period && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class FibonacciPivots : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public PivotRange Range { get; set; }
        [JsonProperty(Required = Required.Always)]
        public HLCCalculationMode CalcMode { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }
        [JsonProperty(Required = Required.Always)]
        public FibonacciPivotValue ValuePlot { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"FibonacciPivots({Price}, PivotRange.{Range}, HLCCalculationMode.{CalcMode}, 0, 0, 0, 20)";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return (Range.GetHashCode() * 3 + CalcMode.GetHashCode()) * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is FibonacciPivots other)
            {
                if (Range == other.Range && CalcMode == other.CalcMode && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class KeltnerChannel : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string Period { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string OffsetMultiplier { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ChannelValue ValuePlot { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"KeltnerChannel({Price}, {OffsetMultiplier}, {Period})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return (Period.GetHashCode() * 3 + OffsetMultiplier.GetHashCode()) * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is KeltnerChannel other)
            {
                if (Period == other.Period && OffsetMultiplier == other.OffsetMultiplier && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class MACD : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string Fast { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Slow { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Smooth { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }
        [JsonProperty(Required = Required.Always)]
        public MACDValue ValuePlot { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"MACD({Price}, {Fast}, {Slow}, {Smooth})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return Helper.ToStringHelperForOffset($"{varName}.{(ValuePlot == MACDValue.MACD ? "Default" : $"{ValuePlot}")}[{BarsAgo}]", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return ((Fast.GetHashCode() * 3 + Slow.GetHashCode()) * 3 + Smooth.GetHashCode()) * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is MACD other)
            {
                if (Fast == other.Fast && Slow == other.Slow && Smooth == other.Smooth && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class MAX : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string Period { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"MAX({Price}, {Period})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return Period.GetHashCode() * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is MAX other)
            {
                if (Period == other.Period && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class MIN : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string Period { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"MIN({Price}, {Period})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return Period.GetHashCode() * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is MIN other)
            {
                if (Period == other.Period && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class Pivots : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public PivotRange Range { get; set; }
        [JsonProperty(Required = Required.Always)]
        public HLCCalculationMode CalcMode { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }
        [JsonProperty(Required = Required.Always)]
        public FibonacciPivotValue ValuePlot { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"Pivots({Price}, PivotRange.{Range}, HLCCalculationMode.{CalcMode}, 0, 0, 0, 20)";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return (Range.GetHashCode() * 3 + CalcMode.GetHashCode()) * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Pivots other)
            {
                if (Range == other.Range && CalcMode == other.CalcMode && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class PriorDayOHLC : ICompareData, IIndicator
    {
        //Params

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }
        [JsonProperty(Required = Required.Always)]
        public PriorDayOHLCValue ValuePlot { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"PriorDayOHLC({Price})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is PriorDayOHLC other)
            {
                if (Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class Range : ICompareData, IIndicator
    {
        //Params

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"Range({Price})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Range other)
            {
                if (Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class RSI : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string Period { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Smooth { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }
        [JsonProperty(Required = Required.Always)]
        public RSIValue ValuePlot { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"RSI({Price}, {Period}, {Smooth})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return Helper.ToStringHelperForOffset($"{varName}.{(ValuePlot == RSIValue.RSI ? "Default" : $"{ValuePlot}")}[{BarsAgo}]", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return (Period.GetHashCode() * 3 + Smooth.GetHashCode()) * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is RSI other)
            {
                if (Period == other.Period && Smooth == other.Smooth && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class SMA : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string Period { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"SMA({Price}, {Period})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return Period.GetHashCode() * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is SMA other)
            {
                if (Period == other.Period && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class StandardDeviation : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string Period { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"StdDev({Price}, {Period})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return Period.GetHashCode() * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is StandardDeviation other)
            {
                if (Period == other.Period && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class Stochastics : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string PeriodD { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string PeriodK { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Smooth { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }
        [JsonProperty(Required = Required.Always)]
        public StochasticsValue ValuePlot { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"Stochastics({Price}, {PeriodD}, {PeriodK}, {Smooth})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return ((PeriodD.GetHashCode() * 3 + PeriodK.GetHashCode()) * 3 + Smooth.GetHashCode()) * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Stochastics other)
            {
                if (PeriodD == other.PeriodD && PeriodK == other.PeriodK && Smooth == other.Smooth && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class StochasticsFast : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string PeriodD { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string PeriodK { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }
        [JsonProperty(Required = Required.Always)]
        public StochasticsValue ValuePlot { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"StochasticsFast({Price}, {PeriodD}, {PeriodK})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return (PeriodD.GetHashCode() * 3 + PeriodK.GetHashCode()) * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is StochasticsFast other)
            {
                if (PeriodD == other.PeriodD && PeriodK == other.PeriodK && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class Swing : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string Strength { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }
        [JsonProperty(Required = Required.Always)]
        public SwingValue ValuePlot { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"Swing({Price}, {Strength})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return Strength.GetHashCode() * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Swing other)
            {
                if (Strength == other.Strength && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class Trend : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string Period { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"Trend({Price}, {Period})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return Period.GetHashCode() * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Trend other)
            {
                if (Period == other.Period && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class UltimateOscillator : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public string Fast { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Intermediate { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Slow { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"UltimateOscillator({Price}, {Fast}, {Intermediate}, {Slow})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return ((Fast.GetHashCode() * 3 + Intermediate.GetHashCode()) * 3 + Slow.GetHashCode()) * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is UltimateOscillator other)
            {
                if (Fast == other.Fast && Intermediate == other.Intermediate && Slow == other.Slow && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class VOL : ICompareData, IIndicator
    {
        //Params

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"VOL({Price})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is VOL other)
            {
                if (Price == other.Price) return true;
                else return false;
            }
            else return false;
        }
    }
    class ZigZag : ICompareData, IIndicator
    {
        //Params
        [JsonProperty(Required = Required.Always)]
        public DeviationType DeviationType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string DeviationValue { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool UseHighLow { get; set; }

        //Options
        [JsonProperty(Required = Required.Always)]
        public PriceType Price { get; set; }
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool PlotOnChart { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ZigZagValue ValuePlot { get; set; }


        //Overrides
        public string ToCtorString()
        {
            return $"ZigZag({Price}, DeviationType.{DeviationType}, {DeviationValue}, {UseHighLow})";
        }
        public string ToFormatString(string varName, bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"{varName}.{ValuePlot}", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return ((DeviationType.GetHashCode() * 3 + DeviationValue.GetHashCode()) * 3 + UseHighLow.GetHashCode()) * 3 + Price.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is ZigZag other)
            {
                if (DeviationType == other.DeviationType && DeviationValue == other.DeviationValue && UseHighLow == other.UseHighLow && Price == other.Price) return true;
                else return false;
            }
            else return false;
        }

    }

    //Personal Indicator
    //public class SuperTrend
    //{
    //    //Params
    //    public int Factor1 { get; set; }
    //    public int Factor2 { get; set; }
    //    public int Factor3 { get; set; }
    //    public int PD1 { get; set; }
    //    public int PD2 { get; set; }
    //    public int PD3 { get; set; }

    //}

    //Price Actions
    class Ask : ICompareData, IPriceAction
    {
        //Options
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }


        //Overrides
        public string ToFormatString(bool includeBarsAgo = true)
        {
            return Helper.ToStringHelperForOffset("GetCurrentAsk(0)", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Ask)
            {
                return true;
            }
            else return false;
        }
    }
    class Bid : ICompareData, IPriceAction
    {
        //Options
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }


        //Overrides
        public string ToFormatString(bool includeBarsAgo = true)
        {
            return Helper.ToStringHelperForOffset("GetCurrentBid(0)", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Bid)
            {
                return true;
            }
            else return false;
        }
    }
    class Open : ICompareData, IPriceAction
    {
        //Options
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }


        //Overrides
        public string ToFormatString(bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"Open[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"Open", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Open)
            {
                return true;
            }
            else return false;
        }
    }
    class Close : ICompareData, IPriceAction
    {
        //Options
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }


        //Overrides
        public string ToFormatString(bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"Close[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"Close", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Close)
            {
                return true;
            }
            else return false;
        }
    }
    class High : ICompareData, IPriceAction
    {
        //Options
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }


        //Overrides
        public string ToFormatString(bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"High[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"High", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is High)
            {
                return true;
            }
            else return false;
        }
    }
    class Low : ICompareData, IPriceAction
    {
        //Options
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }


        //Overrides
        public string ToFormatString(bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"Low[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"Low", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Low)
            {
                return true;
            }
            else return false;
        }
    }
    class Median : ICompareData, IPriceAction
    {
        //Options
        [JsonProperty(Required = Required.Always)]
        public OffsetType OffsetType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public ArithmeticOperator Operator { get; set; } = ArithmeticOperator.Plus;
        [JsonProperty(Required = Required.Always)]
        public string Offset { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }


        //Overrides
        public string ToFormatString(bool includeBarsAgo = true)
        {
            return includeBarsAgo ? Helper.ToStringHelperForOffset($"Median[{BarsAgo}]", Offset, OffsetType, Operator) : Helper.ToStringHelperForOffset($"Median", Offset, OffsetType, Operator);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Median)
            {
                return true;
            }
            else return false;
        }
    }

    //Volume
    class AskVolume : ICompareData, IPriceAction
    {
        //Overrides
        public string ToFormatString(bool includeBarsAgo = true)
        {
            return "GetCurrentAskVolume(0)";
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is AskVolume)
            {
                return true;
            }
            else return false;
        }
    }
    class BidVolume : ICompareData, IPriceAction
    {
        //Overrides
        public string ToFormatString(bool includeBarsAgo = true)
        {
            return "GetCurrentBidVolume(0)";
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is BidVolume)
            {
                return true;
            }
            else return false;
        }
    }
    class Volume : ICompareData, IPriceAction
    {
        [JsonProperty(Required = Required.Always)]
        public int BarsAgo { get; set; }


        //Overrides
        public string ToFormatString(bool includeBarsAgo = true)
        {
            return includeBarsAgo ? $"Volume[{BarsAgo}]" : $"Volume";
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Volume)
            {
                return true;
            }
            else return false;
        }
    }

    //Time
    class DateValue : ICompareData
    {
        [JsonProperty(Required = Required.Always)]
        public DateTime Date { get; set; }

        //Overrides
        public string ToFormatString()
        {
            return $"new DateTime({Date.Year}, {Date.Month}, {Date.Day})";
        }
        public override int GetHashCode()
        {
            return Date.Date.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is DateValue other)
            {
                if (Date.Date == other.Date.Date) return true;
                else return false;
            }
            else return false;
        }
    }
    class TimeValue : ICompareData
    {
        [JsonProperty(Required = Required.Always)]
        public DateTime Time { get; set; }


        //Overrides
        public string ToFormatString()
        {
            return $"new TimeSpan({Time.Hour}, {Time.Minute}, {Time.Second})";
        }
        public override int GetHashCode()
        {
            return Time.TimeOfDay.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is TimeValue other)
            {
                if (Time.TimeOfDay == other.Time.TimeOfDay) return true;
                else return false;
            }
            else return false;
        }
    }
    class DayofWeek : ICompareData
    {
        [JsonProperty(Required = Required.Always)]
        public DayOfWeek DayOfWeek { get; set; }


        //Overrides
        public string ToFormatString()
        {
            return $"DayOfWeek.{DayOfWeek}";
        }
        public override int GetHashCode()
        {
            return DayOfWeek.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is DayofWeek other)
            {
                if (DayOfWeek == other.DayOfWeek) return true;
                else return false;
            }
            else return false;
        }
    }

    //User Variables and Inputs
    class Variable : IVariable
    {
        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; }
        [JsonProperty(Required = Required.Always)]
        public VariableType Type { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Value { get; set; }


        //Overrides
        public string ToFormatString()
        {
            return Name;
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj is Variable other)
            {
                if (Name == other.Name) return true;
                else return false;
            }
            else return false;
        }
    }
    class Input : Variable
    {
        [JsonProperty(Required = Required.Always)]
        public string Description { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Minimum { get; set; }
    }

    class VariableReference : ICompareData, IReference
    {
        [JsonProperty(Required = Required.Always)]
        public string Reference { get; set; }
    }

    class InputReference : ICompareData, IReference
    {
        [JsonProperty(Required = Required.Always)]
        public string Reference { get; set; }
    }

    //Actions
    class Long : IOperation
    {
        [JsonProperty(Required = Required.Always)]
        public TargetActionType Action { get; set; }
        [JsonProperty(Required = Required.Always)]
        public double Quantity { get; set; }
    }
    class Short : IOperation
    {
        [JsonProperty(Required = Required.Always)]
        public TargetActionType Action { get; set; }
        [JsonProperty(Required = Required.Always)]
        public double Quantity { get; set; }
    }
}
