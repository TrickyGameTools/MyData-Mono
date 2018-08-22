using System;
using Gtk;
using System.Reflection;

namespace MyData
{
    class MainClass
    {
        static public string filename;
        public static MainWindow win;

        static bool ChooseTheFile(Window w){
            FileChooserDialog fcd = new FileChooserDialog("Choose database", w, FileChooserAction.Open,"Select", ResponseType.Accept, "Cancel", ResponseType.Close);
            fcd.SelectMultiple = false;
            var r = fcd.Run(); // This opens the window and waits for the response
            bool alright = false;
            if (r == (int)ResponseType.Accept) {
                filename = fcd.Filename;
                alright = true;
            }
            fcd.Destroy(); // The dialog does not automatically close when clicking the buttons, so you have to manually close it with this
            return alright;
        }

        static void TestIncbin(MainWindow w){
            string[] resNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            string tout = "";
            foreach (string resName in resNames)
                tout += resName; //Console.WriteLine(resName);
            w.Title = "I got: " + tout;
        }
        public static void Main(string[] args) {
            Application.Init();
            win = new MainWindow();
            win.Title = "MyData - Coded by Tricky";
            TestIncbin(win); // debug ONLY!
            win.Resize(1000, 800);
            if (!ChooseTheFile(win)) return;
            if (!MyDataBase.Load(filename))
            win.Show();

            Application.Run(); 
        }
    }
}
