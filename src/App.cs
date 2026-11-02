// SPDX-License-Identifier: Apache-2.0
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
namespace Persik
{
    public partial class CatWindow:Form
    {
        internal Pet pet;
        internal RoomArt art=new RoomArt();
        Audio audio=new Audio();
        Timer timer=new Timer();
        DateTime last=DateTime.UtcNow,lastSave=DateTime.UtcNow,lastPet=DateTime.MinValue;
        DateTime gameEnd;
        float uiScale=1;
        float time,actionLeft,toastLeft,petQuiet;
        string action="",toast="",page="care",sub="";
        int shopPage,score,gameMode,historyPage;
        bool playing;
        float washProgress,memoryStart,memoryFlashUntil;
        int memoryInput,memoryMistakes,memoryFlash=-1;
        List<int> memory=new List<int>();
        ToolTip tips=new ToolTip();
        List<Firefly> flies=new List<Firefly>();
        Random random=new Random();
        Rectangle room;
        Point mouse;
        NotifyIcon tray;
        Icon appIcon;
        Bitmap portrait;
        List<Tile> content=new List<Tile>();
        Tile soundButton,pinButton,miniButton,settingsButton,closeButton,minButton,expandButton;
        Tile[] tabs;
        string[] badgeIds=
        {
            "hello","chef","play","love","bath","decor","record","friend"
        };
        string[] badgeNames=
        {
            "Первое мурчание","Личный повар","Напарники","Лучший друг","Чистые лапки","Дизайнер","Ловкие лапки","Большая дружба"
        };
        [DllImport("user32.dll")] static extern bool ReleaseCapture();
        [DllImport("user32.dll")] static extern IntPtr SendMessage(IntPtr h,int msg,IntPtr w,IntPtr l);
        public CatWindow()
        {
            pet=Store.Read();
            UI.Dark=pet.DarkTheme;
            BackColor=UI.Paper;
            Text=pet.Name+" • уютный дом";
            FormBorderStyle=FormBorderStyle.None;
            AutoScaleMode=AutoScaleMode.None;
            DoubleBuffered=true;
            StartPosition=FormStartPosition.CenterScreen;
            TopMost=pet.Pinned;
            KeyPreview=true;
            AccessibleName="Персик: пиксельный питомец";
            appIcon=MakeIcon();
            Icon=appIcon;
            portrait=appIcon.ToBitmap();
            soundButton=Header("sound","Звук",delegate
            {
                pet.Muted=!pet.Muted;Say(pet.Muted?"Мурчание выключено":"Мурчание включено");Commit();
            }
            );
            pinButton=Header("pin","Поверх окон",delegate
            {
                pet.Pinned=!pet.Pinned;TopMost=pet.Pinned;Commit();
            }
            );
            miniButton=Header("home","Компактный виджет",delegate
            {
                SetCompact(!pet.Compact);
            }
            );
            settingsButton=Header("settings","Настройки",delegate
            {
                Navigate(page=="settings"?"care":"settings");
            }
            );
            minButton=Header("min","Свернуть на панель задач",delegate
            {
                WindowState=FormWindowState.Minimized;
            }
            );
            closeButton=Header("close","Закрыть и сохранить",delegate
            {
                Close();
            }
            );
            tabs=new Tile[]
            {
                new Tile("Забота","","paw"),new Tile("Магазин","","shop"),new Tile("Гардероб","","star"),new Tile("Дневник","","book")
            };
            string[] names=
            {
                "care","shop","wardrobe","diary"
            };
            for(int i=0;i<tabs.Length;i++)
            {
                int n=i;
                Controls.Add(tabs[i]);
                tabs[i].Click+=delegate
                {
                    Navigate(names[n]);
                };
            }
            expandButton=new Tile("Открыть заботу и магазин","","paw");
            expandButton.Primary=true;
            expandButton.Click+=delegate
            {
                SetCompact(false);
            };
            Controls.Add(expandButton);
            tray=new NotifyIcon();
            tray.Icon=appIcon;
            tray.Text="Персик — твой маленький друг";
            ContextMenuStrip menu=new ContextMenuStrip();
            menu.Items.Add("Открыть домик",null,delegate
            {
                Show();WindowState=FormWindowState.Normal;Activate();
            }
            );
            menu.Items.Add("Компактный виджет",null,delegate
            {
                Show();SetCompact(true);WindowState=FormWindowState.Normal;
            }
            );
            menu.Items.Add("Скрыть окно",null,delegate
            {
                Hide();
            }
            );
            menu.Items.Add("Кормить",null,delegate
            {
                Feed(0);
            }
            );
            menu.Items.Add("Спать / проснуться",null,delegate
            {
                Sleep();
            }
            );
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Закрыть",null,delegate
            {
                Close();
            }
            );
            tray.ContextMenuStrip=menu;
            tray.DoubleClick+=delegate
            {
                Show();
                WindowState=FormWindowState.Normal;
                Activate();
            };
            tray.Visible=true;
            MouseDown+=HandleDown;
            MouseMove+=HandleMove;
            MouseLeave+=delegate
            {
                art.Mouse=new Point(-1,-1);
            };
            KeyDown+=delegate(object s,KeyEventArgs e)
            {
                if(e.KeyCode==Keys.Escape)
                {
                    if(playing)FinishGame(true);
                    else if(sub!="")
                    {
                        sub="";
                        Rebuild();
                    }
                    else if(page!="care")Navigate("care");
                }
                if(e.Control&&e.KeyCode==Keys.M)
                {
                    SetCompact(!pet.Compact);
                    e.Handled=true;
                }
            };
            FormClosing+=delegate
            {
                if(playing)FinishGame(true);
                Store.Save(pet);
                tray.Visible=false;
                timer.Stop();
            };
            LayoutWindow();
            Rebuild();
            timer.Interval=40;
            timer.Tick+=delegate
            {
                Tick();
            };
            timer.Start();
            if(Store.Error!="")Say(Store.Error);
            else Say("Добро пожаловать домой! Погладь котика мышкой.",7);
        }
        Tile Header(string symbol,string label,Action fn)
        {
            Tile t=new Tile("","",symbol);
            t.Quiet=true;
            t.AccessibleName=label;
            tips.SetToolTip(t,label);
            t.Click+=delegate
            {
                fn();
            };
            Controls.Add(t);
            return t;
        }
        static Icon MakeIcon()
        {
            using(Bitmap b=new Bitmap(32,32))using(Graphics g=Graphics.FromImage(b))
            {
                g.Clear(UI.C("#F5E8E0"));
                using(Brush body=new SolidBrush(UI.C("#DBA16F")))
                {
                    g.FillRectangle(body,5,9,22,19);
                    g.FillRectangle(body,5,4,6,10);
                    g.FillRectangle(body,21,4,6,10);
                }
                using(Brush eye=new SolidBrush(UI.C("#40354E")))
                {
                    g.FillRectangle(eye,10,16,3,3);
                    g.FillRectangle(eye,21,16,3,3);
                }
                using(Brush nose=new SolidBrush(UI.Accent))g.FillRectangle(nose,15,22,4,2);
                IntPtr h=b.GetHicon();
                try
                {
                    return (Icon)System.Drawing.Icon.FromHandle(h).Clone();
                }
                finally
                {
                    DestroyIcon(h);
                }
            }
        }
        [DllImport("user32.dll")]static extern bool DestroyIcon(IntPtr h);
        void LayoutWindow()
        {
            uiScale=pet.Compact?1:Math.Min(1,Math.Max(.6f,(Screen.FromControl(this).WorkingArea.Height-24)/830f));
            ClientSize=pet.Compact?new Size(360,328):new Size((int)(528*uiScale),(int)(830*uiScale));
            room=pet.Compact?new Rectangle(16,62,328,205):new Rectangle(24,112,480,300);
            int[] xs=pet.Compact?new int[]
            {
                122,154,186,218,250,282
            }
            :new int[]
            {
                306,338,370,402,434,466
            };
            Tile[] h=
            {
                soundButton,pinButton,miniButton,settingsButton,minButton,closeButton
            };
            for(int i=0;i<h.Length;i++)h[i].Place(xs[i],12,30,28,uiScale);
            if(pet.Compact)
            {
                soundButton.Visible=pinButton.Visible=settingsButton.Visible=false;
                miniButton.Left=250;
                minButton.Left=282;
                closeButton.Left=314;
            }
            else soundButton.Visible=pinButton.Visible=settingsButton.Visible=true;
            expandButton.Visible=pet.Compact;
            expandButton.Place(16,282,328,32,uiScale);
            for(int i=0;i<tabs.Length;i++)
            {
                tabs[i].Visible=!pet.Compact;
                tabs[i].Place(24+i*122,568,114,40,uiScale);
            }
            if(IsHandleCreated)
            {
                using(GraphicsPath p=UI.Round(new RectangleF(0,0,Width,Height),20))
                {
                    Region old=Region;
                    Region=new Region(p);
                    if(old!=null)old.Dispose();
                }
            }
            foreach(Tile t in content)
            {
                t.Visible=!pet.Compact;
                Rectangle d=t.DesignBounds;
                t.Place(d.X,d.Y,d.Width,d.Height,uiScale);
            }
            Rectangle area=Screen.FromControl(this).WorkingArea;
            if(Left<area.Left)Left=area.Left;
            if(Top<area.Top)Top=area.Top;
            if(Right>area.Right)Left=Math.Max(area.Left,area.Right-Width);
            if(Bottom>area.Bottom)Top=Math.Max(area.Top,area.Bottom-Height);
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if(miniButton!=null)LayoutWindow();
        }
        internal void SetCompact(bool compact)
        {
            pet.Compact=compact;
            LayoutWindow();
            Rebuild();
            Commit();
        }
        void Navigate(string name)
        {
            page=name;
            sub="";
            Rebuild();
            Invalidate();
        }
        Tile Add(string title,string desc,string symbol,int x,int y,int w,int h,Action fn)
        {
            Tile b=new Tile(title,desc,symbol);
            b.Place(x,y,w,h,uiScale);
            b.Visible=!pet.Compact;
            b.Click+=delegate
            {
                fn();
            };
            Controls.Add(b);
            content.Add(b);
            if(!string.IsNullOrEmpty(desc))tips.SetToolTip(b,desc);
            return b;
        }
        void Rebuild()
        {
            foreach(Tile t in content)
            {
                Controls.Remove(t);
                t.Dispose();
            }
            content.Clear();
            for(int i=0;i<tabs.Length;i++)tabs[i].Selected=(page==new string[]
            {
                "care","shop","wardrobe","diary"
            }
            [i]);
            soundButton.Symbol=pet.Muted?"mute":"sound";
            pinButton.Selected=pet.Pinned;
            miniButton.Selected=pet.Compact;
            settingsButton.Selected=page=="settings";
            if(page=="care")
            {
                if(sub=="food")
                {
                    Add("Назад","","",420,620,84,26,delegate
                    {
                        sub="";Rebuild();
                    }
                    );
                    string[] titles=
                    {
                        "Домашний корм","Рыбное угощение","Кошачье лакомство"
                    };
                    string[] desc=
                    {
                        "+22 сытости\nБесплатно","+35 сытости\n8 монет","+12 сытости, +16 счастья\n6 монет"
                    };
                    for(int i=0;i<3;i++)
                    {
                        int n=i;
                        Tile b=Add(titles[i],desc[i],i==0?"food":i==1?"fish":"heart",24+i*164,650,152,116,delegate
                        {
                            Feed(n);
                        }
                        );
                        b.Tone=i==0?"#B98755":i==1?"#6C9595":"#B5798D";
                    }
                }
                else if(sub=="games")
                {
                    Add("Назад","","",420,620,84,26,delegate
                    {
                        sub="";Rebuild();
                    }
                    );
                    for(int i=0;i<GameNames.Length;i++)
                    {
                        int n=i;
                        Add(GameNames[i],GameDescriptions[i],GameIcons[i],24+(i%3)*164,650+(i/3)*66,152,58,delegate
                        {
                            sub="";StartGame(n);Rebuild();
                        }
                        );
                    }
                }
                else
                {
                    string[] titles=
                    {
                        "Кормить","Играть","Погладить","Помыть",pet.Sleeping?"Разбудить":"Спать"
                    };
                    string[] icons=
                    {
                        "food","play","heart","drop","moon"
                    };
                    string[] desc=
                    {
                        "Выбрать еду","Шесть игр","Мур-р-р","Чистые лапки",pet.Sleeping?"Доброе утро":"В лежанку"
                    };
                    Action[] fns=
                    {
                        delegate
                        {
                            sub="food";
                            Rebuild();
                        },delegate
                        {
                            sub="games";
                            Rebuild();
                        },delegate
                        {
                            Stroke(true);
                        },delegate
                        {
                            Wash();
                        },delegate
                        {
                            Sleep();
                        }
                    };
                    string[] colors=
                    {
                        "#AE8456","#8A7EA4","#B6788E","#729899","#7C83A5"
                    };
                    for(int i=0;i<5;i++)
                    {
                        Tile b=Add(titles[i],desc[i],icons[i],24+i*98,626,88,94,fns[i]);
                        b.Tone=colors[i];
                    }
                    Add("Задания на сегодня","","star",24,752,264,34,delegate
                    {
                        Navigate("diary");
                    }
                    );
                    Add("Как играть","","book",300,752,204,34,delegate
                    {
                        sub="help";Rebuild();
                    }
                    );
                    if(sub=="help")
                    {
                        foreach(Tile b in content)b.Visible=false;
                        Add("Понятно","","check",378,750,126,34,delegate
                        {
                            sub="";Rebuild();
                        }
                        );
                    }
                }
            }
            else if(page=="shop")
            {
                Add("‹","","",422,620,34,27,delegate
                {
                    shopPage=(shopPage+ShopPages-1)%ShopPages;Rebuild();
                }
                );
                Add("›","","",468,620,36,27,delegate
                {
                    shopPage=(shopPage+1)%ShopPages;Rebuild();
                }
                );
                for(int i=0;i<3;i++)
                {
                    ShopItem item=ShopItem.All[shopPage*3+i];
                    string label=pet.Owned.Contains(item.Id)?(item.Id.StartsWith("hat_")?(pet.Has(item.Id)?"Снять":"Надеть"):(pet.Has(item.Id)?"Использовать":"Поставить")):item.Price+" монет";
                    Tile b=Add(label,"","",34+i*164,748,132,30,delegate
                    {
                        if(pet.Has(item.Id)&&!item.Id.StartsWith("hat_"))UseItem(item.Id);else Buy(item.Id);
                    }
                    );
                    b.MouseUp+=delegate(object sender,MouseEventArgs e)
                    {
                        if(e.Button==MouseButtons.Right&&pet.Has(item.Id))Buy(item.Id);
                    };
                    tips.SetToolTip(b,"Клик — использовать. Правая кнопка — убрать вещь.");
                    b.Primary=!pet.Owned.Contains(item.Id);
                    b.Selected=pet.Has(item.Id);
                }
            }
            else if(page=="diary")
            {
                Add("Задания","","",24,620,114,28,delegate
                {
                    sub="";Rebuild();
                }
                ).Selected=sub=="";
                Add("Достижения","","",146,620,136,28,delegate
                {
                    sub="badges";Rebuild();
                }
                ).Selected=sub=="badges";
                Add("История","","",290,620,114,28,delegate
                {
                    sub="history";Rebuild();
                }
                ).Selected=sub=="history";
                if(sub=="")for(int i=0;i<3;i++)
                {
                    int n=i;
                    bool done=QuestProgress(i)>=QuestTarget(i),claimed=(pet.Claimed&(1<<i))!=0;
                    Tile b=Add(claimed?"Получено":done?"Забрать +15":"+15 монет","","",372,658+i*44,132,32,delegate
                    {
                        Claim(n);
                    }
                    );
                    b.Primary=done&&!claimed;
                    b.Selected=claimed;
                }
                if(sub=="history")
                {
                    Add("‹","","",422,620,34,27,delegate
                    {
                        historyPage=Math.Max(0,historyPage-1);Rebuild();
                    }
                    );
                    Add("›","","",468,620,36,27,delegate
                    {
                        historyPage=Math.Min(Math.Max(0,(pet.Journal.Count-1)/4),historyPage+1);Rebuild();
                    }
                    );
                }
                if(sub=="badges")
                {
                    string[] hints=
                    {
                        "Погладить котика впервые","Покормить котика 10 раз","Закончить 3 игры","Погладить котика 50 раз","Помыть котика 5 раз","Купить первое украшение","Набрать 15 очков за игру","Достичь 5 уровня дружбы"
                    };
                    for(int i=0;i<8;i++)
                    {
                        int n=i;
                        Tile b=Add("","","",24+(i%4)*124,659+(i/4)*64,110,55,delegate
                        {
                            Say(hints[n],5);
                        }
                        );
                        b.Quiet=true;
                        b.Overlay=true;
                        tips.SetToolTip(b,hints[n]);
                    }
                }
            }
            else if(page=="wardrobe")
            {
                BuildWardrobe();
            }
            else if(page=="settings")
            {
                Add("Имя: "+pet.Name,"","",24,620,154,31,delegate
                {
                    Rename();
                }
                );
                Add(pet.DarkTheme?"Светлая тема":"Тёмная тема","",pet.DarkTheme?"sun":"moon",188,620,154,31,delegate
                {
                    SetDarkTheme(!pet.DarkTheme);
                }
                );
                Add(pet.PauseRoaming?"Гуляет: нет":"Гуляет сам","","",352,620,152,31,delegate
                {
                    pet.PauseRoaming=!pet.PauseRoaming;Commit();Rebuild();
                }
                );
                string[] th=
                {
                    "peach","sage","lilac"
                },labels=
                {
                    "Персиковая","Лесная","Лавандовая"
                };
                for(int i=0;i<3;i++)
                {
                    int n=i;
                    Add(labels[i],"","",24+i*164,677,152,30,delegate
                    {
                        pet.Theme=th[n];Commit();Rebuild();
                    }
                    ).Selected=pet.Theme==th[n];
                }
                string[] fur=
                {
                    "ginger","cream","gray"
                },fl=
                {
                    "Рыжий","Кремовый","Дымчатый"
                };
                for(int i=0;i<3;i++)
                {
                    int n=i;
                    Add(fl[i],"","",24+i*164,742,152,30,delegate
                    {
                        pet.Fur=fur[n];Commit();Rebuild();
                    }
                    ).Selected=pet.Fur==fur[n];
                }
            }
            Invalidate();
        }
        internal void SetDarkTheme(bool dark)
        {
            pet.DarkTheme=dark;
            UI.Dark=dark;
            BackColor=UI.Paper;
            tray.ContextMenuStrip.BackColor=UI.Paper;
            tray.ContextMenuStrip.ForeColor=UI.Ink;
            foreach(ToolStripItem item in tray.ContextMenuStrip.Items)
            {
                item.BackColor=UI.Paper;
                item.ForeColor=UI.Ink;
            }
            Commit();
            Rebuild();
            Invalidate(true);
        }
        void Say(string text,float seconds=4)
        {
            toast=text;
            toastLeft=seconds;
            Invalidate();
        }
        void Commit()
        {
            Store.Save(pet);
            soundButton.Symbol=pet.Muted?"mute":"sound";
            pinButton.Selected=pet.Pinned;
            Invalidate();
        }
        void Badge(string id)
        {
            int i=Array.IndexOf(badgeIds,id);
            string msg=pet.Award(id,badgeNames[i]);
            if(msg!="")Say(msg,5);
        }
        void Gain(int xp,int coins)
        {
            int before=pet.Level;
            pet.Gain(xp,coins);
            if(pet.Level>before)
            {
                Say("Уровень дружбы "+pet.Level+"! Мы стали ещё ближе.",5);
                pet.Log("Дружба выросла до уровня "+pet.Level+".");
            }
            if(pet.Level>=5)Badge("friend");
        }
        bool CanAct()
        {
            if(playing)
            {
                Say("Сначала закончим игру");
                return false;
            }
            if(action=="wash"&&actionLeft>0)
            {
                Say("Сначала закончим душ — осталось совсем чуть-чуть");
                return false;
            }
            if(pet.Sleeping)
            {
                Say("Котик спит. Нажми «Разбудить», когда захочешь.");
                return false;
            }
            return true;
        }
        internal void Feed(int food)
        {
            pet.NewDay();
            if(!CanAct())return;
            if(action=="eat"&&actionLeft>0)
            {
                Say("Дай мне доесть — это очень вкусно!");
                return;
            }
            if(pet.Food>=95)
            {
                Say("Я уже сыт. Давай оставим вкусняшку на потом.");
                return;
            }
            int cost=food==1?8:food==2?6:0;
            if(pet.Coins<cost)
            {
                Say("Не хватает монет. Домашний корм всегда бесплатный.");
                return;
            }
            pet.Room="kitchen";
            life.Reset(45,127);
            FocusCat();
            pet.Coins-=cost;
            pet.Food=Pet.Clamp(pet.Food+(food==1?35:food==2?12:22));
            pet.Joy=Pet.Clamp(pet.Joy+(food==2?16:4));
            pet.Feeds++;
            pet.DayFeed++;
            Gain(8,food==0?3:0);
            action="eat";
            actionLeft=4;
            pet.Log(food==1?"Праздничный обед с рыбкой.":food==2?"Вкусняшка и счастливые усы.":"Сытный домашний обед.");
            Say("Ням-ням! "+(food==0?"Спасибо за обед. +3 монеты":"Это было восхитительно!"));
            if(pet.Feeds>=10)Badge("chef");
            sub="";
            Commit();
            Rebuild();
        }
        internal void Stroke(bool explicitClick)
        {
            if(playing)return;
            if(!explicitClick&&(action=="eat"||action=="wash"))return;
            if((DateTime.UtcNow-lastPet).TotalSeconds<.8)return;
            lastPet=DateTime.UtcNow;
            pet.NewDay();
            pet.Strokes++;
            pet.DayPet++;
            pet.Joy=Pet.Clamp(pet.Joy+1.5);
            if(pet.DayPet<=15)Gain(2,0);
            if(!pet.Sleeping)
            {
                action="pet";
                actionLeft=2.5f;
            }
            petQuiet=4;
            audio.Purr(pet.Muted);
            Say(pet.Sleeping?"Тихое мурчание во сне…":"Мур-р-р… ещё чуть-чуть за ушком, пожалуйста.");
            if(pet.Strokes==1)Badge("hello");
            if(pet.Strokes>=50)Badge("love");
            Commit();
        }
        internal void Wash()
        {
            if(!CanAct())return;
            pet.Room="bath";
            life.Reset(128,117);
            FocusCat();
            if(pet.Clean>95)
            {
                Say("Лапки уже чистые. Можно просто побыть в ванной.");
                Commit();
                return;
            }
            washProgress=0;
            action="wash";
            actionLeft=30;
            Say("Котик под душем! Проводи мышкой по нему, чтобы намылить.",6);
            Commit();
        }
        void CompleteWash()
        {
            if(action!="wash")return;
            action="";
            actionLeft=0;
            washProgress=1;
            pet.Clean=Pet.Clamp(pet.Clean+45);
            pet.Joy=Pet.Clamp(pet.Joy+5);
            pet.Baths++;
            Gain(6,3);
            pet.Log("Тёплый душ, пена и чистые лапки.");
            Say("Чистый, пушистый и довольный! +3 монеты");
            if(pet.Baths>=5)Badge("bath");
            Commit();
        }
        void NewMemoryRound(int length)
        {
            memory.Clear();
            for(int i=0;i<length;i++)memory.Add(random.Next(4));
            memoryInput=0;
            memoryStart=time;
            memoryFlash=-1;
        }
        void MemoryClick(Point pt)
        {
            if(time-memoryStart<memory.Count*.65f+.3f)return;
            int n=(pt.X-37)/43;
            if(pt.X<37||n>3||pt.Y<65||pt.Y>105)return;
            memoryFlash=n;
            memoryFlashUntil=time+.28f;
            if(n!=memory[memoryInput])
            {
                memoryMistakes++;
                Say("Ой! Ошибка "+memoryMistakes+" из 3. Попробуем ещё раз.");
                if(memoryMistakes>=3)FinishGame(false);
                else NewMemoryRound(2);
                return;
            }
            memoryInput++;
            if(memoryInput==memory.Count)
            {
                score+=memory.Count;
                audio.Purr(pet.Muted);
                NewMemoryRound(Math.Min(6,memory.Count+1));
            }
        }
        internal void Sleep()
        {
            if(playing)
            {
                Say("Сначала закончи игру или нажми Esc");
                return;
            }
            pet.Sleeping=!pet.Sleeping;
            if(pet.Sleeping)
            {
                pet.Room="bedroom";
                life.Reset(161,105);
            }
            FocusCat();
            action="";
            actionLeft=0;
            pet.Log(pet.Sleeping?"Устроился в лежанке и заснул.":"Проснулся и потянулся.");
            Say(pet.Sleeping?"Уютная лежанка и сон про рыбок…":"Доброе утро! Какие у нас планы?");
            Commit();
            Rebuild();
        }
        internal void Buy(string id)
        {
            ShopItem item=Array.Find(ShopItem.All,delegate(ShopItem x)
            {
                return x.Id==id;
            }
            );
            if(item==null)return;
            bool hat=id.StartsWith("hat_");
            if(pet.Owned.Contains(id))
            {
                if(hat)
                {
                    pet.Hat=pet.Hat==id?"":id;
                    Say(pet.Hat==""?"Шляпа снята":"Новая шляпа — новое настроение!");
                }
                else
                {
                    if(pet.Has(id))pet.Equipped.Remove(id);
                    else pet.Equipped.Add(id);
                    Say(pet.Has(id)?"Украшение поставлено в комнате.":"Убрано на хранение.");
                }
            }
            else
            {
                if(pet.Coins<item.Price)
                {
                    Say("Нужно ещё "+(item.Price-pet.Coins)+" монет. Выполняй задания и играй!");
                    return;
                }
                pet.Coins-=item.Price;
                pet.Owned.Add(id);
                if(hat)pet.Hat=id;
                else pet.Equipped.Add(id);
                pet.Log("Покупка: "+item.Title+".");
                Say("Покупка: "+item.Title+". Уже у котика!");
                Badge("decor");
            }
            Commit();
            Rebuild();
        }
        int QuestProgress(int n)
        {
            return n==0?pet.DayFeed:n==1?pet.DayPlay:pet.DayPet;
        }
        int QuestTarget(int n)
        {
            return n==0?2:n==1?1:5;
        }
        internal void Claim(int n)
        {
            pet.NewDay();
            if(QuestProgress(n)<QuestTarget(n)||(pet.Claimed&(1<<n))!=0)return;
            pet.Claimed|=1<<n;
            Gain(10,15);
            pet.Log("Выполнено задание дня. +15 монет.");
            Say("Задание выполнено! +15 монет и +10 дружбы");
            Commit();
            Rebuild();
        }
        void Rename()
        {
            using(RenameDialog d=new RenameDialog(pet.Name))
            {
                if(d.ShowDialog(this)==DialogResult.OK&&!string.IsNullOrWhiteSpace(d.Input.Text))
                {
                    pet.Name=d.Input.Text.Trim();
                    Text=pet.Name+" • уютный дом";
                    Say("Будем знакомы! Теперь меня зовут "+pet.Name+".");
                    Commit();
                    Rebuild();
                }
            }
        }
        internal void StartGame(int mode=0)
        {
            if(mode<0||mode>=GameNames.Length)return;
            if(playing)return;
            if(!CanAct())return;
            if(pet.Energy<15||pet.Food<12)
            {
                Say(pet.Food<12?"Сначала нужно покормить котика.":"Энергии мало. Сначала немного поспим.");
                return;
            }
            double wait=60-(DateTime.UtcNow-new DateTime(Math.Max(0,Math.Min(DateTime.MaxValue.Ticks,pet.LastGame)),DateTimeKind.Utc)).TotalSeconds;
            if(wait>0)
            {
                Say("Передышка после игры: ещё "+Math.Ceiling(wait)+" сек.");
                return;
            }
            pet.Room="living";
            life.Reset(120,123);
            FocusCat();
            playing=true;
            gameMode=mode;
            score=0;
            action="game";
            flies.Clear();
            for(int i=0;i<(mode>=3?0:mode==0||mode==2?3:1);i++)flies.Add(NewFly(mode!=1&&i==0));
            if(mode==3)
            {
                memoryMistakes=0;
                NewMemoryRound(2);
            }
            gameEnd=DateTime.UtcNow.AddSeconds(25);
            lastPopHit=-1;
            popSeed=random.Next(9);
            lastTimingHit=DateTime.MinValue;
            timingCenter=.5f;
            pet.Energy=Pet.Clamp(pet.Energy-6);
            pet.Food=Pet.Clamp(pet.Food-3);
            pet.Clean=Pet.Clamp(pet.Clean-4);
            Say(GameDescriptions[mode]+". Раунд — 25 секунд.",5);
            Commit();
        }
        Firefly NewFly(bool gold)
        {
            return new Firefly
            {
                X=35+random.Next(170),Y=35+random.Next(60),Phase=(float)(random.NextDouble()*6.28),Golden=gold
            };
        }
        PointF FlyPosition(Firefly f)
        {
            if(gameMode==2)return new PointF(120+(float)Math.Sin(time*(f.Golden?1.7:1.1)+f.Phase)*68,73+(float)Math.Cos(time*.9+f.Phase)*22);
            if(gameMode==1)return new PointF(110+(float)Math.Sin(time*1.8+f.Phase)*70,126-(float)Math.Abs(Math.Sin(time*3+f.Phase))*9);
            return new PointF(f.X+(float)Math.Sin(time*1.6+f.Phase)*8,f.Y+(float)Math.Cos(time*1.3+f.Phase)*6);
        }
        internal void FinishGame(bool cancelled)
        {
            if(!playing)return;
            playing=false;
            action="";
            flies.Clear();
            pet.LastGame=DateTime.UtcNow.Ticks;
            if(cancelled)
            {
                Say("Игра остановлена. Игрушки подождут.");
                Commit();
                return;
            }
            pet.NewDay();
            pet.Games++;
            pet.DayPlay++;
            pet.BestScore=Math.Max(pet.BestScore,score);
            pet.Joy=Pet.Clamp(pet.Joy+12+Math.Min(15,score));
            int coins=Math.Min(30,4+score*2);
            Gain(Math.Min(30,8+score),coins);
            pet.Log((GameNames[gameMode]+": ")+score+" очков, +"+coins+" монет.");
            Say("Поймано: "+score+" очков! +"+coins+" монет. Рекорд: "+pet.BestScore,7);
            if(pet.Games>=3)Badge("play");
            if(score>=15)Badge("record");
            Commit();
            Rebuild();
        }
        void Tick()
        {
            DateTime now=DateTime.UtcNow;
            float dt=(float)Math.Max(0,(now-last).TotalSeconds);
            last=now;
            time+=Math.Min(.2f,dt);
            pet.Advance(dt);
            actionLeft-=dt;
            toastLeft-=dt;
            petQuiet-=dt;
            if(action=="wash"&&actionLeft>0)
            {
                washProgress=Math.Min(1,washProgress+dt/7);
                if(washProgress>=1)CompleteWash();
            }
            if(playing)
            {
                if(now>=gameEnd)FinishGame(false);
                else art.CatX=gameMode==1?(int)FlyPosition(flies[0]).X-20:(int)(110+Math.Sin(time*1.5)*40);
            }
            else
            {
                if(actionLeft<=0)action="";
                UpdateHome(dt);
            }
            if((now-lastSave).TotalSeconds>15)
            {
                string oldDay=pet.Day;
                pet.NewDay();
                Store.Save(pet);
                lastSave=now;
                tray.Text=(pet.Name+" — "+Mood()).Substring(0,Math.Min(63,(pet.Name+" — "+Mood()).Length));
                if(oldDay!=pet.Day)Rebuild();
            }
            if(!pet.Sleeping&&page=="care"&&sub=="")
            {
                Tile b=content.Find(delegate(Tile t)
                {
                    return t.Symbol=="moon";
                }
                );
                if(b!=null&&b.Label!="Спать")
                {
                    b.Label="Спать";
                    b.Sub="В лежанку";
                    b.Invalidate();
                }
            }
            timer.Interval=(WindowState==FormWindowState.Minimized||!Visible)?250:playing?33:life.Stage=="walk"||life.Stage=="jump"||life.Stage=="exit"||life.Stage=="enter"?40:80;
            if(WindowState!=FormWindowState.Minimized&&Visible)Invalidate();
        }
        Point Logical(Point pt)
        {
            return new Point((pt.X-room.X)*240/room.Width,(pt.Y-room.Y)*150/room.Height);
        }
        void HandleMove(object s,MouseEventArgs e)
        {
            e=new MouseEventArgs(e.Button,e.Clicks,(int)(e.X/uiScale),(int)(e.Y/uiScale),e.Delta);
            mouse=e.Location;
            if(!room.Contains(mouse))
            {
                Cursor=Cursors.Default;
                return;
            }
            Point pt=Logical(mouse);
            art.Mouse=pt;
            bool hot=playing||House.At(viewedRoom,pt,pet)!=null||art.CatBounds.Contains(pt)||(pt.X>=20&&pt.X<64&&pt.Y>105)||(pt.X>173&&pt.Y>109)||(pt.X<102&&pt.Y<95);
            Cursor=hot?Cursors.Hand:Cursors.Default;
            if(!playing&&art.CatBounds.Contains(pt))
            {
                if(pet.InBathroom&&action=="wash")
                {
                    washProgress=Math.Min(1,washProgress+.012f);
                    petQuiet=1;
                }
                else Stroke(false);
            }
        }
        void HandleDown(object s,MouseEventArgs e)
        {
            e=new MouseEventArgs(e.Button,e.Clicks,(int)(e.X/uiScale),(int)(e.Y/uiScale),e.Delta);
            if(e.Button!=MouseButtons.Left)return;
            if(e.Y<105&&!room.Contains(e.Location))
            {
                ReleaseCapture();
                SendMessage(Handle,0xA1,new IntPtr(2),IntPtr.Zero);
                return;
            }
            if(!room.Contains(e.Location))return;
            if(NavigateHome(e.Location))return;
            Point pt=Logical(e.Location);
            if(playing)
            {
                if(gameMode==3)
                {
                    MemoryClick(pt);
                    return;
                }
                if(gameMode==4)
                {
                    WhackClick(pt);
                    return;
                }
                if(gameMode==5)
                {
                    if(new Rectangle(82,91,76,29).Contains(pt))TimingClick();
                    return;
                }
                for(int i=0;i<flies.Count;i++)
                {
                    PointF f=FlyPosition(flies[i]);
                    double d=Math.Pow(pt.X-f.X,2)+Math.Pow(pt.Y-f.Y,2);
                    if(d<85)
                    {
                        score+=flies[i].Golden?3:1;
                        flies[i]=NewFly(gameMode!=1&&random.Next(5)==0);
                        art.CatX=Math.Max(65,Math.Min(179,pt.X));
                        audio.Purr(pet.Muted);
                        break;
                    }
                }
                return;
            }
            if(art.CatBounds.Contains(pt))
            {
                if(pet.InBathroom&&action=="wash")washProgress=Math.Min(1,washProgress+.04f);
                else Stroke(true);
                return;
            }
            RoomClick(pt);
        }
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                timer.Dispose();
                tips.Dispose();
                audio.Dispose();
                art.Dispose();
                if(tray!=null)
                {
                    tray.Visible=false;
                    tray.ContextMenuStrip.Dispose();
                    tray.Dispose();
                }
                if(portrait!=null)portrait.Dispose();
                if(appIcon!=null)appIcon.Dispose();
            }
            base.Dispose(disposing);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g=e.Graphics;
            g.ScaleTransform(uiScale,uiScale);
            g.SmoothingMode=SmoothingMode.AntiAlias;
            g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            UI.Card(g,new RectangleF(0,0,Width/uiScale-1,Height/uiScale-1),"#FBF8F5",20);
            using(GraphicsPath border=UI.Round(new RectangleF(.5f,.5f,Width/uiScale-1,Height/uiScale-1),20))using(Pen pen=new Pen(UI.C(UI.Palette("#D9CFD9"))))g.DrawPath(pen,border);
            if(pet.Compact)
            {
                UI.TextBox(g,pet.Name,21,UI.Ink,new RectangleF(18,17,225,28),true);
                DrawRoom(g);
                string[] sy=
                {
                    "food","bolt","heart","drop"
                };
                double[] vs=
                {
                    pet.Food,pet.Energy,pet.Joy,pet.Clean
                };
                for(int i=0;i<4;i++)
                {
                    UI.Icon(g,sy[i],18+i*84,47,11,UI.Muted);
                    UI.Text(g,((int)vs[i]).ToString(),10,UI.Ink,35+i*84,47);
                }
                return;
            }
            // A small portrait next to the pet's name anchors the home screen.
            UI.Card(g,new RectangleF(24,28,52,58),"#F2E5DE",17);
            g.InterpolationMode=InterpolationMode.NearestNeighbor;
            g.DrawImage(portrait,new Rectangle(34,39,32,32));
            UI.TextBox(g,pet.Name,28,UI.Ink,new RectangleF(88,34,290,37),true);
            UI.Text(g,"Наш уютный уголок",13,UI.Muted,90,72);
            UI.Card(g,new RectangleF(390,62,114,30),"#F4EAD8",12);
            UI.Icon(g,"coin",401,69,17,UI.C("#B18C4E"));
            UI.Text(g,pet.Coins.ToString(),14,UI.C("#8D7042"),428,69,true);
            DrawRoom(g);
            DrawSpeech(g);
            DrawStats(g);
            UI.Card(g,new RectangleF(24,568,480,40),"#F0EBF0",13);
            if(page=="care")DrawCare(g);
            else if(page=="shop")DrawShop(g);
            else if(page=="diary")DrawDiary(g);
            else if(page=="wardrobe")DrawWardrobe(g);
            else if(page=="settings")DrawSettings(g);
            UI.Line(g,UI.C(UI.Palette("#EDE5E9")),24,798,504,798);
            if(Store.Error!="")UI.Text(g,"Не удалось сохранить — проверь папку сохранений",11,UI.C("#B47170"),24,808);
            else
            {
                UI.Icon(g,"heart",24,808,13,UI.Accent);
                UI.Text(g,"Дружба "+pet.Level+" ур.",11,UI.Ink,44,808,true);
                UI.Card(g,new RectangleF(135,812,77,4),"#E9E0E7",2);
                UI.Card(g,new RectangleF(135,812,Math.Max(4,(pet.XP%100)*.77f),4),"#C395AA",2);
                UI.Text(g,(pet.XP%100)+" / 100",10,UI.Muted,222,808);
                UI.Text(g,pet.Badges.Count+" из 8 достижений",11,UI.Muted,382,808);
            }
        }
        void DrawRoom(Graphics g)
        {
            Bitmap b=RenderScene();
            GraphicsState st=g.Save();
            using(GraphicsPath clip=UI.Round(room,16))g.SetClip(clip);
            g.InterpolationMode=InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode=PixelOffsetMode.Half;
            g.DrawImage(b,room,0,0,240,150,GraphicsUnit.Pixel);
            g.Restore(st);
            if(!playing)DrawHomeNavigation(g);
            if(playing)
            {
                float left=(float)Math.Max(0,(gameEnd-DateTime.UtcNow).TotalSeconds);
                UI.Card(g,new RectangleF(room.X+10,room.Y+10,room.Width-20,28),"#51465D",10);
                UI.Text(g,(GameNames[gameMode]+"   ")+score+" очков",12,Color.White,room.X+20,room.Y+17,true);
                UI.Text(g,Math.Ceiling(left)+" сек. · Esc — выйти",10,UI.C("#E6DDEB"),room.Right-150,room.Y+19);
                DrawGameBoard(g);
                foreach(Firefly f in flies)
                {
                    PointF pt=FlyPosition(f);
                    float x=room.X+pt.X*room.Width/240,y=room.Y+pt.Y*room.Height/150;
                    if(gameMode==1)
                    {
                        float u=room.Width/240f;
                        using(Brush ball=new SolidBrush(UI.C("#B97D9E")))g.FillRectangle(ball,x-6*u,y-5*u,12*u,10*u);
                        using(Brush stripe=new SolidBrush(UI.C("#F3C9D8")))for(int i=-3;i<5;i+=3)g.FillRectangle(stripe,x+i*u,y-4*u,u,8*u);
                        UI.Line(g,UI.C("#B97D9E"),x+5*u,y+3*u,x+12*u,y+7*u,2);
                    }
                    else if(gameMode==2)
                    {
                        UI.Icon(g,"fish",x-12,y-12,24,f.Golden?UI.C("#FFE4A4"):UI.C("#FFD1B2"));
                    }
                    else
                    {
                        using(Brush glow=new SolidBrush(Color.FromArgb(70,f.Golden?UI.C("#FFE9A2"):UI.C("#D6FFCF"))))g.FillEllipse(glow,x-15,y-15,30,30);
                        using(Brush core=new SolidBrush(UI.C(f.Golden?"#A78C54":"#829078")))g.FillEllipse(core,x-9,y-9,18,18);
                        UI.Icon(g,"star",x-7,y-7,14,f.Golden?UI.C("#FFE5A1"):UI.C("#F4F8D3"));
                    }
                }
            }
        }
        void DrawGameBoard(Graphics g)
        {
            if(gameMode>=4)
            {
                DrawExtraGame(g);
                return;
            }
            float sx=room.Width/240f,sy=room.Height/150f;
            if(gameMode==2)
            {
                UI.Card(g,new RectangleF(room.X+24*sx,room.Y+38*sy,192*sx,74*sy),"#5B8797",18);
                for(int i=0;i<7;i++)
                {
                    float y=room.Y+(45+i*9)*sy;
                    UI.Line(g,UI.C("#8AB0B6"),room.X+35*sx,y,room.X+(60+i%3*35)*sx,y,1);
                }
                return;
            }
            if(gameMode!=3)return;
            UI.Card(g,new RectangleF(room.X+20*sx,room.Y+34*sy,200*sx,84*sy),"#51465D",16);
            float elapsed=time-memoryStart;
            bool reveal=elapsed<memory.Count*.65f+.3f;
            int lit=-1;
            if(reveal&&elapsed<memory.Count*.65f&&elapsed%.65f<.45f)lit=memory[(int)(elapsed/.65f)];
            else if(!reveal&&time<memoryFlashUntil)lit=memoryFlash;
            UI.Center(g,reveal?"Смотри и запоминай…":"Твой ход · "+memoryInput+" / "+memory.Count+" · ошибок "+memoryMistakes+" / 3",11,Color.White,new RectangleF(room.X+20*sx,room.Y+40*sy,200*sx,18*sy));
            string[] colors=UI.Dark?new string[]
            {
                "#6B4257","#3E6267","#74633F","#5E4D76"
            }
            :new string[]
            {
                "#C593A7","#8CABAB","#D4B77C","#A69AC2"
            };
            for(int i=0;i<4;i++)
            {
                RectangleF r=new RectangleF(room.X+(37+i*43)*sx,room.Y+65*sy,35*sx,38*sy);
                UI.Card(g,r,i==lit?(UI.Dark?"#936A39":"#FFF1CE"):colors[i],10);
                UI.Icon(g,"paw",r.X+9*sx,r.Y+5*sy,17*sx,UI.Dark?Color.White:UI.C("#51465D"));
                UI.Center(g,(i+1).ToString(),11,UI.Dark?Color.White:UI.C("#51465D"),new RectangleF(r.X,r.Y+23*sy,r.Width,13*sy),true);
            }
        }
        string Mood()
        {
            return pet.Sleeping?"Тихо мурчит во сне":pet.Food<25?"Мяу… животик урчит. Покорми меня!":pet.Energy<20?"Глазки слипаются. Пора в лежанку…":pet.Clean<30?"Кажется, я испачкал лапки. Помоем?":pet.Joy<30?"Мне немного скучно. Побудешь со мной?":"Погладь меня мышкой. Я так люблю обниматься.";
        }
        void DrawSpeech(Graphics g)
        {
            UI.Card(g,new RectangleF(24,426,480,46),"#F1E9EF",14);
            UI.Icon(g,"paw",38,440,17,UI.C("#B58BA2"));
            UI.TextBox(g,toastLeft>0?toast:Mood(),12,UI.Ink,new RectangleF(66,441,424,23));
        }
        void DrawStats(Graphics g)
        {
            string[] labels=
            {
                "Сытость","Энергия","Счастье","Чистота"
            },icons=
            {
                "food","bolt","heart","drop"
            },cols=
            {
                "#C79964","#929F7C","#C28CA0","#86A9AD"
            };
            double[] vals=
            {
                pet.Food,pet.Energy,pet.Joy,pet.Clean
            };
            for(int i=0;i<4;i++)
            {
                int x=24+i*124;
                UI.Icon(g,icons[i],x,491,15,UI.C(cols[i]));
                UI.Text(g,labels[i],11,UI.Muted,x+22,491);
                UI.Text(g,((int)vals[i]).ToString(),20,UI.Ink,x,511,true);
                UI.Text(g,"/ 100",10,UI.Muted,x+33,520);
                UI.Card(g,new RectangleF(x,543,108,5),"#EDE5E4",2);
                UI.Card(g,new RectangleF(x,543,(float)Math.Max(4,108*vals[i]/100),5),vals[i]<25?"#C98683":cols[i],2);
            }
        }
        void DrawCare(Graphics g)
        {
            if(sub=="food")
            {
                UI.Text(g,"Что сегодня в мисочке?",15,UI.Ink,24,621,true);
                return;
            }
            if(sub=="games")
            {
                UI.Text(g,"Во что поиграем?",15,UI.Ink,24,621,true);
                return;
            }
            if(sub=="help")
            {
                UI.Text(g,"Маленькие секреты нашего домика",16,UI.Ink,24,627,true);
                UI.TextBox(g,"Гладь котика движением мыши. Нажми на миску, чтобы покормить, на лежанку — уложить спать, на клубок — начать игру. Окно меняет погоду. За заботу получай монеты и украшай комнату.",13,UI.Muted,new RectangleF(24,660,480,82));
                return;
            }
            int done=0;
            for(int i=0;i<3;i++)if(QuestProgress(i)>=QuestTarget(i))done++;
            UI.Text(g,playing?GameDescriptions[gameMode]:"Задания дня: "+done+" / 3. За заботу получай монеты и украшай домик.",11,UI.Muted,26,731);
        }
        void DrawShop(Graphics g)
        {
            UI.Text(g,new string[]
            {
                "Больше уюта","Маленькие радости","Для любимого домика","Коллекция уюта","Шляпки и короны","Ещё больше шляп"
            }
            [shopPage],16,UI.Ink,24,623,true);
            UI.Text(g,(shopPage+1)+" / "+ShopPages,11,UI.Muted,374,629);
            string[] symbols=
            {
                "star","home","drop","heart","fish","home","moon","heart","drop","star","paw","book","star","moon","star","home","drop","sun"
            };
            for(int i=0;i<3;i++)
            {
                int x=24+i*164;
                ShopItem it=ShopItem.All[shopPage*3+i];
                UI.Card(g,new RectangleF(x,656,152,130),"#F2ECE8",14);
                UI.Icon(g,symbols[shopPage*3+i],x+12,666,21,UI.C("#AA8A91"));
                UI.TextBox(g,it.Title,12,UI.Ink,new RectangleF(x+12,695,132,18),true);
                UI.TextBox(g,it.Description,10,UI.Muted,new RectangleF(x+12,714,132,27));
                if(pet.Has(it.Id))UI.Icon(g,"check",x+122,669,16,UI.Accent);
            }
        }
        void DrawDiary(Graphics g)
        {
            if(sub=="")
            {
                string[] titles=
                {
                    "Два вкусных обеда","Сыграть с котиком","Пять нежных поглаживаний"
                };
                for(int i=0;i<3;i++)
                {
                    int y=657+i*44;
                    bool done=QuestProgress(i)>=QuestTarget(i);
                    UI.Icon(g,done?"check":i==0?"food":i==1?"play":"heart",26,y+7,18,done?UI.C("#93A384"):UI.C("#B599AE"));
                    UI.Text(g,titles[i],12,UI.Ink,55,y+3,true);
                    UI.Text(g,Math.Min(QuestProgress(i),QuestTarget(i))+" / "+QuestTarget(i),10,UI.Muted,55,y+21);
                }
            }
            else if(sub=="badges")
            {
                for(int i=0;i<8;i++)
                {
                    int x=24+(i%4)*124,y=659+(i/4)*64;
                    bool owned=pet.Badges.Contains(badgeIds[i]);
                    UI.Card(g,new RectangleF(x,y,110,55),owned?"#F0E6DD":"#F0EDF0",12);
                    UI.Icon(g,owned?"star":"paw",x+44,y+6,20,owned?UI.C("#BC985A"):UI.C("#C9C0CE"));
                    UI.Center(g,badgeNames[i],9,owned?UI.Ink:UI.Muted,new RectangleF(x,y+29,110,22));
                }
            }
            else
            {
                for(int i=0;i<Math.Min(4,pet.Journal.Count-historyPage*4);i++)
                {
                    string[] row=pet.Journal[historyPage*4+i].Split('|');
                    UI.Text(g,row[0],9,UI.Muted,25,662+i*30);
                    UI.TextBox(g,row.Length>1?row[1]:"",11,UI.Ink,new RectangleF(112,660+i*30,388,24));
                }
                if(pet.Journal.Count==0)UI.Text(g,"Здесь будет история вашей дружбы.",13,UI.Muted,24,672);
            }
        }
        void DrawSettings(Graphics g)
        {
            UI.Text(g,"Цвет комнаты",11,UI.Muted,24,659);
            UI.Text(g,"Шёрстка котика",11,UI.Muted,24,722);
        }
    }
}
