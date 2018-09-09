// Lic:
// 	MyData for C#
// 	Main Program
// 	
// 	
// 	
// 	(c) Jeroen P. Broks, 2018, All rights reserved
// 	
// 		This program is free software: you can redistribute it and/or modify
// 		it under the terms of the GNU General Public License as published by
// 		the Free Software Foundation, either version 3 of the License, or
// 		(at your option) any later version.
// 		
// 		This program is distributed in the hope that it will be useful,
// 		but WITHOUT ANY WARRANTY; without even the implied warranty of
// 		MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// 		GNU General Public License for more details.
// 		You should have received a copy of the GNU General Public License
// 		along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 		
// 	Exceptions to the standard GNU license are available with Jeroen's written permission given prior 
// 	to the project the exceptions are needed for.
// Version: 18.09.09
// EndLic

﻿using System;
using Gtk;
using System.Reflection;
using System.Collections.Generic;
using TrickyUnits;
using UseJCR6;



namespace MyData
{
    class MainClass
    {
        // Core crap
        public static Dictionary<string, export> exportdrivers = new Dictionary<string, export>();

        // GUI
        public const int winwidth = 1250;
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
        public static Dictionary<string, TextView> DStrings = new Dictionary<string, TextView>();
        public static Dictionary<string, RadioButton> RBTbools = new Dictionary<string, RadioButton>();
        public static Dictionary<string, RadioButton> RBFbools = new Dictionary<string, RadioButton>();
        public static Dictionary<string, TreeView> mc = new Dictionary<string, TreeView>();

        static MainClass(){
            MKL.Version("MyData For C# - Program.cs","18.09.09");
            MKL.Lic    ("MyData For C# - Program.cs","GNU General Public License 3");
            new JCR6_WAD();
            new JCR6_lzma();
            var rd = new JCR6_RealDir(); rd.automerge = false;
            new JCR_QuakePack();
        }

        static public void Configure(Gdk.EventConfigure args){
            Girl.SetSizeRequest(390, 364);
            MenuBoxRoot.SetSizeRequest(args.Width - 390, 364);
            ListRecords.SetSizeRequest(250, args.Height-390);
            WorkBox.SetSizeRequest(args.Width - 250, args.Height - 390);
        }

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
            Window tWin = new Window("MyData");
            tWin.Resize(400, 400);
            if (!ChooseTheFile(tWin)) return;
            tWin.Hide();
            //TestIncbin(win); // debug ONLY!
            if (!MyDataBase.Load(filename)) return;
            win = new MainWindow();
            win.Title = filename + " - MyData - Coded by Tricky";
            MainBox = new VBox();
            HeadBox = new HBox();
            SetIcon(win);
            ButNew.SetSizeRequest(203, 121);
            ButDupe.SetSizeRequest(203, 121);
            ButRemove.SetSizeRequest(203, 121);
            ButRename.SetSizeRequest(203, 121);
            ButForceMod.SetSizeRequest(203, 121);
            ButSave.SetSizeRequest(203, 121);
            HeadBox.Add(Girl);
            HeadBox.Add(MenuBoxRoot);
            //Girl.SetSizeRequest(390, 364);
            //MenuBoxRoot.SetSizeRequest(1000 - 390, 364);
            MenuBoxRoot.Add(MenuBoxRow1); MenuBoxRow1.SetSizeRequest(1000 - 390, 121);
            MenuBoxRoot.Add(MenuBoxRow2); MenuBoxRow2.SetSizeRequest(1000 - 390, 121);
            MenuBoxRoot.Add(MenuBoxRow3); MenuBoxRow3.SetSizeRequest(1000 - 390, 121);
            MenuBoxRow1.Add(ButNew);
            MenuBoxRow1.Add(ButDupe);
            MenuBoxRow1.Add(ButRemove);
            MenuBoxRow2.Add(ButRename);
            MenuBoxRow2.Add(ButForceMod);
            MenuBoxRow2.Add(ButSave);
            MainBox.Add(HeadBox);
            MainBox.Add(WorkBox);
            //ListRecords.SetSizeRequest(250, 800);
            WorkBox.Add(ListRecords);
            WorkBox.Add(Pages);
            //WorkBox.SetSizeRequest(1000, 800 - 390);
            win.Add(MainBox);
            win.Resize(1000, 800);
            win.ShowAll();
            Application.Run(); 
        }
    }
}
