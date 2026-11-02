// SPDX-License-Identifier: Apache-2.0
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Collections.Generic;
namespace Persik
{
    public static class UI
    {
        public static Color C(string h)
        {
            return ColorTranslator.FromHtml(h);
        }
        public static bool Dark;
        public static Color Ink
        {
            get
            {
                return C(Dark?"#EFE8F3":"#40354E");
            }
        }
        public static Color Muted
        {
            get
            {
                return C(Dark?"#B6ACBF":"#918699");
            }
        }
        public static Color Paper
        {
            get
            {
                return C(Dark?"#24232D":"#FBF8F5");
            }
        }
        public static Color Accent
        {
            get
            {
                return C(Dark?"#DBA2BB":"#B76F82");
            }
        }
        public static string Palette(string c)
        {
            if(!Dark)return c;
            switch(c.ToUpperInvariant())
            {
                case "#FBF8F5":return "#24232D";
                case "#FFFFFF":return "#34313F";
                case "#EFE5EE":return "#504153";
                case "#F0EAEF":return "#443D4D";
                case "#E8DFE7":return "#56485E";
                case "#E4D4E5":return "#64506A";
                case "#F7F0F2":return "#443745";
                case "#F2E5DE":return "#473B3B";
                case "#F4EAD8":return "#4B4032";
                case "#F0EBF0":return "#2E2B37";
                case "#F1E9EF":return "#3B323F";
                case "#F2ECE8":return "#33303B";
                case "#F0E6DD":return "#484033";
                case "#F0EDF0":return "#302D39";
                case "#E9E0E7":case "#EDE5E4":return "#46404D";
                case "#F9F1E5":return "#40383D";
                case "#D9CFD9":case "#EDE5E9":return "#514858";
                default:return c;
            }
        }
        public static Color Readable(Color c)
        {
            if(!Dark||c.GetBrightness()>.61f)return c;
            return Color.FromArgb(c.A,(int)(c.R*.45+140),(int)(c.G*.45+140),(int)(c.B*.45+140));
        }
        static Dictionary<string,Font> fonts=new Dictionary<string,Font>();
        public static Font Font(float size,bool bold)
        {
            string k=size.ToString()+bold;
            if(!fonts.ContainsKey(k))fonts[k]=new Font("Segoe UI",size,bold?FontStyle.Bold:FontStyle.Regular,GraphicsUnit.Pixel);
            return fonts[k];
        }
        public static GraphicsPath Round(RectangleF r,float rad)
        {
            float d=rad*2;
            GraphicsPath p=new GraphicsPath();
            p.AddArc(r.X,r.Y,d,d,180,90);
            p.AddArc(r.Right-d,r.Y,d,d,270,90);
            p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);
            p.AddArc(r.X,r.Bottom-d,d,d,90,90);
            p.CloseFigure();
            return p;
        }
        public static void Card(Graphics g,RectangleF r,string color,int radius)
        {
            using(GraphicsPath p=Round(r,radius))using(Brush b=new SolidBrush(C(Palette(color))))g.FillPath(b,p);
        }
        public static void Text(Graphics g,string text,float size,Color color,float x,float y,bool bold=false)
        {
            using(Brush b=new SolidBrush(Readable(color)))g.DrawString(text,Font(size,bold),b,x,y,StringFormat.GenericTypographic);
        }
        public static void TextBox(Graphics g,string text,float size,Color color,RectangleF r,bool bold=false)
        {
            using(Brush b=new SolidBrush(Readable(color)))using(StringFormat sf=new StringFormat())
            {
                sf.Trimming=StringTrimming.EllipsisCharacter;
                g.DrawString(text,Font(size,bold),b,r,sf);
            }
        }
        public static void Center(Graphics g,string text,float size,Color color,RectangleF r,bool bold=false)
        {
            using(Brush b=new SolidBrush(Readable(color)))using(StringFormat sf=new StringFormat())
            {
                sf.Alignment=StringAlignment.Center;
                sf.LineAlignment=StringAlignment.Center;
                sf.Trimming=StringTrimming.EllipsisCharacter;
                g.DrawString(text,Font(size,bold),b,r,sf);
            }
        }
        public static void Line(Graphics g,Color c,float x,float y,float xx,float yy,float w=1)
        {
            using(Pen p=new Pen(c,w))
            {
                p.StartCap=p.EndCap=LineCap.Round;
                g.DrawLine(p,x,y,xx,yy);
            }
        }
        // Small purpose-drawn symbols keep the interface independent of emoji fonts.
        public static void Icon(Graphics g,string name,float x,float y,float size,Color c)
        {
            c=Readable(c);
            GraphicsState st=g.Save();
            g.TranslateTransform(x,y);
            g.ScaleTransform(size/24,size/24);
            using(Pen p=new Pen(c,1.7f))using(Brush b=new SolidBrush(c))
            {
                p.StartCap=p.EndCap=LineCap.Round;
                p.LineJoin=LineJoin.Round;
                if(name=="heart")
                {
                    g.FillEllipse(b,3,5,10,10);
                    g.FillEllipse(b,11,5,10,10);
                    g.FillPolygon(b,new PointF[]
                    {
                        new PointF(3,11),new PointF(21,11),new PointF(12,22)
                    }
                    );
                }
                else if(name=="food")
                {
                    g.DrawArc(p,3,7,18,14,0,180);
                    g.DrawLine(p,3,14,21,14);
                    g.DrawLine(p,8,21,16,21);
                    g.DrawArc(p,8,2,4,8,90,180);
                    g.DrawArc(p,14,1,4,9,90,180);
                }
                else if(name=="moon")
                {
                    using(GraphicsPath q=new GraphicsPath())
                    {
                        q.AddArc(3,2,18,19,65,285);
                        q.AddArc(10,0,14,16,140,-180);
                        q.CloseFigure();
                        g.DrawPath(p,q);
                    }
                }
                else if(name=="bolt")
                {
                    g.DrawPolygon(p,new PointF[]
                    {
                        new PointF(14,2),new PointF(5,14),new PointF(11,14),new PointF(10,22),new PointF(20,9),new PointF(13,9)
                    }
                    );
                }
                else if(name=="drop")
                {
                    using(GraphicsPath q=new GraphicsPath())
                    {
                        q.AddBezier(12,2,9,7,4,11,5,16);
                        q.AddBezier(5,16,7,24,19,23,19,15);
                        q.AddBezier(19,15,18,10,12,2,12,2);
                        g.DrawPath(p,q);
                    }
                }
                else if(name=="play")
                {
                    g.DrawEllipse(p,3,3,18,18);
                    g.DrawArc(p,5,5,17,13,50,210);
                    g.DrawArc(p,4,8,15,14,200,190);
                    g.DrawLine(p,18,19,23,22);
                }
                else if(name=="home")
                {
                    g.DrawLines(p,new PointF[]
                    {
                        new PointF(2,11),new PointF(12,3),new PointF(22,11)
                    }
                    );
                    g.DrawLines(p,new PointF[]
                    {
                        new PointF(5,10),new PointF(5,21),new PointF(19,21),new PointF(19,10)
                    }
                    );
                    g.DrawRectangle(p,10,14,4,7);
                }
                else if(name=="book")
                {
                    g.DrawRectangle(p,5,3,15,18);
                    g.DrawLine(p,9,3,9,21);
                    g.DrawLine(p,12,8,17,8);
                    g.DrawLine(p,12,12,17,12);
                }
                else if(name=="shop")
                {
                    g.DrawRectangle(p,4,9,16,13);
                    g.DrawArc(p,8,1,8,12,180,180);
                }
                else if(name=="coin")
                {
                    g.DrawEllipse(p,3,3,18,18);
                    g.DrawEllipse(p,7,6,10,12);
                    g.DrawLine(p,12,8,12,16);
                }
                else if(name=="sound")
                {
                    g.DrawPolygon(p,new PointF[]
                    {
                        new PointF(3,9),new PointF(7,9),new PointF(12,4),new PointF(12,20),new PointF(7,15),new PointF(3,15)
                    }
                    );
                    g.DrawArc(p,10,5,10,14,-65,130);
                    g.DrawArc(p,10,1,16,22,-60,120);
                }
                else if(name=="mute")
                {
                    Icon(g,"sound",0,0,24,c);
                    g.DrawLine(p,2,2,22,22);
                }
                else if(name=="pin")
                {
                    g.DrawLines(p,new PointF[]
                    {
                        new PointF(7,3),new PointF(18,3),new PointF(16,11),new PointF(20,15),new PointF(5,15),new PointF(9,11),new PointF(7,3)
                    }
                    );
                    g.DrawLine(p,12,15,12,23);
                }
                else if(name=="settings")
                {
                    g.DrawEllipse(p,5,5,14,14);
                    g.DrawEllipse(p,9,9,6,6);
                    for(int a=0;a<8;a++)
                    {
                        double t=a*Math.PI/4;
                        g.DrawLine(p,12+(float)Math.Cos(t)*8,12+(float)Math.Sin(t)*8,12+(float)Math.Cos(t)*11,12+(float)Math.Sin(t)*11);
                    }
                }
                else if(name=="close")
                {
                    g.DrawLine(p,6,6,18,18);
                    g.DrawLine(p,18,6,6,18);
                }
                else if(name=="min")
                {
                    g.DrawLine(p,5,15,19,15);
                }
                else if(name=="paw")
                {
                    g.FillEllipse(b,7,11,11,10);
                    g.FillEllipse(b,3,7,5,7);
                    g.FillEllipse(b,9,2,5,7);
                    g.FillEllipse(b,16,4,5,7);
                    g.FillEllipse(b,19,11,4,6);
                }
                else if(name=="star")
                {
                    PointF[] points=new PointF[10];
                    for(int i=0;i<10;i++)
                    {
                        double t=-Math.PI/2+i*Math.PI/5;
                        float r=i%2==0?10:4.4f;
                        points[i]=new PointF(12+(float)Math.Cos(t)*r,12+(float)Math.Sin(t)*r);
                    }
                    g.DrawPolygon(p,points);
                }
                else if(name=="check")
                {
                    g.DrawLines(p,new PointF[]
                    {
                        new PointF(5,12),new PointF(10,17),new PointF(20,6)
                    }
                    );
                }
                else if(name=="sun")
                {
                    g.DrawEllipse(p,8,8,8,8);
                    for(int a=0;a<8;a++)
                    {
                        double t=a*Math.PI/4;
                        g.DrawLine(p,12+(float)Math.Cos(t)*7,12+(float)Math.Sin(t)*7,12+(float)Math.Cos(t)*10,12+(float)Math.Sin(t)*10);
                    }
                }
                else if(name=="fish")
                {
                    g.DrawEllipse(p,3,7,13,10);
                    g.DrawPolygon(p,new PointF[]
                    {
                        new PointF(16,11),new PointF(22,6),new PointF(22,18)
                    }
                    );
                    g.FillEllipse(b,5,10,2,2);
                }
            }
            g.Restore(st);
        }
    }
    public partial class RoomArt:IDisposable
    {
        public Rectangle CatBounds;
        public int CatX=125;
        public float Time;
        public string Action="";
        public int ToyX=170,ToyY=123;
        public Point Mouse=new Point(-1,-1);
        public bool HideCat,FreePose,UsingObject,FacingLeft;
        public float CatY=123,ObjectPhase;
        public string ActiveObject="";
        readonly Dictionary<string,SolidBrush> paint=new Dictionary<string,SolidBrush>();
        Bitmap scene=new Bitmap(240,150);
        Graphics g;
        Pet p;
        int frame;
        string wall,wallDark,accent,light;
        public void Dispose()
        {
            foreach(SolidBrush b in paint.Values)b.Dispose();
            paint.Clear();
            scene.Dispose();
        }
        void R(string color,int x,int y,int w,int h)
        {
            SolidBrush b;
            if(!paint.TryGetValue(color,out b))
            {
                b=new SolidBrush(UI.C(color));
                paint.Add(color,b);
            }
            g.FillRectangle(b,x,y,w,h);
        }
        void A(int alpha,string color,int x,int y,int w,int h)
        {
            using(Brush b=new SolidBrush(Color.FromArgb(alpha,UI.C(color))))g.FillRectangle(b,x,y,w,h);
        }
        void L(string color,int x,int y,int xx,int yy)
        {
            using(Pen pen=new Pen(UI.C(color)))g.DrawLine(pen,x,y,xx,yy);
        }
        void Cloud(int x,int y)
        {
            R("#DCE8DD",x,y,24,4);
            R("#E8F1DF",x+5,y-3,12,5);
            R("#F6F3DC",x+9,y-5,8,4);
        }
        void Heart(int x,int y,string c)
        {
            R(c,x,y,3,2);
            R(c,x+4,y,3,2);
            R(c,x-1,y+2,9,2);
            R(c,x,y+4,7,1);
            R(c,x+1,y+5,5,1);
            R(c,x+2,y+6,3,1);
            R(c,x+3,y+7,1,1);
        }
        void Leaf(int x,int y,string c,bool right)
        {
            R(c,x,y,7,4);
            R(c,x+(right?2:-2),y-2,7,4);
            R(c,x+2,y+4,4,2);
        }
        public Bitmap Render(Pet pet,float time)
        {
            p=pet;
            Time=time;
            frame=(int)(time*12);
            using(g=Graphics.FromImage(scene))
            {
                g.SmoothingMode=SmoothingMode.None;
                wall=p.Theme=="sage"?"#D5DCC7":p.Theme=="lilac"?"#DDD1E5":"#EDD6C1";
                wallDark=p.Theme=="sage"?"#C4CFB8":p.Theme=="lilac"?"#CEC0D9":"#DFC3AC";
                accent=p.Theme=="sage"?"#94AC98":p.Theme=="lilac"?"#A696BB":"#C68F96";
                light=p.Theme=="sage"?"#CFDCC7":p.Theme=="lilac"?"#D6CAE4":"#E9BCC0";
                R(wall,0,0,240,150);
                R(wallDark,0,0,240,4);
                R("#F8E6CE",0,5,240,2);
                for(int y=16;y<100;y+=16)for(int x=8;x<240;x+=16)
                {
                    R(wallDark,x,y,1,3);
                    R(wallDark,x-1,y+1,3,1);
                }
                Window();
                Shelves();
                R("#AB806B",0,100,240,50);
                R("#916650",0,98,240,3);
                R("#CA9D7E",0,101,240,2);
                for(int y=109;y<150;y+=12)
                {
                    R("#916D59",0,y,240,1);
                    for(int x=(y/12%2)*25;x<240;x+=53)
                    {
                        R("#916D59",x,y,1,12);
                        R("#BF9477",x+4,y+3,26,1);
                        R("#B88B70",x+12,y+7,34,1);
                    }
                }
                // Light falling from the window, made of broad translucent pixels.
                if(p.Weather=="sun"&&!p.Sleeping)
                {
                    for(int i=0;i<17;i++)A(23,"#FFF3C9",20+i*2,103+i,62,1);
                }
                Rug();
                Furniture();
                Bowl();
                Bed();
                Plant(216,104,false);
                if(p.Has("plant"))
                {
                    Plant(198,91,true);
                    R("#BE9E7F",190,106,37,4);
                    R("#A68064",194,110,3,13);
                    R("#A68064",221,110,3,13);
                }
                if(p.Has("house"))House();
                else
                {
                    R("#7D6668",16,125,10,4);
                    R("#C49D9B",15,123,11,3);
                    R("#9D7C81",28,126,10,4);
                    R("#D1AAA5",27,124,11,3);
                }
                // A little yarn ball that also starts the mini-game.
                Yarn(176,131);
                if(p.Has("lights")&&!p.LightsOff)Lights();
                Extras();
                DetailPass("living");
                Door();
                DrawCat();
                ObjectEffect();
                // Quiet dust motes; outside rain stays inside the window.
                for(int i=0;i<6;i++)
                {
                    int x=16+(i*37)%212,y=22+(int)(Time*3+i*19)%90;
                    A(100,"#FFF5D9",x,y,1,1);
                }
                if(p.Weather=="night"||p.Sleeping)A(42,"#413653",0,0,240,150);
                if(p.DarkTheme)A(28,"#252335",0,0,240,150);
            }
            return scene;
        }
        public Bitmap RenderBathroom(Pet pet,float time)
        {
            p=pet;
            Time=time;
            frame=(int)(time*12);
            using(g=Graphics.FromImage(scene))
            {
                R("#C5D8D9",0,0,240,150);
                for(int y=0;y<104;y+=16)for(int x=0;x<240;x+=20)
                {
                    R("#DCE7E0",x+1,y+1,18,14);
                    R("#D1E1DA",x+1,y+13,18,2);
                    if((x/20+y/16)%4==0)R("#A9C6C5",x+8,y+6,4,4);
                }
                R("#8DABA9",0,102,240,3);
                R("#BBC9BE",0,105,240,45);
                for(int y=105;y<150;y+=12)for(int x=0;x<240;x+=20)R((x/20+y/12)%2==0?"#C9D6CC":"#B0C4BE",x+1,y+1,18,10);
                R("#AF9981",17,21,42,47);
                R("#ECDFCC",20,24,36,41);
                R("#A1C0C0",22,26,32,37);
                R("#C9DDDA",24,28,6,30);
                R("#E5EEDE",29,28,4,10);
                R("#B2CBC7",31,35,20,1);
                R("#A58E77",15,87,47,30);
                R("#CEB99C",17,91,43,23);
                R("#B09980",37,91,2,23);
                R("#8F7C70",30,98,2,6);
                R("#8F7C70",46,98,2,6);
                R("#F1EADB",12,81,53,9);
                R("#D1DDD5",17,83,42,4);
                R("#899F9E",37,75,3,8);
                R("#899F9E",38,73,9,3);
                R("#6F9292",45,75,3,3);
                R("#A9BDAA",19,73,7,8);
                R("#E7DCC6",21,71,3,2);
                R("#E6C1AD",52,74,6,7);
                R("#92ABA9",78,117,91,19);
                R("#EEE9DA",75,113,94,19);
                R("#FFFFFF",77,111,90,5);
                R("#CCD8CE",79,117,86,10);
                R("#A6BCB7",83,129,78,2);
                R("#839C9D",121,22,3,27);
                R("#D3DED8",124,24,2,24);
                R("#839C9D",119,21,21,3);
                R("#B8CAC5",136,23,7,7);
                R("#839C9D",133,28,12,3);
                R("#DFE6DD",134,28,10,1);
                R("#A1B3A9",72,17,103,2);
                R("#A5BDB3",166,20,13,85);
                R("#C7D7C9",166,22,3,80);
                R("#92ACA4",173,23,3,82);
                for(int y=24;y<105;y+=14)R("#E3E9D8",168,y,3,4);
                R("#8C9D99",192,37,36,2);
                R("#D2A7B1",194,39,13,37);
                R("#EDC6CA",195,41,3,32);
                R("#ADBDB0",211,39,13,28);
                R("#D3DFCB",212,41,3,23);
                R("#A88B70",194,106,32,28);
                R("#C5AB87",192,103,36,6);
                for(int y=111;y<132;y+=4)R("#D5BC95",196,y,27,1);
                for(int x=198;x<226;x+=7)R("#D5BC95",x,108,1,24);
                R("#A3B6AA",179,131,6,5);
                R("#DFD8C5",179,128,6,3);
                R("#B7C3AE",187,129,5,7);
                R("#DFD8C5",187,127,5,2);
                R("#89AFAC",92,139,64,9);
                R("#B5CFBF",95,141,58,5);
                for(int x=98;x<153;x+=4)R("#94B6AC",x,142,1,3);
                if(Action=="wash")for(int i=0;i<12;i++)
                {
                    int x=126+(i%7)*3,y=34+(int)(Time*58+i*11)%74;
                    R("#A9D6DB",x,y,1,4);
                    R("#E0EEDE",x,y,1,1);
                }
                DetailPass("bath");
                Door();
                DrawCat();
                if(Action=="wash")
                {
                    for(int i=0;i<7;i++)
                    {
                        int x=112+(i*7)%27,y=91+(frame+i*7)%24;
                        R("#EAF2E7",x,y,3,3);
                        R("#FFFFFF",x,y,2,1);
                    }
                    for(int i=0;i<6;i++)
                    {
                        int x=126+i*3,y=34+(int)(Time*65+i*13)%77;
                        A(130,"#D9EDE8",x,y,1,3);
                    }
                }
                if(p.DarkTheme)A(38,"#242538",0,0,240,150);
            }
            return scene;
        }
        void DetailPass(string room)
        {
            if(room=="living")
            {
                R("#AA826B",153,103,37,2);
                R("#D4B18F",152,79,1,18);
                for(int i=0;i<4;i++)R("#F5DBC2",111+i*6,143,2,1);
            }
            else if(room=="kitchen")
            {
                R("#DBB9A6",111,89,48,2);
                for(int x=113;x<160;x+=6)R("#F1D2BC",x,90,2,2);
                R("#E5D5B6",209,85,10,13);
                for(int i=0;i<3;i++)R("#ADAA8C",211,88+i*3,6,1);
                R("#D5A7AB",212,83,3,3);
                R("#A8B48C",16,94,4,9);
                R("#D6C5AA",20,95,1,8);
                R("#F0E7D4",22,92,1,12);
            }
            else if(room=="bedroom")
            {
                R("#997F91",107,126,102,2);
                for(int y=104;y<122;y+=5)for(int x=114;x<132;x+=6)R("#F5E3DC",x,y,1,1);
                for(int i=0;i<7;i++)
                {
                    int x=112+i*15,y=36+(int)(Math.Sin(i*.5)*4);
                    R("#A797A6",x,y,13,1);
                    R("#F3DCAE",x+6,y+1,2,3);
                }
                R("#C9ADBB",88,131,8,5);
                R("#E6C9D0",87,129,10,3);
            }
            else
            {
                for(int i=0;i<5;i++)
                {
                    R("#DDBBC4",195+i*2,69,1,4);
                    R("#C8D8C7",212+i*2,60,1,3);
                }
                R("#F1EBDF",52,73,6,1);
                R("#EEF2E3",81,116,82,1);
                if(Action=="wash")for(int i=0;i<5;i++)
                {
                    int x=101+i*11,y=132+(frame+i)%4;
                    R("#CBE5DE",x,y,3,1);
                }
            }
        }
        void Door()
        {
            R("#8F8172",229,65,11,64);
            R("#C5B39A",231,67,9,61);
            R("#9F927D",233,71,7,52);
            R("#E2C69A",234,100,2,2);
            R("#7D786B",229,128,11,2);
        }
        public Bitmap RenderNewRoom(Pet pet,float time,string room)
        {
            p=pet;
            Time=time;
            frame=(int)(time*12);
            using(g=Graphics.FromImage(scene))
            {
                g.SmoothingMode=SmoothingMode.None;
                if(room=="kitchen")Kitchen();
                else Bedroom();
                DetailPass(room);
                Door();
                DrawCat();
                ObjectEffect();
                if(p.DarkTheme)A(30,"#252335",0,0,240,150);
            }
            return scene;
        }
        void Floor(string a,string b)
        {
            R(a,0,106,240,44);
            R("#A58C7A",0,103,240,4);
            for(int y=108;y<150;y+=12)for(int x=0;x<240;x+=24)
            {
                R(b,x+1,y+1,22,10);
                R("#D7C2A6",x+4,y+3,9,1);
            }
        }
        void Kitchen()
        {
            R("#DCE3D1",0,0,240,106);
            for(int y=0;y<105;y+=12)for(int x=0;x<240;x+=16)
            {
                R("#EAF0DF",x+1,y+1,14,10);
                if((x/16+y/12)%5==0)R("#B9CDBB",x+6,y+4,3,3);
            }
            Floor("#BAAF96","#C9C0A6");
            R("#9D8770",17,20,64,60);
            R("#F1E4CA",20,23,58,54);
            R("#A2C8CA",23,26,52,47);
            R("#DCE9D9",26,35,23,5);
            R("#F3DBA2",61,32,8,8);
            R("#799D86",23,63,52,10);
            R("#F1E4CA",47,26,3,47);
            R("#F1E4CA",23,48,52,3);
            R("#B09879",15,77,69,4);
            R("#B39E80",11,85,66,27);
            R("#F6E7CC",8,81,73,5);
            R("#CBB99A",14,88,29,21);
            R("#CBB99A",47,88,26,21);
            R("#8D846F",36,93,2,5);
            R("#8D846F",50,93,2,5);
            R("#8BA8A1",19,81,29,3);
            R("#9FADA1",33,73,3,8);
            R("#9FADA1",35,71,10,3);
            R("#D6C5A5",62,72,9,9);
            R("#F0D9AE",64,69,5,4);
            R("#A78F76",91,28,91,4);
            R("#D2BA97",88,25,97,4);
            for(int i=0;i<5;i++)
            {
                R(i%2==0?"#C19D8E":"#A6B4A0",95+i*17,16,10,9);
                R("#E2D2B5",96+i*17,14,8,3);
            }
            R("#9A8B77",108,40,1,17);
            R("#9A8B77",125,40,1,17);
            R("#8A8C79",104,55,9,12);
            R("#D0AD8E",121,55,9,12);
            R("#889E97",194,30,32,78);
            R("#D7DED1",197,32,26,73);
            R("#BCCFC2",198,52,25,2);
            R("#81938B",200,40,2,9);
            R("#81938B",200,61,2,15);
            R("#D9B888",209,58,9,8);
            Heart(209,74,"#C58E9B");
            R("#8F9D8A",198,106,5,4);
            R("#8F9D8A",219,106,5,4);
            R("#98745E",88,88,95,7);
            R("#D4AB7E",86,83,99,6);
            R("#F0CFA0",89,83,93,2);
            R("#A17D60",94,95,5,35);
            R("#A17D60",174,95,5,35);
            R("#B1906F",97,111,77,3);
            R("#B68D94",118,84,34,4);
            R("#D5ACAF",120,82,30,2);
            R("#DFD7BA",129,78,14,5);
            R("#B69568",131,75,10,5);
            R("#E7C894",132,73,8,5);
            R("#C9B293",162,76,7,8);
            R("#F3DFC0",163,74,5,3);
            R("#B49F82",169,77,3,4);
            R("#6F7C74",51,81,24,2);
            R("#889184",54,79,7,2);
            R("#889184",65,79,7,2);
            R("#BFA78B",62,74,9,6);
            R("#DED1B8",61,73,11,2);
            R("#8D9B88",61,76,1,3);
            for(int i=0;i<3;i++)A(75,"#F7F0D8",63+i*3,66-(frame+i*3)%6,1,3);
            R("#9C816C",80,108,14,4);
            R("#C1A080",79,104,17,5);
            R("#9C816C",81,111,3,19);
            R("#9C816C",91,111,3,19);
            R("#85A9A4",30,129,27,6);
            R("#BDD1BA",28,127,31,3);
            R("#D1AC76",34,125,20,3);
            R("#A9C0AA",22,139,48,7);
            R("#AF8C76",181,121,12,12);
            R("#CFAB83",180,118,14,4);
            R("#8DA177",185,105,3,14);
            Leaf(178,106,"#A8B98A",false);
            Leaf(187,109,"#92AD81",true);
        }
        void Bedroom()
        {
            R("#D9D1E2",0,0,240,106);
            for(int y=10;y<102;y+=16)for(int x=8;x<240;x+=16)
            {
                R("#CBBFD6",x,y,1,2);
                R("#EDE2E8",x+1,y+1,2,1);
            }
            Floor("#A88778","#B99782");
            R("#A79496",15,21,66,55);
            R("#EAE0D8",18,24,60,49);
            R("#757D9E",21,27,54,43);
            R("#EEE3BF",59,33,8,9);
            R("#757D9E",63,30,7,9);
            for(int i=0;i<9;i++)R("#E8DED2",24+i*6,30+(i*7)%29,1,1);
            R("#EAE0D8",46,27,3,43);
            R("#EAE0D8",21,49,54,2);
            R("#AE9696",12,76,72,3);
            R("#9D8B9D",104,73,105,55);
            R("#C1A8B9",107,75,99,50);
            R("#92778D",105,118,105,12);
            R("#DDC5CF",105,94,106,29);
            R("#F2DFDB",110,87,94,12);
            R("#EBD2CD",110,94,25,14);
            R("#EBD2CD",174,94,29,14);
            R("#B89DB4",136,102,35,24);
            for(int y=105;y<126;y+=6)R("#CCB1C2",137,y,33,1);
            R("#93798A",110,130,5,5);
            R("#93798A",201,130,5,5);
            R("#A88B79",188,78,35,20);
            R("#C5AA8E",187,75,37,5);
            R("#D3BEA4",191,82,28,12);
            R("#947D70",204,87,4,2);
            R("#9B8A76",203,62,2,14);
            R("#E1CAA1",194,53,20,11);
            R("#F3DEAF",197,50,14,13);
            R("#FFF0C6",193,62,22,2);
            R("#A58B7C",28,82,58,5);
            R("#C5A68D",27,79,60,4);
            R("#A58B7C",30,87,4,30);
            R("#A58B7C",80,87,4,30);
            R("#D9C8B0",37,73,20,6);
            R("#A39CAF",35,71,24,3);
            R("#EDE1CF",38,69,20,2);
            R("#B996A3",68,69,9,10);
            R("#D6B7BC",69,67,7,3);
            R("#B39BA9",37,118,145,22);
            R("#CFB9C5",40,120,139,18);
            for(int x=45;x<176;x+=10)R("#E0CBD1",x,123,3,12);
            Yarn(66,129);
            R("#A78A78",136,23,56,4);
            R("#D4BBA0",134,20,60,4);
            for(int i=0;i<4;i++)
            {
                R(i%2==0?"#A59CB0":"#B3979F",140+i*8,8+i%2*3,6,12-i%2*3);
                R("#E5D5BA",141+i*8,17,4,1);
            }
            R("#B2A28E",177,13,9,7);
            R("#C9B8A1",178,11,7,3);
            R("#9E8B92",12,131,15,3);
            R("#D8B5B5",13,122,13,10);
            R("#EACBC7",15,118,9,9);
            R("#EACBC7",15,113,3,8);
            R("#EACBC7",21,113,3,8);
            R("#9B7B83",17,122,1,1);
            R("#9B7B83",22,122,1,1);
            R("#C18F9B",19,125,2,1);
            R("#BC9DB1",15,128,9,3);
            R("#927A6E",95,42,24,29);
            R("#E4CEB1",97,44,20,25);
            R("#BFA9C1",99,46,16,21);
            Heart(103,52,"#E9CED5");
        }
        void ObjectEffect()
        {
            if(!UsingObject||string.IsNullOrEmpty(ActiveObject))return;
            HomeSpot s=Persik.House.Find(ActiveObject);
            if(s==null)return;
            int x=(int)s.X,y=(int)s.Y;
            for(int i=0;i<3;i++)
            {
                int xx=x-12+i*11,yy=y-35-(int)(ObjectPhase*8+i*5)%17;
                R(i%2==0?"#F9E6B7":"#E8B6C4",xx,yy,1,3);
                R(i%2==0?"#F9E6B7":"#E8B6C4",xx-1,yy+1,3,1);
            }
            if(ActiveObject=="mouse")
            {
                int mx=x+10+(int)(Math.Sin(Time*6)*7);
                R("#A699A6",mx,y-1,9,4);
                R("#D8BFC8",mx+4,y-3,3,3);
                R("#615461",mx,y,1,1);
                L("#9C879B",mx+9,y+2,mx+15,y+5);
            }
            if(ActiveObject=="tea")
            {
                for(int i=0;i<3;i++)R("#F4E7D6",x+3+i*2,y-17-(int)(Time*5+i*4)%11,1,3);
            }
        }
        void Window()
        {
            R("#B68770",22,16,76,77);
            R("#F9EAD0",20,13,77,77);
            R("#B28F7E",24,17,69,68);
            string sky=p.Weather=="night"?"#454F70":p.Weather=="rain"?"#93ACB8":"#A9CDCF";
            R(sky,26,19,65,64);
            if(p.Weather=="night")
            {
                R("#FBE6B0",73,26,9,10);
                R(sky,77,23,8,10);
                for(int i=0;i<9;i++)R("#D9DDDC",29+i*7,23+(i*13)%36,1,1);
            }
            else
            {
                R("#F5DFAB",72,26,10,10);
                if(p.Weather=="sun")
                {
                    R("#F9E8BC",74,28,6,6);
                    Cloud(29+(int)(Time/4)%12,39);
                    Cloud(60,55);
                }
                else
                {
                    Cloud(28,31);
                    Cloud(59,38);
                }
            }
            R("#86AAA1",26,68,65,15);
            for(int i=0;i<17;i++)
            {
                int h=(int)(8+6*Math.Sin(i*.65));
                R("#71988B",26+i*4,77-h,4,h+6);
            }
            for(int i=0;i<6;i++)
            {
                int x=30+i*11,y=65+(i%3)*3;
                R("#5E8279",x,y,1,17);
                R("#719486",x-3,y+3,7,8);
                R("#87A68F",x-2,y,5,5);
            }
            if(p.Weather=="rain")for(int i=0;i<25;i++)
            {
                int x=27+(i*17)%62,y=20+(int)(Time*40+i*11)%61;
                L("#C5DADB",x,y,x-1,Math.Min(82,y+4));
            }
            R("#F8EAD0",56,18,3,66);
            R("#F8EAD0",25,50,67,3);
            R("#E4CEAE",23,83,71,4);
            R("#916D5D",17,90,84,3);
            R("#C09B7F",16,87,85,4);
            // Gathered curtains with folds and ties.
            R(accent,14,12,8,68);
            R(light,15,14,5,46);
            R(accent,91,12,10,68);
            R(light,94,14,5,45);
            R("#B68187",16,56,7,4);
            R("#B68187",92,56,7,4);
            R("#987666",11,10,93,3);
            R("#E7CAA9",10,9,3,5);
            R("#E7CAA9",103,9,3,5);
            // Tiny plant and mug on the sill.
            R("#C9977C",34,79,9,9);
            R("#E3B18E",33,78,11,3);
            R("#6F9775",38,67,2,11);
            Leaf(32,70,"#8EAC7C",false);
            R("#7C9F79",40,66,6,4);
        }
        void Shelves()
        {
            R("#AA816A",129,35,91,4);
            R("#D7B591",127,32,95,4);
            R("#916D59",133,39,3,4);
            R("#916D59",211,39,3,4);
            string[] cols=
            {
                "#899C94","#B8838A","#D2AE76","#9B91A7","#C9B49B"
            };
            for(int i=0;i<5;i++)
            {
                int x=135+i*6,h=15+i%3*3;
                R(cols[i],x,32-h,5,h);
                R("#E7D3BA",x+1,30-h,3,1);
                R("#E7D3BA",x+1,28,3,1);
            }
            // Clock with moving hands.
            R("#886A61",177,15,17,17);
            R("#B2947B",175,17,21,13);
            R("#F2E0BF",178,17,15,13);
            R("#685657",185,20,1,6);
            R("#685657",185,25,5,1);
            R("#967A69",180,30,2,2);
            R("#967A69",190,30,2,2);
            R("#BC9983",206,23,8,9);
            R("#8EA57E",208,16,3,8);
            R("#9FB38B",204,18,10,3);
            // Framed fish illustration.
            R("#A17A64",113,51,26,29);
            R("#E4C69D",115,53,22,25);
            R("#F7EAD4",117,55,18,21);
            R("#8FAFA9",120,63,10,5);
            R("#8FAFA9",129,61,3,9);
            R("#567E78",121,64,1,1);
            R("#D9C8AF",120,72,11,1);
        }
        void Rug()
        {
            string c=p.Has("rug")?accent:"#C6B99D",li=p.Has("rug")?light:"#DDD0B4";
            R("#967460",65,124,108,21);
            R(c,63,121,108,21);
            R(li,65,123,104,17);
            R(c,69,126,96,11);
            if(p.Has("rug"))
            {
                for(int x=75;x<163;x+=15)
                {
                    R(li,x,129,1,5);
                    R(li,x-1,130,3,3);
                    R(li,x-2,131,5,1);
                }
            }
            else for(int x=72;x<164;x+=6)R(li,x,128,1,7);
            for(int x=64;x<171;x+=4)
            {
                R(li,x,119,1,2);
                R(li,x,142,1,2);
            }
        }
        void Furniture()
        {
            R("#775A4E",149,97,44,6);
            R("#BC9275",146,74,48,27);
            R("#CCA585",145,73,50,4);
            R("#A57A61",147,78,46,2);
            R("#D6B395",150,82,40,7);
            R("#D6B395",150,91,40,7);
            R("#9B765F",168,84,5,2);
            R("#9B765F",168,93,5,2);
            R("#815F50",150,101,3,8);
            R("#815F50",188,101,3,8);
            if(p.Has("aquarium"))
            {
                R("#789BA0",150,52,30,20);
                R("#B8D7CC",152,54,26,15);
                R("#719F95",152,68,26,3);
                R("#809B6D",154,62,2,7);
                R("#94AE7D",156,60,2,6);
                int fx=161+(int)(Math.Sin(Time*(UsingObject&&ActiveObject=="aquarium"?3:1))*5);
                R("#DBA265",fx,60,6,3);
                R("#EABD7D",fx+6,59,2,5);
                R("#5B625A",fx,60,1,1);
                R("#E0EEE3",173,56+(frame/5)%8,1,1);
                R("#7E9694",149,50,32,3);
            }
            else
            {
                R("#938396",156,65,15,8);
                R("#C3AFB8",155,62,17,3);
                R("#E7D4C5",158,60,11,3);
                R("#A8919C",160,58,7,2);
            }
            // A warm table lamp.
            R("#8B7360",184,62,2,11);
            R("#8B7360",179,72,12,2);
            R("#E8C58B",177,54,16,9);
            R("#F4DA9D",179,51,12,11);
            R("#FFF0C2",176,61,18,3);
        }
        void Bowl()
        {
            R("#896C60",34,121,24,4);
            R("#8AABA7",33,115,25,7);
            R("#B7D0BF",31,113,29,3);
            R("#6D8D88",35,114,20,2);
            if(p.Food>10)
            {
                R("#B78454",36,112,17,3);
                for(int i=0;i<6;i++)R("#E4BA7F",37+i*3,111+i%2,2,1);
            }
            R("#B5C8BD",37,118,5,1);
        }
        void Bed()
        {
            R("#8F6D6B",176,119,53,17);
            R(accent,174,115,55,18);
            R(light,179,112,46,16);
            R("#F5DAC9",184,114,36,10);
            R(accent,174,111,7,15);
            R(accent,224,111,7,15);
            R("#B8848B",178,129,47,3);
            R("#EAC8B9",182,116,3,5);
        }
        void Plant(int x,int y,bool flower)
        {
            R("#987260",x-5,y-3,12,9);
            R("#C59577",x-6,y-6,14,5);
            R("#DFB18F",x-4,y-5,3,10);
            R("#69896C",x,y-28,2,23);
            Leaf(x-9,y-25,"#7F9A76",false);
            Leaf(x+2,y-21,"#97AE83",true);
            Leaf(x-8,y-14,"#8FA87C",false);
            R("#65886A",x-2,y-32,6,8);
            R("#9BB48A",x-1,y-32,2,7);
            if(flower)
            {
                R("#D39AAB",x+2,y-33,5,5);
                R("#F1C9BD",x+3,y-32,3,3);
                R("#E9C896",x+4,y-31,1,1);
            }
        }
        void House()
        {
            R("#93705B",10,112,34,20);
            R("#D8B78F",9,104,36,25);
            for(int i=0;i<7;i++)R("#E4C8A0",11+i*2,102-i,32-i*4,2);
            R("#A37E64",18,113,17,16);
            R("#775F56",21,112,11,18);
            R("#C18D89",18,128,17,3);
        }
        void Lights()
        {
            for(int x=8;x<236;x+=3)
            {
                int y=7+(int)(Math.Sin((x-8)/228.0*Math.PI)*8);
                R("#8E7868",x,y,3,1);
            }
            for(int i=0;i<10;i++)
            {
                int x=12+i*24,y=7+(int)(Math.Sin((x-8)/228.0*Math.PI)*8);
                A(40,"#FFE6AB",x-3,y-1,9,9);
                R(i%3==0?"#F8CF9E":"#FFE9B6",x,y+1,3,4);
                R("#FFF3CF",x+1,y+1,1,2);
            }
        }
        void Yarn(int x,int y)
        {
            R("#9C778F",x+1,y,9,8);
            R("#BB93AB",x,y+1,11,6);
            R("#DFB6C6",x+2,y+1,1,5);
            R("#DFB6C6",x+5,y,1,7);
            R("#DFB6C6",x+8,y+2,1,6);
            L("#9C778F",x+9,y+6,x+16,y+9);
        }
        void Extras()
        {
            if(p.Has("poster"))
            {
                R("#726780",117,55,18,21);
                R("#ECD7AC",124,58,7,8);
                R("#726780",127,56,6,7);
                R("#F7DFC1",120,62,1,1);
                R("#F7DFC1",129,70,1,1);
                R("#C2A9C2",120,73,12,1);
            }
            if(p.Has("pillow"))
            {
                R("#AD758C",194,117,17,7);
                Heart(198,114,"#E6ADB9");
                R("#F2C7CB",199,116,2,1);
            }
            if(p.Has("flowers"))
            {
                R("#9A828D",67,81,9,7);
                R("#D4B3B5",68,78,7,9);
                for(int i=0;i<3;i++)
                {
                    int x=67+i*4,y=70+i%2*3;
                    R("#769373",x+1,y+3,1,7);
                    R("#CC8D9F",x-1,y,5,3);
                    R("#E7B4BC",x,y-1,3,5);
                    R("#EED6B2",x+1,y+1,1,1);
                }
            }
            if(p.Has("mobile"))
            {
                R("#AA9278",200,40,1,9);
                R("#AA9278",187,48,27,1);
                for(int i=0;i<3;i++)
                {
                    int x=188+i*12,y=57+i%2*8;
                    R("#AA9278",x,49,1,y-49);
                    int sway=(int)(Math.Sin(Time*1.5+i)*(UsingObject&&ActiveObject=="mobile"?3.5:1.5));
                    R("#D8B878",x-3+sway,y,7,1);
                    R("#E9CC91",x-1+sway,y-3,3,7);
                    R("#FAE5AC",x+sway,y-1,1,3);
                }
            }
            if(p.Has("mouse")&&!(UsingObject&&ActiveObject=="mouse"))
            {
                R("#9B8D9B",77,135,9,4);
                R("#C2B4BD",76,134,9,3);
                R("#D4B2BE",80,132,3,3);
                R("#615461",76,135,1,1);
                L("#9C879B",85,137,92,140);
            }
            if(p.Has("tea"))
            {
                R("#8C9B92",114,94,19,3);
                R("#D6B58B",116,91,17,3);
                R("#BA8F96",113,88,20,3);
                R("#A8988A",122,83,11,5);
                R("#F2DEBD",123,76,8,8);
                R("#957664",124,77,6,2);
                R("#E2CAA7",131,78,3,4);
                R("#E6C193",114,81,5,3);
                R("#AD8258",116,81,1,1);
            }
        }
        void DrawCat()
        {
            if(HideCat)
            {
                CatBounds=Rectangle.Empty;
                return;
            }
            int x=CatX,y=118;
            string sleepFur=Coat("fur");
            string sleepDark=Coat("dark");
            string sleepPale=Coat("pale");
            if(p.Sleeping)
            {
                x=p.Room=="bedroom"?161:201;
                y=p.Room=="bedroom"?103:116;
                R("#B08D87",x-17,y+5,36,3);
                string[] curled=
                {
                    ".....oo............................",".....odo...........................","....odffooooooo....................","...odfffffffffdoooooooo............","...odfffffffffffddddddoo...........","..odfffffffffffddddddddddoo.........","..odffkkffffffdddddfffffdddo........","..odffffffpfffddddffddddfdddo.......","...odffccccccfdddffddffddfddo.......","....odcccccccfdddfddffffdfddo.......",".....odddddddddddfddfffddfddo.......","......odddddddddddffddfffddo........",".......ooooooooooooooooooo........."
                };
                for(int sy=0;sy<curled.Length;sy++)for(int sx=0;sx<curled[sy].Length;sx++)
                {
                    char ch=curled[sy][sx];
                    string c=ch=='o'?"#6B524B":ch=='d'?sleepDark:ch=='f'?sleepFur:ch=='c'?sleepPale:ch=='k'?"#6B524B":ch=='p'?"#C58187":null;
                    if(c!=null)R(c,x-18+sx,y-8+sy,1,1);
                }
                DrawHat(x-9,y-7);
                CatBounds=new Rectangle(x-17,y-20,35,31);
                if(frame%20<17)
                {
                    int rise=frame/7%3;
                    R("#F1E1CD",x+7,y-22-rise,4,1);
                    R("#F1E1CD",x+9,y-21-rise,1,2);
                    R("#F1E1CD",x+7,y-19-rise,4,1);
                }
                return;
            }
            if(Action=="eat")
            {
                x=p.Room=="kitchen"?44:47;
                y=p.Room=="kitchen"?126:112;
            }
            else if(Action=="wash")y=117;
            else if(Action=="game")
            {
                x=Math.Max(65,Math.Min(179,CatX));
                y=118-(int)(Math.Abs(Math.Sin(Time*6))*5);
            }
            if(p.InBathroom)
            {
                x=128;
                y=117;
            }
            if(FreePose)
            {
                x=CatX;
                y=(int)Math.Round(CatY);
            }
            if(UsingObject&&ActiveObject=="house")
            {
                R("#F4D398",23,120,2,1);
                R("#F4D398",29,120,2,1);
                CatBounds=new Rectangle(10,95,35,37);
                return;
            }
            bool walking=Action=="walk"||Action=="game";
            int bob=walking?(frame%6<3?1:0):((int)(Time*1.5)%2);
            y+=bob;
            R("#9B7766",x-13,y+2,28,3);
            R("#AC8B77",x-9,y+4,20,1);
            string[] sprite=
            {
                "....oo............oo......", "....odo..........odo......", "....oddo........oddo......", "....odffooooooooffdo......", "....odffffdddddfffddo.....", "...odfffffdddddffffddo....", "...odffffffffffffffdo....", "...odffffffffffffffdo....", "...odffkfffffffkfffdo....", "...odffkfffffffkfffdo....", "...odfffffppfffffffdo....", "....odffcccccccfffdo.....", ".....odffcccccfffdo......", "......odddddddddo........", ".....odddffffddddo.......", ".....odddfcccfdddo.......", "....odddffccccfdddo...oo.", "....odddffccccfdddo..odo.", "....odddffccccfdddo.odo..", "....odddffccccfddddoodo..", "....odddffccccfddddddo...", ".....odddfccccfdddddo....", ".....oddddooodddddo......", "......oooo...ooooo......."
            };
            string fur=Coat("fur");
            string dark=Coat("dark");
            string pale=Coat("pale");
            if(walking)
            {
                WalkingCat(x,y,fur,dark,pale);
                return;
            }
            int left=x-13,top=y-24;
            GraphicsState catState=g.Save();
            g.TranslateTransform(x,y);
            g.ScaleTransform(1.35f,1.35f);
            g.TranslateTransform(-x,-y);
            for(int yy=0;yy<sprite.Length;yy++)for(int xx=0;xx<sprite[yy].Length;xx++)
            {
                char ch=sprite[yy][xx];
                string color=ch=='o'?"#6B524B":ch=='d'?dark:ch=='f'?fur:ch=='c'?pale:ch=='k'?"#394D46":ch=='p'?"#C58187":null;
                if(color!=null)R(color,left+xx,top+yy,1,1);
            }
            if(p.Fur=="calico")
            {
                R("#A67654",left+4,top+5,4,4);
                R("#76624F",left+15,top+15,4,5);
                R("#C38D59",left+5,top+18,3,3);
            }
            bool eyes=Action=="pet"||Action=="eat"||(frame%64>=60);
            if(eyes)
            {
                R(fur,left+7,top+8,1,2);
                R(fur,left+15,top+8,1,2);
                R("#69534E",left+6,top+9,3,1);
                R("#69534E",left+14,top+9,3,1);
            }
            else
            {
                R("#F9EAD2",left+7,top+8,1,1);
                R("#F9EAD2",left+15,top+8,1,1);
            }
            R("#D9988B",left+5,top+11,2,1);
            R("#D9988B",left+17,top+11,2,1);
            R(dark,left+10,top+5,1,3);
            R(dark,left+13,top+4,1,4);
            R(dark,left+16,top+5,1,3);
            R(pale,left+5,top+4,2,2);
            R(pale,left+17,top+4,2,2);
            R("#765A50",left+2,top+10,3,1);
            R("#765A50",left+20,top+10,3,1);
            R("#F7D8AE",left+3,top+12,3,1);
            R("#F7D8AE",left+19,top+12,3,1);
            R("#9B685F",left+11,top+11,1,1);
            R("#9B685F",left+10,top+12,3,1);
            if(p.Has("bow"))
            {
                R(Bow("dark"),x-5,top+14,10,1);
                R(Bow("light"),x-2,top+13,2,3);
                R(Bow("light"),x+1,top+13,2,3);
                R(Bow("shine"),x,top+14,1,1);
            }
            if(!walking)
            {
                int sway=(int)(Math.Sin(Time*2)*2);
                R(dark,left+23+sway,top+17,2,4);
                R("#6B524B",left+24+sway,top+16,1,3);
                R(pale,left+23+sway,top+16,1,2);
            }
            if(walking)
            {
                R(dark,left+6,top+22,4,2);
                R(dark,left+15,top+22,4,2);
                R("#6B524B",left+6+(frame%4<2?1:0),top+24,4,1);
                R("#6B524B",left+14+(frame%4<2?0:1),top+24,4,1);
            }
            if(Action=="pet")Heart(x+14,top-12-(frame/3%4),"#CC8BA0");
            if(Action=="wash")
            {
                for(int i=0;i<7;i++)
                {
                    int bx=x-14+i*5,by=top+(frame+i*7)%26;
                    R("#D5E9E3",bx,by,3,3);
                    R("#F0F8E9",bx,by,2,1);
                }
            }
            DrawHat(x,y-20);
            g.Restore(catState);
            CatBounds=new Rectangle(x+(int)((left-3-x)*1.35f),y+(int)((top-3-y)*1.35f),44,41);
        }
        void WalkingCat(int x,int y,string fur,string dark,string pale)
        {
            GraphicsState st=g.Save();
            if(FacingLeft)
            {
                g.TranslateTransform(x,y);
                g.ScaleTransform(-1,1);
                g.TranslateTransform(-x,-y);
            }
            string outline="#70564E";
            int step=(int)(Time*8)%4;
            int back=step==0?-1:step==2?1:0;
            int front=-back;
            // A compact rounded silhouette: the head overlaps the body instead of forming a long neck.
            R(outline,x-10,y-17,15,1);
            R(outline,x-13,y-16,20,2);
            R(outline,x-15,y-13,23,8);
            R(outline,x-12,y-5,18,2);
            R(fur,x-10,y-16,14,1);
            R(fur,x-12,y-14,19,8);
            R(fur,x-14,y-12,21,5);
            R(dark,x-13,y-7,20,2);
            R(pale,x-9,y-7,13,3);
            R(dark,x-9,y-15,2,4);
            R(dark,x-4,y-15,2,3);
            R(dark,x+1,y-14,2,3);
            R(pale,x-12,y-12,1,2);
            // Soft cheeks, small triangular ears, a short muzzle and a bright almond eye.
            R(outline,x+3,y-27,10,1);
            R(outline,x+1,y-26,14,2);
            R(outline,x,y-24,17,10);
            R(outline,x+2,y-14,13,2);
            R(outline,x+5,y-12,7,1);
            R(fur,x+3,y-26,10,2);
            R(fur,x+1,y-24,14,9);
            R(fur,x+3,y-15,11,2);
            R(pale,x+8,y-16,7,3);
            R(pale,x+5,y-13,7,1);
            R(outline,x+2,y-31,2,6);
            R(outline,x+4,y-29,2,4);
            R(fur,x+3,y-29,2,4);
            R("#DCA1A1",x+3,y-28,1,2);
            R(outline,x+12,y-30,2,5);
            R(outline,x+10,y-28,2,3);
            R(fur,x+11,y-28,2,3);
            R("#DCA1A1",x+12,y-27,1,2);
            R(dark,x+5,y-25,2,3);
            R(dark,x+8,y-26,1,3);
            if(frame%64>=60)R(outline,x+10,y-21,4,1);
            else
            {
                R("#506F5B",x+10,y-23,3,3);
                R("#3B4E45",x+12,y-22,1,2);
                R("#FFF0D8",x+10,y-23,1,1);
            }
            R("#CE8A94",x+15,y-18,2,1);
            R("#916D62",x+14,y-15,2,1);
            R("#E6B0A1",x+7,y-18,3,1);
            R("#84645A",x+15,y-17,3,1);
            R(pale,x+15,y-16,3,1);
            // Far legs are darker and alternate behind the near paws.
            R(dark,x-8-back,y-5,3,5);
            R(dark,x+3-front,y-5,3,5);
            R(outline,x-12+back,y-4,4,5);
            R(fur,x-11+back,y-4,3,4);
            R(pale,x-12+back,y,5,1);
            R(outline,x+5+front,y-4,4,5);
            R(fur,x+6+front,y-4,3,4);
            R(pale,x+5+front,y,5,1);
            // Curved upright tail: thicker at the base and rounded at the tip.
            int sway=(int)Math.Round(Math.Sin(Time*2)*1.0);
            R(outline,x-18,y-16,4,5);
            R(outline,x-20+sway,y-22,4,8);
            R(outline,x-19+sway,y-25,3,4);
            R(outline,x-17+sway,y-26,3,3);
            R(dark,x-17,y-16,2,4);
            R(fur,x-19+sway,y-21,2,7);
            R(fur,x-18+sway,y-24,2,4);
            R(pale,x-17+sway,y-25,2,2);
            if(p.Has("bow"))
            {
                R(Bow("dark"),x+1,y-13,7,2);
                R(Bow("light"),x+3,y-14,2,4);
                R(Bow("shine"),x+3,y-13,1,1);
            }
            if(p.Fur=="calico")
            {
                R("#AB7B59",x-11,y-13,4,4);
                R("#76624F",x-5,y-10,4,4);
            }
            DrawHat(x+8,y-25);
            g.Restore(st);
            CatBounds=new Rectangle(x-21,y-32,40,35);
        }
    }
}
