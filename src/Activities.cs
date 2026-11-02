// SPDX-License-Identifier: Apache-2.0
using System;
using System.Drawing;
namespace Persik
{
    public partial class CatWindow
    {
        static readonly string[] GameNames=
        {
            "Светлячки","Клубочек","Рыбалка","Запомни лапки","Охота на мышку","Ловкий рыбак"
        };
        static readonly string[] GameDescriptions=
        {
            "Лови огоньки: золотой даёт 3 очка","Кликай по убегающему клубочку","Лови рыбок в пруду: золотая даёт 3 очка","Запомни огоньки и повтори порядок","Лови мышку, когда она выглядывает из норки","Нажми «Лови!», когда бегунок попадёт в зелёную зону"
        };
        static readonly string[] GameIcons=
        {
            "star","play","fish","paw","paw","bolt"
        };
        int lastPopHit=-1,popSeed;
        DateTime lastTimingHit=DateTime.MinValue;
        float timingCenter=.5f;
        int ShopPages
        {
            get
            {
                return (ShopItem.All.Length+2)/3;
            }
        }
        int PopRound
        {
            get
            {
                return Math.Max(0,(int)((25-(gameEnd-DateTime.UtcNow).TotalSeconds)*1.4));
            }
        }
        Point Hole(int n)
        {
            return new Point(62+(n%3)*58,52+(n/3)*27);
        }
        internal void WhackClick(Point pt)
        {
            if(!playing||gameMode!=4)return;
            int round=PopRound;
            if(lastPopHit==round)return;
            Point h=Hole((round*7+popSeed)%9);
            if((pt.X-h.X)*(pt.X-h.X)+(pt.Y-h.Y)*(pt.Y-h.Y)<144)
            {
                lastPopHit=round;
                score+=2;
                audio.Purr(pet.Muted);
            }
        }
        float TimingPosition
        {
            get
            {
                return (float)((Math.Sin((25-(gameEnd-DateTime.UtcNow).TotalSeconds)*2.8)+1)/2);
            }
        }
        internal void TimingClick()
        {
            if(!playing||gameMode!=5||(DateTime.UtcNow-lastTimingHit).TotalSeconds<.6)return;
            lastTimingHit=DateTime.UtcNow;
            float distance=Math.Abs(TimingPosition-timingCenter);
            if(distance<=.11f)
            {
                int gain=distance<=.035f?5:3;
                score+=gain;
                Say(gain==5?"Идеальное попадание! +5":"Рыбка поймана! +3",1.5f);
                timingCenter=.22f+(float)random.NextDouble()*.56f;
                audio.Purr(pet.Muted);
            }
            else
            {
                score=Math.Max(0,score-1);
                Say("Рано или поздно… попробуй ещё раз",1.2f);
            }
        }
        RectangleF GameRect(float x,float y,float width,float height)
        {
            return new RectangleF(room.X+x*room.Width/240f,room.Y+y*room.Height/150f,width*room.Width/240f,height*room.Height/150f);
        }
        void DrawExtraGame(Graphics g)
        {
            if(gameMode==4)
            {
                UI.Card(g,GameRect(22,33,196,98),"#6B5961",16);
                int round=PopRound,index=(round*7+popSeed)%9;
                for(int i=0;i<9;i++)
                {
                    Point h=Hole(i);
                    RectangleF r=GameRect(h.X-18,h.Y-10,36,20);
                    using(Brush b=new SolidBrush(UI.C("#483D49")))g.FillEllipse(b,r);
                    if(i==index&&lastPopHit!=round)
                    {
                        RectangleF head=GameRect(h.X-10,h.Y-11,20,16);
                        UI.Card(g,head,"#BCADB9",9);
                        using(Brush b=new SolidBrush(UI.C("#E2BFCB")))
                        {
                            g.FillEllipse(b,GameRect(h.X-11,h.Y-14,7,8));
                            g.FillEllipse(b,GameRect(h.X+4,h.Y-14,7,8));
                        }
                        using(Brush b=new SolidBrush(UI.C("#4B414D")))
                        {
                            g.FillEllipse(b,GameRect(h.X-5,h.Y-5,2,2));
                            g.FillEllipse(b,GameRect(h.X+3,h.Y-5,2,2));
                        }
                        UI.Card(g,GameRect(h.X-1,h.Y-1,2,2),"#D695A4",1);
                    }
                }
                return;
            }
            UI.Card(g,GameRect(22,38,196,90),"#51465D",16);
            UI.Center(g,"Поймай момент! Точное попадание — 5 очков",11,Color.White,GameRect(29,46,182,15));
            UI.Card(g,GameRect(34,67,172,12),"#807486",6);
            UI.Card(g,GameRect(34+(timingCenter-.11f)*172,67,.22f*172,12),"#94BB9D",5);
            float needle=34+TimingPosition*172;
            UI.Card(g,GameRect(needle-1,63,2,20),"#FFF0C5",2);
            UI.Card(g,GameRect(82,91,76,26),"#AC748F",10);
            UI.Center(g,"Лови!",15,Color.White,GameRect(82,91,76,26),true);
        }
    }
}
