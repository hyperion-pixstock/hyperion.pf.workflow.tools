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
public partial class ServiceInterfaceWorkflow : AppStateMachine<States, Events>, IAsyncPassiveStateMachine {
	public static string Name = "Pixstock.Core.ServiceInterfaceWorkflow";
public void Setup() {
In(States.INIT)
.On(Events.TRNS_CONNECTED)
.Goto(States.RUN);
In(States.RUN)
.On(Events.TRNS_DISCONNECTED)
.Goto(States.INIT);
In(States.INIT)
.On(Events.ACT_CONNECTING_SERVICE)
.Execute<object>(ConnectingService);
In(States.RUN)
.On(Events.ACT_GETARTIFACTLIST)
.Execute<object>(GetArtifactList);
In(States.RUN)
.On(Events.ACT_LOAD_PREVIEW)
.Execute<object>(ACT_LOAD_PREVIEW);
In(States.RUN)
.On(Events.ACT_LOAD_CATEGORY)
.Execute<object>(ACT_LOAD_CATEGORY);
In(States.RUN)
.On(Events.ACT_PUT_CATEGORY)
.Execute<object>(ACT_PUT_CATEGORY);
In(States.RUN)
.On(Events.ACT_POST_CATEGORY)
.Execute<object>(ACT_POST_CATEGORY);
In(States.RUN)
.On(Events.ACT_GET_CATEGORY)
.Execute<object>(ACT_GET_CATEGORY);
In(States.RUN)
.On(Events.ACT_FETCH_CATEGORY)
.Execute<object>(ACT_FETCH_CATEGORY);
	Initialize(States.INIT);
}
public virtual async Task ConnectingService(object param) {
	Events.ACT_CONNECTING_SERVICE.FireInvokeWorkflowEvent(new WorkflowMessageEventArgs(param));
	await OnConnectingService(param);
	Events.ACT_CONNECTING_SERVICE.FireCallbackWorkflowEvent(new WorkflowMessageEventArgs(param));
}
public virtual async Task GetArtifactList(object param) {
	Events.ACT_GETARTIFACTLIST.FireInvokeWorkflowEvent(new WorkflowMessageEventArgs(param));
	await OnGetArtifactList(param);
	Events.ACT_GETARTIFACTLIST.FireCallbackWorkflowEvent(new WorkflowMessageEventArgs(param));
}
public virtual async Task ACT_LOAD_PREVIEW(object param) {
	Events.ACT_LOAD_PREVIEW.FireInvokeWorkflowEvent(new WorkflowMessageEventArgs(param));
	await OnACT_LOAD_PREVIEW(param);
	Events.ACT_LOAD_PREVIEW.FireCallbackWorkflowEvent(new WorkflowMessageEventArgs(param));
}
public virtual async Task ACT_LOAD_CATEGORY(object param) {
	Events.ACT_LOAD_CATEGORY.FireInvokeWorkflowEvent(new WorkflowMessageEventArgs(param));
	await OnACT_LOAD_CATEGORY(param);
	Events.ACT_LOAD_CATEGORY.FireCallbackWorkflowEvent(new WorkflowMessageEventArgs(param));
}
public virtual async Task ACT_PUT_CATEGORY(object param) {
	Events.ACT_PUT_CATEGORY.FireInvokeWorkflowEvent(new WorkflowMessageEventArgs(param));
	await OnACT_PUT_CATEGORY(param);
	Events.ACT_PUT_CATEGORY.FireCallbackWorkflowEvent(new WorkflowMessageEventArgs(param));
}
public virtual async Task ACT_POST_CATEGORY(object param) {
	Events.ACT_POST_CATEGORY.FireInvokeWorkflowEvent(new WorkflowMessageEventArgs(param));
	await OnACT_POST_CATEGORY(param);
	Events.ACT_POST_CATEGORY.FireCallbackWorkflowEvent(new WorkflowMessageEventArgs(param));
}
public virtual async Task ACT_GET_CATEGORY(object param) {
	Events.ACT_GET_CATEGORY.FireInvokeWorkflowEvent(new WorkflowMessageEventArgs(param));
	await OnACT_GET_CATEGORY(param);
	Events.ACT_GET_CATEGORY.FireCallbackWorkflowEvent(new WorkflowMessageEventArgs(param));
}
public virtual async Task ACT_FETCH_CATEGORY(object param) {
	Events.ACT_FETCH_CATEGORY.FireInvokeWorkflowEvent(new WorkflowMessageEventArgs(param));
	await OnACT_FETCH_CATEGORY(param);
	Events.ACT_FETCH_CATEGORY.FireCallbackWorkflowEvent(new WorkflowMessageEventArgs(param));
}
}
}
