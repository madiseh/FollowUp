namespace FollowUp;

public sealed class EditorWindow : Window
{
    private readonly StackPanel fields=new();
    private readonly TextBlock error=new() { Foreground=Brushes.Firebrick, TextWrapping=TextWrapping.Wrap, Margin=new Thickness(0,8,0,12) };
    public EditorWindow(Window owner,string title,string hint="")
    {
        Style=(Style)FindResource(typeof(Window));
        Owner=owner; Title=title; Width=560; MaxHeight=SystemParameters.WorkArea.Height-30; SizeToContent=SizeToContent.Height;
        WindowStartupLocation=WindowStartupLocation.CenterOwner; FlowDirection=FlowDirection.LeftToRight; ResizeMode=ResizeMode.NoResize;
        var root=new DockPanel { Margin=new Thickness(24),FlowDirection=FlowDirection.RightToLeft };
        var heading=new StackPanel(); heading.Children.Add(new TextBlock { Text=title,FontSize=23,FontWeight=FontWeights.Bold });
        if(hint!="") heading.Children.Add(new TextBlock { Text=hint,TextWrapping=TextWrapping.Wrap,Foreground=(Brush)FindResource("Muted"),Margin=new Thickness(0,8,0,16) });
        DockPanel.SetDock(heading,Dock.Top); root.Children.Add(heading);
        var bottom=new StackPanel(); bottom.Children.Add(error);
        var buttons=new StackPanel { Orientation=Orientation.Horizontal };
        var save=new Button { Content="ذخیره",IsDefault=true }; save.Click+=(_,_)=>Save(); buttons.Children.Add(save);
        var cancel=new Button { Content="انصراف",IsCancel=true,Style=(Style)FindResource("Secondary") }; buttons.Children.Add(cancel);
        bottom.Children.Add(buttons); DockPanel.SetDock(bottom,Dock.Bottom); root.Children.Add(bottom);
        root.Children.Add(new ScrollViewer { Content=fields,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,MaxHeight=Math.Max(200,SystemParameters.WorkArea.Height-250) }); Content=root;
    }
    public Action? OnSave { get; set; }
    private void Save() { try { OnSave?.Invoke(); DialogResult=true; } catch(Exception ex) { error.Text=ex.Message; } }
    public TextBox Text(string label,string value="",bool multiline=false)
    {
        fields.Children.Add(new TextBlock { Text=label });
        var box=new TextBox { Text=value,AcceptsReturn=multiline,TextWrapping=multiline?TextWrapping.Wrap:TextWrapping.NoWrap };
        if(multiline) box.Height=72;
        System.Windows.Automation.AutomationProperties.SetName(box,label); fields.Children.Add(box); return box;
    }
    public ComboBox Choice<T>(string label,IEnumerable<T> items,T? selected=default,bool editable=false)
    {
        fields.Children.Add(new TextBlock { Text=label });
        var box=new ComboBox { ItemsSource=items,IsEditable=editable,SelectedItem=selected };
        System.Windows.Automation.AutomationProperties.SetName(box,label); fields.Children.Add(box); return box;
    }
    public TextBox Date(string label,DateTime value)
    {
        fields.Children.Add(new TextBlock { Text=label+" (شمسی: سال/ماه/روز)" });
        var row=new DockPanel();
        var today=new Button { Content="امروز",Style=(Style)FindResource("Secondary"),Margin=new Thickness(8,4,0,12) }; DockPanel.SetDock(today,Dock.Left); row.Children.Add(today);
        var box=new TextBox { Text=PersianDates.Format(value),FlowDirection=FlowDirection.LeftToRight,HorizontalContentAlignment=HorizontalAlignment.Right };
        System.Windows.Automation.AutomationProperties.SetName(box,label); row.Children.Add(box); fields.Children.Add(row);
        today.Click+=(_,_)=>box.Text=PersianDates.Format(DateTime.Today); return box;
    }
    public void Note(string text) => fields.Children.Add(new TextBlock { Text=text,TextWrapping=TextWrapping.Wrap,Foreground=(Brush)FindResource("Muted"),Margin=new Thickness(0,0,0,12) });
    public void Presets(TextBox due,TextBox reminder,TextBox basis)
    {
        var panel=new WrapPanel { Margin=new Thickness(0,0,0,12) };
        foreach(var days in new[]{5,20,30}) {
            var b=new Button { Content=PersianDates.Digits(days.ToString())+" روز از مبنا",Style=(Style)FindResource("Secondary") };
            b.Click+=(_,_)=> { try { var d=PersianDates.Parse(basis.Text).AddDays(days); due.Text=PersianDates.Format(d); reminder.Text=PersianDates.Format(d.AddDays(-1)); } catch(Exception ex) { error.Text=ex.Message; } }; panel.Children.Add(b);
        }
        fields.Children.Add(panel);
    }
}
