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
	public partial class CategoryTreeTransitionWorkflow : FrameStateMachine<States, Events>, IAsyncPassiveStateMachine {
		public static string Name = "Pixstock.ContentBrowser.CategoryTreeTransitionWorkflow";

		public void Setup() {
			// 以降は、Wofkflowライブラリの初期設定
			// 記述内容については、Workflowライブラリのドキュメントを参照してください。
			DefineHierarchyOn(States.ROOT)
				.WithHistoryType(HistoryType.None)
				.WithInitialSubState(States.CategoryTreeTop)
			;
			DefineHierarchyOn(States.CategoryTreeTop)
				.WithHistoryType(HistoryType.None)
				.WithInitialSubState(States.CategoryTreeIdle)
				.WithSubState(States.CategoryTreeLoading)
				.WithSubState(States.CategoryEditDialog)
			;
			In(States.INIT)
				.On(Events.TRNS_TOPSCREEN)
				.Goto(States.CategoryTreeTop);
			In(States.ROOT)
				.On(Events.TRNS_EXIT)
				.Goto(States.INIT);
			In(States.CategoryTreeTop)
				.On(Events.TRNS_IDLE)
				.Goto(States.CategoryTreeIdle);
			In(States.CategoryTreeIdle)
				.On(Events.TRNS_CategoryTreeLoading)
				.Goto(States.CategoryTreeLoading);
			In(States.CategoryTreeIdle)
				.On(Events.TRNS_DISP_CATEGORYEDIT)
				.Goto(States.CategoryEditDialog);
			In(States.CategoryTreeLoading)
				.On(Events.TRNS_CategoryTreeIdle)
				.Goto(States.CategoryTreeIdle);
			In(States.CategoryTreeTop)
				.ExecuteOnEntry(__FTC_Event_CategoryTreeTop_Entry);
			In(States.CategoryTreeTop)
				.ExecuteOnExit(__FTC_Event_CategoryTreeTop_Exit);
			In(States.CategoryTreeTop)
				.On(Events.ACT_SELECTED_ITEM)
				.Execute<object>(ACT_SELECTED_ITEM);
			In(States.CategoryTreeTop)
				.On(Events.RBM_DEBUG_2)
				.Execute<object>(RibbonButtonMenu_DEBUG_2);
			In(States.CategoryTreeIdle)
				.ExecuteOnEntry(CategoryTreeIdle_Entry);
			In(States.CategoryTreeIdle)
				.ExecuteOnExit(CategoryTreeIdle_Exit);
			In(States.CategoryTreeLoading)
				.ExecuteOnEntry(CategoryTreeLoading_Entry);
			In(States.CategoryTreeLoading)
				.ExecuteOnExit(CategoryTreeLoading_Exit);
			In(States.CategoryEditDialog)
				.ExecuteOnEntry(CategoryEditDialog_Entry);
			In(States.CategoryEditDialog)
				.ExecuteOnExit(CategoryEditDialog_Exit);
			In(States.CategoryEditDialog)
				.On(Events.ACT_SAVE_CATEGORY_ONCATEGORYEDIT)
				.Execute<object>(ACT_SAVE_CATEGORY_ONCATEGORYEDIT);
			Initialize(States.INIT);
		}

		
		public virtual async Task __FTC_Event_CategoryTreeTop_Entry() {
			// ※「FrameTransition=true」のため、「FRM:」で始まる状態のEntryは特殊な処理が含まれる
			ICollection<int> ribbonMenuEventId = new List<int>{ (int)RibbonMenuDefType.DEBUG_2 };
			await UIDispatcher.InvokeAsync(() => ShowFrame("Pixstock.Applus.Containers.CategoryTreeTop",ribbonMenuEventId));
		}
		public virtual async Task __FTC_Event_CategoryTreeTop_Exit() {
			ICollection<int> ribbonMenuEventId = new List<int>{ (int)RibbonMenuDefType.DEBUG_2 };
			await UIDispatcher.InvokeAsync(() => HideFrame(ribbonMenuEventId));
		}
		public virtual async Task ACT_SELECTED_ITEM(object param) {
			Events.ACT_SELECTED_ITEM.FireInvokeWorkflowEvent(new WorkflowMessageEventArgs(param));
			await OnACT_SELECTED_ITEM(param);
			Events.ACT_SELECTED_ITEM.FireCallbackWorkflowEvent(new WorkflowMessageEventArgs(param));
		}
		public virtual async Task RibbonButtonMenu_DEBUG_2(object param) {
			Events.RBM_DEBUG_2.FireInvokeWorkflowEvent(new WorkflowMessageEventArgs(param));
			await OnRibbonButtonMenu_DEBUG_2(param);
			Events.RBM_DEBUG_2.FireCallbackWorkflowEvent(new WorkflowMessageEventArgs(param));
		}
		public virtual async Task CategoryTreeIdle_Entry() {
			await OnCategoryTreeIdle_Entry();
		}
		public virtual async Task CategoryTreeIdle_Exit() {
			await OnCategoryTreeIdle_Exit();
		}
		public virtual async Task CategoryTreeLoading_Entry() {
			await OnCategoryTreeLoading_Entry();
		}
		public virtual async Task CategoryTreeLoading_Exit() {
			await OnCategoryTreeLoading_Exit();
		}
		public virtual async Task CategoryEditDialog_Entry() {
			await OnCategoryEditDialog_Entry();
		}
		public virtual async Task CategoryEditDialog_Exit() {
			await OnCategoryEditDialog_Exit();
		}
		public virtual async Task ACT_SAVE_CATEGORY_ONCATEGORYEDIT(object param) {
			Events.ACT_SAVE_CATEGORY_ONCATEGORYEDIT.FireInvokeWorkflowEvent(new WorkflowMessageEventArgs(param));
			await OnACT_SAVE_CATEGORY_ONCATEGORYEDIT(param);
			Events.ACT_SAVE_CATEGORY_ONCATEGORYEDIT.FireCallbackWorkflowEvent(new WorkflowMessageEventArgs(param));
		}
	}
}
