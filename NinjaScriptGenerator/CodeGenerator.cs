using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace NinjaScriptGenerator
{
    static class CodeGenerator
    {
        public static string GenerateFromStrategyData(StrategyData strategyData)
        {
            // Creating Code Structure...............................
            string err;
            if (!Helper.FormatComponents(strategyData, out err)) return err;

            CodeCompileUnit cunit = new CodeCompileUnit();

            CodeNamespace gns = new CodeNamespace();
            gns.Imports.AddRange(new CodeNamespaceImport[]
            {
                new CodeNamespaceImport("System"),
                new CodeNamespaceImport("System.Collections.Generic"),
                new CodeNamespaceImport("System.ComponentModel"),
                new CodeNamespaceImport("System.ComponentModel.DataAnnotations"),
                new CodeNamespaceImport("System.Linq"),
                new CodeNamespaceImport("System.Text"),
                new CodeNamespaceImport("System.Threading.Tasks"),
                new CodeNamespaceImport("System.Windows"),
                new CodeNamespaceImport("System.Windows.Input"),
                new CodeNamespaceImport("System.Windows.Media"),
                new CodeNamespaceImport("System.Globalization"),
                new CodeNamespaceImport("System.Xml.Serialization"),
                new CodeNamespaceImport("NinjaTrader.Cbi"),
                new CodeNamespaceImport("NinjaTrader.Gui"),
                new CodeNamespaceImport("NinjaTrader.Gui.Chart"),
                new CodeNamespaceImport("NinjaTrader.Gui.SuperDom"),
                new CodeNamespaceImport("NinjaTrader.Gui.Tools"),
                new CodeNamespaceImport("NinjaTrader.Data"),
                new CodeNamespaceImport("NinjaTrader.NinjaScript"),
                new CodeNamespaceImport("NinjaTrader.Core.FloatingPoint"),
                new CodeNamespaceImport("NinjaTrader.NinjaScript.Indicators"),
                new CodeNamespaceImport("NinjaTrader.NinjaScript.DrawingTools")
            });

            CodeNamespace ns = new CodeNamespace()
            {
                Name = "NinjaTrader.NinjaScript.Strategies",
            };
            ns.Comments.Add(new CodeCommentStatement("This namespace holds Strategies in this folder and is required. Do not change it."));

            CodeTypeDeclaration strategyclass = new CodeTypeDeclaration()
            {
                Name = strategyData.Name,
                IsClass = true,
                TypeAttributes = TypeAttributes.Public,
            };
            strategyclass.BaseTypes.Add("Strategy");

            var class_fields = new List<CodeMemberField>();
            var class_properties = new List<CodeTypeMember>();

            CodeMemberMethod statechange = new CodeMemberMethod()
            {
                Name = "OnStateChange",
                Attributes = MemberAttributes.Family | MemberAttributes.Override,
                ReturnType = new CodeTypeReference(typeof(void)),
            };

            CodeMemberMethod barupdate = new CodeMemberMethod()
            {
                Name = "OnBarUpdate",
                Attributes = MemberAttributes.Family | MemberAttributes.Override,
                ReturnType = new CodeTypeReference(typeof(void))
            };

            var cond_setdef = new List<CodeStatement>();
            var cond_setconf = new List<CodeStatement>();
            var cond_setdatald = new List<CodeStatement>();

            var barupdate_statements = new List<CodeStatement>();

            // Entries of Code Logic.........................................

            cond_setdef.AddRange(new CodeStatement[]
            {
                new CodeCommentStatement("Default Properties Logic"),
                new CodeAssignStatement(new CodeVariableReferenceExpression("Description"), new CodeSnippetExpression($"@\"{strategyData.Description}\"")),
                new CodeAssignStatement(new CodeVariableReferenceExpression("Name"), new CodePrimitiveExpression(strategyData.Name)),
                new CodeAssignStatement(new CodeVariableReferenceExpression("Calculate"), new CodeSnippetExpression($"Calculate.{strategyData.Defaults.Calculate}")),
                new CodeAssignStatement(new CodeVariableReferenceExpression("EntriesPerDirection"), new CodePrimitiveExpression(strategyData.Defaults.ContractsPerEntry)),
                new CodeAssignStatement(new CodeVariableReferenceExpression("EntryHandling"), new CodeSnippetExpression(@"EntryHandling.AllEntries")),
                new CodeAssignStatement(new CodeVariableReferenceExpression("IsExitOnSessionCloseStrategy"), new CodePrimitiveExpression(strategyData.Defaults.ExitOnSessionClose)),
                new CodeAssignStatement(new CodeVariableReferenceExpression("ExitOnSessionCloseSeconds"), new CodePrimitiveExpression(30)),
                new CodeAssignStatement(new CodeVariableReferenceExpression("IsFillLimitOnTouch"), new CodePrimitiveExpression(false)),
                new CodeAssignStatement(new CodeVariableReferenceExpression("MaximumBarsLookBack"), new CodeSnippetExpression(@"MaximumBarsLookBack.TwoHundredFiftySix")),
                new CodeAssignStatement(new CodeVariableReferenceExpression("OrderFillResolution"), new CodeSnippetExpression(@"OrderFillResolution.Standard")),
                new CodeAssignStatement(new CodeVariableReferenceExpression("Slippage"), new CodePrimitiveExpression(0)),
                new CodeAssignStatement(new CodeVariableReferenceExpression("StartBehavior"), new CodeSnippetExpression(@"StartBehavior.WaitUntilFlat")),
                new CodeAssignStatement(new CodeVariableReferenceExpression("TimeInForce"), new CodeSnippetExpression(@"TimeInForce.Gtc")),
                new CodeAssignStatement(new CodeVariableReferenceExpression("TraceOrders"), new CodePrimitiveExpression(false)),
                new CodeAssignStatement(new CodeVariableReferenceExpression("RealtimeErrorHandling"), new CodeSnippetExpression(@"RealtimeErrorHandling.StopCancelClose")),
                new CodeAssignStatement(new CodeVariableReferenceExpression("StopTargetHandling"), new CodeSnippetExpression(@"StopTargetHandling.PerEntryExecution")),
                new CodeAssignStatement(new CodeVariableReferenceExpression("BarsRequiredToTrade"), new CodePrimitiveExpression(20)),
                new CodeCommentStatement("Disable this property for performance gains in Strategy Analyzer optimizations"),
                new CodeCommentStatement("See the Help Guide for additional information"),
                new CodeAssignStatement(new CodeVariableReferenceExpression("IsInstantiatedOnEachOptimizationIteration"), new CodePrimitiveExpression(true))
            });

            barupdate_statements.Add(new CodeConditionStatement(new CodeSnippetExpression("BarsInProgress != 0"), new CodeStatement[] { new CodeMethodReturnStatement() }));

            barupdate_statements.Add(new CodeConditionStatement(new CodeSnippetExpression("CurrentBars[0] < 0"), new CodeStatement[] { new CodeMethodReturnStatement() }));

            int inp_counter = 1;
            foreach (Input inp in strategyData.Inputs)
            {
                if (strategyData.Inputs.Count() != strategyData.Inputs.Distinct().Count()) return Errors.DuplicateInputs;
                string type = inp.Type == VariableType.Time ? "DateTime" : Type.GetType($"System.{inp.Type}").ToString();
                string backing_var_name = $"_{inp.Name}";
                while (strategyData.Variables.Where(a => a.Name == backing_var_name).Count() > 0) backing_var_name = $"_{backing_var_name}";
                var backing = new CodeMemberField(new CodeTypeReference(type), backing_var_name) { Attributes = MemberAttributes.Private };
                var prop = new CodeMemberProperty() { Name = inp.Name, Type = new CodeTypeReference(type), Attributes = MemberAttributes.Public | MemberAttributes.Final };
                prop.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(null, backing_var_name)));
                prop.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, backing_var_name), new CodePropertySetValueReferenceExpression()));
                prop.CustomAttributes.Add(new CodeAttributeDeclaration("NinjaScriptProperty"));
                if ((inp.Type == VariableType.Int32 || inp.Type == VariableType.Double) && inp.Minimum != "") prop.CustomAttributes.Add(new CodeAttributeDeclaration("Range", new CodeAttributeArgument(new CodeSnippetExpression(inp.Minimum)), new CodeAttributeArgument(new CodeSnippetExpression($"{Type.GetType(type).Name}.MaxValue"))));
                else if (inp.Type == VariableType.Time) prop.CustomAttributes.Add(new CodeAttributeDeclaration("PropertyEditor", new CodeAttributeArgument(new CodePrimitiveExpression("NinjaTrader.Gui.Tools.TimeEditorKey"))));
                prop.CustomAttributes.Add(new CodeAttributeDeclaration("Display", new CodeAttributeArgument("Name", new CodePrimitiveExpression(inp.Name)), new CodeAttributeArgument("Description", new CodePrimitiveExpression(inp.Description)), new CodeAttributeArgument("Order", new CodePrimitiveExpression(inp_counter++)), new CodeAttributeArgument("GroupName", new CodePrimitiveExpression("Parameters"))));
                class_fields.Add(backing);
                class_properties.Add(prop);
                if (inp.Value != "")
                {
                    if (inp.Type == VariableType.Time) cond_setdef.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, inp.Name), new CodeSnippetExpression($"DateTime.Parse(\"{inp.Value}\", CultureInfo.InvariantCulture)")));
                    else if (inp.Type == VariableType.String) cond_setdef.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, inp.Name), new CodePrimitiveExpression(inp.Value)));
                    else cond_setdef.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, inp.Name), new CodeSnippetExpression(inp.Value)));
                }
            }

            foreach (Variable var in strategyData.Variables)
            {
                if (strategyData.Variables.Count() != strategyData.Variables.Distinct().Count()) return Errors.DuplicateVariables;
                string type = var.Type == VariableType.Time ? "DateTime" : Type.GetType($"System.{var.Type}").ToString();
                class_fields.Add(new CodeMemberField(type, var.Name) { Attributes = MemberAttributes.Private });
                if (var.Type == VariableType.Time) cond_setdef.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, var.Name), new CodeSnippetExpression($"DateTime.Parse(\"{var.Value}\", CultureInfo.InvariantCulture)")));
                else if (var.Type == VariableType.String) cond_setdef.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, var.Name), new CodePrimitiveExpression(var.Value)));
                else cond_setdef.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, var.Name), new CodeSnippetExpression(var.Value)));
            }

            foreach (InstrumentData ins in strategyData.Instruments)
            {
                if (strategyData.Instruments.Count() != strategyData.Instruments.Distinct().Count()) return Errors.DuplicateInstruments;
                var namepieces = ins.Name.ToString().Split('_');
                cond_setconf.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddDataSeries", new CodePrimitiveExpression($"{namepieces[1]} {namepieces[2]}-{namepieces[3]}"), new CodeSnippetExpression($"Data.BarsPeriodType.{ins.Type}"), new CodePrimitiveExpression(ins.Value), new CodeSnippetExpression("Data.MarketDataType.Last"))));
            }

            cond_setdatald.Add(new CodeCommentStatement("DataLoaded logics here..."));

            Dictionary<Type, Dictionary<ICompareData, int>> pairs = new Dictionary<Type, Dictionary<ICompareData, int>>();
            foreach (ConditionSet cnd_set in strategyData.ConditionSets)
            {
                List<CodeExpression> conditions = new List<CodeExpression>();
                List<CodeStatement> actions = new List<CodeStatement>();

                //Conditions
                foreach (CompareData cmp in cnd_set.Compares)
                {
                    string expression_frst = "";
                    string expression_scnd = "";
                    if (cmp.FirstObject == null || cmp.SecondObject == null || !Enum.IsDefined(typeof(CompareType), cmp.Operation)) return Errors.InvalidCompares;
                    if (cmp.Operation == CompareType.CrossAbove || cmp.Operation == CompareType.CrossBelow)
                    {
                        if (Double.Parse((string)cmp.FirstObject.GetType().GetProperty("Offset")?.GetValue(cmp.FirstObject)) != 0) return Errors.IncompatibleCompares;
                        if (Double.Parse((string)cmp.SecondObject.GetType().GetProperty("Offset")?.GetValue(cmp.SecondObject)) != 0) return Errors.IncompatibleCompares;

                        if (cmp.FirstObject is Ask || cmp.FirstObject is AskVolume || cmp.FirstObject is Bid || cmp.FirstObject is BidVolume || cmp.FirstObject is DateValue || cmp.FirstObject is TimeValue || cmp.FirstObject is DayofWeek) return Errors.IncompatibleCompares;
                        if (cmp.SecondObject is Ask || cmp.SecondObject is AskVolume || cmp.SecondObject is Bid || cmp.SecondObject is BidVolume || cmp.SecondObject is DateValue || cmp.SecondObject is TimeValue || cmp.SecondObject is DayofWeek) return Errors.IncompatibleCompares;
                        if (cmp.SecondObject is IReference)
                        {
                            IVariable scnd = Helper.GetFromReference((IReference)cmp.SecondObject, strategyData);
                            if (scnd == null)
                            {
                                if (cmp.SecondObject is VariableReference) return Errors.InvalidVariableRef;
                                else if (cmp.SecondObject is InputReference) return Errors.InvalidInputRef;
                                else return Errors.InternalFatal;
                            }
                            if (scnd.Type == VariableType.Boolean || scnd.Type == VariableType.String || scnd.Type == VariableType.Time) return Errors.IncompatibleCompares;
                        }
                        if (cmp.FirstObject is IReference)
                        {
                            IVariable frst = Helper.GetFromReference((IReference)cmp.FirstObject, strategyData);
                            if (frst == null)
                            {
                                if (cmp.FirstObject is VariableReference) return Errors.InvalidVariableRef;
                                else if (cmp.FirstObject is InputReference) return Errors.InvalidInputRef;
                                else return Errors.InternalFatal;
                            }
                            expression_frst = frst.ToFormatString();
                            if (frst.Type == VariableType.Boolean || frst.Type == VariableType.String || frst.Type == VariableType.Time) return Errors.IncompatibleCompares;
                            else if (frst.Type == VariableType.Int32 || frst.Type == VariableType.Double)
                            {
                                if (cmp.SecondObject is IReference) return Errors.IncompatibleCompares;
                                else if (cmp.SecondObject is IPriceAction)
                                {
                                    expression_scnd = ((IPriceAction)cmp.SecondObject).ToFormatString(false);
                                }
                                else if (cmp.SecondObject is IIndicator)
                                {
                                    var type = cmp.SecondObject.GetType();
                                    string varName = type.Name;
                                    if (pairs.ContainsKey(type))
                                    {
                                        var plotchartflag = ((IIndicator)cmp.SecondObject).PlotOnChart;
                                        if (pairs[type].ContainsKey(cmp.SecondObject))
                                        {
                                            varName += pairs[type][cmp.SecondObject];
                                            try
                                            {
                                                if (((IIndicator)pairs[type].Keys.Single(a => a.Equals(cmp.SecondObject))).PlotOnChart) plotchartflag = false;                                               
                                            }
                                            catch
                                            {
                                                return Errors.InternalFatal;
                                            }
                                        }
                                        else
                                        {
                                            var num = pairs[type].Count + 1;
                                            varName += num;
                                            pairs[type].Add(cmp.SecondObject, num);
                                            class_fields.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Private });
                                            cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                        }

                                        if (plotchartflag)
                                        {
                                            cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                            cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeSnippetExpression($"{varName}"))));
                                        }
                                    }
                                    else
                                    {
                                        pairs.Add(type, new Dictionary<ICompareData, int>());
                                        pairs[type].Add(cmp.SecondObject, 1);
                                        varName += "1";
                                        class_fields.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Private });
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                        if (((IIndicator)cmp.SecondObject).PlotOnChart)
                                        {
                                            cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                            cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeSnippetExpression($"{varName}"))));
                                        }
                                    }
                                    expression_scnd = ((IIndicator)cmp.SecondObject).ToFormatString(varName, false);
                                }
                                else return Errors.InternalFatal;
                            }
                            else return Errors.InternalFatal;
                        }
                        else if (cmp.FirstObject is IPriceAction)
                        {
                            expression_frst = ((IPriceAction)cmp.FirstObject).ToFormatString(false);
                            if (cmp.SecondObject is IIndicator)
                            {
                                var type = cmp.SecondObject.GetType();
                                string varName = type.Name;
                                if (pairs.ContainsKey(type))
                                {
                                    var plotchartflag = ((IIndicator)cmp.SecondObject).PlotOnChart;
                                    if (pairs[type].ContainsKey(cmp.SecondObject))
                                    {
                                        varName += pairs[type][cmp.SecondObject];
                                        try
                                        {
                                            if (((IIndicator)pairs[type].Keys.Single(a => a.Equals(cmp.SecondObject))).PlotOnChart) plotchartflag = false;
                                        }
                                        catch
                                        {
                                            return Errors.InternalFatal;
                                        }
                                    }
                                    else
                                    {
                                        var num = pairs[type].Count + 1;
                                        varName += num;
                                        pairs[type].Add(cmp.SecondObject, num);
                                        class_fields.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Private });
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                    }

                                    if (plotchartflag)
                                    {
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                        cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeSnippetExpression($"{varName}"))));
                                    }
                                }
                                else
                                {
                                    pairs.Add(type, new Dictionary<ICompareData, int>());
                                    pairs[type].Add(cmp.SecondObject, 1);
                                    varName += "1";
                                    class_fields.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Private });
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                    if (((IIndicator)cmp.SecondObject).PlotOnChart)
                                    {
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                        cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeSnippetExpression($"{varName}"))));
                                    }
                                }
                                expression_scnd = ((IIndicator)cmp.SecondObject).ToFormatString(varName, false);
                            }
                            else if (cmp.SecondObject is IReference) expression_scnd = Helper.GetFromReference((IReference)cmp.SecondObject, strategyData).ToFormatString();
                            else if (cmp.SecondObject is IPriceAction)
                            {
                                if (cmp.FirstObject is Volume && cmp.SecondObject is Volume) return Errors.IncompatibleCompares;
                                else expression_scnd = ((IPriceAction)cmp.SecondObject).ToFormatString(false);
                            }
                        }
                        else if (cmp.FirstObject is IIndicator)
                        {
                            var type = cmp.FirstObject.GetType();
                            string varName = type.Name;
                            if (pairs.ContainsKey(type))
                            {
                                var plotchartflag = ((IIndicator)cmp.FirstObject).PlotOnChart;
                                if (pairs[type].ContainsKey(cmp.FirstObject))
                                {
                                    varName += pairs[type][cmp.FirstObject];
                                    try
                                    {
                                        if (((IIndicator)pairs[type].Keys.Single(a => a.Equals(cmp.FirstObject))).PlotOnChart) plotchartflag = false;
                                    }
                                    catch
                                    {
                                        return Errors.InternalFatal;
                                    }
                                }
                                else
                                {
                                    var num = pairs[type].Count + 1;
                                    varName += num;
                                    pairs[type].Add(cmp.FirstObject, num);
                                    class_fields.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Private });
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.FirstObject).ToCtorString())));
                                }

                                if (plotchartflag)
                                {
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                    cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeSnippetExpression($"{varName}"))));
                                }
                            }
                            else
                            {
                                pairs.Add(type, new Dictionary<ICompareData, int>());
                                pairs[type].Add(cmp.FirstObject, 1);
                                varName += "1";
                                class_fields.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Private });
                                cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.FirstObject).ToCtorString())));
                                if (((IIndicator)cmp.FirstObject).PlotOnChart)
                                {
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                    cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeSnippetExpression($"{varName}"))));
                                }
                            }
                            expression_frst = ((IIndicator)cmp.FirstObject).ToFormatString(varName, false);
                            if (cmp.SecondObject is IReference) expression_scnd = Helper.GetFromReference((IReference)cmp.SecondObject, strategyData).ToFormatString();
                            else if (cmp.SecondObject is IPriceAction) expression_scnd = ((IPriceAction)cmp.SecondObject).ToFormatString(false);
                            else if (cmp.SecondObject is IIndicator)
                            {
                                type = cmp.SecondObject.GetType();
                                varName = type.Name;
                                if (pairs.ContainsKey(type))
                                {
                                    var plotchartflag = ((IIndicator)cmp.SecondObject).PlotOnChart;
                                    if (pairs[type].ContainsKey(cmp.SecondObject))
                                    {
                                        varName += pairs[type][cmp.SecondObject];
                                        try
                                        {
                                            if (((IIndicator)pairs[type].Keys.Single(a => a.Equals(cmp.SecondObject))).PlotOnChart) plotchartflag = false;
                                        }
                                        catch
                                        {
                                            return Errors.InternalFatal;
                                        }
                                    }
                                    else
                                    {
                                        var num = pairs[type].Count + 1;
                                        varName += num;
                                        pairs[type].Add(cmp.SecondObject, num);
                                        class_fields.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Private });
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                    }

                                    if (plotchartflag)
                                    {
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                        cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeSnippetExpression($"{varName}"))));
                                    }
                                }
                                else
                                {
                                    pairs.Add(type, new Dictionary<ICompareData, int>());
                                    pairs[type].Add(cmp.SecondObject, 1);
                                    varName += "1";
                                    class_fields.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Private });
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                    if (((IIndicator)cmp.SecondObject).PlotOnChart)
                                    {
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                        cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeSnippetExpression($"{varName}"))));
                                    }
                                }
                                expression_scnd = ((IIndicator)cmp.SecondObject).ToFormatString(varName, false);
                            }
                            else return Errors.InternalFatal;
                        }
                        if (expression_frst != "" && expression_scnd != "") conditions.Add(new CodeMethodInvokeExpression(null, cmp.Operation.ToString(), new CodeSnippetExpression(expression_frst), new CodeSnippetExpression(expression_scnd), new CodePrimitiveExpression(1)));
                    }
                    else
                    {
                        if (cmp.FirstObject is DateValue)
                        {
                            expression_frst = ((DateValue)cmp.FirstObject).ToFormatString();
                            if (cmp.SecondObject is DateValue)
                            {
                                expression_scnd = ((DateValue)cmp.SecondObject).ToFormatString();
                            }
                            else if (cmp.SecondObject is DayofWeek)
                            {
                                expression_frst += ".DayOfWeek";
                                expression_scnd = ((DayofWeek)cmp.SecondObject).ToFormatString();
                            }
                            else return Errors.IncompatibleCompares;
                        }
                        else if (cmp.FirstObject is TimeValue)
                        {
                            expression_frst = ((TimeValue)cmp.FirstObject).ToFormatString();
                            if (cmp.SecondObject is TimeValue)
                            {
                                expression_scnd = ((TimeValue)cmp.SecondObject).ToFormatString();
                            }
                            else if (cmp.SecondObject is IReference)
                            {
                                IVariable vr_scnd = Helper.GetFromReference((IReference)cmp.SecondObject, strategyData);
                                if (vr_scnd == null)
                                {
                                    if (cmp.SecondObject is VariableReference) return Errors.InvalidVariableRef;
                                    else if (cmp.SecondObject is InputReference) return Errors.InvalidInputRef;
                                    else return Errors.InternalFatal;
                                }
                                if (vr_scnd.Type == VariableType.Time) expression_scnd = vr_scnd.ToFormatString() + ".TimeOfDay";
                                else return Errors.IncompatibleCompares;
                            }
                            else return Errors.IncompatibleCompares;
                        }
                        else if (cmp.FirstObject is DayofWeek)
                        {
                            expression_frst = ((DayofWeek)cmp.FirstObject).ToFormatString();
                            if (cmp.SecondObject is DayofWeek)
                            {
                                expression_scnd = ((DayofWeek)cmp.SecondObject).ToFormatString();
                            }
                            else if (cmp.SecondObject is DateValue)
                            {
                                expression_scnd = ((DateValue)cmp.SecondObject).ToFormatString() + ".DayOfWeek";
                            }
                            else return Errors.IncompatibleCompares;
                        }
                        else if (cmp.FirstObject is IReference)
                        {
                            IVariable frst = Helper.GetFromReference((IReference)cmp.FirstObject, strategyData);
                            if (frst == null)
                            {
                                if (cmp.FirstObject is VariableReference) return Errors.InvalidVariableRef;
                                else if (cmp.FirstObject is InputReference) return Errors.InvalidInputRef;
                                else return Errors.InternalFatal;
                            }
                            if (frst.Type == VariableType.Time)
                            {
                                expression_frst = frst.ToFormatString() + ".TimeOfDay";
                                if (cmp.SecondObject is TimeValue) expression_scnd = ((TimeValue)cmp.SecondObject).ToFormatString();
                                else if (cmp.SecondObject is IReference)
                                {
                                    var scnd_tmp = Helper.GetFromReference((IReference)cmp.SecondObject, strategyData);
                                    if (scnd_tmp == null)
                                    {
                                        if (cmp.SecondObject is VariableReference) return Errors.InvalidVariableRef;
                                        else if (cmp.SecondObject is InputReference) return Errors.InvalidInputRef;
                                        else return Errors.InternalFatal;
                                    }
                                    if (scnd_tmp.Type == VariableType.Time) expression_scnd = scnd_tmp.ToFormatString() + ".TimeOfDay";
                                    else return Errors.IncompatibleCompares;
                                }
                                else return Errors.IncompatibleCompares;
                            }
                            else if (frst.Type == VariableType.String || frst.Type == VariableType.Boolean) return Errors.IncompatibleCompares;
                            else
                            {
                                expression_frst = frst.ToFormatString();
                                if (cmp.SecondObject is IReference)
                                {
                                    IVariable scnd = Helper.GetFromReference((IReference)cmp.SecondObject, strategyData);
                                    if (scnd == null)
                                    {
                                        if (cmp.SecondObject is VariableReference) return Errors.InvalidVariableRef;
                                        else if (cmp.SecondObject is InputReference) return Errors.InvalidInputRef;
                                        else return Errors.InternalFatal;
                                    }
                                    if (scnd.Type == VariableType.Double || scnd.Type == VariableType.Int32) expression_scnd = scnd.ToFormatString();
                                    else return Errors.IncompatibleCompares;
                                }
                                else if (cmp.SecondObject is IPriceAction) expression_scnd = ((IPriceAction)cmp.SecondObject).ToFormatString();
                                else if (cmp.SecondObject is IIndicator)
                                {
                                    var type = cmp.SecondObject.GetType();
                                    string varName = type.Name;
                                    if (pairs.ContainsKey(type))
                                    {
                                        var plotchartflag = ((IIndicator)cmp.SecondObject).PlotOnChart;
                                        if (pairs[type].ContainsKey(cmp.SecondObject))
                                        {
                                            varName += pairs[type][cmp.SecondObject];
                                            try
                                            {
                                                if (((IIndicator)pairs[type].Keys.Single(a => a.Equals(cmp.SecondObject))).PlotOnChart) plotchartflag = false;
                                            }
                                            catch
                                            {
                                                return Errors.InternalFatal;
                                            }
                                        }
                                        else
                                        {
                                            var num = pairs[type].Count + 1;
                                            varName += num;
                                            pairs[type].Add(cmp.SecondObject, num);
                                            class_fields.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Private });
                                            cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                        }

                                        if (plotchartflag)
                                        {
                                            cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                            cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeSnippetExpression($"{varName}"))));
                                        }
                                    }
                                    else
                                    {
                                        pairs.Add(type, new Dictionary<ICompareData, int>());
                                        pairs[type].Add(cmp.SecondObject, 1);
                                        varName += "1";
                                        class_fields.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Private });
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                        if (((IIndicator)cmp.SecondObject).PlotOnChart)
                                        {
                                            cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                            cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeSnippetExpression($"{varName}"))));
                                        }
                                    }
                                    expression_scnd = ((IIndicator)cmp.SecondObject).ToFormatString(varName);
                                }
                                else return Errors.IncompatibleCompares;
                            }
                        }
                        else if (cmp.FirstObject is IPriceAction)
                        {
                            expression_frst = ((IPriceAction)cmp.FirstObject).ToFormatString();
                            if (cmp.SecondObject is IIndicator)
                            {
                                var type = cmp.SecondObject.GetType();
                                string varName = type.Name;
                                if (pairs.ContainsKey(type))
                                {
                                    var plotchartflag = ((IIndicator)cmp.SecondObject).PlotOnChart;
                                    if (pairs[type].ContainsKey(cmp.SecondObject))
                                    {
                                        varName += pairs[type][cmp.SecondObject];
                                        try
                                        {
                                            if (((IIndicator)pairs[type].Keys.Single(a => a.Equals(cmp.SecondObject))).PlotOnChart) plotchartflag = false;
                                        }
                                        catch
                                        {
                                            return Errors.InternalFatal;
                                        }
                                    }
                                    else
                                    {
                                        var num = pairs[type].Count + 1;
                                        varName += num;
                                        pairs[type].Add(cmp.SecondObject, num);
                                        class_fields.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Private });
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                    }

                                    if (plotchartflag)
                                    {
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                        cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeSnippetExpression($"{varName}"))));
                                    }
                                }
                                else
                                {
                                    pairs.Add(type, new Dictionary<ICompareData, int>());
                                    pairs[type].Add(cmp.SecondObject, 1);
                                    varName += "1";
                                    class_fields.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Private });
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                    if (((IIndicator)cmp.SecondObject).PlotOnChart)
                                    {
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                        cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeSnippetExpression($"{varName}"))));
                                    }
                                }
                                expression_scnd = ((IIndicator)cmp.SecondObject).ToFormatString(varName);
                            }
                            else if (cmp.SecondObject is IReference)
                            {
                                IVariable scnd = Helper.GetFromReference((IReference)cmp.SecondObject, strategyData);
                                if (scnd == null)
                                {
                                    if (cmp.SecondObject is VariableReference) return Errors.InvalidVariableRef;
                                    else if (cmp.SecondObject is InputReference) return Errors.InvalidInputRef;
                                    else return Errors.InternalFatal;
                                }
                                if (scnd.Type == VariableType.Int32 || scnd.Type == VariableType.Double) expression_scnd = scnd.ToFormatString();
                                else return Errors.IncompatibleCompares;
                            }
                            else if (cmp.SecondObject is IPriceAction) expression_scnd = ((IPriceAction)cmp.SecondObject).ToFormatString();
                        }
                        else if (cmp.FirstObject is IIndicator)
                        {
                            var type = cmp.FirstObject.GetType();
                            string varName = type.Name;
                            if (pairs.ContainsKey(type))
                            {
                                var plotchartflag = ((IIndicator)cmp.FirstObject).PlotOnChart;
                                if (pairs[type].ContainsKey(cmp.FirstObject))
                                {
                                    varName += pairs[type][cmp.FirstObject];
                                    try
                                    {
                                        if (((IIndicator)pairs[type].Keys.Single(a => a.Equals(cmp.FirstObject))).PlotOnChart) plotchartflag = false;
                                    }
                                    catch
                                    {
                                        return Errors.InternalFatal;
                                    }
                                }
                                else
                                {
                                    var num = pairs[type].Count + 1;
                                    varName += num;
                                    pairs[type].Add(cmp.FirstObject, num);
                                    class_fields.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Private });
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.FirstObject).ToCtorString())));
                                }

                                if (plotchartflag)
                                {
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                    cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeSnippetExpression($"{varName}"))));
                                }
                            }
                            else
                            {
                                pairs.Add(type, new Dictionary<ICompareData, int>());
                                pairs[type].Add(cmp.FirstObject, 1);
                                varName += "1";
                                class_fields.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Private });
                                cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.FirstObject).ToCtorString())));
                                if (((IIndicator)cmp.FirstObject).PlotOnChart)
                                {
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                    cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeSnippetExpression($"{varName}"))));
                                }
                            }
                            expression_frst = ((IIndicator)cmp.FirstObject).ToFormatString(varName);
                            if (cmp.SecondObject is IReference)
                            {
                                IVariable scnd = Helper.GetFromReference((IReference)cmp.SecondObject, strategyData);
                                if (scnd == null)
                                {
                                    if (cmp.SecondObject is VariableReference) return Errors.InvalidVariableRef;
                                    else if (cmp.SecondObject is InputReference) return Errors.InvalidInputRef;
                                    else return Errors.InternalFatal;
                                }
                                if (scnd.Type == VariableType.Int32 || scnd.Type == VariableType.Double) expression_scnd = scnd.ToFormatString();
                                else return Errors.IncompatibleCompares;
                            }
                            else if (cmp.SecondObject is IPriceAction) expression_scnd = ((IPriceAction)cmp.SecondObject).ToFormatString();
                            else if (cmp.SecondObject is IIndicator)
                            {
                                var scnd_type = cmp.SecondObject.GetType();
                                string scnd_varName = scnd_type.Name;
                                if (pairs.ContainsKey(scnd_type))
                                {
                                    var plotchartflag = ((IIndicator)cmp.SecondObject).PlotOnChart;
                                    if (pairs[scnd_type].ContainsKey(cmp.SecondObject))
                                    {
                                        scnd_varName += pairs[scnd_type][cmp.SecondObject];
                                        try
                                        {
                                            if (((IIndicator)pairs[scnd_type].Keys.Single(a => a.Equals(cmp.SecondObject))).PlotOnChart) plotchartflag = false;
                                        }
                                        catch
                                        {
                                            return Errors.InternalFatal;
                                        }
                                    }
                                    else
                                    {
                                        var num = pairs[scnd_type].Count + 1;
                                        scnd_varName += num;
                                        pairs[scnd_type].Add(cmp.SecondObject, num);
                                        class_fields.Add(new CodeMemberField(scnd_type.Name, scnd_varName) { Attributes = MemberAttributes.Private });
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, scnd_varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                    }

                                    if (plotchartflag)
                                    {
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{scnd_varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                        cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeSnippetExpression($"{scnd_varName}"))));
                                    }
                                }
                                else
                                {
                                    pairs.Add(scnd_type, new Dictionary<ICompareData, int>());
                                    pairs[scnd_type].Add(cmp.SecondObject, 1);
                                    scnd_varName += "1";
                                    class_fields.Add(new CodeMemberField(scnd_type.Name, scnd_varName) { Attributes = MemberAttributes.Private });
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, scnd_varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                    if (((IIndicator)cmp.SecondObject).PlotOnChart)
                                    {
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{scnd_varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                        cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeSnippetExpression($"{scnd_varName}"))));
                                    }
                                }
                                expression_scnd = ((IIndicator)cmp.SecondObject).ToFormatString(scnd_varName);
                            }
                            else return Errors.IncompatibleCompares;
                        }
                        if (expression_frst != "" && expression_scnd != "") conditions.Add(new CodeBinaryOperatorExpression(new CodeSnippetExpression(expression_frst), (CodeBinaryOperatorType)cmp.Operation, new CodeSnippetExpression(expression_scnd)));
                    }
                }

                //Actions
                for (int i = 0; i < cnd_set.Operations.Length; i++)
                {
                    if (cnd_set.Operations[i] is Long)
                    {
                        if (cnd_set.Operations[i].Action == TargetActionType.Entry) actions.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "EnterLong", new CodeSnippetExpression($"{(cnd_set.Operations[i].Quantity <= 0 ? strategyData.Defaults.ContractsPerEntry : cnd_set.Operations[i].Quantity)}"), new CodePrimitiveExpression(""))));
                        else if (cnd_set.Operations[i].Action == TargetActionType.Exit) actions.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "ExitLong", new CodeSnippetExpression($"{(cnd_set.Operations[i].Quantity <= 0 ? strategyData.Defaults.ContractsPerEntry : cnd_set.Operations[i].Quantity)}"), new CodePrimitiveExpression(""), new CodePrimitiveExpression(""))));
                        else return Errors.InternalFatal;
                    }
                    else if (cnd_set.Operations[i] is Short)
                    {
                        if (cnd_set.Operations[i].Action == TargetActionType.Entry) actions.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "EnterShort", new CodeSnippetExpression($"{(cnd_set.Operations[i].Quantity <= 0 ? strategyData.Defaults.ContractsPerEntry : cnd_set.Operations[i].Quantity)}"), new CodePrimitiveExpression(""))));
                        else if (cnd_set.Operations[i].Action == TargetActionType.Exit) actions.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "ExitShort", new CodeSnippetExpression($"{(cnd_set.Operations[i].Quantity <= 0 ? strategyData.Defaults.ContractsPerEntry : cnd_set.Operations[i].Quantity)}"), new CodePrimitiveExpression(""), new CodePrimitiveExpression(""))));
                        else return Errors.InternalFatal;
                    }
                    else return Errors.InternalFatal;
                }

                //Code Construction
                if (conditions.Count > 0)
                {
                    if (!Enum.IsDefined(typeof(ConditionType), cnd_set.ConditionType)) return Errors.InvalidCondition;
                    CodeExpression main_cond = conditions[0];
                    CodeBinaryOperatorType code_op = (CodeBinaryOperatorType)cnd_set.ConditionType;
                    for (int i = 1; i < conditions.Count; i++)
                    {
                        main_cond = new CodeBinaryOperatorExpression(main_cond, code_op, conditions[i]);
                    }
                    barupdate_statements.Add(new CodeConditionStatement(main_cond, actions.ToArray()));
                }
                else
                {
                    barupdate_statements.AddRange(actions);
                }
            }

            foreach (var ta in strategyData.TargetActions)
            {
                if (ta.Type == ProfitLossType.Disabled) continue;
                if (Convert.ToDouble(ta.Value) == 0) ta.Value = strategyData.Defaults.ContractsPerEntry.ToString();
                if (ta.TargetType == TargetType.TakeProfit) cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "SetProfitTarget", new CodePrimitiveExpression(""), new CodeSnippetExpression($"CalculationMode.{ta.Type}"), new CodeSnippetExpression($"{ ta.Value }"))));
                else if (ta.TargetType == TargetType.StopLoss) cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "SetStopLoss", new CodePrimitiveExpression(""), new CodeSnippetExpression($"CalculationMode.{ta.Type}"), new CodeSnippetExpression($"{ta.Value}"), new CodePrimitiveExpression(false))));
                else return Errors.InternalFatal;
            }

            // Assembly of Code Structure.....................................................

            CodeConditionStatement cond_statechange = new CodeConditionStatement(
                new CodeSnippetExpression("State == State.SetDefaults"),
                cond_setdef.ToArray(),
                new CodeStatement[]
                {
                    new CodeConditionStatement(
                        new CodeSnippetExpression("State == State.Configure"),
                        cond_setconf.ToArray(),
                        new CodeStatement[]
                        {
                            new CodeConditionStatement(
                                new CodeSnippetExpression("State == State.DataLoaded"),
                                cond_setdatald.ToArray()
                            )
                        }
                    )
                }
            );
            statechange.Statements.Add(cond_statechange);

            barupdate.Statements.AddRange(barupdate_statements.ToArray());

            if (class_properties.Count > 0)
            {
                class_properties[0].StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "Properties"));
                class_properties[class_properties.Count - 1].EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, ""));
            }

            cunit.Namespaces.Add(gns);
            cunit.Namespaces.Add(ns);
            ns.Types.Add(strategyclass);
            strategyclass.Members.AddRange(class_fields.ToArray());
            strategyclass.Members.Add(new CodeSnippetTypeMember());
            strategyclass.Members.Add(statechange);
            strategyclass.Members.Add(new CodeSnippetTypeMember());
            strategyclass.Members.Add(barupdate);
            strategyclass.Members.Add(new CodeSnippetTypeMember());
            strategyclass.Members.AddRange(class_properties.ToArray());

            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions op = new CodeGeneratorOptions();
            op.BlankLinesBetweenMembers = false;
            op.BracingStyle = "C";
            op.VerbatimOrder = true;

            try
            {
                foreach (var file in new DirectoryInfo("Temp\\Strategies").GetFiles()) file.Delete();
                using (StreamWriter sw = new StreamWriter(File.OpenWrite($"Temp\\Strategies\\{strategyData.Name}.cs")))
                {
                    provider.GenerateCodeFromCompileUnit(cunit, sw, op);
                }
                ZipFile.CreateFromDirectory($"Temp", $"{strategyData.Name}.zip");
                if (File.Exists($"Temp\\Strategies\\{strategyData.Name}.cs")) File.Delete($"Temp\\Strategies\\{strategyData.Name}.cs");
            }
            catch
            {
                return Errors.FileGeneration;
            }
            
            //provider.GenerateCodeFromCompileUnit(cunit, Console.Out, op);

            return Errors.Success;
        }
    }
}
