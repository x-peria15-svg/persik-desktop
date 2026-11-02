// SPDX-License-Identifier: Apache-2.0
using System;
using System.Windows.Forms;
using System.Threading;
namespace Persik
{
    public static class Program
    {
        [STAThread] public static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using(Mutex mutex=new Mutex(false,"Local\\PixelCat.Persik"))
            {
                bool acquired=false;
                try
                {
                    try
                    {
                        acquired=mutex.WaitOne(0,false);
                    }
                    catch(AbandonedMutexException)
                    {
                        acquired=true;
                    }
                    if(!acquired)
                    {
                        MessageBox.Show("Котик уже дома! Открой его через панель задач или значок в системном трее.","Персик");
                        return;
                    }
                    Application.Run(new CatWindow());
                }
                finally
                {
                    if(acquired)mutex.ReleaseMutex();
                }
            }
        }
    }
}
