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
public partial class MasterControlWorkflow : AppStateMachine<States, Events>, IAsyncPassiveStateMachine {
	public static string Name = "Pixstock.Core.MasterControlWorkflow";
public void Setup() {
DefineHierarchyOn(States.ROOT)
.WithHistoryType(HistoryType.None)
.WithInitialSubState(States.INIT)
.WithSubState(States.RUN)
;
DefineHierarchyOn(States.RUN)
.WithHistoryType(HistoryType.None)
.WithInitialSubState(States.IDLE)
.WithSubState(States.LOADING)
;
DefineHierarchyOn(States.LOADING)
.WithHistoryType(HistoryType.None)
.WithInitialSubState(States.LOADING_PREVIEW)
.WithSubState(States.LOADING_CONTENTLIST)
;
In(States.ROOT)
.On(Events.TRNS_INIT)
.Goto(States.INIT);
In(States.INIT)
.On(Events.TRNS_RUN)
.Goto(States.IDLE);
In(States.RUN)
.On(Events.TRNS_IDLE)
.Goto(States.IDLE);
In(States.RUN)
.On(Events.TRNS_EXIT)
.Goto(States.INIT);
In(States.IDLE)
.On(Events.TRNS_LOADING_PREVIEW)
.Goto(States.LOADING_PREVIEW);
In(States.IDLE)
.On(Events.TRNS_LOADING_CONTENTLIST)
.Goto(States.LOADING_CONTENTLIST);
In(States.INIT)
.ExecuteOnEntry(INIT_Entry);
In(States.IDLE)
.ExecuteOnEntry(IDLE_Entry);
In(States.IDLE)
.ExecuteOnExit(IDLE_Exit);
In(States.IDLE)
.On(Events.ACT_SAVE_CATEGORY)
.Execute<object>(ACT_SAVE_CATEGORY);
In(States.LOADING)
.ExecuteOnEntry(LOADING_Entry);
In(States.LOADING)
.ExecuteOnExit(LOADING_Exit);
In(States.LOADING_PREVIEW)
.ExecuteOnEntry(LOAD_PREVIEW_Entry);
In(States.LOADING_CONTENTLIST)
.ExecuteOnEntry(LOADING_CONTENTLIST_Entry);
	Initialize(States.ROOT);
}
public virtual async Task INIT_Entry() {
	await OnINIT_Entry();
}
public virtual async Task IDLE_Entry() {
	await OnIDLE_Entry();
}
public virtual async Task IDLE_Exit() {
	await OnIDLE_Exit();
}
public virtual async Task ACT_SAVE_CATEGORY(object param) {
	Events.ACT_SAVE_CATEGORY.FireInvokeWorkflowEvent(new WorkflowMessageEventArgs(param));
	await OnACT_SAVE_CATEGORY(param);
	Events.ACT_SAVE_CATEGORY.FireCallbackWorkflowEvent(new WorkflowMessageEventArgs(param));
}
public virtual async Task LOADING_Entry() {
	await OnLOADING_Entry();
}
public virtual async Task LOADING_Exit() {
	await OnLOADING_Exit();
}
public virtual async Task LOAD_PREVIEW_Entry() {
	await OnLOAD_PREVIEW_Entry();
}
public virtual async Task LOADING_CONTENTLIST_Entry() {
	await OnLOADING_CONTENTLIST_Entry();
}
}
}
