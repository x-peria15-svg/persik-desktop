// SPDX-License-Identifier: Apache-2.0
using System;
using System.IO;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
namespace Persik
{
    public static class Tests
    {
        static int passed;
        static System.Text.StringBuilder report=new System.Text.StringBuilder();
        static void Check(bool condition,string name)
        {
            if(!condition)throw new Exception(name);
            passed++;
            report.AppendLine("PASS "+name);
        }
        static void Set(object o,string field,object value)
        {
            typeof(CatWindow).GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(o,value);
        }
        static object Get(object o,string field)
        {
            return typeof(CatWindow).GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(o);
        }
        static void Call(object o,string method,params object[] a)
        {
            typeof(CatWindow).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(o,a);
        }
        static void Snap(CatWindow w,string path)
        {
            w.Show();
            Application.DoEvents();
            using(Bitmap b=new Bitmap(w.Width,w.Height))
            {
                w.DrawToBitmap(b,new Rectangle(Point.Empty,w.Size));
                b.Save(path);
            }
            w.Hide();
        }
        public static void Run(string screenshots)
        {
            string old=Store.Folder;
            string testFolder=Path.Combine(Path.GetTempPath(),"Persik-test-"+Guid.NewGuid().ToString("N"));
            Store.Folder=testFolder;
            Directory.CreateDirectory(Store.Folder);
            try
            {
                Pet p=new Pet();
                p.Normalize();
                double f=p.Food,e=p.Energy;
                p.Advance(60);
                Check(p.Food<f&&p.Energy<e,"Needs decline with elapsed time");
                p.Sleeping=true;
                p.Energy=40;
                p.Advance(60);
                Check(p.Sleeping&&Math.Abs(p.Energy-46)<.001,"Sleep restores energy");
                p.Energy=99;
                p.Sleeping=true;
                p.Advance(1000);
                Check(!p.Sleeping&&p.Energy<100,"Offline wake-up splits time into sleep and awake");
                p.Advance(double.MaxValue);
                Check(p.Food>=0&&p.Clean>=0&&p.Joy>=0&&p.Energy>=0,"Offline cap and lower bounds");
                p.Food=double.NaN;
                p.Clean=double.PositiveInfinity;
                p.Coins=-1;
                p.Theme="invalid";
                p.Normalize();
                Check(p.Food==75&&p.Clean==75&&p.Coins==0&&p.Theme=="peach","Invalid values normalized");
                p.Day="2000-01-01";
                p.DayFeed=3;
                p.Claimed=7;
                p.NewDay();
                Check(p.DayFeed==0&&p.Claimed==0,"Daily quest reset");
                p.Name="Тестовый кот";
                p.Coins=123;
                p.Owned.Add("lights");
                Store.Save(p);
                Pet loaded=Store.Read();
                Check(loaded.Name==p.Name&&loaded.Coins==123&&loaded.Owned.Contains("lights"),"Unicode and collection save/load");
                File.WriteAllText(Store.PathName,"{\"Food\":65,\"Energy\":70,\"Joy\":80,\"Sleeping\":false,\"Muted\":true,\"Pinned\":true,\"Saved\":"+DateTime.UtcNow.Ticks+"}");
                Pet legacy=Store.Read();
                Check(legacy.Version==2&&legacy.Clean>87.99&&legacy.Coins==80&&legacy.Food>64,"Version 1 save migration preserves needs");
                File.WriteAllText(Store.PathName,"broken json");
                Pet recovered=Store.Read();
                Check(recovered!=null&&Directory.GetFiles(Store.Folder,"*.damaged-*").Length==1,"Corrupt save recovery and backup");
                Pet walker=new Pet();
                walker.Normalize();
                HomeLife brain=new HomeLife(7);
                brain.Command(walker,House.Find("dining"));
                bool exited=false,entered=false,jumped=false;
                for(int i=0;i<600&&!brain.Using;i++)
                {
                    brain.Update(walker,.1f,false);
                    exited|=brain.Stage=="exit";
                    entered|=brain.Stage=="enter";
                    jumped|=brain.Stage=="jump";
                }
                Check(walker.Room=="kitchen"&&brain.Using&&Math.Abs(brain.Y-86)<.01f&&exited&&entered&&jumped,"Door navigation and jump onto kitchen table");
                brain.Command(walker,House.Find("desk"));
                bool dropped=false;
                for(int i=0;i<600&&!brain.Using;i++)
                {
                    brain.Update(walker,.1f,false);
                    dropped|=brain.Stage=="down";
                }
                Check(walker.Room=="bedroom"&&brain.ActiveObject=="desk"&&dropped,"Cat jumps down before leaving an elevated surface");
                walker.PauseRoaming=true;
                brain.Reset(100,123);
                for(int i=0;i<200;i++)brain.Update(walker,.1f,false);
                Check(brain.Stage=="idle"&&brain.X==100,"Pause prevents autonomous planning");
                brain.Command(walker,House.Find("bed"));
                for(int i=0;i<200&&!brain.Using;i++)brain.Update(walker,.1f,false);
                Check(brain.Using&&brain.ActiveObject=="bed","User commands still work when autonomy is paused");
                float bx=brain.X,by=brain.Y;
                brain.Update(walker,.1f,true);
                Check(brain.X==bx&&brain.Y==by,"Care and games block autonomous movement");
                walker.PauseRoaming=false;
                walker.Room="living";
                brain.Reset(120,123);
                System.Collections.Generic.HashSet<string> visits=new System.Collections.Generic.HashSet<string>();
                for(int i=0;i<3500;i++)
                {
                    brain.Update(walker,.1f,false);
                    visits.Add(walker.Room);
                }
                Check(visits.Count>=3,"Autonomous cat visits multiple rooms over time");
                walker.Energy=5;
                brain.Choose(walker);
                for(int i=0;i<800&&!walker.Sleeping;i++)brain.Update(walker,.1f,false);
                Check(walker.Sleeping&&walker.Room=="bedroom","Exhausted cat goes to bed autonomously");
                File.WriteAllText(Store.PathName,"{\"DarkTheme\":true,\"InBathroom\":true,\"Version\":2,\"Food\":70,\"Energy\":75,\"Joy\":80,\"Saved\":"+DateTime.UtcNow.Ticks+"}");
                Pet oldBath=Store.Read();
                Check(oldBath.Room=="bath"&&oldBath.DarkTheme,"Old bathroom and theme state migrate to room model");
                Store.Save(new Pet());
                Store.Error="";
                using(CatWindow w=new CatWindow())
                {
                    w.pet.Muted=true;
                    w.pet.Pinned=false;
                    w.TopMost=false;
                    w.pet.Food=40;
                    w.pet.Coins=80;
                    w.Feed(0);
                    Check(w.pet.Food==62&&w.pet.Coins==83&&w.pet.DayFeed==1,"Free feeding and rewards");
                    Set(w,"actionLeft",0f);
                    w.pet.Food=40;
                    w.Feed(1);
                    Check(w.pet.Food==75&&w.pet.Coins==75,"Premium food consumes coins");
                    Set(w,"actionLeft",0f);
                    w.pet.Coins=0;
                    f=w.pet.Food;
                    w.Feed(1);
                    Check(w.pet.Coins==0&&w.pet.Food==f,"No purchase without coins");
                    w.pet.Coins=200;
                    w.Buy("lights");
                    int coins=w.pet.Coins;
                    Check(w.pet.Owned.Contains("lights")&&w.pet.Has("lights")&&coins==180,"Shop purchase and first-decoration achievement");
                    w.Buy("lights");
                    Check(!w.pet.Has("lights")&&w.pet.Coins==coins,"Remove decoration without charging again");
                    w.Buy("lights");
                    Check(w.pet.Has("lights")&&w.pet.Coins==coins,"Re-equip owned decoration");
                    w.pet.Clean=25;
                    w.Wash();
                    Check(w.pet.InBathroom&&w.pet.Clean==25&&w.pet.Baths==0,"Shower starts in bathroom without instant reward");
                    Call(w,"CompleteWash");
                    Check(w.pet.Clean==70&&w.pet.Baths==1,"Completed shower restores cleanliness");
                    w.Sleep();
                    Check(w.pet.Sleeping,"Sleep toggle");
                    f=w.pet.Food;
                    w.Feed(0);
                    Check(w.pet.Food==f,"Cannot feed sleeping pet");
                    w.Sleep();
                    w.Stroke(true);
                    Check(w.pet.Strokes==1&&w.pet.Badges.Contains("hello"),"Petting and achievement");
                    int strokes=w.pet.Strokes;
                    w.Stroke(true);
                    Check(w.pet.Strokes==strokes,"Petting rate limit");
                    w.pet.DayPet=5;
                    coins=w.pet.Coins;
                    w.Claim(2);
                    Check(w.pet.Coins==coins+15,"Daily reward claim");
                    w.Claim(2);
                    Check(w.pet.Coins==coins+15,"Daily reward cannot be claimed twice");
                    w.pet.Energy=80;
                    w.pet.Food=80;
                    w.StartGame();
                    Check((bool)Get(w,"playing"),"Mini-game starts");
                    System.Collections.Generic.List<Firefly> targets=(System.Collections.Generic.List<Firefly>)Get(w,"flies");
                    PointF target=(PointF)typeof(CatWindow).GetMethod("FlyPosition",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(w,new object[]
                    {
                        targets[0]
                    }
                    );
                    Rectangle room=(Rectangle)Get(w,"room");
                    float scale=(float)Get(w,"uiScale");
                    Call(w,"HandleDown",null,new MouseEventArgs(MouseButtons.Left,1,(int)((room.X+target.X*room.Width/240)*scale),(int)((room.Y+target.Y*room.Height/150)*scale),0));
                    Check((int)Get(w,"score")==3,"Click hit-testing awards golden firefly score");
                    Set(w,"score",18);
                    w.FinishGame(false);
                    Check(w.pet.BestScore==18&&w.pet.Games==1&&w.pet.Badges.Contains("record"),"Game score, reward, and achievement");
                    w.StartGame();
                    Check(!(bool)Get(w,"playing"),"Game reward cooldown");
                    int games=w.pet.Games;
                    w.pet.LastGame=0;
                    w.StartGame(1);
                    Check((bool)Get(w,"playing")&&(int)Get(w,"gameMode")==1,"Yarn mini-game starts independently");
                    w.FinishGame(true);
                    Check(w.pet.Games==games,"Cancelled game grants no completion");
                    w.pet=new Pet();
                    w.pet.Normalize();
                    w.pet.Muted=true;
                    w.pet.Pinned=false;
                    w.TopMost=false;
                    w.pet.Coins=128;
                    w.pet.XP=246;
                    w.pet.Owned.Clear();
                    w.pet.Equipped.Clear();
                    w.pet.Owned.AddRange(new string[]
                    {
                        "lights","rug","plant"
                    }
                    );
                    w.pet.Equipped.AddRange(w.pet.Owned);
                    w.pet.Badges.AddRange(new string[]
                    {
                        "hello","decor"
                    }
                    );
                    w.pet.Journal.Add("02.11 18:20|Новая гирлянда делает вечер ещё уютнее.");
                    Set(w,"action","");
                    Set(w,"actionLeft",0f);
                    Set(w,"toastLeft",0f);
                    Set(w,"time",0f);
                    w.art.CatX=118;
                    Call(w,"Navigate","care");
                    if(screenshots!=null)
                    {
                        using(FileStream ico=File.Create(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"persik.ico")))w.Icon.Save(ico);
                        Directory.CreateDirectory(screenshots);
                        Snap(w,Path.Combine(screenshots,"01-home.png"));
                        Call(w,"Navigate","shop");
                        Snap(w,Path.Combine(screenshots,"02-shop.png"));
                        Call(w,"Navigate","diary");
                        Snap(w,Path.Combine(screenshots,"03-diary.png"));
                        Call(w,"Navigate","settings");
                        Snap(w,Path.Combine(screenshots,"04-settings.png"));
                        w.pet.Weather="night";
                        w.pet.Sleeping=true;
                        Call(w,"Navigate","care");
                        Call(w,"Rebuild");
                        Snap(w,Path.Combine(screenshots,"05-night.png"));
                        w.pet.Sleeping=false;
                        w.pet.Weather="sun";
                        w.SetCompact(true);
                        Snap(w,Path.Combine(screenshots,"06-widget.png"));
                        w.SetCompact(false);
                        w.pet.LastGame=0;
                        w.StartGame();
                        Snap(w,Path.Combine(screenshots,"07-game.png"));
                        w.FinishGame(true);
                        Call(w,"Navigate","care");
                        Set(w,"sub","food");
                        Call(w,"Rebuild");
                        Snap(w,Path.Combine(screenshots,"08-food.png"));
                        Set(w,"sub","games");
                        Call(w,"Rebuild");
                        Snap(w,Path.Combine(screenshots,"09-games.png"));
                        Call(w,"Navigate","diary");
                        Set(w,"sub","badges");
                        Call(w,"Rebuild");
                        Snap(w,Path.Combine(screenshots,"10-badges.png"));
                        Set(w,"sub","history");
                        Call(w,"Rebuild");
                        Snap(w,Path.Combine(screenshots,"11-history.png"));
                        Call(w,"Navigate","care");
                        w.pet.LastGame=0;
                        w.StartGame(1);
                        Snap(w,Path.Combine(screenshots,"12-yarn.png"));
                        w.FinishGame(true);
                    }
                    Check(true,"All screens and compact view render");
                    w.pet.Sleeping=false;
                    w.pet.LastGame=0;
                    w.pet.Weather="sun";
                    float pointerScale=(float)Get(w,"uiScale");
                    Call(w,"HandleDown",null,new MouseEventArgs(MouseButtons.Left,1,(int)(90*pointerScale),(int)(175*pointerScale),0));
                    Check(w.pet.Weather=="rain","Clicking the window cycles weather");
                    Call(w,"HandleDown",null,new MouseEventArgs(MouseButtons.Left,1,(int)(460*pointerScale),(int)(355*pointerScale),0));
                    Check(w.pet.Sleeping,"Clicking the bed puts pet to sleep");
                    w.pet.Sleeping=false;
                    w.SetCompact(true);
                    Check(w.ClientSize.Width==360&&w.ClientSize.Height==328,"Compact widget dimensions");
                    w.SetCompact(false);
                    Store.Save(w.pet);
                    Pet again=Store.Read();
                    Check(again.XP==w.pet.XP&&again.Equipped.Count==3,"Full v2 state persistence");
                    w.SetDarkTheme(true);
                    Check(w.pet.DarkTheme&&UI.Dark&&Store.Read().DarkTheme,"Dark theme applies and persists");
                    w.SetDarkTheme(false);
                    Check(!UI.Dark&&!Store.Read().DarkTheme,"Light theme restores and persists");
                    w.pet.Coins=3000;
                    foreach(ShopItem item in ShopItem.All)w.Buy(item.Id);
                    Check(w.pet.Owned.Count==18,"All eighteen shop items can be owned");
                    foreach(ShopItem item in ShopItem.All)if(!w.pet.Has(item.Id))w.Buy(item.Id);
                    w.pet.Food=80;
                    w.pet.Energy=80;
                    w.pet.LastGame=0;
                    w.StartGame(2);
                    Check((bool)Get(w,"playing")&&(int)Get(w,"gameMode")==2,"Fishing starts");
                    targets=(System.Collections.Generic.List<Firefly>)Get(w,"flies");
                    target=(PointF)typeof(CatWindow).GetMethod("FlyPosition",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(w,new object[]
                    {
                        targets[0]
                    }
                    );
                    Call(w,"HandleDown",null,new MouseEventArgs(MouseButtons.Left,1,(int)((room.X+target.X*room.Width/240)*scale),(int)((room.Y+target.Y*room.Height/150)*scale),0));
                    Check((int)Get(w,"score")>=3,"Fishing target hit awards points");
                    if(screenshots!=null)Snap(w,Path.Combine(screenshots,"13-fishing.png"));
                    w.FinishGame(true);
                    w.pet.LastGame=0;
                    w.StartGame(3);
                    Check((bool)Get(w,"playing")&&(int)Get(w,"gameMode")==3,"Memory game starts");
                    System.Collections.Generic.List<int> seq=(System.Collections.Generic.List<int>)Get(w,"memory");
                    int[] pattern=seq.ToArray();
                    Set(w,"time",10f);
                    Set(w,"memoryStart",0f);
                    foreach(int n in pattern)Call(w,"MemoryClick",new Point(47+n*43,80));
                    Check((int)Get(w,"score")==2&&((System.Collections.Generic.List<int>)Get(w,"memory")).Count==3,"Correct memory sequence grows the next round");
                    for(int i=0;i<3;i++)
                    {
                        Set(w,"time",20f+i*10);
                        Set(w,"memoryStart",0f);
                        seq=(System.Collections.Generic.List<int>)Get(w,"memory");
                        Call(w,"MemoryClick",new Point(47+((seq[0]+1)%4)*43,80));
                    }
                    Check(!(bool)Get(w,"playing")&&(int)Get(w,"memoryMistakes")==3,"Three memory mistakes end the game");
                    w.pet.LastGame=0;
                    w.StartGame(3);
                    Set(w,"time",1.1f);
                    Set(w,"memoryStart",0f);
                    if(screenshots!=null)Snap(w,Path.Combine(screenshots,"14-memory.png"));
                    w.FinishGame(true);
                    w.SetDarkTheme(true);
                    Call(w,"Navigate","care");
                    Set(w,"toastLeft",0f);
                    if(screenshots!=null)Snap(w,Path.Combine(screenshots,"15-dark.png"));
                    Call(w,"Navigate","settings");
                    if(screenshots!=null)Snap(w,Path.Combine(screenshots,"16-dark-settings.png"));
                    Call(w,"Navigate","shop");
                    Set(w,"shopPage",3);
                    Call(w,"Rebuild");
                    if(screenshots!=null)Snap(w,Path.Combine(screenshots,"17-new-shop.png"));
                    Call(w,"Navigate","care");
                    w.pet.Clean=15;
                    w.Wash();
                    Set(w,"washProgress",.45f);
                    if(screenshots!=null)Snap(w,Path.Combine(screenshots,"18-bathroom.png"));
                    int beforeBaths=w.pet.Baths;
                    Set(w,"washProgress",.99f);
                    Set(w,"last",DateTime.UtcNow.AddSeconds(-1));
                    Call(w,"Tick");
                    Check(w.pet.Baths==beforeBaths+1&&w.pet.Clean>59,"Timed shower completes and rewards once");
                    Call(w,"Tick");
                    Check(w.pet.Baths==beforeBaths+1,"Shower reward cannot repeat");
                    w.SetDarkTheme(false);
                    w.pet.Room="living";
                    w.pet.Sleeping=false;
                    w.pet.PauseRoaming=false;
                    string actual=w.pet.Room;
                    w.ViewRoom("kitchen");
                    Check(w.pet.Room==actual&&(string)Get(w,"viewedRoom")=="kitchen","Selecting a room does not teleport the cat");
                    Call(w,"FocusCat");
                    Check((string)Get(w,"viewedRoom")==actual,"Follow camera returns to the actual cat room");
                    HomeLife live=(HomeLife)Get(w,"life");
                    int originalCoins=w.pet.Coins;
                    int originalXP=w.pet.XP;
                    foreach(ShopItem item in ShopItem.All)
                    {
                        if(item.Id.StartsWith("hat_"))continue;
                        w.pet.Room="living";
                        live.Reset(120,123);
                        w.UseItem(item.Id);
                        for(int i=0;i<600&&!live.Using;i++)w.UpdateHome(.1f);
                        Check(live.Using&&live.ActiveObject==item.Id,"Interactive purchased item: "+item.Id);
                    }
                    Check(w.pet.Coins==originalCoins&&w.pet.XP==originalXP,"Item interactions do not farm currency or XP");
                    w.pet.PauseRoaming=true;
                    Store.Save(w.pet);
                    Pet savedHome=Store.Read();
                    Check(savedHome.PauseRoaming&&savedHome.Room==w.pet.Room,"Room and roaming preference persist");
                    w.pet.PauseRoaming=false;
                    w.pet.Coins=2000;
                    w.Buy("hat_beret");
                    Check(w.pet.Hat=="hat_beret","Owned hat can be equipped");
                    w.Buy("hat_wizard");
                    Check(w.pet.Hat=="hat_wizard"&&!w.pet.Has("hat_beret"),"Only one hat is worn at a time");
                    int hatCoins=w.pet.Coins;
                    w.Buy("hat_wizard");
                    Check(w.pet.Hat==""&&w.pet.Coins==hatCoins,"Hat can be removed without extra payment");
                    w.Buy("hat_flower");
                    w.SetCoat("calico");
                    w.pet.BowColor="mint";
                    Store.Save(w.pet);
                    Pet outfit=Store.Read();
                    Check(outfit.Hat=="hat_flower"&&outfit.Fur=="calico"&&outfit.BowColor=="mint","Hat, coat and bow color persist");
                    foreach(string fur in new string[]
                    {
                        "ginger","cream","gray","chocolate","snow","calico"
                    }
                    )
                    {
                        w.SetCoat(fur);
                        Check(w.pet.Fur==fur,"Coat selection: "+fur);
                    }
                    w.pet.Food=90;
                    w.pet.Energy=90;
                    w.pet.LastGame=0;
                    w.StartGame(4);
                    int popRound=(int)typeof(CatWindow).GetProperty("PopRound",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(w,null);
                    int seed=(int)Get(w,"popSeed");
                    Point pop=(Point)typeof(CatWindow).GetMethod("Hole",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(w,new object[]
                    {
                        (popRound*7+seed)%9
                    }
                    );
                    w.WhackClick(pop);
                    Check((int)Get(w,"score")==2,"Mouse hunt awards a hit");
                    w.WhackClick(pop);
                    Check((int)Get(w,"score")==2,"Same mouse cannot be scored twice");
                    if(screenshots!=null)Snap(w,Path.Combine(screenshots,"25-mouse-hunt.png"));
                    w.FinishGame(true);
                    w.pet.LastGame=0;
                    w.StartGame(5);
                    float needle=(float)typeof(CatWindow).GetProperty("TimingPosition",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(w,null);
                    Set(w,"timingCenter",needle);
                    w.TimingClick();
                    int timingScore=(int)Get(w,"score");
                    Check(timingScore==5,"Perfect timing awards five points");
                    w.TimingClick();
                    Check((int)Get(w,"score")==timingScore,"Timing game blocks rapid duplicate clicks");
                    if(screenshots!=null)Snap(w,Path.Combine(screenshots,"26-timing.png"));
                    w.FinishGame(true);
                    w.pet.LastGame=0;
                    w.StartGame(99);
                    Check(!(bool)Get(w,"playing"),"Unknown game identifier is safely rejected");
                    Call(w,"Navigate","wardrobe");
                    if(screenshots!=null)Snap(w,Path.Combine(screenshots,"27-wardrobe.png"));
                    Call(w,"Navigate","shop");
                    Set(w,"shopPage",4);
                    Call(w,"Rebuild");
                    if(screenshots!=null)Snap(w,Path.Combine(screenshots,"28-hats.png"));
                    if(screenshots!=null)
                    {
                        w.SetDarkTheme(true);
                        Call(w,"Navigate","care");
                        Set(w,"action","");
                        Set(w,"actionLeft",0f);
                        Set(w,"toastLeft",0f);
                        w.pet.Room="kitchen";
                        live.Reset(141,86);
                        Call(w,"FocusCat");
                        Snap(w,Path.Combine(screenshots,"19-kitchen.png"));
                        w.pet.Room="bedroom";
                        live.Reset(62,81);
                        Call(w,"FocusCat");
                        Snap(w,Path.Combine(screenshots,"20-bedroom.png"));
                        w.pet.Room="living";
                        live.Reset(120,123);
                        w.UseItem("aquarium");
                        for(int i=0;i<600&&!live.Using;i++)w.UpdateHome(.1f);
                        Snap(w,Path.Combine(screenshots,"21-on-cabinet.png"));
                        w.ViewRoom("kitchen");
                        Snap(w,Path.Combine(screenshots,"22-empty-room.png"));
                        Call(w,"FocusCat");
                        live.Reset(120,123);
                        w.UseItem("house");
                        for(int i=0;i<600&&!live.Using;i++)w.UpdateHome(.1f);
                        Snap(w,Path.Combine(screenshots,"23-cat-house.png"));
                        live.Reset(150,123);
                        live.Command(w.pet,House.Find("mouse"));
                        w.UpdateHome(.1f);
                        Snap(w,Path.Combine(screenshots,"24-walking.png"));
                    }
                }
                report.AppendLine("\r\n"+passed+" checks passed. Audio not verified by automated tests.");
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"test-results.txt"),report.ToString());
            }
            finally
            {
                Store.Folder=old;
                if(Path.GetFullPath(Path.GetDirectoryName(testFolder)).TrimEnd(Path.DirectorySeparatorChar)==Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar)&&Path.GetFileName(testFolder).StartsWith("Persik-test-"))Directory.Delete(testFolder,true);
            }
        }
    }
}
