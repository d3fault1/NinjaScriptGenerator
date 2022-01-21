using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;

namespace NinjaScriptGenerator
{
    static class CodeGenerator
    {
        public static string GenerateFromStrategyData(StrategyData strategyData)
        {
            // Creating Code Structure...............................

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

            var class_members = new List<CodeMemberField>();

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
                new CodeAssignStatement(new CodeVariableReferenceExpression("Description"), new CodePrimitiveExpression("@" + strategyData.Description)),
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

            foreach (Variable var in strategyData.Variables)
            {
                string type = var.Type == VariableType.Time ? "DateTime" : Type.GetType($"System.{var.Type}").ToString();
                class_members.Add(new CodeMemberField(type, var.Name) { Attributes = MemberAttributes.Private });
                if (var.Type == VariableType.Time) cond_setdef.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, var.Name), new CodeSnippetExpression($"DateTime.Parse(\"{var.Value}\", CultureInfo.InvariantCulture)")));
                else cond_setdef.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, var.Name), new CodePrimitiveExpression(Convert.ChangeType(var.Value, (TypeCode)var.Type))));
            }

            foreach (InstrumentData ins in strategyData.Instruments)
            {
                var namepieces = ins.Name.ToString().Split('_');
                cond_setconf.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddDataSeries", new CodeExpression[] { new CodePrimitiveExpression($"{namepieces[1]} {namepieces[2]}-{namepieces[3]}"), new CodeSnippetExpression($"Data.BarsPeriodType.{ins.Type}"), new CodePrimitiveExpression(ins.Value), new CodeSnippetExpression("Data.MarketDataType.Last") })));
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
                        if (cmp.FirstObject.GetType().GetProperty("Offset") != null)
                        {
                            if ((double)cmp.FirstObject.GetType().GetProperty("Offset").GetValue(cmp.FirstObject) != 0) return Errors.IncompatibleCompares;
                        }
                        if (cmp.SecondObject.GetType().GetProperty("Offset") != null)
                        {
                            if ((double)cmp.SecondObject.GetType().GetProperty("Offset").GetValue(cmp.SecondObject) != 0) return Errors.IncompatibleCompares;
                        }
                        if (cmp.FirstObject is Ask || cmp.FirstObject is AskVolume || cmp.FirstObject is Bid || cmp.FirstObject is BidVolume || cmp.FirstObject is DateValue || cmp.FirstObject is TimeValue || cmp.FirstObject is DayofWeek) return Errors.IncompatibleCompares;
                        if (cmp.SecondObject is Ask || cmp.SecondObject is AskVolume || cmp.SecondObject is Bid || cmp.SecondObject is BidVolume || cmp.SecondObject is DateValue || cmp.SecondObject is TimeValue || cmp.SecondObject is DayofWeek) return Errors.IncompatibleCompares;
                        if (cmp.SecondObject is Variable)
                        {
                            Variable scnd = (Variable)cmp.SecondObject;
                            if (scnd.Type == VariableType.Boolean || scnd.Type == VariableType.String || scnd.Type == VariableType.Time) return Errors.IncompatibleCompares;
                        }
                        if (cmp.FirstObject is Variable)
                        {
                            Variable frst = (Variable)cmp.FirstObject;
                            expression_frst = frst.ToFormatString();
                            if (frst.Type == VariableType.Boolean || frst.Type == VariableType.String || frst.Type == VariableType.Time) return Errors.IncompatibleCompares;
                            else if (frst.Type == VariableType.Int32 || frst.Type == VariableType.Double)
                            {
                                if (cmp.SecondObject is Variable) return Errors.IncompatibleCompares;
                                else if (cmp.SecondObject is IPriceAction)
                                {
                                    expression_scnd = ((IPriceAction)cmp.SecondObject).ToFormatString();
                                }
                                else if (cmp.SecondObject is IIndicator)
                                {
                                    var type = cmp.SecondObject.GetType();
                                    string varName = type.Name;
                                    if (pairs.ContainsKey(type))
                                    {
                                        if (pairs[type].ContainsKey(cmp.SecondObject))
                                        {
                                            varName += pairs[type][cmp.SecondObject];
                                        }
                                        else
                                        {
                                            var num = pairs[type].Count + 1;
                                            varName += num;
                                            pairs[type].Add(cmp.SecondObject, num);
                                            class_members.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Family });
                                            cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                            cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                            cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeExpression[] { new CodeSnippetExpression($"{varName}") })));
                                        }
                                    }
                                    else
                                    {
                                        pairs.Add(type, new Dictionary<ICompareData, int>());
                                        pairs[type].Add(cmp.SecondObject, 1);
                                        varName += "1";
                                        class_members.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Family });
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                        cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeExpression[] { new CodeSnippetExpression($"{varName}") })));
                                    }
                                    expression_scnd = ((IIndicator)cmp.SecondObject).ToFormatString(varName);
                                }
                                else return Errors.InternalFatal;
                            }
                            else return Errors.InternalFatal;
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
                                    if (pairs[type].ContainsKey(cmp.SecondObject))
                                    {
                                        varName += pairs[type][cmp.SecondObject];
                                    }
                                    else
                                    {
                                        var num = pairs[type].Count + 1;
                                        varName += num;
                                        pairs[type].Add(cmp.SecondObject, num);
                                        class_members.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Family });
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                        cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeExpression[] { new CodeSnippetExpression($"{varName}") })));
                                    }
                                }
                                else
                                {
                                    pairs.Add(type, new Dictionary<ICompareData, int>());
                                    pairs[type].Add(cmp.SecondObject, 1);
                                    varName += "1";
                                    class_members.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Family });
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                    cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeExpression[] { new CodeSnippetExpression($"{varName}") })));
                                }
                                expression_scnd = ((IIndicator)cmp.SecondObject).ToFormatString(varName);
                            }
                            else if (cmp.SecondObject is Variable) expression_scnd = ((Variable)cmp.SecondObject).ToFormatString();
                            else if (cmp.SecondObject is IPriceAction)
                            {
                                if (cmp.FirstObject is Volume && cmp.SecondObject is Volume) return Errors.IncompatibleCompares;
                                else expression_scnd = ((IPriceAction)cmp.SecondObject).ToFormatString();
                            }
                        }
                        else if (cmp.FirstObject is IIndicator)
                        {
                            var type = cmp.FirstObject.GetType();
                            string varName = type.Name;
                            if (pairs.ContainsKey(type))
                            {
                                if (pairs[type].ContainsKey(cmp.FirstObject))
                                {
                                    varName += pairs[type][cmp.FirstObject];
                                }
                                else
                                {
                                    var num = pairs[type].Count + 1;
                                    varName += num;
                                    pairs[type].Add(cmp.FirstObject, num);
                                    class_members.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Family });
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.FirstObject).ToCtorString())));
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                    cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeExpression[] { new CodeSnippetExpression($"{varName}") })));
                                }
                            }
                            else
                            {
                                pairs.Add(type, new Dictionary<ICompareData, int>());
                                pairs[type].Add(cmp.FirstObject, 1);
                                varName += "1";
                                class_members.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Family });
                                cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.FirstObject).ToCtorString())));
                                cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeExpression[] { new CodeSnippetExpression($"{varName}") })));
                            }
                            expression_frst = ((IIndicator)cmp.FirstObject).ToFormatString(varName);
                            if (cmp.SecondObject is Variable) expression_scnd = ((Variable)cmp.SecondObject).ToFormatString();
                            else if (cmp.SecondObject is IPriceAction) expression_scnd = ((IPriceAction)cmp.SecondObject).ToFormatString();
                            else if (cmp.SecondObject is IIndicator)
                            {
                                type = cmp.SecondObject.GetType();
                                varName = type.Name;
                                if (pairs.ContainsKey(type))
                                {
                                    if (pairs[type].ContainsKey(cmp.SecondObject))
                                    {
                                        varName += pairs[type][cmp.SecondObject];
                                    }
                                    else
                                    {
                                        var num = pairs[type].Count + 1;
                                        varName += num;
                                        pairs[type].Add(cmp.SecondObject, num);
                                        class_members.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Family });
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                        cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeExpression[] { new CodeSnippetExpression($"{varName}") })));
                                    }
                                }
                                else
                                {
                                    pairs.Add(type, new Dictionary<ICompareData, int>());
                                    pairs[type].Add(cmp.SecondObject, 1);
                                    varName += "1";
                                    class_members.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Family });
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                    cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeExpression[] { new CodeSnippetExpression($"{varName}") })));
                                }
                                expression_scnd = ((IIndicator)cmp.SecondObject).ToFormatString(varName);
                            }
                            else return Errors.InternalFatal;
                        }
                        conditions.Add(new CodeMethodInvokeExpression(null, cmp.Operation.ToString(), new CodeExpression[] { new CodeSnippetExpression(expression_frst), new CodeSnippetExpression(expression_scnd), new CodePrimitiveExpression(1) }));
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
                            else if (cmp.SecondObject is Variable)
                            {
                                Variable vr_scnd = (Variable)cmp.SecondObject;
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
                        else if (cmp.FirstObject is Variable)
                        {
                            Variable frst = (Variable)cmp.FirstObject;
                            if (frst.Type == VariableType.Time)
                            {
                                expression_frst = frst.ToFormatString() + ".TimeOfDay";
                                if (cmp.SecondObject is TimeValue) expression_scnd = ((TimeValue)cmp.SecondObject).ToFormatString();
                                else if (cmp.SecondObject is Variable)
                                {
                                    if (((Variable)cmp.SecondObject).Type == VariableType.Time) expression_scnd = ((Variable)cmp.SecondObject).ToFormatString() + ".TimeOfDay";
                                    else return Errors.IncompatibleCompares;
                                }
                                else return Errors.IncompatibleCompares;
                            }
                            else if (frst.Type == VariableType.String || frst.Type == VariableType.Boolean) return Errors.IncompatibleCompares;
                            else
                            {
                                expression_frst = frst.ToFormatString();
                                if (cmp.SecondObject is Variable)
                                {
                                    Variable scnd = (Variable)cmp.SecondObject;
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
                                        if (pairs[type].ContainsKey(cmp.SecondObject))
                                        {
                                            varName += pairs[type][cmp.SecondObject];
                                        }
                                        else
                                        {
                                            var num = pairs[type].Count + 1;
                                            varName += num;
                                            pairs[type].Add(cmp.SecondObject, num);
                                            class_members.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Family });
                                            cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                            cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                            cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeExpression[] { new CodeSnippetExpression($"{varName}") })));
                                        }
                                    }
                                    else
                                    {
                                        pairs.Add(type, new Dictionary<ICompareData, int>());
                                        pairs[type].Add(cmp.SecondObject, 1);
                                        varName += "1";
                                        class_members.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Family });
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                        cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeExpression[] { new CodeSnippetExpression($"{varName}") })));
                                    }
                                    expression_scnd = ((IIndicator)cmp.SecondObject).ToFormatString(varName);
                                }
                                else return Errors.IncompatibleCompares;
                            }
                        }
                        else
                        {
                            if (cmp.FirstObject is IIndicator)
                            {
                                var type = cmp.FirstObject.GetType();
                                string varName = type.Name;
                                if (pairs.ContainsKey(type))
                                {
                                    if (pairs[type].ContainsKey(cmp.FirstObject))
                                    {
                                        varName += pairs[type][cmp.FirstObject];
                                    }
                                    else
                                    {
                                        var num = pairs[type].Count + 1;
                                        varName += num;
                                        pairs[type].Add(cmp.FirstObject, num);
                                        class_members.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Family });
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.FirstObject).ToCtorString())));
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                        cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeExpression[] { new CodeSnippetExpression($"{varName}") })));
                                    }
                                }
                                else
                                {
                                    pairs.Add(type, new Dictionary<ICompareData, int>());
                                    pairs[type].Add(cmp.FirstObject, 1);
                                    varName += "1";
                                    class_members.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Family });
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.FirstObject).ToCtorString())));
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                    cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeExpression[] { new CodeSnippetExpression($"{varName}") })));
                                }
                                expression_frst = ((IIndicator)cmp.FirstObject).ToFormatString(varName);
                            }
                            else if (cmp.FirstObject is IPriceAction)
                            {
                                expression_frst = ((IPriceAction)cmp.FirstObject).ToFormatString();
                            }
                            if (cmp.SecondObject is IIndicator)
                            {
                                var type = cmp.SecondObject.GetType();
                                string varName = type.Name;
                                if (pairs.ContainsKey(type))
                                {
                                    if (pairs[type].ContainsKey(cmp.SecondObject))
                                    {
                                        varName += pairs[type][cmp.SecondObject];
                                    }
                                    else
                                    {
                                        var num = pairs[type].Count + 1;
                                        varName += num;
                                        pairs[type].Add(cmp.SecondObject, num);
                                        class_members.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Family });
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                        cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                        cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeExpression[] { new CodeSnippetExpression($"{varName}") })));
                                    }
                                }
                                else
                                {
                                    pairs.Add(type, new Dictionary<ICompareData, int>());
                                    pairs[type].Add(cmp.SecondObject, 1);
                                    varName += "1";
                                    class_members.Add(new CodeMemberField(type.Name, varName) { Attributes = MemberAttributes.Family });
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(null, varName), new CodeSnippetExpression(((IIndicator)cmp.SecondObject).ToCtorString())));
                                    cond_setdatald.Add(new CodeAssignStatement(new CodeSnippetExpression($"{varName}.Plots[0].Brush"), new CodeSnippetExpression("Brushes.Red")));
                                    cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "AddChartIndicator", new CodeExpression[] { new CodeSnippetExpression($"{varName}") })));
                                }
                                expression_scnd = ((IIndicator)cmp.SecondObject).ToFormatString(varName);
                            }
                            else if (cmp.SecondObject is IPriceAction)
                            {
                                expression_scnd = ((IPriceAction)cmp.SecondObject).ToFormatString();
                            }
                        }

                        if (expression_frst != "" && expression_scnd != "") conditions.Add(new CodeBinaryOperatorExpression(new CodeSnippetExpression(expression_frst), (CodeBinaryOperatorType)cmp.Operation, new CodeSnippetExpression(expression_scnd)));
                    }
                }

                //Actions
                for (int i = 0; i < cnd_set.Operations.Length; i++)
                {
                    if (cnd_set.Operations[i] is Long)
                    {
                        if (cnd_set.Operations[i].Action == TargetActionType.Entry) actions.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "EnterLong", new CodeExpression[] { new CodeSnippetExpression($"{(cnd_set.Operations[i].Quantity == 0 ? strategyData.Defaults.ContractsPerEntry : cnd_set.Operations[i].Quantity)}"), new CodeSnippetExpression("\"\"") })));
                        else if (cnd_set.Operations[i].Action == TargetActionType.Exit) actions.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "ExitLong", new CodeExpression[] { new CodeSnippetExpression($"{(cnd_set.Operations[i].Quantity == 0 ? strategyData.Defaults.ContractsPerEntry : cnd_set.Operations[i].Quantity)}"), new CodeSnippetExpression("\"\""), new CodeSnippetExpression("\"\"") })));
                        else return Errors.InternalFatal;
                    }
                    else if (cnd_set.Operations[i] is Short)
                    {
                        if (cnd_set.Operations[i].Action == TargetActionType.Entry) actions.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "EnterShort", new CodeExpression[] { new CodeSnippetExpression($"{(cnd_set.Operations[i].Quantity == 0 ? strategyData.Defaults.ContractsPerEntry : cnd_set.Operations[i].Quantity)}"), new CodeSnippetExpression("\"\"") })));
                        else if (cnd_set.Operations[i].Action == TargetActionType.Exit) actions.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "ExitShort", new CodeExpression[] { new CodeSnippetExpression($"{(cnd_set.Operations[i].Quantity == 0 ? strategyData.Defaults.ContractsPerEntry : cnd_set.Operations[i].Quantity)}"), new CodeSnippetExpression("\"\""), new CodeSnippetExpression("\"\"") })));
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
                if (ta.TargetType == TargetType.TakeProfit) cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "SetProfitTarget", new CodeExpression[] { new CodePrimitiveExpression(""), new CodeSnippetExpression($"CalculationMode.{ta.Type}"), new CodeSnippetExpression($"{ta.Value}") })));
                else if (ta.TargetType == TargetType.StopLoss) cond_setdatald.Add(new CodeExpressionStatement(new CodeMethodInvokeExpression(null, "SetStopLoss", new CodeExpression[] { new CodePrimitiveExpression(""), new CodeSnippetExpression($"CalculationMode.{ta.Type}"), new CodeSnippetExpression($"{ta.Value}"), new CodePrimitiveExpression(false) })));
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

            cunit.Namespaces.Add(gns);
            cunit.Namespaces.Add(ns);
            ns.Types.Add(strategyclass);
            strategyclass.Members.AddRange(class_members.ToArray());
            strategyclass.Members.Add(new CodeSnippetTypeMember());
            strategyclass.Members.Add(statechange);
            strategyclass.Members.Add(new CodeSnippetTypeMember());
            strategyclass.Members.Add(barupdate);

            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CodeGeneratorOptions op = new CodeGeneratorOptions();
            op.BlankLinesBetweenMembers = false;
            op.BracingStyle = "C";
            op.VerbatimOrder = true;
            //using (StreamWriter sw = new StreamWriter(File.OpenWrite("F:\\bal.cs")))
            //{
            //    provider.GenerateCodeFromCompileUnit(cunit, sw, op);
            //}

            provider.GenerateCodeFromCompileUnit(cunit, Console.Out, op);

            return Errors.Success;
        }
    }
}
