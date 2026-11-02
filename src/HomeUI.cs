// SPDX-License-Identifier: Apache-2.0
using System;
using System.Drawing;
using System.Windows.Forms;
namespace Persik
{
    public partial class CatWindow
    {
        HomeLife life=new HomeLife(Environment.TickCount);
        string viewedRoom="living";
        bool followCat=true;
        Pet scenery=new Pet();
        void FocusCat()
        {
            followCat=true;
            viewedRoom=pet.Room;
        }
        internal void ViewRoom(string name)
        {
            if(Array.IndexOf(House.Rooms,name)<0)return;
            viewedRoom=name;
            followCat=false;
            Say("Это "+House.Name(name).ToLowerInvariant()+". Котик сейчас: "+House.Name(pet.Room)+".");
            Invalidate();
        }
        internal void UseItem(string id)
        {
            if(!pet.Has(id))
            {
                Say("Сначала поставь эту вещь из магазина.");
                return;
            }
            if(!CanAct())return;
            HomeSpot spot=House.Find(id);
            if(spot==null)return;
            if(id=="lights")pet.LightsOff=!pet.LightsOff;
            life.Command(pet,spot);
            action="";
            actionLeft=0;
            FocusCat();
            Say(spot.Label+". Котик сам подойдёт к предмету.",5);
            Commit();
        }
        internal void UpdateHome(float dt)
        {
            bool blocked=playing||actionLeft>0;
            string before=pet.Room;
            life.Update(pet,dt,blocked);
            if(before!=pet.Room)
            {
                if(followCat)viewedRoom=pet.Room;
                pet.Log("Перешёл в комнату: "+House.Name(pet.Room)+".");
            }
            if(followCat)viewedRoom=pet.Room;
        }
        Bitmap RenderScene()
        {
            if(followCat)viewedRoom=pet.Room;
            scenery.Hat=pet.Hat;
            scenery.BowColor=pet.BowColor;
            scenery.Room=viewedRoom;
            scenery.DarkTheme=pet.DarkTheme;
            scenery.Theme=pet.Theme;
            scenery.Fur=pet.Fur;
            scenery.Weather=pet.Weather;
            scenery.Food=pet.Food;
            scenery.Equipped=pet.Equipped;
            scenery.LightsOff=pet.LightsOff;
            scenery.Sleeping=pet.Room==viewedRoom&&pet.Sleeping;
            art.HideCat=pet.Room!=viewedRoom;
            art.FreePose=!playing&&(action==""||action=="walk"||action=="pet");
            if(art.FreePose)
            {
                art.CatX=(int)Math.Round(life.X);
                art.CatY=life.Y;
            }
            art.FacingLeft=life.FacingLeft;
            art.Action=action;
            if(art.FreePose&&action!="pet")
            {
                art.Action=life.Stage=="walk"||life.Stage=="exit"||life.Stage=="enter"?"walk":life.Stage=="jump"||life.Stage=="down"?"jump":life.Using&&(life.ActiveObject=="rug"||life.ActiveObject=="pillow")?"pet":"";
            }
            art.UsingObject=life.Using&&!pet.Sleeping&&!playing&&actionLeft<=0&&pet.Room==viewedRoom;
            art.ActiveObject=life.ActiveObject;
            art.ObjectPhase=life.Phase;
            if(viewedRoom=="bath")return art.RenderBathroom(scenery,time);
            if(viewedRoom=="kitchen"||viewedRoom=="bedroom")return art.RenderNewRoom(scenery,time,viewedRoom);
            return art.Render(scenery,time);
        }
        void DrawHomeNavigation(Graphics g)
        {
            float step=(room.Width-20)/5f;
            string[] names=
            {
                "Домик","Ванная","Кухня","Спальня","За котом"
            };
            for(int i=0;i<5;i++)
            {
                bool selected=i==4?followCat:viewedRoom==House.Rooms[i];
                RectangleF r=new RectangleF(room.X+10+i*step,room.Y+10,step-4,25);
                UI.Card(g,r,selected?"#EFE5EE":"#F9F1E5",8);
                UI.Center(g,names[i],pet.Compact?9:10,UI.Ink,r,selected);
            }
            if(!pet.Compact)
            {
                string label=action=="wash"?"Тёплый душ · "+(int)(washProgress*100)+"%":pet.Room!=viewedRoom?"Котик: "+House.Name(pet.Room):pet.Sleeping?"Сладко спит":life.Activity;
                UI.Card(g,new RectangleF(214,374,274,24),"#F9F1E5",9);
                UI.Center(g,label,10,UI.Ink,new RectangleF(218,374,266,24));
            }
        }
        bool NavigateHome(Point design)
        {
            if(playing||design.Y<room.Y+10||design.Y>=room.Y+35||design.X<room.X+10||design.X>=room.Right-10)return false;
            if(action=="wash")
            {
                Say("Сначала закончи душ");
                return true;
            }
            int n=(int)((design.X-room.X-10)/((room.Width-20)/5f));
            if(n==4)
            {
                FocusCat();
                Say("Камера следует за котиком по всему дому.");
            }
            else if(n>=0&&n<4)ViewRoom(House.Rooms[n]);
            return true;
        }
        void RoomClick(Point pt)
        {
            if(viewedRoom=="living"&&pt.X<102&&pt.Y>18&&pt.Y<95)
            {
                HomeSpot small=House.At("living",pt,pet);
                if(small!=null&&small.Id!="window")
                {
                    UseItem(small.Id);
                    return;
                }
                pet.Weather=pet.Weather=="sun"?"rain":pet.Weather=="rain"?"night":"sun";
                if(!pet.Sleeping&&!playing&&action!="wash")life.Command(pet,House.Find("window"));
                Say("За окном: "+(pet.Weather=="sun"?"солнышко":pet.Weather=="rain"?"дождь":"звёздная ночь"));
                Commit();
                return;
            }
            if(viewedRoom=="bath"&&pt.X>77&&pt.X<166)
            {
                Wash();
                return;
            }
            if((viewedRoom=="living"&&pt.X>=27&&pt.X<64&&pt.Y>=107&&pt.Y<122)||(viewedRoom=="kitchen"&&pt.X>=28&&pt.X<61&&pt.Y>=124))
            {
                Feed(0);
                return;
            }
            HomeSpot spot=House.At(viewedRoom,pt,pet);
            if(spot!=null)
            {
                if(House.IsBought(spot.Id))
                {
                    UseItem(spot.Id);
                    return;
                }
                if(spot.Id=="bed")
                {
                    Sleep();
                    return;
                }
                if(!CanAct())return;
                life.Command(pet,spot);
                Say(spot.Label);
                return;
            }
            if(viewedRoom=="living"&&pt.X>=174&&pt.Y>=110)
            {
                Sleep();
                return;
            }
            if(pt.Y>109&&pt.X>15&&pt.X<226)
            {
                if(!CanAct())return;
                life.Command(pet,new HomeSpot("goto",viewedRoom,pt.X,123,Rectangle.Empty,"Идёт на зов"));
                Say("Иду к тебе! Если котик далеко — немного подожди.");
            }
        }
    }
}
