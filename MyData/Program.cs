using System;
using Gtk;
using System.Reflection;
using System.Collections.Generic;


namespace MyData
{
    class MainClass
    {
        static public string filename;
        static public MyBase Base = new MyBase();
        public static MainWindow win;
        static VBox MainBox;
        static HBox HeadBox;
        static Image Girl;
        static VBox MenuBoxRoot = new VBox();
        static HBox MenuBoxRow1 = new HBox();
        static HBox MenuBoxRow2 = new HBox();
        static HBox MenuBoxRow3 = new HBox();
        public static Button ButNew = new Button("New Record");
        public static Button ButDupe = new Button("Duplicate Record");
        public static Button ButRemove = new Button("Remove Record");
        public static Button ButRename = new Button("Rename Record");
        public static Button ButForceMod = new Button("Force Modified");
        public static Button ButSave = new Button("Save and Export");
        static HBox WorkBox = new HBox();
        public static TreeView ListRecords = new TreeView();
        public static Notebook Pages = new Notebook();
        public static List<VBox> Panels = new List<VBox>();

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
            stream.Dispose();
            stream = asm.GetManifestResourceStream("MyData.Properties.Icon.png");
            Girl = new Image(stream);
            stream.Dispose();
        }

        public static void Main(string[] args) {
            Application.Init();
            Window tWin = new Window("Schijt");
            tWin.Resize(400, 400);
            if (!ChooseTheFile(tWin)) return;
            tWin.Hide();
            //TestIncbin(win); // debug ONLY!
            if (!MyDataBase.Load(filename)) return;
            win = new MainWindow();
            win.Resize(1000, 800);
            win.Title = filename + " - MyData - Coded by Tricky";
            MainBox = new VBox();
            HeadBox = new HBox();
            SetIcon(win);
            HeadBox.Add(Girl);
            HeadBox.Add(MenuBoxRoot);
            MenuBoxRoot.Add(MenuBoxRow1);
            MenuBoxRoot.Add(MenuBoxRow2);
            MenuBoxRoot.Add(MenuBoxRow3);
            MenuBoxRow1.Add(ButNew);
            MenuBoxRow1.Add(ButDupe);
            MenuBoxRow1.Add(ButRemove);
            MenuBoxRow2.Add(ButRename);
            MenuBoxRow2.Add(ButForceMod);
            MenuBoxRow2.Add(ButSave);
            MainBox.Add(HeadBox);
            MainBox.Add(WorkBox);
            WorkBox.Add(ListRecords);
            WorkBox.Add(Pages);
            win.Add(MainBox);
            win.ShowAll();
            Application.Run(); 
        }
    }
}
