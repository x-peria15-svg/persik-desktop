// SPDX-License-Identifier: Apache-2.0
using System;
using System.Collections.Generic;
using System.Drawing;
namespace Persik
{
    public class HomeSpot
    {
        public string Id,Room,Label;
        public float X,Y;
        public Rectangle Hit;
        public HomeSpot(string id,string room,float x,float y,Rectangle hit,string label)
        {
            Id=id;
            Room=room;
            X=x;
            Y=y;
            Hit=hit;
            Label=label;
        }
    }
    // All positions use the room's 240 x 150 pixel grid. Navigation and hit-testing share this map.
    public static class House
    {
        public static string[] Rooms=
        {
            "living","bath","kitchen","bedroom"
        };
        public static string Name(string room)
        {
            int n=Array.IndexOf(Rooms,room);
            return n<0?"Домик":new string[]
            {
                "Домик","Ванная","Кухня","Спальня"
            }
            [n];
        }
        public static HomeSpot[] Spots=
        {
            new HomeSpot("table","living",170,73,new Rectangle(146,74,48,27),"Забрался на комод"), new HomeSpot("window","living",69,87,new Rectangle(20,20,78,72),"Наблюдает за птичками"), new HomeSpot("lights","living",207,122,new Rectangle(5,3,230,15),"Любуется огоньками"), new HomeSpot("rug","living",115,130,new Rectangle(64,122,106,22),"Катается на мягком ковре"), new HomeSpot("plant","living",197,121,new Rectangle(190,65,19,45),"Нюхает цветочки"), new HomeSpot("bow","living",120,123,new Rectangle(0,0,0,0),"Красуется новым бантиком"), new HomeSpot("aquarium","living",167,73,new Rectangle(148,49,34,24),"Следит за рыбкой с комода"), new HomeSpot("house","living",27,126,new Rectangle(9,93,36,40),"Прячется в своём домике"), new HomeSpot("poster","living",124,122,new Rectangle(113,51,26,29),"Разглядывает лунную картину"), new HomeSpot("pillow","living",201,117,new Rectangle(190,112,27,17),"Обнимает подушку-сердце"), new HomeSpot("flowers","living",71,87,new Rectangle(65,65,15,23),"Нюхает букет у окна"), new HomeSpot("mobile","living",204,122,new Rectangle(185,40,30,35),"Ловит подвесные звёздочки"), new HomeSpot("mouse","living",83,135,new Rectangle(74,130,20,13),"Играет с игрушечной мышкой"), new HomeSpot("tea","living",123,88,new Rectangle(112,76,23,22),"Устроился рядом с книжками"), new HomeSpot("dining","kitchen",141,86,new Rectangle(87,83,96,33),"Забрался на кухонный стол"), new HomeSpot("kitchenwindow","kitchen",57,76,new Rectangle(20,24,57,53),"Смотрит из кухонного окна"), new HomeSpot("fridge","kitchen",207,126,new Rectangle(194,31,31,75),"Проверяет, где хранят вкусняшки"), new HomeSpot("bed","bedroom",161,105,new Rectangle(102,98,110,34),"Отдыхает на большой кровати"), new HomeSpot("desk","bedroom",62,81,new Rectangle(28,81,57,30),"Забрался на письменный стол"), new HomeSpot("bedlamp","bedroom",204,77,new Rectangle(189,52,29,45),"Греется у ночника"), new HomeSpot("mirror","bath",42,87,new Rectangle(16,21,47,68),"Разглядывает себя в зеркале"), new HomeSpot("towel","bath",200,123,new Rectangle(192,37,36,39),"Трогает мягкое полотенце")
        };
        public static HomeSpot Find(string id)
        {
            return Array.Find(Spots,delegate(HomeSpot s)
            {
                return s.Id==id;
            }
            );
        }
        public static bool IsBought(string id)
        {
            return Array.Exists(ShopItem.All,delegate(ShopItem s)
            {
                return s.Id==id;
            }
            );
        }
        public static HomeSpot At(string room,Point pt,Pet p)
        {
            // Smaller purchased objects have priority over the furniture behind them.
            foreach(HomeSpot s in Spots)if(s.Room==room&&IsBought(s.Id)&&p.Has(s.Id)&&s.Hit.Contains(pt))return s;
            foreach(HomeSpot s in Spots)if(s.Room==room&&!IsBought(s.Id)&&s.Hit.Contains(pt))return s;
            return null;
        }
    }
    public class HomeLife
    {
        public float X=120,Y=123;
        public string Stage="idle",Activity="Осматривается",ActiveObject="";
        public float Phase;
        public bool FacingLeft;
        HomeSpot goal;
        string targetRoom;
        float elapsed,idleLeft=5,startY;
        int turns;
        Random random;
        public HomeLife(int seed)
        {
            random=new Random(seed);
        }
        public bool Using
        {
            get
            {
                return Stage=="use";
            }
        }
        public void Reset(float x,float y)
        {
            X=x;
            Y=y;
            Stage="idle";
            ActiveObject="";
            goal=null;
            idleLeft=7;
            elapsed=0;
            Activity="Осматривается";
        }
        public void Command(Pet p,HomeSpot spot)
        {
            goal=spot;
            targetRoom=spot.Room;
            ActiveObject="";
            elapsed=0;
            startY=Y;
            Stage=Y<112?"down":"route";
            Activity="Идёт: "+spot.Label.ToLowerInvariant();
        }
        public void Visit(Pet p,string room)
        {
            Command(p,new HomeSpot("visit",room,150,123,Rectangle.Empty,"Осматривается в комнате"));
        }
        public void Choose(Pet p)
        {
            turns++;
            if(p.Energy<12)
            {
                Command(p,House.Find("bed"));
                Activity="Идёт спать в спальню";
                return;
            }
            if(p.Food<25&&p.Room!="kitchen")
            {
                Visit(p,"kitchen");
                return;
            }
            if(turns%2==0)
            {
                int current=Array.IndexOf(House.Rooms,p.Room);
                Visit(p,House.Rooms[(current+1+random.Next(3))%4]);
                return;
            }
            List<HomeSpot> options=new List<HomeSpot>();
            foreach(HomeSpot s in House.Spots)if(s.Room==p.Room&&(!House.IsBought(s.Id)||p.Has(s.Id)))options.Add(s);
            if(options.Count>0)Command(p,options[random.Next(options.Count)]);
            else Visit(p,"living");
        }
        public void Update(Pet p,float dt,bool blocked)
        {
            if(blocked||p.Sleeping)
            {
                return;
            }
            dt=Math.Max(0,Math.Min(.25f,dt));
            elapsed+=dt;
            Phase=elapsed;
            if(Stage=="idle")
            {
                if(p.PauseRoaming)return;
                idleLeft-=dt;
                if(idleLeft<=0)Choose(p);
                return;
            }
            if(goal!=null&&House.IsBought(goal.Id)&&!p.Has(goal.Id))
            {
                Reset(X,Y);
                return;
            }
            if(Stage=="down")
            {
                float a=Math.Min(1,elapsed/.55f);
                Y=startY+(123-startY)*a-(float)Math.Sin(a*Math.PI)*7;
                if(a>=1)
                {
                    Y=123;
                    Stage="route";
                    elapsed=0;
                }
                return;
            }
            if(Stage=="route")
            {
                Stage=p.Room!=targetRoom?"exit":"walk";
                elapsed=0;
            }
            float targetX=Stage=="exit"?250:goal==null?120:goal.X;
            if(Stage=="walk"||Stage=="exit"||Stage=="enter")
            {
                if(Stage=="enter")targetX=194;
                FacingLeft=targetX<X;
                float step=dt*34;
                X+=Math.Sign(targetX-X)*Math.Min(step,Math.Abs(targetX-X));
                Y=123;
                if(Math.Abs(X-targetX)<.1f)
                {
                    if(Stage=="exit")
                    {
                        p.Room=targetRoom;
                        X=250;
                        Stage="enter";
                        elapsed=0;
                        Activity="Заходит: "+House.Name(p.Room);
                    }
                    else if(Stage=="enter")
                    {
                        Stage="walk";
                        elapsed=0;
                    }
                    else if(goal!=null&&goal.Y<112)
                    {
                        Stage="jump";
                        startY=Y;
                        elapsed=0;
                        Activity="Прыг! "+goal.Label;
                    }
                    else BeginUse(p);
                }
                return;
            }
            if(Stage=="jump")
            {
                float a=Math.Min(1,elapsed/.65f);
                Y=startY+(goal.Y-startY)*a-(float)Math.Sin(a*Math.PI)*11;
                if(a>=1)
                {
                    Y=goal.Y;
                    BeginUse(p);
                }
                return;
            }
            if(Stage=="use")
            {
                if(ActiveObject=="bed")p.Energy=Pet.Clamp(p.Energy+dt*.25);
                if(elapsed>7)
                {
                    Stage="idle";
                    idleLeft=4+random.Next(5);
                    ActiveObject="";
                    Activity="Немного отдыхает";
                }
                return;
            }
        }
        void BeginUse(Pet p)
        {
            Stage="use";
            elapsed=0;
            Phase=0;
            ActiveObject=goal==null?"":goal.Id;
            Y=goal==null?123:goal.Y;
            Activity=goal==null?"Осматривается":goal.Label;
            p.Joy=Pet.Clamp(p.Joy+1.5);
            if(ActiveObject=="bed"&&p.Energy<12)
            {
                p.Sleeping=true;
                Activity="Сладко спит на кровати";
            }
        }
    }
}
