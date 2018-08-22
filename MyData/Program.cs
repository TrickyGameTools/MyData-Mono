using System;
using Gtk;

namespace MyData
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Application.Init();
            MainWindow win = new MainWindow();
            win.Title = "MyData - Coded by Tricky";
            win.Resize(1000, 800);
            win.Show();
            Application.Run();
        }
    }
}
