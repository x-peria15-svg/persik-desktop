// SPDX-License-Identifier: Apache-2.0
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
namespace Persik
{
    [DataContract] public class Pet
    {
        [DataMember] public int Version=2;
        [DataMember] public string Name="Персик";
        [DataMember] public double Food=76,Energy=84,Joy=82,Clean=88;
        [DataMember] public bool Sleeping,Muted,Pinned=true,Compact;
        [DataMember] public bool DarkTheme;
        [DataMember] public string Hat="",BowColor="rose";
        [DataMember] public string Room="living";
        [DataMember] public bool PauseRoaming,LightsOff;
        [DataMember] public bool InBathroom
        {
            get
            {
                return Room=="bath";
            }
            set
            {
                if(value)Room="bath";
                else if(Room=="bath")Room="living";
            }
        }
        [DataMember] public int Coins=80,XP,Feeds,Games,Strokes,Baths,BestScore;
        [DataMember] public long Saved=DateTime.UtcNow.Ticks,Born=DateTime.UtcNow.Ticks,LastGame;
        [DataMember] public string Day="",Theme="peach",Fur="ginger",Weather="sun";
        [DataMember] public int DayFeed,DayPlay,DayPet,Claimed;
        [DataMember] public List<string> Owned=new List<string>
        {
            "rug"
        };
        [DataMember] public List<string> Equipped=new List<string>
        {
            "rug"
        };
        [DataMember] public List<string> Badges=new List<string>();
        [DataMember] public List<string> Journal=new List<string>();
        public int Level
        {
            get
            {
                return 1+XP/100;
            }
        }
        public static double Clamp(double v)
        {
            return double.IsNaN(v)||double.IsInfinity(v)?75:Math.Max(0,Math.Min(100,v));
        }
        public void Normalize()
        {
            if(Array.IndexOf(House.Rooms,Room)<0)Room="living";
            Name=string.IsNullOrWhiteSpace(Name)?"Персик":Name.Substring(0,Math.Min(16,Name.Length));
            Food=Clamp(Food);
            Energy=Clamp(Energy);
            Joy=Clamp(Joy);
            Clean=Version<2?88:Clamp(Clean);
            Coins=Math.Max(0,Math.Min(999999,Coins));
            XP=Math.Max(0,Math.Min(999900,XP));
            if(Owned==null)Owned=new List<string>();
            if(Equipped==null)Equipped=new List<string>();
            if(Badges==null)Badges=new List<string>();
            if(Journal==null)Journal=new List<string>();
            if(Version<2)
            {
                Coins=80;
                Born=DateTime.UtcNow.Ticks;
                Version=2;
                if(!Owned.Contains("rug"))Owned.Add("rug");
                if(!Equipped.Contains("rug"))Equipped.Add("rug");
            }
            if(Born<=0||Born>DateTime.UtcNow.Ticks)Born=DateTime.UtcNow.Ticks;
            if(Saved<=0||Saved>DateTime.UtcNow.Ticks)Saved=DateTime.UtcNow.Ticks;
            if(LastGame<0||LastGame>DateTime.UtcNow.Ticks)LastGame=0;
            if(Theme!="peach"&&Theme!="sage"&&Theme!="lilac")Theme="peach";
            if(Array.IndexOf(new string[]
            {
                "ginger","cream","gray","chocolate","snow","calico"
            },Fur)<0)Fur="ginger";
            if(Array.IndexOf(new string[]
            {
                "rose","blue","gold","mint"
            },BowColor)<0)BowColor="rose";
            if(string.IsNullOrEmpty(Hat)||!Owned.Contains(Hat)||!Hat.StartsWith("hat_"))Hat="";
            if(Weather!="sun"&&Weather!="rain"&&Weather!="night")Weather="sun";
            NewDay();
        }
        public void NewDay()
        {
            string d=DateTime.Now.ToString("yyyy-MM-dd");
            if(Day!=d)
            {
                Day=d;
                DayFeed=DayPlay=DayPet=Claimed=0;
            }
        }
        public void Advance(double seconds)
        {
            seconds=Math.Max(0,Math.Min(28800,seconds));
            // Integrate across wake-up, so energy cannot stay full throughout a long absence.
            if(Sleeping)
            {
                double rest=Math.Min(seconds,(100-Energy)/0.10);
                Energy=Clamp(Energy+rest*.10);
                Food=Clamp(Food-rest*.006);
                Clean=Clamp(Clean-rest*.0015);
                Joy=Clamp(Joy-rest*.001);
                seconds-=rest;
                if(Energy>=99.999)
                {
                    Sleeping=false;
                    Energy=100;
                }
            }
            Food=Clamp(Food-seconds*.008);
            Energy=Clamp(Energy-seconds*.005);
            Clean=Clamp(Clean-seconds*.0025);
            Joy=Clamp(Joy-seconds*(Food<25||Clean<25?.010:.002));
        }
        public void Log(string s)
        {
            Journal.Insert(0,DateTime.Now.ToString("dd.MM HH:mm")+"|"+s);
            if(Journal.Count>40)Journal.RemoveAt(Journal.Count-1);
        }
        public string Award(string id,string title)
        {
            if(Badges.Contains(id))return "";
            Badges.Add(id);
            Coins+=15;
            Log("Открыто: «"+title+"». +15 монет");
            return "Новое достижение: "+title+"  +15";
        }
        public void Gain(int xp,int coins)
        {
            XP+=xp;
            Coins+=coins;
        }
        public bool Has(string id)
        {
            return id.StartsWith("hat_")?Hat==id:Equipped.Contains(id);
        }
    }
    public static class Store
    {
        public static string Folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"PixelCat");
        public static string Error="";
        public static string PathName
        {
            get
            {
                return Path.Combine(Folder,"pet.json");
            }
        }
        public static Pet Read()
        {
            if(!File.Exists(PathName))return Fresh();
            try
            {
                using(FileStream f=File.OpenRead(PathName))
                {
                    Pet p=(Pet)new DataContractJsonSerializer(typeof(Pet)).ReadObject(f);
                    if(p==null)throw new Exception();
                    p.Normalize();
                    p.Advance((DateTime.UtcNow-new DateTime(p.Saved,DateTimeKind.Utc)).TotalSeconds);
                    return p;
                }
            }
            catch
            {
                try
                {
                    File.Copy(PathName,PathName+".damaged-"+DateTime.Now.ToString("yyyyMMddHHmmss"),true);
                }
                catch
                {
                }
                Error="Сохранение повреждено. Создан новый питомец; старый файл оставлен в резервной копии.";
                return Fresh();
            }
        }
        static Pet Fresh()
        {
            Pet p=new Pet();
            p.Normalize();
            p.Log("Добро пожаловать домой! Начинается наша дружба.");
            return p;
        }
        public static bool Save(Pet p)
        {
            try
            {
                Directory.CreateDirectory(Folder);
                p.Saved=DateTime.UtcNow.Ticks;
                string t=PathName+".tmp";
                using(FileStream f=File.Create(t))new DataContractJsonSerializer(typeof(Pet)).WriteObject(f,p);
                if(File.Exists(PathName))File.Replace(t,PathName,null);
                else File.Move(t,PathName);
                Error="";
                return true;
            }
            catch(Exception)
            {
                Error="Не удалось сохранить. Проверь доступ к папке сохранений.";
                return false;
            }
        }
    }
    public class ShopItem
    {
        public string Id,Title,Description;
        public int Price;
        public ShopItem(string id,string title,string description,int price)
        {
            Id=id;
            Title=title;
            Description=description;
            Price=price;
        }
        public static ShopItem[] All=
        {
            new ShopItem("lights","Звёздная гирлянда","Маленькие огоньки под потолком",35),new ShopItem("rug","Ковёр с ромбами","Мягкий ковёр в цвет комнаты",30),new ShopItem("plant","Цветущий сад","Ещё больше зелени и цветов",40),new ShopItem("bow","Бантик для котика","Розовый бантик на ошейнике",25),new ShopItem("aquarium","Аквариум","Рыбка и пузырьки на комоде",60),new ShopItem("house","Кошачий домик","Свой маленький уголок у окна",75),new ShopItem("poster","Лунная картина","Месяц и звёзды в рамке",30),new ShopItem("pillow","Подушка-сердце","Мягкая подушка в лежанку",25),new ShopItem("flowers","Букет на окне","Розовые цветы на подоконнике",35),new ShopItem("mobile","Звёздный мобиль","Подвесные звёзды над лежанкой",45),new ShopItem("mouse","Игрушечная мышка","Маленькая игрушка на ковре",20),new ShopItem("tea","Чайный уголок","Кружка, печенье и книжки",40),new ShopItem("hat_beret","Малиновый берет","Мягкий берет для художника",35),new ShopItem("hat_wizard","Колпак волшебника","Фиолетовый колпак со звёздами",55),new ShopItem("hat_crown","Золотая корона","Для настоящего короля дома",75),new ShopItem("hat_beanie","Вязаная шапочка","Голубая шапка с помпоном",35),new ShopItem("hat_flower","Цветочный венок","Розовые цветочки и листья",40),new ShopItem("hat_straw","Соломенная шляпа","Летняя шляпа с ленточкой",45)
        };
    }
}
