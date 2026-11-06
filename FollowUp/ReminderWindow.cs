namespace FollowUp;
public sealed class ReminderWindow : Window
{
    private readonly Repository db;
    private readonly DataGrid grid=new();
    private List<FollowUpTask> pending;
    public ReminderWindow(Repository repository,List<FollowUpTask> tasks)
    {
        db=repository; pending=tasks; Style=(Style)FindResource(typeof(Window));
        Title="فالوآپ — زمان پیگیری است"; Width=820; Height=390; MinWidth=650; MinHeight=320; Topmost=true;
        FlowDirection=FlowDirection.LeftToRight; WindowStartupLocation=WindowStartupLocation.CenterScreen;
        var root=new DockPanel { Margin=new Thickness(24),FlowDirection=FlowDirection.RightToLeft };
        var heading=new StackPanel { Margin=new Thickness(0,0,0,18) };
        heading.Children.Add(new TextBlock { Text="زمان پیگیری است",FontSize=25,FontWeight=FontWeights.Bold });
        heading.Children.Add(new TextBlock { Text="اقدام را انجام‌شده علامت بزنید یا زمان یادآوری را عقب بیندازید.",Margin=new Thickness(0,8,0,0) }); DockPanel.SetDock(heading,Dock.Top); root.Children.Add(heading);
        var actions=new WrapPanel { Margin=new Thickness(0,16,0,0),FlowDirection=FlowDirection.RightToLeft };
        void Button(string label,Action action) { var b=new System.Windows.Controls.Button { Content=label }; b.Click+=(_,_)=>action(); actions.Children.Add(b); }
        Button("انجام شد",()=>Act(t=>db.CompleteTask(t.Id)));
        Button("۱۵ دقیقه بعد",()=>Act(t=>db.Snooze(t.Id,DateTime.Now.AddMinutes(15))));
        Button("یک ساعت بعد",()=>Act(t=>db.Snooze(t.Id,DateTime.Now.AddHours(1))));
        Button("تعویق همه، ۱۵ دقیقه",()=>{ foreach(var t in pending) db.Snooze(t.Id,DateTime.Now.AddMinutes(15)); pending=[]; Close(); });
        DockPanel.SetDock(actions,Dock.Bottom); root.Children.Add(actions);
        grid.Columns.Add(new DataGridTextColumn { Header="اقدام",Binding=new System.Windows.Data.Binding("Title"),Width=new DataGridLength(1,DataGridLengthUnitType.Star) });
        grid.Columns.Add(new DataGridTextColumn { Header="شرکت",Binding=new System.Windows.Data.Binding("CompanyName"),Width=170 });
        grid.Columns.Add(new DataGridTextColumn { Header="موعد شمسی",Binding=new System.Windows.Data.Binding("DueText"),Width=180 });
        root.Children.Add(grid); Content=root; Update(tasks);
        Closing+=(_,_)=>{ foreach(var t in pending) db.Snooze(t.Id,DateTime.Now.AddMinutes(15)); };
    }
    public void Update(List<FollowUpTask> tasks)
    {
        pending=tasks; var selected=(grid.SelectedItem as FollowUpTask)?.Id; grid.ItemsSource=tasks;
        grid.SelectedItem=tasks.FirstOrDefault(x=>x.Id==selected)??tasks.FirstOrDefault();
        if(tasks.Count==0) Close();
    }
    private void Act(Action<FollowUpTask> action)
    {
        if(grid.SelectedItem is not FollowUpTask t) return;
        action(t); Update(db.DueReminders(DateTime.Now));
    }
}
