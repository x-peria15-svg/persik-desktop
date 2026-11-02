// SPDX-License-Identifier: Apache-2.0
using System;
using System.Drawing;
namespace Persik
{
    public partial class CatWindow
    {
        static readonly string[] CoatIds=
        {
            "ginger","cream","gray","chocolate","snow","calico"
        };
        static readonly string[] CoatNames=
        {
            "Рыжий","Крем","Дым","Шоколад","Снежный","Трёхцвет"
        };
        void BuildWardrobe()
        {
            for(int i=0;i<CoatIds.Length;i++)
            {
                int n=i;
                Add(CoatNames[i],"","",24+i*82,648,72,30,delegate
                {
                    SetCoat(CoatIds[n]);
                }
                ).Selected=pet.Fur==CoatIds[i];
            }
            string[] ids=
            {
                "rose","blue","gold","mint"
            },labels=
            {
                "Розовый","Голубой","Золотой","Мятный"
            };
            for(int i=0;i<4;i++)
            {
                int n=i;
                Add(labels[i],"","",24+i*122,710,114,30,delegate
                {
                    pet.BowColor=ids[n];Commit();Rebuild();
                }
                ).Selected=pet.BowColor==ids[i];
            }
            Add(pet.Owned.Contains("bow")?(pet.Has("bow")?"Снять бантик":"Надеть бантик"):"Купить бантик · 25","","heart",24,756,232,31,delegate
            {
                Buy("bow");
            }
            );
            Add("Выбрать шляпу","","star",268,756,236,31,delegate
            {
                page="shop";shopPage=4;sub="";Rebuild();
            }
            );
        }
        internal void SetCoat(string id)
        {
            if(Array.IndexOf(CoatIds,id)<0)return;
            pet.Fur=id;
            Commit();
            Rebuild();
        }
        void DrawWardrobe(Graphics g)
        {
            UI.Text(g,"Шёрстка котика · бесплатно",13,UI.Ink,24,624,true);
            UI.Text(g,"Цвет бантика",13,UI.Ink,24,688,true);
            UI.Text(g,pet.Hat==""?"Без шляпы":"Шляпа надета",11,UI.Muted,355,626);
        }
    }
    public partial class RoomArt
    {
        string Coat(string part)
        {
            string[] values=p.Fur=="gray"?new string[]
            {
                "#A6ADB3","#838E9D","#DDE1DB"
            }
            :p.Fur=="cream"?new string[]
            {
                "#E9D4AE","#C5AC85","#F7EBCC"
            }
            :p.Fur=="chocolate"?new string[]
            {
                "#AB8873","#866653","#DDC5A6"
            }
            :p.Fur=="snow"?new string[]
            {
                "#E2E6E1","#B8C2C6","#FFF4DB"
            }
            :p.Fur=="calico"?new string[]
            {
                "#E8D4B2","#D0B998","#FAEACB"
            }
            :new string[]
            {
                "#E9AF75","#C98C59","#F5D4A0"
            };
            return values[part=="fur"?0:part=="dark"?1:2];
        }
        string Bow(string part)
        {
            string[] colors=p.BowColor=="blue"?new string[]
            {
                "#6C91AE","#A4C5DA","#D7EAF2"
            }
            :p.BowColor=="gold"?new string[]
            {
                "#B8934E","#E2C27B","#FAE7B3"
            }
            :p.BowColor=="mint"?new string[]
            {
                "#6E9C8A","#A4CCB3","#DBEEDD"
            }
            :new string[]
            {
                "#B5768A","#D491A3","#F3C6CB"
            };
            return colors[part=="dark"?0:part=="light"?1:2];
        }
        void DrawHat(int x,int y)
        {
            switch(p.Hat)
            {
                case "hat_beret":R("#874E64",x-8,y-2,17,3);
                R("#C9819B",x-8,y-6,17,4);
                R("#DAA0B0",x-5,y-8,12,3);
                R("#874E64",x+1,y-10,2,3);
                break;
                case "hat_wizard":R("#6C587F",x-10,y-2,21,3);
                for(int i=0;i<7;i++)R("#9B81B4",x-6+i,y-3-i*2,13-i*2,2);
                R("#F1DCAE",x-2,y-8,2,2);
                R("#EAD2A0",x+1,y-13,1,2);
                R("#C4A2CC",x-5,y-3,12,2);
                break;
                case "hat_crown":R("#A98748",x-8,y-1,17,3);
                R("#E8C77C",x-8,y-6,17,5);
                for(int i=0;i<3;i++)R("#F8D990",x-8+i*7,y-10,3,6);
                R("#DA91AA",x-1,y-4,3,2);
                R("#FFF0C0",x-5,y-3,1,2);
                break;
                case "hat_beanie":R("#5F87A7",x-9,y-2,19,3);
                R("#96BACE",x-8,y-6,17,4);
                R("#ADCBD9",x-5,y-9,12,4);
                R("#D4E2E3",x,y-13,4,4);
                for(int i=0;i<5;i++)R("#729EB8",x-6+i*3,y-5,1,4);
                break;
                case "hat_flower":R("#7FA187",x-9,y-2,19,2);
                for(int i=0;i<4;i++)
                {
                    int xx=x-8+i*5;
                    R("#C98DA5",xx,y-6,4,4);
                    R("#E7B7C7",xx+1,y-7,2,6);
                    R("#F4DEA7",xx+1,y-5,2,2);
                }
                break;
                case "hat_straw":R("#B39365",x-11,y-1,23,2);
                R("#ECD09C",x-11,y-3,23,2);
                R("#D8B786",x-6,y-8,13,5);
                R("#EBCB95",x-4,y-10,9,3);
                R("#B57F94",x-6,y-4,13,2);
                R("#F5DEB4",x-4,y-8,1,3);
                break;
            }
        }
    }
}
