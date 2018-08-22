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
                tout += resName+"; "; //Console.WriteLine(resName);
            w.Title = "I got: " + tout;
        }

        static void SetIcon(MainWindow w){

            Assembly asm = Assembly.GetExecutingAssembly();
            System.IO.Stream stream = asm.GetManifestResourceStream("MyData.Properties.Icon.png");
            Gdk.Pixbuf PIX = new Gdk.Pixbuf(stream);
            win.Icon = PIX;
        }

        public static void Main(string[] args) {
            Application.Init();
            win = new MainWindow();
            win.Resize(1000, 800);
            SetIcon(win);
            if (!ChooseTheFile(win)) return;
            win.Title = filename+" - MyData - Coded by Tricky";
            //TestIncbin(win); // debug ONLY!
            if (!MyDataBase.Load(filename))
            win.Show();

            Application.Run(); 
        }
    }
}
