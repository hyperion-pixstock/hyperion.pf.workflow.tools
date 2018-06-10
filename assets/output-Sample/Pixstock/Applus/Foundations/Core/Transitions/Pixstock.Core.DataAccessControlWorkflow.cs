using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Hyperion.Pf.Workflow;
using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Infrastructure;
using Appccelerate.StateMachine.Machine;
using Appccelerate.StateMachine.Persistence;
using Appccelerate.StateMachine.Reports;

namespace Pixstock.Applus.Foundations.Core.Transitions {
public partial class DataAccessControlWorkflow : AppStateMachine<States, Events>, IAsyncPassiveStateMachine {
	public static string Name = "Pixstock.Core.DataAccessControlWorkflow";
public void Setup() {
DefineHierarchyOn(States.ROOT)
.WithHistoryType(HistoryType.None)
.WithInitialSubState(States.RUN)
;
In(States.RUN)
.On(Events.ACT_SAVE_CATEGORY)
.Execute<object>(ACT_SAVE_CATEGORY);
In(States.RUN)
.On(Events.ACT_LOAD_CATEGORYLIST)
.Execute<object>(ACT_LOAD_CATEGORYLIST);
	Initialize(States.ROOT);
}
public virtual async Task ACT_SAVE_CATEGORY(object param) {
	Events.ACT_SAVE_CATEGORY.FireInvokeWorkflowEvent(new WorkflowMessageEventArgs(param));
	await OnACT_SAVE_CATEGORY(param);
	Events.ACT_SAVE_CATEGORY.FireCallbackWorkflowEvent(new WorkflowMessageEventArgs(param));
}
public virtual async Task ACT_LOAD_CATEGORYLIST(object param) {
	Events.ACT_LOAD_CATEGORYLIST.FireInvokeWorkflowEvent(new WorkflowMessageEventArgs(param));
	await OnACT_LOAD_CATEGORYLIST(param);
	Events.ACT_LOAD_CATEGORYLIST.FireCallbackWorkflowEvent(new WorkflowMessageEventArgs(param));
}
}
}
