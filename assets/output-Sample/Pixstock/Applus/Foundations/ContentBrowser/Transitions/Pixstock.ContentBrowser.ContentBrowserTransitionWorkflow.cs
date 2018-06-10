using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Hyperion.Pf.Workflow;
using Appccelerate.StateMachine;
using Appccelerate.StateMachine.Infrastructure;
using Appccelerate.StateMachine.Machine;
using Appccelerate.StateMachine.Persistence;
using Appccelerate.StateMachine.Reports;

namespace Pixstock.Applus.Foundations.ContentBrowser.Transitions {
public partial class ContentBrowserTransitionWorkflow : FrameStateMachine<States, Events>, IAsyncPassiveStateMachine {
	public static string Name = "Pixstock.ContentBrowser.ContentBrowserTransitionWorkflow";
public void Setup() {
DefineHierarchyOn(States.ROOT)
.WithHistoryType(HistoryType.None)
.WithInitialSubState(States.INIT)
.WithSubState(States.RUN)
;
DefineHierarchyOn(States.RUN)
.WithHistoryType(HistoryType.None)
.WithInitialSubState(States.ContentBrowserTop)
;
DefineHierarchyOn(States.ContentBrowserTop)
.WithHistoryType(HistoryType.None)
.WithInitialSubState(States.ContentBrowserIdle)
.WithSubState(States.ContentBrowserLoading)
;
In(States.INIT)
.On(Events.TRNS_TOPSCREEN)
.Goto(States.ContentBrowserTop);
In(States.RUN)
.On(Events.TRNS_EXIT)
.Goto(States.INIT);
In(States.ContentBrowserIdle)
.On(Events.TRNS_LOADING)
.Goto(States.ContentBrowserLoading);
In(States.ContentBrowserLoading)
.On(Events.TRNS_IDLE)
.Goto(States.ContentBrowserIdle);
In(States.ROOT)
.On(Events.ACT_SHOW_NAVIPREVIEW)
.Execute<object>(ACT_SHOW_NAVIPREVIEW);
In(States.ContentBrowserTop)
.ExecuteOnEntry(ContentBrowserTop_Entry);
In(States.ContentBrowserTop)
.ExecuteOnExit(ContentBrowserTop_Exit);
In(States.ContentBrowserTop)
.On(Events.ACT_SHOW_SELECTEDPREVIEW)
.Execute<object>(ACT_SHOW_SELECTEDPREVIEW);
In(States.ContentBrowserTop)
.On(Events.RBM_DEBUG_1)
.Execute<object>(RibbonButtonMenu_DEBUG_1);
In(States.ContentBrowserLoading)
.ExecuteOnEntry(ContentBrowserLoading_Entry);
In(States.ContentBrowserLoading)
.ExecuteOnExit(ContentBrowserLoading_Exit);
	Initialize(States.ROOT);
}
public virtual async Task ACT_SHOW_NAVIPREVIEW(object param) {
	Events.ACT_SHOW_NAVIPREVIEW.FireInvokeWorkflowEvent(new WorkflowMessageEventArgs(param));
	await OnACT_SHOW_NAVIPREVIEW(param);
	Events.ACT_SHOW_NAVIPREVIEW.FireCallbackWorkflowEvent(new WorkflowMessageEventArgs(param));
}
public virtual async Task ContentBrowserTop_Entry() {
	await OnContentBrowserTop_Entry();
ICollection<int> ribbonMenuEventId = new List<int>{ (int)RibbonMenuDefType.DEBUG_1 };
	await UIDispatcher.InvokeAsync(() => ShowFrame("Pixstock.Applus.Containers.ContentBrowserTop",ribbonMenuEventId));
}
public virtual async Task ContentBrowserTop_Exit() {
	await OnContentBrowserTop_Exit();
ICollection<int> ribbonMenuEventId = new List<int>{ (int)RibbonMenuDefType.DEBUG_1 };
	await UIDispatcher.InvokeAsync(() => HideFrame(ribbonMenuEventId));
}
public virtual async Task ACT_SHOW_SELECTEDPREVIEW(object param) {
	Events.ACT_SHOW_SELECTEDPREVIEW.FireInvokeWorkflowEvent(new WorkflowMessageEventArgs(param));
	await OnACT_SHOW_SELECTEDPREVIEW(param);
	Events.ACT_SHOW_SELECTEDPREVIEW.FireCallbackWorkflowEvent(new WorkflowMessageEventArgs(param));
}
public virtual async Task RibbonButtonMenu_DEBUG_1(object param) {
	Events.RBM_DEBUG_1.FireInvokeWorkflowEvent(new WorkflowMessageEventArgs(param));
	await OnRibbonButtonMenu_DEBUG_1(param);
	Events.RBM_DEBUG_1.FireCallbackWorkflowEvent(new WorkflowMessageEventArgs(param));
}
public virtual async Task ContentBrowserLoading_Entry() {
	await OnContentBrowserLoading_Entry();
}
public virtual async Task ContentBrowserLoading_Exit() {
	await OnContentBrowserLoading_Exit();
}
}
}
