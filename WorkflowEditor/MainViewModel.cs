using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using PropertyChanged;
using WorkflowEditor.Commands;
using WorkflowEditor.Controls;
using WorkflowEditor.PublicMethods;

namespace WorkflowEditor
{
    [AddINotifyPropertyChangedInterface]
    public class MainViewModel
    {
        public MainViewModel()
        {
            //WorkflowItemViewModels.Add(new WorkflowItemViewModel() { Name = "开始", StepType = StepType.Begin, Left = 100 });
            //WorkflowItemViewModels.Add(new WorkflowItemViewModel() { Name = "love", StepType = StepType.Nomal, Left = 200, Top = 200 });
            //WorkflowItemViewModels.Add(new WorkflowItemViewModel() { Name = "结束", StepType = StepType.End, Left = 300, Top = 400 });
            ////vm内设置必须双向设置，否侧曲线只会更新一端
            //WorkflowItemViewModels.Last().LastStep = WorkflowItemViewModels[1];
            //WorkflowItemViewModels[1].NextStep = WorkflowItemViewModels.Last();

            AddWorkflowItemCommand = new RelayCommand<StepType>((stepType) =>
            {
                var workflowEditor = ((MainWindow)Application.Current.MainWindow).workflowEditor;
                var point = workflowEditor.ContextMenu.TranslatePoint(new Point(), workflowEditor);
                WorkflowItemViewModel workflowItemViewModel = new WorkflowItemViewModel();
                workflowItemViewModel.Name = EnumDescriptionConverter.GetEnumDesc(stepType);
                workflowItemViewModel.StepType = stepType;
                workflowItemViewModel.Left = point.X;
                workflowItemViewModel.Top = point.Y;
                WorkflowItemViewModels.Add(workflowItemViewModel);
            });
            DeleteWorkflowItemCommand = new RelayCommand(() =>
            {
                if (SelectedWorkflowItems != null)
                {
                    for (int i = SelectedWorkflowItems.Count - 1; i >= 0; i--)
                    {
                        WorkflowItemViewModels.Remove(SelectedWorkflowItems[i] as WorkflowItemViewModel);
                    }
                }
            });
            SelectAllCommand = new RelayCommand(() =>
            {
                foreach (var item in WorkflowItemViewModels)
                {
                    item.IsSelected = true;
                }
            });
            UnselectAllCommand = new RelayCommand(() =>
            {
                foreach (var item in WorkflowItemViewModels)
                {
                    item.IsSelected = false;
                }
            });
        }

        public ICommand AddWorkflowItemCommand { get; }

        public ICommand DeleteWorkflowItemCommand { get; }
 
        public ICommand SelectAllCommand { get; }

        public ICommand UnselectAllCommand { get; }

        public ObservableCollection<WorkflowItemViewModel> WorkflowItemViewModels { get; set; } = new ObservableCollection<WorkflowItemViewModel>();

        public IList SelectedWorkflowItems { get; set; }

    }

    [AddINotifyPropertyChangedInterface]
    public class WorkflowItemViewModel
    {
        public bool IsSelected { get; set; }

        public void OnIsSelectedChanged()
        {
            Debug.WriteLine(PathContent);
        }

        public string Name { get; set; }

        public StepType StepType { get; set; } = StepType.Begin;

        public double Width { get; set; } = 200;
        public double Height { get; set; } = 80;

        public double Left { get; set; }
        public double Top { get; set; }

        public string PathContent { get; set; } = "下一节点";

        public WorkflowItemViewModel LastStep { get; set; }
        public WorkflowItemViewModel NextStep { get; set; }
        public WorkflowItemViewModel FromStep { get; set; }
        public WorkflowItemViewModel JumpStep { get; set; }
    }
}