using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using Forms=System.Windows.Forms;

namespace FollowUp;
public partial class MainWindow : Window
{
    private readonly Repository db;
    private readonly Forms.NotifyIcon tray;
    private readonly DispatcherTimer timer;
    private ReminderWindow? alarm;
    private bool exiting;
    public MainWindow(Repository repository)
    {
        db=repository; InitializeComponent();
        using(var key=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run")) StartupToggle.IsChecked=key?.GetValue("FollowUp")!=null;
        var menu=new Forms.ContextMenuStrip(); menu.Items.Add("بازکردن فالوآپ",null,(_,_)=>Dispatcher.Invoke(ShowApp));
        menu.Items.Add("خروج کامل",null,(_,_)=>Dispatcher.Invoke(ExitApp));
        using var iconStream=Application.GetResourceStream(new Uri("pack://application:,,,/FollowUp;component/Assets/FollowUp.ico")).Stream;
        tray=new Forms.NotifyIcon { Text="فالوآپ — نسخه ۱.۰.۱",Icon=new System.Drawing.Icon(iconStream),Visible=true,ContextMenuStrip=menu };
        tray.DoubleClick+=(_,_)=>Dispatcher.Invoke(ShowApp);
        tray.BalloonTipClicked+=(_,_)=>Dispatcher.Invoke(ShowApp);
        Closing+=OnClosing;
        timer=new DispatcherTimer { Interval=TimeSpan.FromSeconds(30) }; timer.Tick+=(_,_)=>{ Refresh(); CheckAlarms(); }; timer.Start();
        Refresh(); Dispatcher.BeginInvoke(CheckAlarms,DispatcherPriority.ApplicationIdle);
    }
    private void ShowApp() { Show(); WindowState=WindowState.Normal; Activate(); Refresh(); }
    private void ExitApp() { exiting=true; timer.Stop(); alarm?.Close(); tray.Visible=false; tray.Dispose(); Application.Current.Shutdown(); }
    private void OnClosing(object? sender,CancelEventArgs e)
    {
        if(exiting) return;
        e.Cancel=true; Hide(); tray.ShowBalloonTip(3500,"فالوآپ فعال است","یادآوری‌ها فعال‌اند. برای خروج کامل از منوی آیکن کنار ساعت استفاده کنید.",Forms.ToolTipIcon.Info);
    }
    private static string Normalize(string s) => PersianDates.Latin(s).Replace('ي','ی').Replace('ك','ک').Replace("‌","").Trim();
    private bool Matches(params string[] values) => Normalize(string.Join(" ",values)).Contains(Normalize(Search.Text),StringComparison.OrdinalIgnoreCase);
    private void Refresh()
    {
        var selected=(CasesGrid.SelectedItem as TaxCase)?.Id;
        var companies=db.Companies(); var cases=db.Cases(); var tasks=db.Tasks();
        CompaniesGrid.ItemsSource=companies.Where(x=>Matches(x.Name,x.NationalId,x.Contact,x.Notes)).ToList();
        var filtered=cases.Where(x=>Matches(x.CompanyName,x.Title,x.Subject,x.Period,x.FileNumber,x.Owner,x.Stage,x.Notes)).ToList();
        CasesGrid.ItemsSource=filtered; if(selected.HasValue) CasesGrid.SelectedItem=filtered.FirstOrDefault(x=>x.Id==selected);
        TasksGrid.ItemsSource=tasks.Where(x=>(ShowDone.IsChecked==true||(!x.IsDone&&!x.IsClosed))&&Matches(x.CompanyName,x.CaseTitle,x.Title,x.Notes)).ToList();
        ActiveCount.Text=PersianDates.Digits(cases.Count(x=>x.Stage!="بسته‌شده").ToString());
        TodayCount.Text=PersianDates.Digits(tasks.Count(x=>!x.IsDone&&!x.IsClosed&&x.DueAt.Date==DateTime.Today).ToString());
        LateCount.Text=PersianDates.Digits(tasks.Count(x=>!x.IsDone&&!x.IsClosed&&x.DueAt<DateTime.Now).ToString());
        TodayLabel.Text="امروز، "+PersianDates.Format(DateTime.Today);
        Footer.Text=companies.Count==0?"برای شروع، در بخش شرکت‌ها یک شرکت ثبت کنید.":$"{PersianDates.Digits(companies.Count.ToString())} شرکت  •  {PersianDates.Digits(cases.Count.ToString())} پرونده";
    }
    private void Guard(Action action) { try { action(); } catch(Exception ex) { MessageBox.Show(this,ex.Message,"فالوآپ",MessageBoxButton.OK,MessageBoxImage.Warning); } }
    private T Selected<T>(DataGrid grid) where T:class => grid.SelectedItem as T ?? throw new InvalidOperationException("ابتدا یک ردیف را انتخاب کنید.");
    private void Search_Changed(object sender,TextChangedEventArgs e) { if(IsInitialized&&db!=null&&CasesGrid!=null) Refresh(); }
    private void ClearSearch_Click(object sender,RoutedEventArgs e) => Search.Clear();
    private void Filter_Click(object sender,RoutedEventArgs e) => Refresh();
    private void AddCompany_Click(object sender,RoutedEventArgs e) => CompanyEditor(null);
    private void EditCompany_Click(object sender,RoutedEventArgs e) => Guard(()=>CompanyEditor(Selected<Company>(CompaniesGrid)));
    private void EditCompany_DoubleClick(object sender,MouseButtonEventArgs e) { if(CompaniesGrid.SelectedItem is Company x) CompanyEditor(x); }
    private void CompanyEditor(Company? x)
    {
        var form=new EditorWindow(this,x==null?"شرکت جدید":"ویرایش شرکت");
        var name=form.Text("نام شرکت *",x?.Name??""); var national=form.Text("شناسه ملی",x?.NationalId??""); var contact=form.Text("نام و شمارهٔ تماس نماینده",x?.Contact??""); var notes=form.Text("یادداشت",x?.Notes??"",true);
        form.OnSave=()=>db.SaveCompany(new Company { Id=x?.Id??0,Name=name.Text,NationalId=national.Text,Contact=contact.Text,Notes=notes.Text });
        if(form.ShowDialog()==true) Refresh();
    }
    private void AddCase_Click(object sender,RoutedEventArgs e) => Guard(()=>CaseEditor(null));
    private void EditCase_Click(object sender,RoutedEventArgs e) => Guard(()=>CaseEditor(Selected<TaxCase>(CasesGrid)));
    private void EditCase_DoubleClick(object sender,MouseButtonEventArgs e) { if(CasesGrid.SelectedItem is TaxCase x) CaseEditor(x); }
    private void CaseEditor(TaxCase? x)
    {
        var companies=db.Companies(); if(companies.Count==0) throw new InvalidOperationException("ابتدا یک شرکت ثبت کنید.");
        var form=new EditorWindow(this,x==null?"پروندهٔ جدید":"ویرایش پرونده","هر شرکت می‌تواند چندین پرونده با موضوع و دورهٔ متفاوت داشته باشد.");
        var company=form.Choice("شرکت *",companies,companies.FirstOrDefault(c=>c.Id==x?.CompanyId)??companies[0]);
        var title=form.Text("عنوان پرونده *",x?.Title??""); var subject=form.Choice("موضوع مالیاتی *",Workflow.Subjects,x?.Subject??Workflow.Subjects[0],true);
        var period=form.Text("سال / دورهٔ شمسی",x?.Period??""); var number=form.Text("شماره / کلاسهٔ پرونده",x?.FileNumber??"");
        ComboBox? stage=x==null?form.Choice("مرحلهٔ شروع",Workflow.Stages,Workflow.Stages[0]):null;
        var owner=form.Text("مسئول پیگیری",x?.Owner??""); var notes=form.Text("توضیحات",x?.Notes??"",true);
        long savedId=0;
        form.OnSave=()=>{
            var record=new TaxCase { Id=x?.Id??0,CompanyId=(company.SelectedItem as Company)?.Id??0,Title=title.Text,Subject=subject.Text,Period=period.Text,FileNumber=number.Text,Stage=stage?.SelectedItem as string??x?.Stage??Workflow.Stages[0],Owner=owner.Text,Notes=notes.Text };
            savedId=db.SaveCase(record); if(x==null) db.RecordStage(savedId,record.Stage,DateTime.Today,"ثبت اولیهٔ پرونده");
        };
        if(form.ShowDialog()==true) { Refresh(); Pages.SelectedIndex=1; CasesGrid.SelectedItem=db.Cases().FirstOrDefault(c=>c.Id==savedId); }
    }
    private void History_Click(object sender,RoutedEventArgs e) => Guard(()=>{
        var x=Selected<TaxCase>(CasesGrid);
        var history=new DataGrid { Margin=new Thickness(20),MinHeight=220 };
        history.Columns.Add(new DataGridTextColumn { Header="تاریخ شمسی",Binding=new System.Windows.Data.Binding("DateText"),Width=140 });
        history.Columns.Add(new DataGridTextColumn { Header="مرحله",Binding=new System.Windows.Data.Binding("Stage"),Width=210 });
        history.Columns.Add(new DataGridTextColumn { Header="نتیجه / توضیحات",Binding=new System.Windows.Data.Binding("Notes"),Width=new DataGridLength(1,DataGridLengthUnitType.Star) });
        history.ItemsSource=db.History(x.Id);
        var window=new Window { Owner=this,Title="سوابق مراحل — "+x.Title,Width=760,Height=430,MinWidth=600,MinHeight=300,WindowStartupLocation=WindowStartupLocation.CenterOwner,FlowDirection=FlowDirection.LeftToRight,Style=(Style)FindResource(typeof(Window)),Content=history };
        window.ShowDialog();
    });
    private void Stage_Click(object sender,RoutedEventArgs e) => Guard(()=>{
        var x=Selected<TaxCase>(CasesGrid);
        var form=new EditorWindow(this,"ثبت مرحله و نتیجه",x.Display+"\nمراحل پیشین حفظ می‌شوند. برای بستن پرونده، «بسته‌شده» را انتخاب کنید.");
        var stage=form.Choice("مرحله / مرجع",Workflow.Stages,x.Stage); var date=form.Date("تاریخ رویداد",DateTime.Today); var notes=form.Text("نتیجه، توافق یا دلیل ارجاع", "",true);
        form.Note("مسیر معمول: تشخیص ← اعتراض ← ۲۳۸ ← بدوی ← اعتراض ← تجدیدنظر. مرجع بعدی را متناسب با پرونده انتخاب کنید. موعدها در کارتابل اقدام‌ها ثبت می‌شوند.");
        form.OnSave=()=>db.RecordStage(x.Id,(string)stage.SelectedItem,PersianDates.Parse(date.Text),notes.Text);
        if(form.ShowDialog()==true) Refresh();
    });
    private void AddTask_Click(object sender,RoutedEventArgs e) => Guard(()=>TaskEditor(null,null));
    private void CaseTask_Click(object sender,RoutedEventArgs e) => Guard(()=>TaskEditor(null,Selected<TaxCase>(CasesGrid).Id));
    private void EditTask_Click(object sender,RoutedEventArgs e) => Guard(()=>TaskEditor(Selected<FollowUpTask>(TasksGrid),null));
    private void EditTask_DoubleClick(object sender,MouseButtonEventArgs e) { if(TasksGrid.SelectedItem is FollowUpTask x) Guard(()=>TaskEditor(x,null)); }
    private void TaskEditor(FollowUpTask? x,long? caseId)
    {
        var cases=db.Cases().Where(c=>c.Stage!="بسته‌شده"||c.Id==x?.CaseId).ToList(); if(cases.Count==0) throw new InvalidOperationException("ابتدا یک پروندهٔ فعال ثبت کنید.");
        if(caseId.HasValue&&!cases.Any(c=>c.Id==caseId)) throw new InvalidOperationException("این پرونده بسته است. ابتدا با ثبت مرحله، آن را بازگشایی کنید.");
        var form=new EditorWindow(this,x==null?"اقدام و یادآوری جدید":"ویرایش اقدام","برای اعتراض، تهیهٔ لایحه، حضور در جلسه یا پیگیری نتیجه، اقدام جدا ثبت کنید.");
        var choice=form.Choice("پرونده *",cases,cases.FirstOrDefault(c=>c.Id==(x?.CaseId??caseId))??cases[0]);
        var title=form.Text("عنوان اقدام *",x?.Title??"");
        var due=form.Date("موعد اقدام",x?.DueAt??DateTime.Today.AddDays(1));
        var time=form.Text("ساعت موعد (۲۴ ساعته)",PersianDates.Digits((x?.DueAt??DateTime.Today.AddHours(9)).ToString("HH:mm")));
        var remind=form.Date("تاریخ یادآوری",x?.RemindAt??DateTime.Today);
        var remindTime=form.Text("ساعت یادآوری",PersianDates.Digits((x?.RemindAt??DateTime.Today.AddHours(9)).ToString("HH:mm")));
        var basis=form.Date("تاریخ مبنا برای محاسبهٔ اختیاری",DateTime.Today); form.Presets(due,remind,basis);
        form.Note("دکمه‌ها تعداد روز تقویمی را اضافه می‌کنند؛ تعطیلات محاسبه نمی‌شوند. موعد نهایی را مطابق پرونده بررسی و در صورت نیاز اصلاح کنید.");
        var notes=form.Text("یادداشت / شمارهٔ ابلاغ",x?.Notes??"",true);
        form.OnSave=()=>db.SaveTask(new FollowUpTask { Id=x?.Id??0,CaseId=(choice.SelectedItem as TaxCase)?.Id??0,Title=title.Text,DueAt=PersianDates.ParseTime(due.Text,time.Text),RemindAt=PersianDates.ParseTime(remind.Text,remindTime.Text),IsDone=x?.IsDone??false,Notes=notes.Text });
        if(form.ShowDialog()==true) { Refresh(); CheckAlarms(); }
    }
    private void CompleteTask_Click(object sender,RoutedEventArgs e) => Guard(()=>{ db.CompleteTask(Selected<FollowUpTask>(TasksGrid).Id); Refresh(); });
    private void SnoozeTask_Click(object sender,RoutedEventArgs e) => Guard(()=>{ db.Snooze(Selected<FollowUpTask>(TasksGrid).Id,DateTime.Now.AddMinutes(15)); Refresh(); });
    private void CheckAlarms()
    {
        TodayLabel.Text="امروز، "+PersianDates.Format(DateTime.Today);
        var due=db.DueReminders(DateTime.Now);
        if(alarm!=null) { alarm.Update(due); return; }
        if(due.Count==0) return;
        tray.ShowBalloonTip(6000,"یادآوری فالوآپ",$"{PersianDates.Digits(due.Count.ToString())} اقدام منتظر پیگیری است.",Forms.ToolTipIcon.Warning);
        System.Media.SystemSounds.Exclamation.Play();
        alarm=new ReminderWindow(db,due); alarm.Closed+=(_,_)=>{ alarm=null; Refresh(); }; alarm.Show();
    }
    private void Documents_Click(object sender,RoutedEventArgs e) => Guard(()=>{
        var x=Selected<TaxCase>(CasesGrid); var window=new Window { Owner=this,Title="مدارک — "+x.Title,Width=720,Height=460,FlowDirection=FlowDirection.RightToLeft,WindowStartupLocation=WindowStartupLocation.CenterOwner };
        var root=new DockPanel { Margin=new Thickness(20) }; var actions=new StackPanel { Orientation=Orientation.Horizontal,Margin=new Thickness(0,0,0,14) };
        var add=new Button { Content="+ افزودن مدرک" }; var open=new Button { Content="بازکردن",Style=(Style)FindResource("Secondary") }; actions.Children.Add(add); actions.Children.Add(open); DockPanel.SetDock(actions,Dock.Top); root.Children.Add(actions);
        var list=new DataGrid(); list.Columns.Add(new DataGridTextColumn { Header="نام سند",Binding=new System.Windows.Data.Binding("Name"),Width=new DataGridLength(1,DataGridLengthUnitType.Star) }); list.Columns.Add(new DataGridTextColumn { Header="تاریخ افزودن",Binding=new System.Windows.Data.Binding("AddedText"),Width=180 }); list.ItemsSource=db.Attachments(x.Id); root.Children.Add(list); window.Content=root;
        add.Click+=(_,_)=>Guard(()=>{ var picker=new OpenFileDialog { Title="انتخاب مدارک",Multiselect=true }; if(picker.ShowDialog(window)==true) { foreach(var f in picker.FileNames) db.AddAttachment(x.Id,f); list.ItemsSource=db.Attachments(x.Id); } });
        open.Click+=(_,_)=>Guard(()=>{ var a=Selected<Attachment>(list); Process.Start(new ProcessStartInfo(a.StoredPath) { UseShellExecute=true }); }); window.ShowDialog();
    });
    private void Startup_Click(object sender,RoutedEventArgs e) => Guard(()=>{
        using var key=Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
        if(StartupToggle.IsChecked==true) key.SetValue("FollowUp",$"\"{Environment.ProcessPath}\" --background"); else key.DeleteValue("FollowUp",false);
    });
    private void Backup_Click(object sender,RoutedEventArgs e) => Guard(()=>{
        var dialog=new SaveFileDialog { Filter="پشتیبان فالوآپ (*.zip)|*.zip",FileName="FollowUp-"+PersianDates.Latin(PersianDates.Format(DateTime.Today)).Replace('/','-')+".zip" };
        if(dialog.ShowDialog(this)!=true) return;
        var staging=Path.Combine(Path.GetTempPath(),"FollowUp-"+Guid.NewGuid()); Directory.CreateDirectory(staging);
        try {
            db.Backup(Path.Combine(staging,"followup.db")); var docs=Path.Combine(App.DataDirectory,"Documents");
            if(Directory.Exists(docs)) { Directory.CreateDirectory(Path.Combine(staging,"Documents")); foreach(var f in Directory.GetFiles(docs)) File.Copy(f,Path.Combine(staging,"Documents",Path.GetFileName(f))); }
            // Build separately so a failed backup never destroys an existing backup.
            var zip=Path.Combine(staging,"backup.zip");
            using(var archive=ZipFile.Open(zip,ZipArchiveMode.Create)) {
                archive.CreateEntryFromFile(Path.Combine(staging,"followup.db"),"followup.db");
                if(Directory.Exists(Path.Combine(staging,"Documents"))) foreach(var f in Directory.GetFiles(Path.Combine(staging,"Documents"))) archive.CreateEntryFromFile(f,"Documents/"+Path.GetFileName(f));
            }
            File.Copy(zip,dialog.FileName,true); MessageBox.Show(this,"پشتیبان اطلاعات و مدارک ذخیره شد.","فالوآپ");
        } finally { Directory.Delete(staging,true); }
    });
}
