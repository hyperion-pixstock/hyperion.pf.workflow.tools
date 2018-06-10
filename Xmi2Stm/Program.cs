using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xmi2Stm.Model;

namespace Xmi2Stm
{
    public class Transitions
    {

        public Event Event { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// イベントクラス
        /// </summary>
        /// <param name="output1"></param>
        public void ExportClass(TextWriter output1)
        {
            if (Event is InternalEvent internalEvent && internalEvent.Prefix != PrefixType.NONE) return;

            StringBuilder sb = new StringBuilder();
            sb.Append($@"public class CLS_{this.Name} : Events {{}}");
            output1.WriteLine(sb.ToString());
        }
    }

    class Program
    {
        /// <summary>
        /// メッセージ／イベント定義クラスの名前空間
        /// </summary>
        const string DeclareDefineCodeNamespace = "Pixstock.Core"; // TODO: 外部からの設定方法がないため、とりあえずハードコーディングしておく

        /// <summary>
        /// ステートマシンの全ての状態一覧（開始疑似状態を含む）
        /// </summary>
        static List<State> AllStateList = new List<State>();

        /// <summary>
        /// 
        /// </summary>
        static List<Transitions> AllTransitionsList = new List<Transitions>();

        static void AddTransition(string transitionName, Event eventObj)
        {
            if (!AllTransitionsList.Exists(p => p.Name == transitionName))
            {
                AllTransitionsList.Add(new Transitions
                {
                    Name = transitionName,
                    Event = eventObj
                });
            }
        }

        static void GenerateStateChartWorkflow(DirectoryInfo outputDirectory, XmlNode rootNode, XmlNode packageNode, XmlNamespaceManager xmlnsManager)
        {
            var packageName = packageNode.SelectSingleNode("@name").Value;
            var packageTagDict = GetPackageTagList(packageNode, xmlnsManager);

            // 状態遷移図取得
            XmlNodeList stNodeList = packageNode.SelectNodes("*/UML:StateMachine[@xmi.id]", xmlnsManager);
            foreach (XmlNode stateNd in stNodeList)
            {
                List<NormalState> StateActionList = new List<NormalState>(); //< 状態遷移図内で、アクションを含む状態の一覧(開始疑似状態は除く)

                NormalState rootState = new NormalState(); // 状態遷移図のルート状態

                var stateMachineId = stateNd.SelectSingleNode("@xmi.id", xmlnsManager).Value;
                var stateMachineName = stateNd.SelectSingleNode("@name", xmlnsManager).Value;

                string stateMachineFullName = packageName + "." + stateMachineName;

                string namespaceValue;
                if (packageTagDict.ContainsKey("namespace"))
                    namespaceValue = packageTagDict["namespace"];
                else
                    namespaceValue = packageName;

                string namespaceDirectoryPath = namespaceValue.Replace(".", "/");
                string pathOutputDirectory = Path.Combine(outputDirectory.FullName, namespaceDirectoryPath);
                Directory.CreateDirectory(pathOutputDirectory);

                string pathOutput = Path.Combine(pathOutputDirectory, $"{stateMachineFullName}.cs");

                //Console.WriteLine("NodeType: " + nd.LocalName);

                var pseudoStates = stateNd.SelectSingleNode("UML:StateMachine.top/UML:CompositeState/UML:CompositeState.subvertex/UML:Pseudostate", xmlnsManager);
                if (pseudoStates != null)
                {
                    if (rootState.PseudoState != null) throw new ApplicationException("1つの状態内には、開始疑似状態は1つのみ設定できます。");
                    var attrnd_id = pseudoStates.SelectSingleNode("@xmi.id");
                    var attrnd_name = pseudoStates.SelectSingleNode("@name");

                    PseudoState pseudoState = new PseudoState()
                    {
                        Id = attrnd_id.Value,
                        Name = attrnd_name.Value
                    };
                    AllStateList.Add(pseudoState);

                    rootState.PseudoState = pseudoState;

                    //Console.WriteLine($"開始状態 {attrnd_name.Value} ({attrnd_id.Value})");
                }

                var statesList = stateNd.SelectNodes("UML:StateMachine.top/UML:CompositeState/UML:CompositeState.subvertex/UML:CompositeState", xmlnsManager);
                foreach (XmlNode states_nd in statesList)
                {
                    rootState.SubState.Add(
                        ParseState(states_nd, xmlnsManager, 0, StateActionList)
                    );
                }


                // 遷移取得
                XmlNodeList transitonsNodeList = stateNd.SelectNodes("UML:StateMachine.transitions", xmlnsManager);
                foreach (XmlNode transition_nd in transitonsNodeList)
                {
                    foreach (XmlNode transition_item in transition_nd.SelectNodes("UML:Transition", xmlnsManager))
                    {
                        string transitionName = "", actionName = "";
                        var transition_bname = transition_item.SelectSingleNode("UML:Transition.trigger/UML:Event/@name", xmlnsManager);
                        if (transition_bname != null) // Transition.triggerを持たない遷移もある（開始疑似状態からの遷移など）
                        {
                            //Console.WriteLine($"TransitionName = {transition_bname.Value}");
                            transitionName = transition_bname.Value;
                        }

                        var transition_effect = transition_item.SelectSingleNode("UML:Transition.effect/UML:Action/@name", xmlnsManager);
                        if (transition_effect != null)
                        {
                            //Console.WriteLine($"ActionName = {transition_effect.Value}");
                            actionName = transition_effect.Value;
                        }


                        string srcId, destId;
                        var transition_source = transition_item.SelectSingleNode("UML:Transition.source/UML:StateVertex/@xmi.idref", xmlnsManager);
                        //Console.WriteLine($"Source = {transition_source.Value}");
                        srcId = transition_source.Value;
                        var transition_target = transition_item.SelectSingleNode("UML:Transition.target/UML:StateVertex/@xmi.idref", xmlnsManager);
                        //Console.WriteLine($"Target = {transition_target.Value}");
                        destId = transition_target.Value;


                        var rs = from u in AllStateList
                                 where u.Id == srcId
                                 select u;
                        var rd = from u in AllStateList
                                 where u.Id == destId
                                 select u;
                        var srcState = rs.FirstOrDefault() as State;
                        var destState = rd.FirstOrDefault() as State;
                        if (srcState != null && destState != null)
                        {

                            if (srcState is PseudoState)
                            {
                                if (destState is NormalState normalState)
                                    normalState.InitialAttribute = true;
                            }
                            else
                            {
                                if (srcState is NormalState normalState_src && destState is NormalState normalState_dest)
                                {
                                    var eventObj = new TransitionEvent
                                    {
                                        Name = transitionName,
                                        MethodName = actionName,
                                        Destination = normalState_dest
                                    };
                                    normalState_src.TransitionEvent.Add(eventObj);

                                    AddTransition(transitionName, eventObj);
                                }
                            }
                        }
                    }
                }

                bool isFrameTransitionStateMachine = false;
                var tags = GetStateMachineTagList(stateMachineId, rootNode, xmlnsManager);
                if (tags.ContainsKey("FrameTransition") && tags["FrameTransition"].ToUpper() == "TRUE")
                {
                    isFrameTransitionStateMachine = true;
                }


                Console.WriteLine(rootState.Dump(0));


                // ステートマシンのワークフローを出力
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(pathOutput))
                {
                    sw.WriteLine(@"using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Hyperion.Pf.Workflow;
using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Infrastructure;
using Appccelerate.StateMachine.Machine;
using Appccelerate.StateMachine.Persistence;
using Appccelerate.StateMachine.Reports;
using Hyperion.Pf.Workflow.StateMachine;
");
                    sw.WriteLine("using " + DeclareDefineCodeNamespace + ";");
                    sw.WriteLine($"namespace {namespaceValue} {{");

                    if (isFrameTransitionStateMachine)
                    {
                        sw.WriteLine($"public partial class {stateMachineName} : FrameStateMachine<States, Events>, IAsyncPassiveStateMachine {{");
                        //sw.WriteLine($"	public {stateMachineName}(Katalib.WPF.Infrastructures.IContent content) : base(content) {{ }}"); // 出力しない
                    }
                    else
                        sw.WriteLine($"public partial class {stateMachineName} : AppStateMachine<States, Events>, IAsyncPassiveStateMachine {{");

                    sw.WriteLine($"	public static string Name = \"{stateMachineFullName}\";");
                    sw.WriteLine("public void Setup() {");

                    rootState.ExportHierarchy(sw);
                    Array.ForEach<NormalState>(StateActionList.Where(prop => prop is NormalState).Cast<NormalState>().ToArray(), prop => prop.ExportTransition(sw));
                    Array.ForEach<NormalState>(StateActionList.Where(prop => prop is NormalState).Cast<NormalState>().ToArray(), prop => prop.ExportEvent(sw, isFrameTransitionStateMachine));

                    var startSubState = rootState.StartSubState;
                    sw.WriteLine($"	Initialize(States.{startSubState.TinyName});");

                    sw.WriteLine("}");

                    Array.ForEach<NormalState>(StateActionList.Where(prop => prop is NormalState).Cast<NormalState>().ToArray(), prop => prop.ExportEventHandler(sw, isFrameTransitionStateMachine));

                    sw.WriteLine("}");

                    sw.WriteLine("}"); // end of namespace
                }
            }

        }

        static Dictionary<string, string> GetPackageTagList(XmlNode packageNode, XmlNamespaceManager xmlnsManager)
        {
            Dictionary<string, string> tags = new Dictionary<string, string>();
            var tagNodes = packageNode.SelectNodes("UML:ModelElement.taggedValue/UML:TaggedValue", xmlnsManager);
            foreach (XmlNode tagNode in tagNodes)
            {
                string tagNodetag = tagNode.SelectSingleNode("@tag").Value;
                string tagNodeValue = tagNode.SelectSingleNode("@value").Value;

                tags.Add(tagNodetag, tagNodeValue);
            }

            return tags;
        }

        static Dictionary<string, string> GetStateMachineTagList(string stateMachineId, XmlNode packageNode, XmlNamespaceManager xmlnsManager)
        {
            Dictionary<string, string> tags = new Dictionary<string, string>();
            var tagNodes = packageNode.SelectNodes($"//JUDE:StateChartDiagram[@xmi.id]/JUDE:StateChartDiagram.semanticModel/UML:StateMachine[@xmi.idref='{stateMachineId}']/../../UML:ModelElement.taggedValue/UML:TaggedValue ", xmlnsManager);
            foreach (XmlNode tagNode in tagNodes)
            {
                string tagNodetag = tagNode.SelectSingleNode("@tag").Value;
                string tagNodeValue = tagNode.SelectSingleNode("@value").Value;

                tags.Add(tagNodetag, tagNodeValue);
            }

            return tags;
        }

        static void Main(string[] args)
        {
            string[] files = System.Environment.GetCommandLineArgs();
            if (files.Length == 1)
            {
                Console.WriteLine("入力ファイルがありません。");
                Console.WriteLine("アプリケーションを終了します。");
                return;
            }

            Console.WriteLine("InputFile=" + files[1]);

            var inputFile = new FileInfo(files[1]);
            string currentDirectory = Path.Combine(inputFile.Directory.FullName, "output");
            Directory.CreateDirectory(currentDirectory);

            using (System.IO.StreamReader sr = new System.IO.StreamReader(inputFile.FullName))
            {
                XmlNamespaceManager xmlnsManager;

                XmlDocument doc = new XmlDocument();
                doc.Load(inputFile.FullName);
                xmlnsManager = new XmlNamespaceManager(doc.NameTable);
                xmlnsManager.AddNamespace("UML", "org.omg.xmi.namespace.UML");
                xmlnsManager.AddNamespace("JUDE", "http://objectclub.esm.co.jp/Jude/namespace/");

                XmlNode root = doc.DocumentElement;


                // Package要素配下の状態遷移図を出力
                XmlNodeList packageNodeList = root.SelectNodes("//UML:Package[@xmi.id]", xmlnsManager);
                foreach (XmlNode packageNd in packageNodeList)
                {
                    GenerateStateChartWorkflow(new DirectoryInfo(currentDirectory), root, packageNd, xmlnsManager);
                }

                // 定義リストの出力
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(Path.Combine(currentDirectory, "StdWorkflowDefs.cs")))
                {
                    sw.WriteLine(@"using System;
using Hyperion.Pf.Workflow;
using Hyperion.Pf.Workflow.StateMachine;
using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Infrastructure;
using Appccelerate.StateMachine.Machine;
using Appccelerate.StateMachine.Persistence;
using Appccelerate.StateMachine.Reports;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;");

                    sw.WriteLine("namespace " + DeclareDefineCodeNamespace + " {");

                    // クラスリストの出力
                    Array.ForEach<NormalState>(AllStateList.Where(prop => prop is NormalState).Cast<NormalState>().GroupBy(prop => prop.Name).Select(g => g.First()).ToArray(), prop => prop.ExportClass(sw));
                    Array.ForEach<Transitions>(AllTransitionsList.ToArray(), prop => prop.ExportClass(sw));
                    sw.WriteLine("public class CLSINVALID_INVALID : Events {}"); // 無効イベントクラスの出力

                    // ステートのインスタンスを出力
                    sw.WriteLine("public partial class States : WorkflowStateBase {");
                    Array.ForEach<NormalState>(AllStateList.Where(prop => prop is NormalState).Cast<NormalState>().GroupBy(prop => prop.Name).Select(g => g.First()).ToArray(), prop => sw.WriteLine($"\tpublic static CLS_{prop.TinyName} {prop.TinyName} {{ get; }} = new CLS_{prop.TinyName}();"));
                    sw.WriteLine("}");

                    // イベントのインスタンスを出力
                    sw.WriteLine("public partial class Events : WorkflowEventBase {");
                    // 文字列からイベントを取得するヘルパ用の静的関数の出力
                    sw.WriteLine(@"        public static Dictionary<string, Events> cacheEventsDict = null;
        public static Events ForName(string name)
        {
            if (cacheEventsDict != null) return cacheEventsDict[name];

            cacheEventsDict = new Dictionary<string, Events>();
            var hogeType = typeof(Events);
            var names = hogeType.GetProperties(BindingFlags.Static | BindingFlags.Public)
                  .Where(x => x.PropertyType.IsSubclassOf(typeof(Events)))
                  .Select(x => new { Name = x.Name, Value = x.GetValue(hogeType, null) as Events }).ToList();
            foreach (var o in names)
            {
                cacheEventsDict.Add(o.Name, o.Value);
            }

            return cacheEventsDict[name];
        }");
                    Array.ForEach<Transitions>(AllTransitionsList.GroupBy(prop => prop.Name).Select(g => g.First()).ToArray(), prop => OutputEventClassList(sw, prop));
                    sw.WriteLine("	public static CLSINVALID_INVALID INVALID { get; } = new CLSINVALID_INVALID();"); // 無効イベントクラスのインスタンス定義

                    sw.WriteLine("}");

                    sw.WriteLine("}");
                }
            }

            Console.WriteLine("アプリケーション終了します");
            Console.ReadLine();
        }

        static void OutputEventClassList(System.IO.StreamWriter sw, Transitions transition)
        {
            if (transition.Event is InternalEvent internalEvent && internalEvent.Prefix == PrefixType.RIBBONEVENT) return;
            sw.WriteLine($"\tpublic static CLS_{transition.Name} {transition.Name} {{ get; }} = new CLS_{transition.Name}();");
        }

        static NormalState ParseState(XmlNode states_nd, XmlNamespaceManager xmlnsManager, int nestCount, List<NormalState> StateActionList)
        {
            StringBuilder spacer = new StringBuilder();
            for (int i = 0; i < nestCount; i++)
            {
                spacer.Append("   ");
            }

            //Console.WriteLine($"{spacer}NodeType: " + states_nd.LocalName);

            var attrnd_id = states_nd.SelectSingleNode("@xmi.id");
            //Console.WriteLine($"{spacer}xmi.id   : " + attrnd_id.Value);
            var attrnd_name = states_nd.SelectSingleNode("@name");
            //Console.WriteLine($"{spacer}xmi.name : " + attrnd_name.Value);

            var normalState = new NormalState
            {
                Id = attrnd_id.Value,
                Name = attrnd_name.Value
            };
            AllStateList.Add(normalState);

            // 内部遷移
            var internalTransitionNodes = states_nd.SelectNodes("UML:State.internalTransition/UML:Transition", xmlnsManager);
            foreach (XmlNode interTrans_nd in internalTransitionNodes)
            {
                string triggerName = "", actionName = "";
                var triggerNode = interTrans_nd.SelectSingleNode("UML:Transition.trigger/UML:Event/@name", xmlnsManager);
                //Console.WriteLine($"{spacer}InterTrans Event[name]: " + triggerNode.Value);
                triggerName = triggerNode.Value;

                var actionNode = interTrans_nd.SelectSingleNode("UML:Transition.effect/UML:Action/@name", xmlnsManager);
                if (actionNode != null)
                {
                    //Console.WriteLine($"{spacer}InterTrans Action[name]: " + actionNode.Value);
                    actionName = actionNode.Value;
                }

                var events = new InternalEvent
                {
                    Name = triggerName,
                    MethodName = actionName
                };
                AddTransition(triggerName, events);
                normalState.InternalEvent.Add(events);
            }

            // 開始疑似状態
            var pseudoStates = states_nd.SelectSingleNode("UML:CompositeState.subvertex/UML:Pseudostate", xmlnsManager);
            if (pseudoStates != null)
            {
                var pseudo_id = pseudoStates.SelectSingleNode("@xmi.id");
                var pseudo_name = pseudoStates.SelectSingleNode("@name");

                PseudoState pseudoState = new PseudoState()
                {
                    Id = pseudo_id.Value,
                    Name = pseudo_name.Value
                };

                normalState.PseudoState = pseudoState;
                AllStateList.Add(pseudoState);

                //Console.WriteLine($"{spacer}開始状態 {pseudo_name.Value} ({pseudo_id.Value})");
            }

            // Entry
            var entryNode = states_nd.SelectSingleNode("UML:State.entry", xmlnsManager);
            if (entryNode != null)
            {
                normalState.EntryMethod = entryNode.SelectSingleNode("UML:Action/@name", xmlnsManager).Value;
            }

            // Exit
            var exitNode = states_nd.SelectSingleNode("UML:State.exit", xmlnsManager);
            if (exitNode != null)
            {
                normalState.ExitMethod = exitNode.SelectSingleNode("UML:Action/@name", xmlnsManager).Value;
            }

            StateActionList.Add(normalState);

            // サブ状態
            var substatesList = states_nd.SelectNodes("UML:CompositeState.subvertex/UML:CompositeState", xmlnsManager);
            if (substatesList.Count > 0)
            {
                if (normalState.PseudoState == null) throw new ApplicationException("サブ状態を定義している場合は、開始疑似状態が必須です。");

                foreach (XmlNode states_nd2 in substatesList)
                {
                    normalState.SubState.Add(
                        ParseState(states_nd2, xmlnsManager, nestCount + 1, StateActionList)
                    );
                }
            }


            return normalState;
        }
    }
}
