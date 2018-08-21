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
            win.Show();
            Application.Run();
        }
    }
}
