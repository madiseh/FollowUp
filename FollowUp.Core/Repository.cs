using Microsoft.Data.Sqlite;
using System.Globalization;

namespace FollowUp.Core;

public sealed class Repository
{
    public string DatabasePath { get; }
    public Repository(string path)
    {
        DatabasePath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);
        using var c = Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = """
            PRAGMA journal_mode=WAL;
            CREATE TABLE IF NOT EXISTS Companies(Id INTEGER PRIMARY KEY, Name TEXT NOT NULL, NationalId TEXT NOT NULL, Contact TEXT NOT NULL, Notes TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS Cases(Id INTEGER PRIMARY KEY, CompanyId INTEGER NOT NULL REFERENCES Companies(Id), Title TEXT NOT NULL, Subject TEXT NOT NULL, Period TEXT NOT NULL, FileNumber TEXT NOT NULL, Stage TEXT NOT NULL, Owner TEXT NOT NULL, Notes TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS History(Id INTEGER PRIMARY KEY, CaseId INTEGER NOT NULL REFERENCES Cases(Id), Stage TEXT NOT NULL, EventDate TEXT NOT NULL, Notes TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS Tasks(Id INTEGER PRIMARY KEY, CaseId INTEGER NOT NULL REFERENCES Cases(Id), Title TEXT NOT NULL, DueAt TEXT NOT NULL, RemindAt TEXT NOT NULL, IsDone INTEGER NOT NULL DEFAULT 0, Notes TEXT NOT NULL);
            CREATE TABLE IF NOT EXISTS Attachments(Id INTEGER PRIMARY KEY, CaseId INTEGER NOT NULL REFERENCES Cases(Id), Name TEXT NOT NULL, StoredPath TEXT NOT NULL, AddedAt TEXT NOT NULL);
            CREATE INDEX IF NOT EXISTS IX_Tasks_Reminder ON Tasks(IsDone,RemindAt);
            CREATE INDEX IF NOT EXISTS IX_Cases_Company ON Cases(CompanyId);
            CREATE INDEX IF NOT EXISTS IX_History_Case ON History(CaseId,Id);
            """;
        cmd.ExecuteNonQuery();
    }
    private SqliteConnection Open()
    {
        var c = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource=DatabasePath, ForeignKeys=true }.ToString());
        c.Open(); return c;
    }
    private static string Iso(DateTime d) => d.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    private static DateTime Date(string s) => DateTime.ParseExact(s, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
    private static SqliteCommand Command(SqliteConnection c, string sql, params object[] values)
    {
        var cmd = c.CreateCommand(); cmd.CommandText = sql;
        for (var i=0; i<values.Length; i++) cmd.Parameters.AddWithValue("@p"+i, values[i]);
        return cmd;
    }
    private static void Required(string value, string name) { if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{name} را وارد کنید."); }
    public List<Company> Companies()
    {
        using var c = Open(); using var cmd = Command(c, "SELECT * FROM Companies ORDER BY Name"); using var r = cmd.ExecuteReader();
        var list = new List<Company>();
        while(r.Read()) list.Add(new Company { Id=r.GetInt64(0), Name=r.GetString(1), NationalId=r.GetString(2), Contact=r.GetString(3), Notes=r.GetString(4) });
        return list;
    }
    public long SaveCompany(Company x)
    {
        Required(x.Name,"نام شرکت");
        using var c = Open(); using var cmd = Command(c, x.Id==0
            ? "INSERT INTO Companies(Name,NationalId,Contact,Notes) VALUES(@p0,@p1,@p2,@p3); SELECT last_insert_rowid();"
            : "UPDATE Companies SET Name=@p0,NationalId=@p1,Contact=@p2,Notes=@p3 WHERE Id=@p4; SELECT @p4;", x.Name.Trim(), x.NationalId.Trim(), x.Contact.Trim(), x.Notes.Trim(), x.Id);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }
    public List<TaxCase> Cases()
    {
        using var c = Open(); using var cmd = Command(c,"SELECT c.*,co.Name FROM Cases c JOIN Companies co ON co.Id=c.CompanyId ORDER BY c.Id DESC"); using var r = cmd.ExecuteReader();
        var list=new List<TaxCase>();
        while(r.Read()) list.Add(new TaxCase { Id=r.GetInt64(0), CompanyId=r.GetInt64(1), Title=r.GetString(2), Subject=r.GetString(3), Period=r.GetString(4), FileNumber=r.GetString(5), Stage=r.GetString(6), Owner=r.GetString(7), Notes=r.GetString(8), CompanyName=r.GetString(9) });
        return list;
    }
    public long SaveCase(TaxCase x)
    {
        Required(x.Title,"عنوان پرونده"); Required(x.Subject,"موضوع");
        if(x.CompanyId<=0) throw new ArgumentException("شرکت را انتخاب کنید.");
        using var c=Open(); using var cmd=Command(c,x.Id==0
            ? "INSERT INTO Cases(CompanyId,Title,Subject,Period,FileNumber,Stage,Owner,Notes) VALUES(@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7); SELECT last_insert_rowid();"
            : "UPDATE Cases SET CompanyId=@p0,Title=@p1,Subject=@p2,Period=@p3,FileNumber=@p4,Owner=@p6,Notes=@p7 WHERE Id=@p8; SELECT @p8;",x.CompanyId,x.Title.Trim(),x.Subject,x.Period.Trim(),x.FileNumber.Trim(),x.Stage,x.Owner.Trim(),x.Notes.Trim(),x.Id);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }
    public void RecordStage(long id,string stage,DateTime date,string notes)
    {
        if(!Workflow.Stages.Contains(stage)) throw new ArgumentException("مرحله معتبر نیست.");
        using var c=Open(); using var tx=c.BeginTransaction();
        using var update=Command(c,"UPDATE Cases SET Stage=@p0 WHERE Id=@p1",stage,id); update.Transaction=tx;
        if(update.ExecuteNonQuery()!=1) throw new ArgumentException("پرونده پیدا نشد.");
        using var add=Command(c,"INSERT INTO History(CaseId,Stage,EventDate,Notes) VALUES(@p0,@p1,@p2,@p3)",id,stage,Iso(date),notes.Trim()); add.Transaction=tx; add.ExecuteNonQuery(); tx.Commit();
    }
    public List<StageEntry> History(long id)
    {
        using var c=Open(); using var cmd=Command(c,"SELECT Id,Stage,EventDate,Notes FROM History WHERE CaseId=@p0 ORDER BY Id DESC",id); using var r=cmd.ExecuteReader();
        var list=new List<StageEntry>(); while(r.Read()) list.Add(new(r.GetInt64(0),r.GetString(1),Date(r.GetString(2)),r.GetString(3))); return list;
    }
    public long SaveTask(FollowUpTask x)
    {
        Required(x.Title,"عنوان اقدام");
        if(x.CaseId<=0) throw new ArgumentException("پرونده را انتخاب کنید.");
        using var c=Open(); using var cmd=Command(c,x.Id==0
            ? "INSERT INTO Tasks(CaseId,Title,DueAt,RemindAt,IsDone,Notes) VALUES(@p0,@p1,@p2,@p3,@p4,@p5); SELECT last_insert_rowid();"
            : "UPDATE Tasks SET CaseId=@p0,Title=@p1,DueAt=@p2,RemindAt=@p3,IsDone=@p4,Notes=@p5 WHERE Id=@p6; SELECT @p6;",x.CaseId,x.Title.Trim(),Iso(x.DueAt),Iso(x.RemindAt),x.IsDone?1:0,x.Notes.Trim(),x.Id);
        return Convert.ToInt64(cmd.ExecuteScalar());
    }
    public List<FollowUpTask> Tasks()
    {
        using var c=Open(); using var cmd=Command(c,"SELECT t.*,c.Title,co.Name,c.Stage FROM Tasks t JOIN Cases c ON c.Id=t.CaseId JOIN Companies co ON co.Id=c.CompanyId ORDER BY t.IsDone,t.DueAt"); using var r=cmd.ExecuteReader();
        var list=new List<FollowUpTask>();
        while(r.Read()) list.Add(new FollowUpTask { Id=r.GetInt64(0),CaseId=r.GetInt64(1),Title=r.GetString(2),DueAt=Date(r.GetString(3)),RemindAt=Date(r.GetString(4)),IsDone=r.GetInt32(5)!=0,Notes=r.GetString(6),CaseTitle=r.GetString(7),CompanyName=r.GetString(8),IsClosed=r.GetString(9)=="بسته‌شده" });
        return list;
    }
    public List<FollowUpTask> DueReminders(DateTime now) => Tasks().Where(t=>!t.IsDone&&!t.IsClosed&&t.RemindAt<=now).ToList();
    public void CompleteTask(long id) { using var c=Open(); using var cmd=Command(c,"UPDATE Tasks SET IsDone=1 WHERE Id=@p0",id); cmd.ExecuteNonQuery(); }
    public void Snooze(long id,DateTime until) { using var c=Open(); using var cmd=Command(c,"UPDATE Tasks SET RemindAt=@p0 WHERE Id=@p1",Iso(until),id); cmd.ExecuteNonQuery(); }
    public List<Attachment> Attachments(long id)
    {
        using var c=Open(); using var cmd=Command(c,"SELECT Id,Name,StoredPath,AddedAt FROM Attachments WHERE CaseId=@p0 ORDER BY Id DESC",id); using var r=cmd.ExecuteReader();
        var list=new List<Attachment>();
        while(r.Read()) {
            var stored=r.GetString(2);
            // Older databases may contain an absolute path. Resolve the managed copy
            // by filename so restored backups work in another profile as well.
            var resolved=Path.Combine(Path.GetDirectoryName(DatabasePath)!,"Documents",Path.GetFileName(stored));
            list.Add(new(r.GetInt64(0),r.GetString(1),resolved,Date(r.GetString(3))));
        }
        return list;
    }
    public void AddAttachment(long id,string source)
    {
        var dir=Path.Combine(Path.GetDirectoryName(DatabasePath)!,"Documents"); Directory.CreateDirectory(dir);
        var target=Path.Combine(dir,Guid.NewGuid()+Path.GetExtension(source)); File.Copy(source,target);
        try { using var c=Open(); using var cmd=Command(c,"INSERT INTO Attachments(CaseId,Name,StoredPath,AddedAt) VALUES(@p0,@p1,@p2,@p3)",id,Path.GetFileName(source),Path.GetFileName(target),Iso(DateTime.Now)); cmd.ExecuteNonQuery(); }
        catch { File.Delete(target); throw; }
    }
    public void Backup(string destination)
    {
        if(Path.GetFullPath(destination).Equals(DatabasePath,StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("فایل پشتیبان باید با پایگاه دادهٔ اصلی متفاوت باشد.");
        using var source=Open(); using var target=new SqliteConnection(new SqliteConnectionStringBuilder { DataSource=destination }.ToString()); target.Open(); source.BackupDatabase(target);
    }
}
