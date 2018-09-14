// Lic:
// 	MyData for C#
// 	GUI build
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
// Version: 18.09.14
// EndLic
﻿using System;
using System.Text;
using System.Collections.Generic;
using TrickyUnits;
using TrickyUnits.GTK;
using Gtk;
namespace MyData
{
    public class Field2Gui
    {
        static bool uneditable = false;
        static Dictionary<object,string> objlink = new Dictionary<object,string>();
        static MyRecord currentrec = null;


        static void OnCombo(object sender, EventArgs e){
            if (uneditable) return;
            var field = objlink[sender];
            var value = (sender as ComboBox).ActiveText;
            currentrec.value[field] = value;
            currentrec.MODIFIED = true;
        }

        static void OnBoolean(object sender,EventArgs e){
            if (uneditable) return;
            var field = objlink[sender];
            if ((sender as RadioButton).Active) {
                currentrec.value[field] = "TRUE";
            } else {
                currentrec.value[field] = "FALSE";
            }
            currentrec.MODIFIED = true;
            //QuickGTK.Info($"Toggle: field = {currentrec.value[field]}"); // debug
        }

        static void OnTxt(object sender,EventArgs e){
            if (uneditable) return;
            if (!objlink.ContainsKey(sender)) { QuickGTK.Error("INTERNAL ERROR!\n\nTextbox not properly bound to memory field!\n\nPlease report!"); return; }
            var field = objlink[sender];
            //var txtf = (sender as TextView);
            var buf = (sender as Entry); //(sender as TextBuffer );//txtf.Buffer;
            var txt = buf.Text;
            int itxt = 0;
            double dtxt = 0;
            //QuickGTK.Info ($"Editing textfield: {field}");//debug
            switch (MyDataBase.fields[field].ToLower()){
                case "string":
                    currentrec.value[field] = txt;
                    break;
                case "int":
                    if (txt != "" && txt != "-")
                    {
                        try
                        {
                            itxt = Int32.Parse(txt);
                        } catch {
                            QuickGTK.Error($"'{txt}' is not a valid integer.");
                            itxt = 0;
                        }
                    }
                    currentrec.value[field] = $"{itxt}";
                    break;
                case "double":
                    if (txt != "" && txt != "-")
                    {
                        try
                        {
                            dtxt = Double.Parse(txt);
                        }
                        catch
                        {
                            QuickGTK.Error($"'{txt}' is not a valid double.");
                            dtxt = 0;
                        }
                    }
                    currentrec.value[field] = $"{dtxt}";
                    break;
                default:
                    QuickGTK.Error($"Internal error!\n\nType {MyDataBase.fields[field].ToLower()} should never have bound itself to a textbox\n\nPlease report this!");
                    break;
            }
            currentrec.MODIFIED = true;
        }

        static Field2Gui()
        {
            MKL.Version("MyData For C# - Field2Gui.cs","18.09.14");
            MKL.Lic    ("MyData For C# - Field2Gui.cs","GNU General Public License 3");
        }

        static public VBox NewPage(string pagename){
            VBox ret = new VBox();
            MainClass.Panels.Add(ret);
            MainClass.Pages.AppendPage(ret, new Label(pagename));
            return ret;
        }

        static public void NewString(VBox pg,string name){
            // Gui
            var tbox = new HBox();
            var tp = new Label("string"); tp.SetSizeRequest(200, 25);
            var nm = new Label(name); nm.SetSizeRequest(400, 25);
            tbox.Add(tp);
            tbox.Add(nm);
            //var ttxt = new TextView(); ttxt.SetSizeRequest(400, 25);
            var ttxt = new Entry(); ttxt.SetSizeRequest(400, 25);
            var col1 = new Gdk.Color(); var suc1 = Gdk.Color.Parse("#b400ff",ref col1); 
            var col2 = new Gdk.Color(); var suc2 = Gdk.Color.Parse("#0b000f",ref col2);
            if (!suc1) tbox.Add(new Label("FG color parse failure!")); // debug only
            if (!suc2) tbox.Add(new Label("BG color parse failure!")); // debug only
            //ttxt.BorderWidth = 2;
            ttxt.ModifyBg(StateType.Normal,col2);
            ttxt.ModifyFg(StateType.Normal,col1);
            MainClass.DStrings[name] = ttxt;
            tbox.Add(ttxt);
            pg.Add(tbox);
            // Database
            MyDataBase.fields[name] = "string";
            MyDataBase.defaults[name] = "";
            //objlink[ttxt.Buffer] = name;
            objlink[ttxt] = name;
            //ttxt.Buffer.Changed += OnTxt;
            ttxt.Changed += OnTxt;

        }

        static public void NewNumber(VBox pg, string numbertype, string name){
            // Gui
            var tbox = new HBox();
            var tp = new Label(numbertype); tp.SetSizeRequest(200, 25);
            var nm = new Label(name); nm.SetSizeRequest(400, 25);
            tbox.Add(tp);
            tbox.Add(nm);
            //var ttxt = new TextView(); ttxt.SetSizeRequest(400, 25);
            var ttxt = new Entry(); ttxt.SetSizeRequest(400, 25);
            var col1 = new Gdk.Color(); var suc1 = Gdk.Color.Parse("#00b4ff", ref col1);
            var col2 = new Gdk.Color(); var suc2 = Gdk.Color.Parse("#000b0f", ref col2);
            if (!suc1) tbox.Add(new Label("FG color parse failure!")); // debug only
            if (!suc2) tbox.Add(new Label("BG color parse failure!")); // debug only
            //ttxt.BorderWidth = 2;
            ttxt.ModifyBg(StateType.Normal, col2);
            ttxt.ModifyFg(StateType.Normal, col1);
            MainClass.DStrings[name] = ttxt; // This is only for widget storage... The types only come into play when exporting.
            tbox.Add(ttxt);
            pg.Add(tbox);
            // Database
            MyDataBase.fields[name] = numbertype;
            MyDataBase.defaults[name] = "";
            //objlink[ttxt.Buffer] = name;
            objlink[ttxt] = name;
            //ttxt.Buffer.Changed += OnTxt;
            ttxt.Changed += OnTxt;
        }

        static public void NewBool(VBox pg,string name){
            var but1 = new RadioButton("True");
            var but2 = new RadioButton(but1, "False");
            var tbox = new HBox();
            var tp = new Label("bool"); tp.SetSizeRequest(200, 25);
            var nm = new Label(name); nm.SetSizeRequest(400, 25);
            tbox.Add(tp);
            tbox.Add(nm);
            var bbox = new HBox(); bbox.SetSizeRequest(400, 25);
            bbox.Add(but1); but1.SetSizeRequest(200, 25);
            bbox.Add(but2); but2.SetSizeRequest(200, 25);
            tbox.Add(bbox);
            pg.Add(tbox);
            MainClass.RBTbools[name] = but1;
            MainClass.RBFbools[name] = but2;
            MyDataBase.fields[name] = "bool";
            MyDataBase.defaults[name] = "TRUE";
            objlink[but1] = name;
            objlink[but2] = name;
            but1.Toggled += OnBoolean;
        }

        /* old
        static public TreeView NewMC(VBox pg,string name){
            var mmc = new TreeView();
            var tbox = new HBox();
            var tvc = new TreeViewColumn();
            var NameCell = new CellRendererText();
            tvc.PackStart(NameCell, true);
            tvc.AddAttribute(NameCell, "text", 0);
            mmc.AppendColumn(tvc);
            tvc.Title = name;
            var tp = new Label("mc"); tp.SetSizeRequest(200, 25); // FYI: mc = Multiple Choice
            var nm = new Label(name); nm.SetSizeRequest(400, 25);
            mmc.SetSizeRequest(400, 25);
            tbox.Add(tp);
            tbox.Add(nm);
            tbox.Add(mmc);

            pg.Add(tbox);
            MyDataBase.fields[name] = "mc";
            MyDataBase.defaults[name] = "";
            mmc.HeadersVisible = false;
            mmc.FixedHeightMode = true;
            mmc.HeightRequest = 30;
            return mmc;
        } */
        static public ComboBox NewMC(VBox pg, string name){
            var mmc = new ComboBox();
            var tbox = new HBox();
            //var NameCell = new CellRendererText();
            //mmc.AddAttribute(NameCell, "text", 0);
            CellRendererText text = new CellRendererText();
            mmc.PackStart(text, false);
            mmc.AddAttribute(text, "text", 0);
            var tp = new Label("mc"); tp.SetSizeRequest(200, 25); // FYI: mc = Multiple Choice
            var nm = new Label(name); nm.SetSizeRequest(400, 25);
            mmc.SetSizeRequest(400, 25);
            tbox.Add(tp);
            tbox.Add(nm);
            tbox.Add(mmc);
            pg.Add(tbox);
            MyDataBase.fields[name] = "mc";
            MyDataBase.defaults[name] = "";
            mmc.HeightRequest = 30;
            MainClass.mc[name] = mmc;
            objlink[mmc] = name;
            mmc.Changed += OnCombo;
            return mmc;

        }

        static public void SelectRecord(string recname)
        {
            uneditable = true;
            // declications
            var rec = MyDataBase.Record[recname];
            currentrec = rec;
            // Activate pages
            MainClass.Pages.Sensitive = true;
            MainClass.ButForceMod.Sensitive = !rec.MODIFIED;
            MainClass.MenuBoxInput.Hide();
            MainClass.ButRemove.Sensitive = true;
            MainClass.ButRename.Sensitive = true;
            MainClass.ButDupe.Sensitive = true;
            // TODO: Full enabling and disabling based on the [ALLOW] tags

            // Strings and other textbox related types
            foreach (string k in MainClass.DStrings.Keys)
            {
                var tv = MainClass.DStrings[k];
                //tv.Buffer.Text = rec.value[k];
                tv.Text = rec.value[k];
            }

            // Booleans
            foreach (string k in MainClass.RBTbools.Keys)
            {
                var btrue = MainClass.RBTbools[k];
                var bfalse = MainClass.RBFbools[k];
                btrue.Active = rec.value[k].ToUpper() == "TRUE";
                bfalse.Active = rec.value[k].ToUpper() != "TRUE";
            }
            // mc
            foreach (string k in MainClass.mc.Keys)
            {
                var mc = MainClass.mc[k];
                var mcid = MainClass.mcval2index;
                if (!mcid.ContainsKey(k)) { QuickGTK.Warn("Empty mc list!"); }
                else
                {
                    var mci = mcid[k];
                    var value = rec.value[k];
                    if (value != "")
                    {
                        if (mci.ContainsKey(value)) mc.Active = mci[value]; else QuickGTK.Error($"The value set for field '{k}' is '{value}', however that value is NOT listed!\n\nSetting ignored!");
                    }
                }

            }
            uneditable = false;
        }
    }
}
