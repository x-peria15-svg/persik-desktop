// SPDX-License-Identifier: Apache-2.0
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.Text;
namespace Persik
{
    public class Tile:Control
    {
        public string Label,Sub,Symbol;
        public string Fill="#FFFFFF",Tone="#78657D";
        public bool Selected,Primary,Quiet,Overlay;
        bool hover,down;
        public float RenderScale=1;
        public Rectangle DesignBounds;
        public void Place(int x,int y,int w,int h,float scale)
        {
            DesignBounds=new Rectangle(x,y,w,h);
            RenderScale=scale;
            SetBounds((int)(x*scale),(int)(y*scale),(int)(w*scale),(int)(h*scale));
        }
        public Tile(string label,string sub,string symbol)
        {
            Label=label;
            Sub=sub;
            Symbol=symbol;
            AccessibleName=label+" "+sub;
            AccessibleRole=AccessibleRole.PushButton;
            TabStop=true;
            Cursor=Cursors.Hand;
            SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.UserPaint|ControlStyles.SupportsTransparentBackColor|ControlStyles.Selectable,true);
            BackColor=Color.Transparent;
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            hover=true;
            Invalidate();
            base.OnMouseEnter(e);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            hover=down=false;
            Invalidate();
            base.OnMouseLeave(e);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            down=true;
            Focus();
            Invalidate();
            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            down=false;
            Invalidate();
            base.OnMouseUp(e);
        }
        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }
        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }
        protected override bool IsInputKey(Keys key)
        {
            return key==Keys.Enter||key==Keys.Space||base.IsInputKey(key);
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Space||e.KeyCode==Keys.Enter)
            {
                OnClick(EventArgs.Empty);
                e.Handled=true;
            }
            base.OnKeyDown(e);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if(Overlay)
            {
                if(Focused)ControlPaint.DrawFocusRectangle(e.Graphics,ClientRectangle);
                return;
            }
            int Width=DesignBounds.Width,Height=DesignBounds.Height;
            Graphics g=e.Graphics;
            g.ScaleTransform(RenderScale,RenderScale);
            g.SmoothingMode=SmoothingMode.AntiAlias;
            g.TextRenderingHint=System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            string color=Primary?"#AA6C82":Selected?"#EFE5EE":hover?"#F0EAEF":Quiet?"#FBF8F5":Fill;
            if(down)color=Primary?"#965C72":"#E8DFE7";
            if(!Quiet||hover||Selected)UI.Card(g,new RectangleF(0,0,Width-1,Height-1),color,Height<=36?10:14);
            Color ink=Enabled?(Primary?Color.White:UI.C(Tone)):UI.Muted;
            if(Height>=76)
            {
                UI.Card(g,new RectangleF(Width/2-17,10,34,34),Selected?"#E4D4E5":"#F7F0F2",11);
                UI.Icon(g,Symbol,Width/2-10,17,20,UI.C(Tone));
                UI.Center(g,Label,12,ink,new RectangleF(1,49,Width-2,20),true);
                if(!string.IsNullOrEmpty(Sub))UI.Center(g,Sub,10,UI.Muted,Height>=110?new RectangleF(6,73,Width-12,Height-80):new RectangleF(1,69,Width-2,17));
            }
            else if(string.IsNullOrEmpty(Label))
            {
                UI.Icon(g,Symbol,(Width-18)/2,(Height-18)/2,18,ink);
            }
            else if(!string.IsNullOrEmpty(Symbol))
            {
                UI.Icon(g,Symbol,12,(Height-18)/2,18,ink);
                UI.Center(g,Label,12,ink,new RectangleF(31,0,Width-35,Height),Selected||Primary);
            }
            else UI.Center(g,Label,12,ink,new RectangleF(4,0,Width-8,Height),Selected||Primary);
            if(Focused)
            {
                using(GraphicsPath p=UI.Round(new RectangleF(2,2,Width-5,Height-5),9))using(Pen pen=new Pen(UI.Accent))
                {
                    pen.DashStyle=DashStyle.Dot;
                    g.DrawPath(pen,p);
                }
            }
        }
    }
    public class Audio:IDisposable
    {
        MemoryStream wave;
        SoundPlayer player;
        public void Purr(bool muted)
        {
            if(muted)return;
            try
            {
                if(player==null)
                {
                    wave=new MemoryStream();
                    BinaryWriter b=new BinaryWriter(wave);
                    int n=15435;
                    b.Write(Encoding.ASCII.GetBytes("RIFF"));
                    b.Write(36+n*2);
                    b.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
                    b.Write(16);
                    b.Write((short)1);
                    b.Write((short)1);
                    b.Write(22050);
                    b.Write(44100);
                    b.Write((short)2);
                    b.Write((short)16);
                    b.Write(Encoding.ASCII.GetBytes("data"));
                    b.Write(n*2);
                    for(int i=0;i<n;i++)
                    {
                        double t=i/22050.0,env=Math.Sin(Math.PI*i/n);
                        double v=(Math.Sin(t*2*Math.PI*106)+.25*Math.Sin(t*2*Math.PI*213))*(.5+.5*Math.Sin(t*2*Math.PI*27));
                        b.Write((short)(v*env*2100));
                    }
                    b.Flush();
                    wave.Position=0;
                    player=new SoundPlayer(wave);
                    player.Load();
                }
                player.Play();
            }
            catch
            {
            }
        }
        public void Dispose()
        {
            if(player!=null)player.Dispose();
            if(wave!=null)wave.Dispose();
        }
    }
    public class RenameDialog:Form
    {
        public TextBox Input=new TextBox();
        public RenameDialog(string name)
        {
            Text="Как зовут котика?";
            Font=UI.Font(14,false);
            ClientSize=new Size(320,164);
            FormBorderStyle=FormBorderStyle.FixedDialog;
            StartPosition=FormStartPosition.CenterParent;
            MaximizeBox=MinimizeBox=false;
            BackColor=UI.Paper;
            ForeColor=UI.Ink;
            ShowInTaskbar=false;
            Label l=new Label();
            l.Text="Новое имя (до 16 символов)";
            l.SetBounds(20,18,280,24);
            Controls.Add(l);
            Input.BackColor=UI.C(UI.Palette("#FFFFFF"));
            Input.ForeColor=UI.Ink;
            Input.Text=name;
            Input.MaxLength=16;
            Input.SetBounds(20,53,280,28);
            Controls.Add(Input);
            Button ok=new Button();
            ok.FlatStyle=FlatStyle.Flat;
            ok.BackColor=UI.C(UI.Palette("#FFFFFF"));
            ok.ForeColor=UI.Ink;
            ok.Text="Сохранить";
            ok.DialogResult=DialogResult.OK;
            ok.SetBounds(164,108,136,34);
            Controls.Add(ok);
            AcceptButton=ok;
            Button cancel=new Button();
            cancel.FlatStyle=FlatStyle.Flat;
            cancel.BackColor=UI.C(UI.Palette("#FFFFFF"));
            cancel.ForeColor=UI.Ink;
            cancel.Text="Отмена";
            cancel.DialogResult=DialogResult.Cancel;
            cancel.SetBounds(20,108,132,34);
            Controls.Add(cancel);
            CancelButton=cancel;
        }
    }
    public class Firefly
    {
        public float X,Y,Phase;
        public bool Golden;
    }
}
