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
// Version: 18.09.16
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
            MKL.Version("MyData For C# - Field2Gui.cs","18.09.16");
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
            var tp = new Label("string"); tp.SetSizeRequest(200, 25); tp.SetAlignment(0, 0);
            var nm = new Label(name.Replace("_","__")); nm.SetSizeRequest(400, 25); nm.SetAlignment(0, 0);
            tbox.Add(tp);
            tbox.Add(nm);
            //var ttxt = new TextView(); ttxt.SetSizeRequest(400, 25);
            var ttxt = new Entry(); ttxt.SetSizeRequest(400, 25);
            var col1 = new Gdk.Color(); var suc1 = Gdk.Color.Parse("#b400ff",ref col1); 
            var col2 = new Gdk.Color(); var suc2 = Gdk.Color.Parse("#0b000f",ref col2);
            if (!suc1) tbox.Add(new Label("FG color parse failure!")); // debug only
            if (!suc2) tbox.Add(new Label("BG color parse failure!")); // debug only
            //ttxt.BorderWidth = 2;
            ttxt.ModifyBase(StateType.Normal,col2);
            ttxt.ModifyText(StateType.Normal,col1);
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

        static void PickColor(object sender, EventArgs e)
        {
            var f = objlink[sender];
            //var rec = MyDataBase.Record[f];
            var c = QuickGTK.SelectColor();
            var myc = MainClass.Colors[f];
            myc.R = c.red;
            myc.G = c.green;
            myc.B = c.blue;
            currentrec.MODIFIED = true;
        }

        static public void NewColor(VBox pg, string name){
            var tbox = new HBox();
            var tp = new Label("color"); tp.SetSizeRequest(200, 25); tp.SetAlignment(0, 0);
            var nm = new Label(name.Replace("_","__")); nm.SetSizeRequest(400, 25); nm.SetAlignment(0, 0);
            tbox.Add(tp);
            tbox.Add(nm);
            var colbox = new HBox(); colbox.SetSizeRequest(400, 25);
            var r = new Entry("0"); r.SetSizeRequest(75, 25);
            var g = new Entry("0"); g.SetSizeRequest(75, 25);
            var b = new Entry("0"); b.SetSizeRequest(75, 25);
            var but = new Button("Pick"); but.SetSizeRequest(100, 25);
            MainClass.Colors[name] = new MyColor(nm, r, g, b);
            objlink[r] = name;
            objlink[g] = name;
            objlink[b] = name;
            objlink[but] = name;
            r.Changed += OnColor;
            g.Changed += OnColor;
            b.Changed += OnColor;
            MyDataBase.fields[name] = "color";
            MyDataBase.defaults[name] = "255;255;255";
            colbox.Add(new Label("R")); colbox.Add(r);
            colbox.Add(new Label("G")); colbox.Add(g);
            colbox.Add(new Label("B")); colbox.Add(b);
            colbox.Add(but);
            but.Clicked += PickColor;
            tbox.Add(colbox);
            pg.Add(tbox);
        }

        static public void NewNumber(VBox pg, string numbertype, string name){
            // Gui
            var tbox = new HBox();
            var tp = new Label(numbertype); tp.SetSizeRequest(200, 25); tp.SetAlignment(0, 0);
            var nm = new Label(name.Replace("_","__")); nm.SetSizeRequest(400, 25); nm.SetAlignment(0, 0);
            tbox.Add(tp);
            tbox.Add(nm);
            //var ttxt = new TextView(); ttxt.SetSizeRequest(400, 25);
            var ttxt = new Entry(); ttxt.SetSizeRequest(400, 25);
            var col1 = new Gdk.Color(); var suc1 = Gdk.Color.Parse("#00b4ff", ref col1);
            var col2 = new Gdk.Color(); var suc2 = Gdk.Color.Parse("#000b0f", ref col2);
            if (!suc1) tbox.Add(new Label("FG color parse failure!")); // debug only
            if (!suc2) tbox.Add(new Label("BG color parse failure!")); // debug only
            //ttxt.BorderWidth = 2;
            ttxt.ModifyBase(StateType.Normal, col2);
            ttxt.ModifyText(StateType.Normal, col1);
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
            var but1 = new RadioButton("True"); but1.SetAlignment(0, 0);
            var but2 = new RadioButton(but1, "False"); but2.SetAlignment(0, 0);
            var tbox = new HBox(); 
            var tp = new Label("bool"); tp.SetSizeRequest(200, 25); tp.SetAlignment(0, 0);
            var nm = new Label(name.Replace("_","__")); nm.SetSizeRequest(400, 25); nm.SetAlignment(0, 0);
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
            var tp = new Label("mc"); tp.SetSizeRequest(200, 25); tp.SetAlignment(0, 0); // FYI: mc = Multiple Choice
            var nm = new Label(name.Replace("_","__")); nm.SetSizeRequest(400, 25); nm.SetAlignment(0, 0);
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

        static void OnColor(object sender, EventArgs e){
            if (uneditable) return;
            var field = objlink[sender];
            var d = MainClass.Colors[field];
            currentrec.value[field] = d.Value; 
            currentrec.MODIFIED = true;
        }

        static void OnDate(object sender, EventArgs e){
            if (uneditable) return;
            var field = objlink[sender];
            var d = MainClass.Dates[field];
            currentrec.value[field] = d.Value;
            currentrec.MODIFIED = true;
            d.weekday.Text =d .DayOfWeek;
        }

        static void OnTime(object sender, EventArgs e){
            if (uneditable) return;
            var field = objlink[sender];
            var d = MainClass.Times[field];
            currentrec.value[field] = d.Value;
            currentrec.MODIFIED = true;
        }

        static string lz(int i, int nullen=0){
            if (nullen == 0) return $"{i}";
            var n = ""; for (int j = 0; j < nullen; j++) n += "0";
            return qstr.Right($"{n}{i}", 2);
        }
        static string[] SR(int start,int eind, int nullen=0){
            var lRet = new List<string>();
            for (int i = start; i <= eind; i++) lRet.Add($"{lz(i,nullen)}");
            return lRet.ToArray();
        }

        static public string[] daysofweek = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };
        static public string[] months = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        static public void NewDate(VBox pg, string name)
        {
            var tbox = new HBox();
            var tp = new Label("date"); tp.SetSizeRequest(200, 25);
            var nm = new Label(name.Replace("_","__")); nm.SetSizeRequest(400, 25);
            var datebox = new HBox(); datebox.SetSizeRequest(400, 25);
            var month = new ComboBox(months);
            var day = new ComboBox(SR(1, 31));
            var year = new ComboBox(SR(MyDate.yearmin,MyDate.yearmax));
            var week = new Label("---");
            tbox.Add(tp);
            tbox.Add(nm);
            tbox.Add(datebox);
            datebox.Add(week);
            datebox.Add(month);
            datebox.Add(day);
            datebox.Add(year);
            objlink[day] = name;
            objlink[month] = name;
            objlink[year] = name;
            MainClass.Dates[name] = new MyDate(week,day, month, year);
            day.Changed += OnDate;
            month.Changed += OnDate;
            year.Changed += OnDate;
            pg.Add(tbox);
        }

        static public void NewTime(VBox pg,string name){
            var tbox = new HBox();
            var tp = new Label("time"); tp.SetSizeRequest(200, 25); tp.SetAlignment(0, 0);
            var nm = new Label(name.Replace("_","__")); nm.SetSizeRequest(400, 25); nm.SetAlignment(0, 0);
            var timebox = new HBox(); timebox.SetSizeRequest(400, 25);
            var uur = new ComboBox(SR(0, 23,2));
            var minuut = new ComboBox(SR(0, 59,2));
            var seconde = new ComboBox(SR(0,59,2));
            timebox.Add(uur);
            timebox.Add(new Label(":"));
            timebox.Add(minuut);
            timebox.Add(new Label(":"));
            timebox.Add(seconde);
            objlink[uur] = name;
            objlink[minuut] = name;
            objlink[seconde] = name;
            uur.Changed += OnTime;
            minuut.Changed += OnTime;
            seconde.Changed += OnTime;
            tbox.Add(tp);
            tbox.Add(nm);
            tbox.Add(timebox);
            MainClass.Times[name] = new MyTime(uur, minuut, seconde);
            pg.Add(tbox);
        }



        static void Dance(string fld, List<string> BanList, bool v){
            var o = v &&( ! BanList.Contains(fld));
            //Print "allow."+fld+" = "+o
            if (!o) BanList.Add(fld); //ListAddLast banlist, fld
            //Select fields.value(fld)
            //        Case "bool"
            //gadfield(fld+".True").setenabled o
            //
            //gadfield(fld+".False").setenabled o
            //
            // Case "color"    
            //gadfield(fld+".Red").setenabled o

            //gadfield(fld+".Green").setenabled o
            //
            //gadfield(fld+".Blue").setenabled o
            //
            //Default
            //gadfield(fld).setenabled o
            //End Select
            foreach(object obj in objlink.Keys){
                if (objlink[obj] == fld)
                {
                    var w = obj as Widget;
                    w.Sensitive = o;
                }
            }
        }// End Function


        static void error(string e){
            QuickGTK.Error(e);
            throw new Exception(e); // Just to force the program to quit!
        }

        public static void RunAllow()
        {
            //var key = "";
            List<string> myList;
            string[] Boolean;
            var v = "";
            //var presult = false;
            var work = "";
            var OutCome = false;
            var r = currentrec.value;
            if (r == null) return;
            //var t = "";
            var Blocked = new List<string>();
            var Allow = MyDataBase.Allow; // I'm lazy I know, I don't need YOU to tell me! :P
            var Fields = MyDataBase.fields;
            var fieldonpage = MyDataBase.fieldonpage;
            var o = false;
            foreach (string key in Allow.Keys)
            {
                myList = Allow[key];//TList(MapValueForKey(Allow, key))
                Boolean = key.Split(' ');
                //'If (Len boolean)<>1 And Len(Boolean)<>3 Error "Misformed Allow condition"
                v = Boolean[0];
                if (qstr.Prefixed(v, "!")) { v = qstr.Right(v, qstr.Len(v) - 1); o = true; }
                switch (Boolean.Length)
                {
                    case 1:
                        switch (Fields[v])
                        {
                            case "string":
                            case "mc":
                            case "color":
                            case "date":
                            case "time":
                                OutCome = r[v] != "";
                                break;
                            case "bool": OutCome = r[v] == "TRUE"; break;
                            case "int":
                            case "double":
                                OutCome = qstr.ToInt(r[v]) != 0;
                                break;
                            case "":
                                error("Non-existent variable: " + v);
                                break; // Unreachable, but required!
                            default:
                                error("Type \"" + Fields[v] + "\" is not (yet) compatible with the [ALLOW] conditional definitions");
                                break;
                        }
                        break;
                    case 3:
                        switch (Boolean[1])
                        {
                            case "=":
                            case "==":
                                OutCome = r[v] == Boolean[2]; //'Print v +" = "+r.value(v)+" = "+boolean[2]+" >>> "+outcome
                                break;
                            case "{}":
                                OutCome = false;
                                foreach (string vvv in Boolean[2].Split(',')) OutCome = OutCome || r[v] == vvv;
                                break;
                            case "!=":
                            case "<>":
                            case "~=":
                            case "=/=":
                                OutCome = r[v] != Boolean[2];
                                break;
                            case "<":
                                switch (Fields[v])
                                {
                                    case "int": OutCome = qstr.ToInt(r[v]) < qstr.ToInt(Fields[Boolean[2]]); break;
                                    case "double": error("double not yet supported for > or < comparing "); break;
                                    //Case "double"   outcome = r.value(v).todouble()<fields.value(boolean[2]).todouble()
                                    default: error("Illegal type"); break;
                                }
                                break;
                            case ">":
                                switch (Fields[v])
                                {
                                    case "int": OutCome = qstr.ToInt(r[v]) > qstr.ToInt(Fields[Boolean[2]]); break;
                                    case "double": error("double not yet supported for > or < comparing "); break;
                                    //Case "double"   outcome = r.value(v).todouble()<fields.value(boolean[2]).todouble()
                                    default: error("Illegal type"); break;
                                }
                                break;
                            case "<=":
                                switch (Fields[v])
                                {
                                    case "int": OutCome = qstr.ToInt(r[v]) <= qstr.ToInt(Fields[Boolean[2]]); break;
                                    case "double": error("double not yet supported for > or < comparing "); break;
                                    //Case "double"   outcome = r.value(v).todouble()<fields.value(boolean[2]).todouble()
                                    default: error("Illegal type"); break;
                                }
                                break;
                            case ">=":
                                switch (Fields[v])
                                {
                                    case "int": OutCome = qstr.ToInt(r[v]) > qstr.ToInt(Fields[Boolean[2]]); break;
                                    case "double": error("double not yet supported for > or < comparing "); break;
                                    //Case "double"   outcome = r.value(v).todouble()<fields.value(boolean[2]).todouble()
                                    default: error("Illegal type"); break;
                                }
                                break;
                            default:
                                error("Unknown operator");
                                break;
                        }
                        break;
                    default: error("Misformed Allow condition"); break;
                }
                if (o) OutCome = !OutCome;
                bool oc;
                foreach (string wwork in Allow[key])
                {
                    work = wwork;
                    oc = OutCome;
                    if (qstr.Prefixed(work, "!")) { work = qstr.RemPrefix(work, "!"); oc = !OutCome; }
                    //'presult = outcome And (Not ListContains(banned,work))  
                    if (qstr.Prefixed(work, "PREFIX:"))
                    {
                        foreach (string w in Fields.Keys)
                            if (qstr.RemPrefix(work, "PREFIX:") == w) Dance(w, Blocked, oc);
                    }
                    else if (qstr.Prefixed(work, "PAGE:"))
                    {
                        //Print "Work with "+work+" >>> "+RemPrefix(work,"PAGE:")
                        foreach (string w in fieldonpage.Keys)
                        {
                            //'Print w+" = "+fieldonpage.value(w)+" "+remprefix(work,"PAGE:")+" "+Int(Trim(remprefix(work,"PAGE:"))=fieldonpage.value(w))
                            if (qstr.MyTrim(qstr.RemPrefix(work, "PAGE:")) == fieldonpage[w]) Dance(w, Blocked, oc);
                        }
                    }
                    else
                    {
                        Dance(work, Blocked, oc);
                    }
                }
            }
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
                if (!rec.value.ContainsKey(k)) { QuickGTK.Warn($"{MyDataBase.fields[k]} contains no data in this record! Creating data!");
                    if (MyDataBase.fields[k] != "string") rec.value[k] = ""; else rec.value[k] = "0";
                }
                var tv = MainClass.DStrings[k];
                //tv.Buffer.Text = rec.value[k];
                tv.Text = rec.value[k];
            }

            // Booleans
            foreach (string k in MainClass.RBTbools.Keys)
            {
                var btrue = MainClass.RBTbools[k];
                var bfalse = MainClass.RBFbools[k];
                if (!rec.value.ContainsKey(k))
                {
                    QuickGTK.Warn($"{MyDataBase.fields[k]} contains no data in this record! Creating data!");
                    rec.value[k] = "TRUE";
                }

                btrue.Active = rec.value[k].ToUpper() == "TRUE";
                bfalse.Active = rec.value[k].ToUpper() != "TRUE";
            }
            // mc
            foreach (string k in MainClass.mc.Keys)
            {
                var mc = MainClass.mc[k];
                var mcid = MainClass.mcval2index;
                if (!rec.value.ContainsKey(k))
                {
                    QuickGTK.Warn($"{MyDataBase.fields[k]} contains no data in this record! Creating data!");
                    rec.value[k] = "";
                }
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
            foreach (string k in MainClass.Dates.Keys)
            {
                if (!rec.value.ContainsKey(k))
                {
                    QuickGTK.Warn($"{MyDataBase.fields[k]} contains no data in record '{recname}'! Creating data!");
                    rec.value[k] = "19/6/1975";
                }
                MainClass.Dates[k].Value = rec.value[k];
            }
            foreach (string k in MainClass.Times.Keys)
            {
                if (!rec.value.ContainsKey(k))
                {
                    QuickGTK.Warn($"{MyDataBase.fields[k]} contains no data in record '{recname}'! Creating data!");
                    rec.value[k] = "01:02:03";
                }
                MainClass.Times[k].Value = rec.value[k];
            }
            foreach (string k in MainClass.Colors.Keys)
            {
                if (!rec.value.ContainsKey(k))
                {
                    QuickGTK.Warn($"{MyDataBase.fields[k]} contains no data in record '{recname}'! Creating data!");
                    rec.value[k] = "255;255;255";
                }
                MainClass.Colors[k].Value = rec.value[k];
            }
            RunAllow();
            uneditable = false;
        }
    }
}


/* Below is the original BlitzMax code of the allow feature, only placed here for reference purposes, and making my life easier :P

  Function Dance(fld$,BanList:TList,v:Byte)
    Local o = v And (Not ListContains(banlist,fld))
    Print "allow."+fld+" = "+o  
    If Not o ListAddLast banlist,fld
    Select fields.value(fld)
        Case "bool"
            gadfield(fld+".True").setenabled o
            gadfield(fld+".False").setenabled o
        Case "color"    
            gadfield(fld+".Red").setenabled o
            gadfield(fld+".Green").setenabled o
            gadfield(fld+".Blue").setenabled o
        Default
            gadfield(fld).setenabled o
    End Select
End Function        
    

Function RunAllow()
    Local key$,List:TList
    Local Boolean$[],v$,o,presult,work$
    Local OutCome:Byte
    Local r:StringMap = rec(currentrec)
    If Not r Return
    Local t$
    Local Blocked:TList = New TList
    For key$ = EachIn MapKeys(Allow)
        list = TList(MapValueForKey(Allow,key))
        boolean = key.split(" ")
        'If (Len boolean)<>1 And Len(Boolean)<>3 Error "Misformed Allow condition"
        v = boolean[0]
        If Prefixed(v,"!") v=Right(v,Len(v)-1); o=1
        Select (Len Boolean)
            Case 1              
                Select Fields.value(v)
                    Case "string","mc","color"
                            outcome = r.value(v)<>""
                    Case "bool" outcome = r.value(v)="TRUE"
                    Case "int","double" 
                            outcome = r.value(v).toInt()<>0
                    Case ""
                            error "Non-existent variable: "+v
                    Default     error "Type ~q"+fields.value(v)+"~q is not (yet) compatible with the [ALLOW] conditional definitions"
                End Select
            Case 3
                Select Boolean[1]
                    Case "=","=="
                            outcome = r.value(v)=boolean[2]; 'Print v +" = "+r.value(v)+" = "+boolean[2]+" >>> "+outcome
                    Case "{}"
                            outcome = False
                            For Local vvvv$=EachIn boolean[2].split(",") 
                                outcome = outcome Or r.value(v)=vvvv
                            Next            
                    Case "!=","<>","~~=","=/="
                            outcome = r.value(v)=boolean[2] 
                    Case "<"
                            Select fields.value(v)
                                Case "int"  outcome = r.value(v).toint()<fields.value(boolean[2]).toint()
                                Case "double"   outcome = r.value(v).todouble()<fields.value(boolean[2]).todouble()
                                Default error "Illegal type"
                            End Select      
                    Case ">"
                            Select fields.value(v)
                                Case "int"  outcome = r.value(v).toint()>fields.value(boolean[2]).toint()
                                Case "double"   outcome = r.value(v).todouble()>fields.value(boolean[2]).todouble()
                                Default error "Illegal type"
                            End Select      
                    Case "<="
                            Select fields.value(v)
                                Case "int"  outcome = r.value(v).toint()<=fields.value(boolean[2]).toint()
                                Case "double"   outcome = r.value(v).todouble()<=fields.value(boolean[2]).todouble()
                                Default error "Illegal type"
                            End Select      
                    Case ">="
                            Select fields.value(v)
                                Case "int"  outcome = r.value(v).toint()>=fields.value(boolean[2]).toint()
                                Case "double"   outcome = r.value(v).todouble()>=fields.value(boolean[2]).todouble()
                                Default error "Illegal type"
                            End Select  
                    Default     Error "Unknown operand: "+boolean[1]    
                End Select                                                  
            Default Error "Misformed Allow condition"
        End Select  
        If o outcome = Not outcome
        Local oc
        For Local wwork$ = EachIn TList(MapValueForKey(allow,key))
            work = wwork
            oc = outcome
            If Prefixed(work,"!") work=RemPrefix(work,"!") oc = Not outcome
            'presult = outcome And (Not ListContains(banned,work))  
            If Prefixed(work,"PREFIX:")
                For Local w$=EachIn MapKeys(fields)
                    If RemPrefix(work,"PREFIX:")=w dance w,blocked,oc
                Next
            ElseIf Prefixed(work,"PAGE:")
                Print "Work with "+work+" >>> "+RemPrefix(work,"PAGE:")
                For Local w$=EachIn MapKeys(fieldonpage)
                    'Print w+" = "+fieldonpage.value(w)+" "+remprefix(work,"PAGE:")+" "+Int(Trim(remprefix(work,"PAGE:"))=fieldonpage.value(w))
                    If Trim(RemPrefix(work,"PAGE:"))=fieldonpage.value(w) dance w,blocked,oc
                Next
            Else
                dance work,blocked,oc   
            EndIf                   
        Next    
    Next
End Function
*/
