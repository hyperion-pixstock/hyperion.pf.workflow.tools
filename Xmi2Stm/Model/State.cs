using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Xmi2Stm.Model
{
    public class NormalState : State
    {
        /// <summary>
        /// 入場動作
        /// </summary>
        public string EntryMethod { get; set; }

        /// <summary>
        /// 退場動作
        /// </summary>
        public string ExitMethod { get; set; }

        /// <summary>
        /// 初期開始位置の状態であることを示すフラグ
        /// </summary>
        public bool InitialAttribute { get; set; }

        /// <summary>
        /// 内部遷移
        /// </summary>
        public IList<Event> InternalEvent { get; private set; } = new List<Event>();

        /// <summary>
        /// 開始疑似状態(ある場合)
        /// </summary>
        public PseudoState PseudoState { get; set; }

        public NormalState StartSubState { get { return this.SubState.First(prop => prop.InitialAttribute == true); } }

        /// <summary>
        /// サブ状態一覧
        /// </summary>
        public IList<NormalState> SubState { get; private set; } = new List<NormalState>();

        /// <summary>
        /// 遷移(宛先のみ)
        /// </summary>
        public IList<Event> TransitionEvent { get; private set; } = new List<Event>();

        public string Dump(int nestCount)
        {
            StringBuilder spacer = new StringBuilder();
            for (int i = 0; i < nestCount; i++)
            {
                spacer.Append("   ");
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(spacer);
            if (this.InitialAttribute) sb.Append("*");

            sb.Append(this.Name).Append("(");
            foreach (var intrEvent in this.InternalEvent)
            {
                sb.Append(intrEvent.Name).Append(",");
            }
            sb.Append(")").AppendLine();

            if (this.PseudoState != null)
                sb.Append(spacer).Append("開始疑似状態:").AppendLine(this.PseudoState.Name);

            // 遷移先リスト
            foreach (TransitionEvent transition in this.TransitionEvent)
            {
                sb.Append(spacer).Append(">>>").Append(transition.Name).Append("=>").AppendLine(transition.Destination.Name);
            }

            // サブ状態ネスト
            foreach (var sub in SubState)
            {
                sb.Append(sub.Dump(nestCount + 1));
            }

            return sb.ToString();
        }

        /// <summary>
        /// イベントクラス
        /// </summary>
        /// <param name="output1"></param>
        public void ExportClass(TextWriter output1)
        {
            if (!string.IsNullOrEmpty(this.Id))
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($@"public class CLS_{this.TinyName} : States {{}}");

                output1.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// 現在の状態で処理するイベント（入場動作・退場動作・内部遷移）の定義コードを出力する
        /// </summary>
        /// <param name="output1"></param>
        public void ExportEvent(TextWriter output1, bool isFrameTransitionStateMachine)
        {
            if (!string.IsNullOrEmpty(this.Id))
            {
                //if (this.TinyName == "CategoryTreeLoading")
                //	Console.WriteLine("AAA");
                //if (this.TinyName == "CategoryEditDialog")
                //	Console.WriteLine("AAA");

                // 入場動作
                if (!isFrameTransitionStateMachine && !string.IsNullOrEmpty(this.EntryMethod))
                {
                    output1.WriteLine($@"In(States.{this.TinyName})");
                    output1.WriteLine($@".ExecuteOnEntry({this.EntryMethod});");
                }
                else
                {
                    if (isFrameTransitionStateMachine)
                    {
                        string entryMethod = "";
                        if (IsFrameState)
                        {
                            if (!string.IsNullOrEmpty(this.EntryMethod))
                                entryMethod = this.EntryMethod;
                            else
                                entryMethod = FormatFrameTransitionEventName(this.TinyName, 1);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(this.EntryMethod))
                                entryMethod = this.EntryMethod;
                        }

                        if (!string.IsNullOrEmpty(entryMethod))
                        {
                            output1.WriteLine($@"In(States.{this.TinyName})");
                            output1.WriteLine($@".ExecuteOnEntry({entryMethod});");
                        }
                    }
                }

                // 退場動作
                if (!isFrameTransitionStateMachine && !string.IsNullOrEmpty(this.ExitMethod))
                {
                    output1.WriteLine($@"In(States.{this.TinyName})");
                    output1.WriteLine($@".ExecuteOnExit({this.ExitMethod});");
                }
                else
                {
                    if (isFrameTransitionStateMachine)
                    {
                        string exitMethod = "";
                        if (IsFrameState)
                        {
                            if (!string.IsNullOrEmpty(this.ExitMethod))
                                exitMethod = this.ExitMethod;
                            else
                                exitMethod = FormatFrameTransitionEventName(this.TinyName, 0);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(this.ExitMethod))
                                exitMethod = this.ExitMethod;
                        }

                        if (!string.IsNullOrEmpty(exitMethod))
                        {
                            output1.WriteLine($@"In(States.{this.TinyName})");
                            output1.WriteLine($@".ExecuteOnExit({exitMethod});");
                        }
                    }
                }

                // 内部遷移イベント
                foreach (InternalEvent innerEvent in this.InternalEvent)
                {
                    if (innerEvent.Name.StartsWith("RBM%3A"))
                    {
                        innerEvent.Prefix = PrefixType.RIBBONEVENT;
                        innerEvent.Name = "RBM_" + RemovePrefix(innerEvent.Name);
                    }

                    string EventsName = "";
                    string eventNamePrefexRemoved = innerEvent.Name;
                    if (innerEvent.Prefix == PrefixType.RIBBONEVENT)
                        EventsName = $"Events.{eventNamePrefexRemoved}";
                    else
                        EventsName = $"Events.{eventNamePrefexRemoved}";

                    string actionName = ActionName(innerEvent);
                    output1.WriteLine($@"In(States.{this.TinyName})
.On({EventsName})
.Execute<object>({actionName});");
                }
            }
        }

        public void ExportEventHandler(TextWriter output1, bool isFrameTransitionStateMachine)
        {
            string ribbonMenuEventCode = "";
            if (IsFrameState)
            {
                // RibbonMenuEvent定義ソースコードの生成
                var r = from u in InternalEvent
                        where u is InternalEvent
                        where ((InternalEvent)u).Prefix == PrefixType.RIBBONEVENT
                        select $"(int)RibbonMenuDefType.{RemovePrefixRBM(u.Name)}";

                string joined = string.Join(",", r);
                ribbonMenuEventCode = $"ICollection<int> ribbonMenuEventId = new List<int>{{ {joined} }};";
            }

            // 入場動作
            if (!isFrameTransitionStateMachine)
            {
                if (!string.IsNullOrEmpty(this.EntryMethod))
                {
                    output1.WriteLine($"public virtual async Task {EntryMethod}() {{");
                    output1.WriteLine($"	await On{EntryMethod}();");
                    output1.WriteLine($"}}");
                }
            }
            else
            {
                string frameStateCode = "";
                if (IsFrameState)
                {
                    // ShowFrame指示ソースコードの生成
                    string screenName = this.TinyName;
                    frameStateCode = $"	ShowFrame(\"{screenName}\",ribbonMenuEventId);";
                }

                if (!string.IsNullOrEmpty(this.EntryMethod))
                {
                    output1.WriteLine($"public virtual async Task {EntryMethod}() {{");
                    output1.WriteLine($"	await On{EntryMethod}();");
                    if (!string.IsNullOrEmpty(frameStateCode))
                    {
                        output1.WriteLine(ribbonMenuEventCode);
                        output1.WriteLine(frameStateCode);
                    }
                    output1.WriteLine($"}}");
                }
                else if (IsFrameState)
                {
                    string entryMethod = FormatFrameTransitionEventName(this.TinyName, 1);
                    output1.WriteLine($"public virtual async Task {entryMethod}() {{");
                    if (!string.IsNullOrEmpty(frameStateCode))
                    {
                        output1.WriteLine(ribbonMenuEventCode);
                        output1.WriteLine(frameStateCode);
                    }
                    output1.WriteLine($"}}");
                }
            }

            // 退場動作
            if (!isFrameTransitionStateMachine)
            {
                if (!string.IsNullOrEmpty(this.ExitMethod))
                {
                    output1.WriteLine($"public virtual async Task {ExitMethod}() {{");
                    output1.WriteLine($"	await On{ExitMethod}();");
                    output1.WriteLine($"}}");
                }
            }
            else
            {
                string frameStateCode = "";
                if (IsFrameState)
                {
                    // HideFrame指示ソースコードの生成
                    string screenName = this.TinyName;
                    frameStateCode = $"	HideFrame(\"{screenName}\", ribbonMenuEventId);";
                }

                if (!string.IsNullOrEmpty(this.ExitMethod))
                {
                    output1.WriteLine($"public virtual async Task {ExitMethod}() {{");
                    output1.WriteLine($"	await On{ExitMethod}();");
                    if (!string.IsNullOrEmpty(frameStateCode))
                    {
                        output1.WriteLine(ribbonMenuEventCode);
                        output1.WriteLine(frameStateCode);
                    }
                    output1.WriteLine($"}}");
                }
                else if (IsFrameState)
                {
                    string exitMethod = FormatFrameTransitionEventName(this.TinyName, 0);
                    output1.WriteLine($"public virtual async Task {exitMethod}() {{");
                    if (!string.IsNullOrEmpty(frameStateCode))
                    {
                        output1.WriteLine(ribbonMenuEventCode);
                        output1.WriteLine(frameStateCode);
                    }
                    output1.WriteLine($"}}");
                }
            }

            // 内部遷移イベント
            foreach (InternalEvent innerEvent in this.InternalEvent)
            {
                string actionName = ActionName(innerEvent);
                string eventName = innerEvent.Name;

                output1.WriteLine($"public virtual async Task {actionName}(object param) {{");
                output1.WriteLine($"	Events.{eventName}.FireInvokeWorkflowEvent(new WorkflowMessageEventArgs(param));");
                output1.WriteLine($"	await On{actionName}(param);");
                output1.WriteLine($"	Events.{eventName}.FireCallbackWorkflowEvent(new WorkflowMessageEventArgs(param));");
                output1.WriteLine($"}}");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="output1"></param>
        public void ExportHierarchy(TextWriter output1)
        {
            if (!string.IsNullOrEmpty(this.Id) && this.SubState.Count > 0)
            {
                output1.WriteLine($@"DefineHierarchyOn(States.{this.TinyName})
.WithHistoryType(HistoryType.None)");

                // 初期開始の状態を設定する
                var initialState = StartSubState;
                if (initialState != null)
                {
                    output1.WriteLine($".WithInitialSubState(States.{initialState.TinyName})");
                }

                foreach (var state in this.SubState)
                {
                    if (state.InitialAttribute == false)
                        output1.WriteLine($".WithSubState(States.{state.TinyName})");
                }
                output1.WriteLine(";");
            }

            // SubStatus
            foreach (var subStatus in this.SubState)
            {
                subStatus.ExportHierarchy(output1);
            }
        }

        /// <summary>
        /// 現在の状態で処理する遷移イベント（状態遷移）の定義コードを出力する
        /// </summary>
        /// <param name="output1"></param>
        public void ExportTransition(TextWriter output1)
        {
            foreach (TransitionEvent transitionEvent in this.TransitionEvent)
            {
                output1.WriteLine($@"In(States.{this.TinyName})
.On(Events.{transitionEvent.Name})
.Goto(States.{transitionEvent.Destination.TinyName});");
            }
        }

        private string ActionName(InternalEvent innerEvent)
        {
            string actionName = "";
            if (!string.IsNullOrEmpty(innerEvent.MethodName))
            {
                actionName = innerEvent.MethodName;
            }
            else
            {
                actionName = RemovePrefix(innerEvent.Name);
            }
            return actionName;
        }

        private string FormatFrameTransitionEventName(string name, int entryOrExit)
        {
            if (entryOrExit == 1)
            {
                return "__FTC_Event_" + name + "_Entry";
            }
            else if (entryOrExit == 0)
            {
                return "__FTC_Event_" + name + "_Exit";
            }

            throw new ApplicationException();
        }

        private string RemovePrefix(string name)
        {
            Regex rgx = new Regex("^.*%3A");
            return rgx.Replace(name, "");
        }

        private string RemovePrefixRBM(string name)
        {
            Regex rgx = new Regex("^RBM_");
            return rgx.Replace(name, "", 1);
        }
    }

    public class PseudoState : State
    {
    }

    public class State
    {
        public string Id { get; set; }
        public bool IsFrameState
        {
            get { return this.Name.LastIndexOf("FRM%3A") >= 0 ? true : false; }
        }

        public string Name { get; set; }

        public string TinyName
        {
            get
            {
                return this.Name.Replace("FRM%3A", "");
            }
        }
    }
}
