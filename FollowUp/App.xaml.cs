using System.Threading;

namespace FollowUp;
public partial class App : Application
{
    private Mutex? single;
    public static string DataDirectory { get; } = Environment.GetEnvironmentVariable("FOLLOWUP_DATA_DIR") is string custom && !string.IsNullOrWhiteSpace(custom)
        ? Path.GetFullPath(custom) : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"FollowUp");
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        single=new Mutex(true,"Local\\FollowUp.TaxCases",out var first);
        if(!first) { MessageBox.Show("فالوآپ در حال اجراست؛ از آیکن کنار ساعت ویندوز آن را باز کنید.","فالوآپ"); Shutdown(); return; }
        Directory.CreateDirectory(DataDirectory);
        DispatcherUnhandledException += (_,args) => {
            File.AppendAllText(Path.Combine(DataDirectory,"errors.log"),DateTime.Now.ToString("O")+" "+args.Exception+Environment.NewLine);
            MessageBox.Show("عملیات انجام نشد. اطلاعات را بررسی کنید و دوباره تلاش کنید.\n"+args.Exception.Message,"فالوآپ",MessageBoxButton.OK,MessageBoxImage.Error);
            args.Handled=true;
        };
        try {
            var window=new MainWindow(new Repository(Path.Combine(DataDirectory,"followup.db")));
            MainWindow=window;
            if(!e.Args.Contains("--background")) window.Show();
        } catch(Exception ex) { MessageBox.Show("بازکردن اطلاعات ممکن نشد:\n"+ex.Message,"فالوآپ"); Shutdown(1); }
    }
    protected override void OnExit(ExitEventArgs e) { single?.Dispose(); base.OnExit(e); }
}
