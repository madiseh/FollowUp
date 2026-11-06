<div dir="rtl">

# فالوآپ — نسخهٔ ۱.۰.۲

<img src="FollowUp/Assets/FollowUp.png" alt="آیکن فالوآپ" width="96" />

برنامهٔ ویندوزی پیگیری پرونده‌های مالیاتی با **WPF** و پایگاه دادهٔ محلی **SQLite**؛ رابط فارسی و راست‌به‌چپ، فونت وزیر و تاریخ شمسی.

## دریافت و اجرا

از بخش [Releases](https://github.com/madiseh/FollowUp/releases) فایل `FollowUp-v1.0.2-win-x64.zip` را دانلود کنید، کل محتویات را در یک پوشهٔ ثابت استخراج و `FollowUp.exe` را اجرا کنید. نسخهٔ انتشار، مستقل از نصب .NET است و برای ویندوز ۶۴ بیتی تهیه می‌شود.

برنامه در حالت بزرگ‌شده باز می‌شود. دکمه‌های استاندارد ویندوز برای مینیمایز، بازگشت به اندازهٔ معمولی و تغییر اندازه در دسترس‌اند. شمارهٔ نسخه در عنوان پنجره و صفحهٔ اصلی دیده می‌شود.

## امکانات نسخهٔ اول

- ثبت و ویرایش شرکت و اطلاعات تماس.
- چندین پرونده برای هر شرکت، با موضوع، دوره، شمارهٔ پرونده و مسئول پیگیری.
- ثبت مراحل تشخیص، اعتراض، مادهٔ ۲۳۸، بدوی، تجدیدنظر و مراجع پیگیری بعدی؛ امکان تکرار مرحله، بازگشایی و حفظ تاریخچه.
- کارتابل اقدام‌ها برای اعتراض، لایحه، جلسه و پیگیری نتیجه؛ ثبت موعد و زمان یادآوری مستقل.
- پنجرهٔ آلارم، صدای سیستم و اعلان کنار ساعت ویندوز؛ انجام‌شده، تعویق ۱۵ دقیقه‌ای یا یک‌ساعته.
- ورود تاریخ شمسی با ارقام فارسی، عربی یا لاتین و نمایش تاریخ‌های برنامه به شمسی.
- جست‌وجوی شرکت، پرونده، موضوع، دوره، مسئول و اقدام؛ پشتیبانی از تفاوت حروف فارسی/عربی و نوع ارقام در جست‌وجو.
- پیوست مدارک و نگهداری کپی مستقل؛ بازکردن مدارک با برنامهٔ پیش‌فرض سیستم.
- پشتیبان ZIP از پایگاه داده و مدارک.
- اجرای اختیاری با ورود به ویندوز و ادامهٔ یادآوری پس از بستن پنجره.

## شروع کار

۱. در زبانهٔ «شرکت‌ها»، شرکت جدید ثبت کنید.

۲. در زبانهٔ «پرونده‌ها»، پرونده‌ای با موضوع و دوره بسازید.

۳. برای ثبت نتیجهٔ رسیدگی، پرونده را انتخاب و «ثبت مرحله / نتیجه» را بزنید. پذیرش یا توافق نهایی را در توضیحات بنویسید و در صورت پایان پیگیری، مرحلهٔ «بسته‌شده» را انتخاب کنید.

۴. «اقدام برای پرونده» را بزنید و عنوان، موعد شمسی، ساعت و زمان یادآوری را وارد کنید. برای هر جلسه یا اعتراض می‌توان اقدام جدا ساخت.

۵. در صورت نیاز «اجرا با ورود به ویندوز» را فعال کنید. برنامه را پس از فعال‌سازی این گزینه جابه‌جا نکنید؛ در صورت جابه‌جایی، گزینه را یک بار خاموش و روشن کنید.

بستن پنجره، برنامه را کنار ساعت ویندوز نگه می‌دارد. برای خروج کامل، روی آیکن کنار ساعت راست‌کلیک و «خروج کامل» را انتخاب کنید. آلارم به روشن‌بودن ویندوز و فعال‌بودن برنامه وابسته است؛ موعدهای ازدست‌رفته در شروع بعدی بررسی می‌شوند. اعلان ویندوز ممکن است تابع تنظیمات اعلان سیستم باشد؛ پنجرهٔ یادآوری برنامه جداست. بستن پنجرهٔ آلارم، موارد آن را ۱۵ دقیقه عقب می‌اندازد. پرونده‌های بسته یادآوری فعال ندارند.

### ثبت تاریخ و مهلت

تاریخ را مانند `۱۴۰۵/۰۸/۱۵` و ساعت را مانند `۰۹:۳۰` وارد کنید. تاریخ نامعتبر پذیرفته نمی‌شود. دکمه‌های ۵، ۲۰ و ۳۰ روز، روز تقویمی را به تاریخ مبنا اضافه می‌کنند؛ تعطیلات و استثناهای قانونی محاسبه نمی‌شوند. این‌ها ابزار ورود سریع‌اند. موعد نهایی و مرجع مناسب را مسئول پرونده تعیین می‌کند. نام مراجع و مسیر اولیه بر اساس نیاز اولیهٔ محصول ثبت شده‌اند و تأیید حقوقی محسوب نمی‌شوند.

### اطلاعات و پشتیبان

اطلاعات پیش‌فرض در `%LOCALAPPDATA%\FollowUp` شامل `followup.db` و پوشهٔ `Documents` ذخیره می‌شود. برای پوشهٔ دادهٔ جدا می‌توان متغیر محیطی `FOLLOWUP_DATA_DIR` را تعیین کرد.

برای پشتیبان، دکمهٔ «پشتیبان‌گیری» را بزنید. برای بازیابی، ابتدا از منوی کنار ساعت خروج کامل کنید؛ از پوشهٔ فعلی داده کپی بگیرید و سپس محتویات ZIP را در پوشهٔ داده استخراج کنید. مدارک به‌صورت قابل انتقال ذخیره می‌شوند. نسخهٔ اول برای استفادهٔ محلی یک کاربر طراحی شده و ورود کاربران و اشتراک شبکه‌ای پایگاه داده ندارد.

</div>

## Build and verify

Requires Windows and .NET SDK 10.

```powershell
dotnet build FollowUp.sln -c Release
dotnet run --project FollowUp.Checks -c Release
dotnet run --project FollowUp
```

`FollowUp.Checks` is a dependency-light executable behavior test suite. It covers Persian calendar boundaries, Persian/Arabic digit input, persistent data and stage history, reminder eligibility and snooze/completion, editable overdue reminders, SQLite snapshots, and portable document restoration. It returns a nonzero exit code on failure and uses isolated temporary databases.

Publish the Windows x64 app:

```powershell
dotnet publish FollowUp/FollowUp.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o dist/win-x64
```

### Project layout

- `FollowUp.Core`: data models, Persian date parsing/formatting and SQLite repository.
- `FollowUp`: WPF interface, system tray integration and reminder windows.
- `FollowUp.Checks`: behavior checks using real SQLite files.
- `scripts/New-AppIcon.ps1`: reproducible multi-size ICO/PNG generation from the app icon geometry.

### Assets

Vazir Regular, Medium and Bold **v30.1.0** are bundled as WPF resources, so no system font installation or network connection is needed. Source: [official Vazir repository, tag v30.1.0](https://github.com/rastikerdar/vazir-font/tree/v30.1.0). Copyright and SIL Open Font License are included in [OFL.txt](FollowUp/Assets/Fonts/OFL.txt). The app icon is provided as SVG, PNG and a seven-size Windows ICO.
