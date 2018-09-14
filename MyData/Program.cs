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
// Version: 18.09.13
// EndLic

﻿using System;
using Gtk;
using System.Reflection;
using System.Collections.Generic;
using TrickyUnits;
using TrickyUnits.GTK;
using UseJCR6;



namespace MyData
{
    class MainClass
    {
        // Core crap
        public static Dictionary<string, Export> exportdrivers = new Dictionary<string, Export>();
        public static Dictionary<string, string> exportext = new Dictionary<string, string>();

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
        static public HBox MenuBoxInput = new HBox();
        static readonly Label QI_Label = new Label("---");
        static readonly Entry QI_Input = new Entry();
        static readonly Button QI_Confirm = new Button("Go for it!");
        static string want = "";
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
        public static Dictionary<string, Dictionary<string,int>> mcval2index = new Dictionary<string,Dictionary<string,int>>();
        public static Dictionary<string, ComboBox> mc = new Dictionary<string, ComboBox>();
        public static string sc_rec;

        static MainClass(){
            MKL.Version("MyData For C# - Program.cs","18.09.13");
            MKL.Lic    ("MyData For C# - Program.cs","GNU General Public License 3");
            new JCR6_WAD();
            new JCR6_lzma();
            var rd = new JCR6_RealDir(); rd.automerge = false;
            new JCR_QuakePack();
        }

        static public void Configure(Gdk.EventConfigure args){
            Girl.SetSizeRequest(390, 364);
            MenuBoxRoot.SetSizeRequest(args.Width - 390, 364);
            WorkBox.SetSizeRequest(args.Width, args.Height - 364);
            ListRecords.SetSizeRequest(250, args.Height-364);
            Pages.SetSizeRequest(args.Width - 250, args.Height - 364);
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

        static void OnSelectRecord(object sender, RowActivatedArgs a){
            //var ra = ListRecords.Selection.Data;
            /* debug
            QuickGTK.Info($"Activated {ra}");
            foreach(string k in ra.Keys){
                var v = ra[k];
                QuickGTK.Info($"Key {k}\nValue {v}");
            }
            // */
            TreeSelection selection = (sender as TreeView).Selection;
            TreeModel model;
            TreeIter iter;
            if (selection.GetSelected(out model, out iter)){
                var rec = (model.GetValue(iter, 0) as string);
                //QuickGTK.Info(rec);
                Field2Gui.SelectRecord(rec);
                sc_rec = rec;
                ButRemove.Sensitive = true;
            } else {
                Pages.Sensitive = false;
                ButRemove.Sensitive = false;
                ButRename.Sensitive = false;
                ButDupe.Sensitive = false;
            }
        }

        static void OnRemove(object sender, EventArgs e){
            if (QuickGTK.Confirm($"Are you sure you want to remove {sc_rec}?")){
                MyDataBase.RemoveRec(sc_rec);
                Pages.Sensitive = false;
                ButRemove.Sensitive = false;
                ButForceMod.Sensitive = false;
                sc_rec="";
                //QuickGTK.Info("KILL AND DESTROY!!"); // debug
            }
        }

        static void OnForceMod(object sender, EventArgs e){
            if (sc_rec == "") { ButForceMod.Sensitive = false; return; }
            if (MyDataBase.Record.ContainsKey(sc_rec)){
                MyDataBase.Record[sc_rec].MODIFIED = true;
                //QuickGTK.Info($"Forcemodding {sc_rec}");
                ButForceMod.Sensitive = false;
            }
            else {
                QuickGTK.Error($"ForceModding {sc_rec} failed!");
            }
        }

        static void OnNewRecord(object sender, EventArgs e){
            want = "NEW";
            QI_Label.Text = "Name new record:";
            QI_Input.Text = "";
            MenuBoxInput.Show();
        }

        static void OnDupe(object s,EventArgs e){
            want = "DUPE";
            QI_Label.Text = "Duplicate to new record:";
            QI_Input.Text = "";
            MenuBoxInput.Show();
        }

        static void OnRename(object s, EventArgs e)
        {
            want = "RENAME";
            QI_Label.Text = "Rename as:";
            QI_Input.Text = sc_rec;
            MenuBoxInput.Show();
        }

        static bool QIValidName(string name){
            var ret = true;
            var ca = name.ToUpper().ToCharArray();
            const string allowed = "ABCDEFGHIJKLMNOPQRSTUVWXYZ_1234567890";
            foreach (char ch in ca)
                ret = ret && allowed.IndexOf(ch)>=0;            
            return ret;
        }

        static void OnQIConfirm(object sender, EventArgs e){
            var input = QI_Input.Text;
            var name = input.ToUpper();
            MenuBoxInput.Hide();
            if (!QIValidName(name)) { QuickGTK.Error("Invalid record name!\n\nRecord names may only contain letters, numbers and underscores!"); return; }
            if (MyDataBase.Record.ContainsKey(name)) { if (!QuickGTK.Confirm($"A record named {name} already exists!\n\nIf you continue the old record will be destroyed and replaced by the new one!\n\nAre you sure?")) return; }
            switch(want){
                case "NEW":
                    var newrec = new MyRecord();
                    MyDataBase.Record[name]=newrec;
                    foreach(string k in MyDataBase.fields.Keys){
                        newrec.value[k] = "";
                        if (MyDataBase.defaults.ContainsKey(k)) newrec.value[k] = MyDataBase.defaults[k];
                    }
                    newrec.MODIFIED = true;
                    MyDataBase.UpdateRecView();
                    // TODO: New record should add default values if set.
                    QuickGTK.Info($"Record {name} has been created!");
                    break;
                case "DUPE":
                    var rec = MyDataBase.Record[sc_rec];
                    newrec = new MyRecord();
                    MyDataBase.Record[name] = newrec;
                    foreach (string k in MyDataBase.fields.Keys)
                    {
                        newrec.value[k] =rec.value[k];
                    }
                    newrec.MODIFIED = true;
                    MyDataBase.UpdateRecView();
                    QuickGTK.Info($"Record {sc_rec} has been duplicated into {name}");
                    break;
                case "RENAME":
                    var crec = MyDataBase.Record[sc_rec];
                    MyDataBase.Record.Remove(sc_rec);
                    MyDataBase.Record[name] = crec;
                    crec.MODIFIED = true;
                    MyDataBase.UpdateRecView();
                    QuickGTK.Info($"Record {sc_rec} has been renamed to {name}\n\nWARNING!\nIf you've set MyData to record-by-record export files with the old name will remain!");
                    break;
                default:
                    QuickGTK.Error($"Internal error!\n\n\nInvalid input request --> {want}!");
                    break;
            }
        }

        static void OnSave(object sender,EventArgs e){
            MyDataBase.Save(filename);
            ButSave.Sensitive = false;
        }

        static void rex(string name,string extentie, Export e){
            exportdrivers[name] = e;
            exportext[name] = extentie;
            Console.WriteLine($"Exporter for {name} set up");
        }

        static void InitExporters(){
            rex("LUA", "lua", new ExportLua());
            rex("XML", "xml", new ExportXML());
            rex("YAML", "yaml", new ExportYAML());
            rex("JSON", "json", new ExportJSON());
            rex("PYTHON", "py", new ExportPython());
            rex("PHP", "php", new ExportPHP());
            rex("GINI", "gini", new ExportGINI());
        }

        public static void Main(string[] args) {
            InitExporters();
            Application.Init();
            Window tWin = new Window("MyData");
            tWin.Resize(400, 400);
            if (!ChooseTheFile(tWin)) return;
            tWin.Hide();
            //TestIncbin(win); // debug ONLY!
            if (!MyDataBase.Load(filename)) return;
            win = new MainWindow();
            QuickGTK.MyMainWindow = win;
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
            MenuBoxRoot.Add(MenuBoxRow1); MenuBoxRow1.SetSizeRequest(1000 - 390, 114);
            MenuBoxRoot.Add(MenuBoxRow2); MenuBoxRow2.SetSizeRequest(1000 - 390, 114);
            MenuBoxRoot.Add(MenuBoxRow3); MenuBoxRow3.SetSizeRequest(1000 - 390, 114);
            MenuBoxRow1.Add(ButNew);
            MenuBoxRow1.Add(ButDupe);
            MenuBoxRow1.Add(ButRemove);
            MenuBoxRow2.Add(ButRename);
            MenuBoxRow2.Add(ButForceMod);
            MenuBoxRow2.Add(ButSave);
            MainBox.Add(HeadBox);
            MainBox.Add(WorkBox);
            ButRemove.Clicked += OnRemove;
            ButRemove.Sensitive = false;
            ButRename.Sensitive = false;
            ButRename.Clicked += OnRename;
            ButDupe.Sensitive = false;
            ButDupe.Clicked += OnDupe;
            ButForceMod.Clicked += OnForceMod;
            ButForceMod.Sensitive = false;
            ButNew.Clicked += OnNewRecord;
            ButSave.Sensitive = false;
            ButSave.Clicked += OnSave;
            //ListRecords.SetSizeRequest(250, 800);
            var tvc = new TreeViewColumn();
            var NameCell = new CellRendererText();
            tvc.Title = "Records";
            tvc.PackStart(NameCell, true);
            tvc.AddAttribute(NameCell, "text", 0);
            ListRecords.HeightRequest = 800 - 390;
            ListRecords.AppendColumn(tvc);
            ListRecords.RowActivated += OnSelectRecord;
            //ListRecords.CursorChanged += OnSelectRecord;
            WorkBox.Add(ListRecords);
            WorkBox.Add(Pages);
            //WorkBox.SetSizeRequest(1000, 800 - 390);
            win.Add(MainBox);
            win.Resize(1000, 800);
            Pages.Sensitive = false;
            MenuBoxInput.Add(QI_Label);
            MenuBoxInput.Add(QI_Input);
            MenuBoxInput.Add(QI_Confirm);
            QI_Confirm.SetSizeRequest(250, 21);
            QI_Input.SetSizeRequest(500, 21);
            QI_Label.SetSizeRequest(250, 21);
            QI_Confirm.Clicked += OnQIConfirm;
            MenuBoxRoot.Add(MenuBoxInput);
            win.ShowAll();
            MenuBoxInput.Hide(); // Only show when requested
            Application.Run(); 
        }
    }
}
