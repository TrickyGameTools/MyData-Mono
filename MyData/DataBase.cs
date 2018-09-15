// Lic:
// 	MyData for C#
// 	Database
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
// Version: 18.09.15
// EndLic

﻿using TrickyUnits;
using TrickyUnits.GTK;
using UseJCR6;
using System;
using System.IO;
using System.Collections.Generic;
using Gtk;

namespace MyData
{

    public class MyRecord
    {
        public SortedDictionary<string, string> value = new SortedDictionary<string, string>();
        bool trueMODIFIED = false;
        public bool MODIFIED
        {
            get => trueMODIFIED; set
            {
                trueMODIFIED = value;
                MainClass.ButForceMod.Sensitive = !trueMODIFIED;
                MainClass.MenuBoxInput.Hide();
                MainClass.ButSave.Sensitive = true;
            }
        }
    }


    public class MyBase
    {
        public SortedDictionary<string, MyRecord> records = new SortedDictionary<string, MyRecord>();
    }

    class MyDate
    {
        ComboBox day;
        ComboBox month;
        ComboBox year;
        public Label weekday;
        public const int yearmin = 1800;
        public const int yearmax = 2100;
        int dayofweek(int d, int m, int y)
        {
            // I translated this routine from "pure c", and I hope it works here. ;)
            int[] t = { 0, 3, 2, 5, 0, 3, 5, 1, 4, 6, 2, 4 };
            if (m < 3) y--; // -= m < 3;
            return (y + y / 4 - y / 100 + y / 400 + t[m - 1] + d) % 7;
        }
        public int Day
        {
            get
            {
                var value = day.ActiveText;
                return Int32.Parse(value);
            }
            set { day.Active = value - 1;  }
        }
        public string MonthString { get => month.ActiveText;  }
        public int Month
        {
            get
            {
                for (int m = 0; m < 12; m++)
                {
                    if (Field2Gui.months[m] == MonthString) return m + 1;
                }
                return 0;
            }
            set { month.Active = value - 1;  }

        }
        public int Year
        {
            get => Int32.Parse(year.ActiveText);
            set
            {
                var i = (value - yearmin);
                year.Active = i;

            }
        }
        public string DayOfWeek
        {
            get
            {
                var ret = "ERROR!";
                var dow = dayofweek(Day, Month, Year);
                if (dow < 7) ret = Field2Gui.daysofweek[dow];
                return ret;
            }
        }

        /// <summary>
        /// Retrieves or sets date into or from the UI Comboboxes
        /// </summary>
        /// <value>The date in mm/dd/yyyy format WITHOUT leading zeros!!!</value>
        public string Value
        {
            get => $"{Day}/{Month}/{Year}";
            set
            {
                //QuickGTK.Info($"Hello, I am going to change the date in this record to {value} - Ready?");
                int[] monthdays = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
                //                   J   F   M   A   M   J   J   A   S   O   N   D
                var defaultdate = "19/6/1975";
                var a = value.Split('/');
                var sd = 19;
                var sm = 6;
                var sy = 1975;
                //QuickGTK.Info($"{value} -- Initial stuff done, let's go!");
                if (a.Length != 3)
                {
                    QuickGTK.Error("Invalid date, resorting to standard date!");
                    a = defaultdate.Split('/');
                }
                try
                {
                    sd = Int32.Parse(a[0]);
                    sm = Int32.Parse(a[1]);
                    sy = Int32.Parse(a[2]);
                }
                catch
                {
                    QuickGTK.Error("Invalid date. Parsing date data failed!");
                }
                if (sy < yearmin || sy > yearmax) { QuickGTK.Error("Year out of range!"); sy = 1975; }
                if (sm < 1 || sm > 12) { QuickGTK.Error("Month value out of range in date!"); sm = 6; }
                var maxdays = monthdays[sm - 1];
                if (sm == 2)
                {
                    if ((sy % 4 == 0 && sy % 100 != 0) || (sy % 400 == 0)) { maxdays = 29; } // leap years every 4 years, but when on 100 round, only once in 400 years. 2000 was leap year, 2100 was not!
                }
                if (sd < 1 || sd > maxdays) { QuickGTK.Error($"In the year {sy} the month {Field2Gui.months[sm]} only had {maxdays} days and not {sd}!"); sd = 19; }
                //QuickGTK.Info($"Ready?\n\n {sd} - {sm} - {sy} ");
                Day = sd; //QuickGTK.Info($"Day set to {sd}");
                Month = sm; //QuickGTK.Info($"Month set to {sm}");
                Year = sy; //QuickGTK.Info($"Year set to {sy}");
                weekday.Text = DayOfWeek;
                //QuickGTK.Info("Done!");

            }
        }
        public MyDate(Label w,ComboBox d, ComboBox m, ComboBox y)
        {
            day = d;
            month = m;
            year = y;
            weekday = w;
        }

    }

    public class MyDataBase
    {
        //static public Dictionary<string, Export> Exporters = new Dictionary<string, Export>();
        static readonly ExportMyData MySave = new ExportMyData();
        static public Dictionary<string, string> defaults = new Dictionary<string, string>();
        static public Dictionary<string, string> fields = new Dictionary<string, string>();
        static public SortedDictionary<string, MyRecord> Record = new SortedDictionary<string, MyRecord>();
        static public SortedDictionary<string, string> MyStructure = new SortedDictionary<string, string>();
        static public Dictionary<string, string> sys = new Dictionary<string, string>();
        static public Dictionary<string, string> recexport = new Dictionary<string, string>();
        static public Dictionary<string, string> basexport = new Dictionary<string, string>();
        static SortedDictionary<string, MyRecord> recs { get => Record; }
        static public bool RemoveNonExistent
        {
            get
            {
                return (sys.ContainsKey("REMOVENONEXISTENT")) && ( sys["REMOVENONEXISTENT"].ToLower() == "yes" || sys["REMOVENONEXISTENT"].ToLower() == "true");
            }
        }

        static public string License
        {
            get
            {
                if (sys.ContainsKey("LICENSE")) return sys["LICENSE"]; else return "<< No License information present >>";
            }
        }

        static MyDataBase()
        {
            MKL.Version("MyData For C# - DataBase.cs","18.09.15");
            MKL.Lic    ("MyData For C# - DataBase.cs","GNU General Public License 3");
        }


        static void CRASH(string message){
            MessageDialog md = new MessageDialog(MainClass.win,
                            DialogFlags.DestroyWithParent, MessageType.Error,
                            ButtonsType.Close, message);
            md.Run();
            md.Destroy();
        }

        static bool ListContains(string [] list, string value){
            foreach (string vcheck in list){
                if (vcheck == value) return true;
            }
            return false;
        }

        static bool exportcheck(string sexp)
        {
            var ok = MainClass.exportdrivers.ContainsKey(sexp);
            if (!ok) { CRASH($"This database instructs me to export to {sexp}, however I do not have the proper export drivers for that.\n\nPossibly a typo in the system block or an outdated version of MyData.\n\nThe instruction has been ignored!"); }
            return ok;
        }

        public static void UpdateRecView(){
            var lst = new ListStore(typeof(string));
            foreach(string k in recs.Keys)
                lst.AppendValues(k);
            MainClass.ListRecords.Model = lst;
        }

        public static void RemoveRec(string record){
            recs.Remove(record);
            UpdateRecView();
        }

        static int mcidx = 0;
        static string mcname = "";
        static ListStore CurrentListStore = null;
        static void regmc(string value){
            var m2i = MainClass.mcval2index;
            CurrentListStore.AppendValues(value);
            if (!m2i.ContainsKey(mcname)) m2i[mcname] = new Dictionary<string, int>();
            if (m2i[mcname].ContainsKey(value))
            {
                QuickGTK.Warn($"Field \"{mcname}\" contains multiple items labelled \"{value}\"!");
            }
            else
            {
                Console.WriteLine($"Registering '{value}' to index '{mcidx}' in field '{mcname}'");
                m2i[mcname][value] = mcidx; mcidx++;
            }
        }
        public static bool Load(string filename){
            bool ret = true;
            string[] lines;
            string OnlyAllowExt = "";
            string[] OnlyAllowExtList = null;
            string OnlyAllowPath = "";
            string[] OnlyAllowPathList = null;
            string OnlyAllowPrefix = "";
            try
            {
                lines = System.IO.File.ReadAllLines(filename);
            } catch {
                MessageDialog md = new MessageDialog(MainClass.win,
                                DialogFlags.DestroyWithParent, MessageType.Error,
                                ButtonsType.Close, "Error loading file");
                md.Run();
                md.Destroy();
                return false;
            }
            int linecount = 0;
            string TL;
            string Chunk = "UNKNOWN";
            int cpage = 0;
            int pagey = 0; // Not sure if this is needed, but I need to be sure
            string pagename = "";
            VBox CurrentPanel = null; // This definition is absolutely LUDICROUS, but it prevents a "Use of unassinged local variable" error....
            //TreeView CurrentMC = null;
            ComboBox CurrentMC = null;
            MyRecord TRec=null;
            CurrentListStore = null;
            foreach (string L in lines)
            {
                linecount++;
                TL = L.Trim();
                if (TL.Length > 0 && TL.Substring(0, 1) != "#" && TL.Substring(0, 2) != "//" && TL.Substring(0, 2) != "--")
                { // No empty lines and no comment either.
                    if (TL.Substring(0, 1) == "[" && TL.Substring(TL.Length - 1, 1) == "]")
                    {

                        if (TL.ToUpper() == "[SYSTEM]")
                        {
                            Chunk = "System";
                        }
                        else if (TL.Length >= 6 && TL.ToUpper().Substring(0, 6) == "[PAGE:")
                        {
                            Chunk = "Structure";
                            cpage++;
                            pagey = 0;
                            pagename = TL.Substring(6, TL.Length - 7).Trim();
                            //if (CurrentPanel != null) { CurrentPanel.Add(new HBox()); }
                            CurrentPanel = Field2Gui.NewPage(pagename);
                        }

                        else if (TL.ToUpper() == "[RECORDS]")
                        {
                            Chunk = "Records";
                        }
                        else if (TL.ToUpper() == "[DEFAULT]")
                        {
                            Chunk = "Default";
                        }
                        else if (TL.Length > 7 && TL.ToUpper().Substring(0, 7) == "[ALLOW:")
                        {
                            Chunk = "Allow";
                        }
                        else
                        {
                            CRASH("Unknown Chunk definition in line #" + linecount + "\n\n" + TL);
                            return false;
                        }
                    }
                    else
                    {
                        string[] SL;
                        switch (Chunk)
                        {
                            case "System":
                                var posIS = TL.IndexOf('=');
                                if (posIS < 0) { CRASH("Invalid System Var Definition!"); return false; }
                                var svar = qstr.Left(TL, posIS ).Trim().ToUpper();
                                var sval = qstr.Right(TL, TL.Length - (posIS+1)).Trim();
                                //CRASH($"svar={svar}; sval={sval}");
                                var sexp = "";
                                if (qstr.Prefixed(svar,"OUTPUT") && qstr.Suffixed(svar,"BASE")) {
                                    sexp = qstr.Mid(svar, 7, svar.Length - 10);
                                    if (exportcheck(sexp)) { basexport[sexp] = sval; }
                                } else if (qstr.Prefixed(svar, "OUTPUT") && qstr.Suffixed(svar, "REC")) {
                                    sexp = qstr.Mid(svar, 7, svar.Length - 9);
                                    if (exportcheck(sexp)) { recexport[sexp] = sval; }
                                }
                                else {
                                    sys[svar] = sval;
                                }
                                    break;
                            case "Default":
                                posIS = TL.IndexOf('=');
                                if (posIS < 0) { CRASH("Invalid Default Var Definition!"); return false; }
                                var dvar = qstr.Left(TL, posIS).Trim().ToUpper();
                                var dval = qstr.Right(TL, TL.Length - (posIS + 1)).Trim();
                                defaults[dvar] = dval;
                                break;
                            case "Structure":
                                var TTL = TL;
                                TTL = TTL.Replace("\t", " ");
                                string OTL;
                                do { OTL = TTL; TTL = TTL.Replace("  ", " "); } while (OTL != TTL);
                                SL = TTL.Split(' ');
                                SL[0] = SL[0].ToLower();
                                if (SL[0] != "strike" && SL.Length < 2)
                                {
                                    CRASH("Invalid structure field declaration in line #" + linecount + "\n\n" + TL);
                                    return false;
                                }
                                if (qstr.Left(SL[0], 1) != "@" && qstr.Left(TL, 6).ToLower() != "strike" && qstr.Left(TL, 4).ToLower() != "info")
                                {
                                    if (fields.ContainsKey(SL[1])){ CRASH("Duplicate field: " + SL[1]); return false; }
                                    fields[SL[1]] = SL[0].ToLower();
                                    // MapInsert fieldonpage, SL[1], pagename
                                }
                                switch (SL[0])
                                {
                                    case "strike": break;
                                    case "info":
                                        CurrentPanel.Add(new Label(TL.Substring(5, TL.Length - 5)));
                                        pagey += 25;
                                        break;
                                    case "date":
                                        if (SL.Length != 2) { CRASH("Invalid date declaration in line #" + linecount + "\n\n" + TL); return false; }
                                        Field2Gui.NewDate(CurrentPanel, SL[1]);
                                        pagey += 25;
                                        break;
                                    case "string":
                                        if (SL.Length != 2) { CRASH("Invalid string declaration in line #" + linecount + "\n\n" + TL); return false; }
                                        Field2Gui.NewString(CurrentPanel, SL[1]);
                                        break;
                                    case "int":
                                    case "double":
                                        if (SL.Length != 2) { CRASH("Invalid number declaration in line #" + linecount + "\n\n" + TL); return false; }
                                        Field2Gui.NewNumber(CurrentPanel, SL[0], SL[1]);
                                        break;
                                    case "bool":
                                        if (SL.Length != 2) { CRASH("Invalid boolean declaration in line #" + linecount + "\n\n" + TL); return false; }
                                        Field2Gui.NewBool(CurrentPanel, SL[1]);
                                        break;
                                    case "mc":
                                        if (SL.Length != 2) { CRASH("Invalid multiple choice declaration in line #" + linecount + "\n\n" + TL); return false; }
                                        if (CurrentMC != null) CurrentMC.Model = CurrentListStore;
                                        CurrentMC = Field2Gui.NewMC(CurrentPanel, SL[1]);
                                        CurrentListStore = new ListStore(typeof(string));
                                        mcidx = 0;
                                        mcname = SL[1];
                                        break;
                                    case "@i":
                                        if (CurrentMC == null) { CRASH("Add item request without a multiple choice declaration in line #" + linecount); return false; }
                                        if (TL.Length < 4) { CRASH("Invalid item in line #" + linecount); return false; }
                                        /*
                                        CurrentMC.AppendText(TL.Substring(3, TL.Length - 3));
                                        CurrentMC.Active = 0;
                                        */
                                        var itext = TL.Substring(3, TL.Length - 3);
                                        // CRASH("Test @i!\n"+itext);
                                        regmc(itext);
                                        //CurrentMC.Add(new Label(itext));
                                        break;
                                    case "@f":
                                        itext = TL.Substring(3, TL.Length - 3);
                                        //CRASH("I cannot yet add the items of \"" + itext + "\" into the multiple choice field, because this requires JCR6 support and JCR6 libraries are (at the present time) non-existent in C#.\n\nI do have plans to make it happen for C#, but I cannot do all at once, ya know!");
                                        if (CurrentMC == null)
                                        {
                                            CRASH("No list to add this item to in line #" + linecount); return false;
                                        }
                                        //Print "Importing JCR: " + Trim(Right(TL, Len(TL) - 3))
                                        var JD = JCR6.Dir(itext); //qstr.MyTrim(qstr.Right(TL, qstr.Len(TL) - 3)));
                                        var Ok = true;
                                        if (JD == null)
                                        {
                                            CRASH("JCR could not read file: " + qstr.MyTrim(qstr.Right(TL, qstr.Len(TL) - 3)) + "\n" + JCR6.JERROR); return false;
                                        }
                                        //'For Local F$=EachIn OnlyAllowPathlist DebugLog "Dir Allowed: "+f; Next
                                        //'For Local F$=EachIn OnlyAllowextlist  DebugLog "Ext Allowed: "+F; Next
                                        CRASH($"JCR6 {itext} has {JD.CountEntries} entries"); // debug
                                        foreach ( string DK in JD.Entries.Keys) //For Local D: TJCREntry = EachIn MapValues(JD.Entries)
                                        {
                                            TJCREntry DE = JD.Entries[DK];
                                            /*
                                  Rem Old and inefficient
                                If Not OnlyAllowExtList
                                    Print "Adding entry: " + D.FileName
                                    AddGadgetItem lastlist, D.FileName
                                 ElseIf ListContains(OnlyAllowExtList, Upper(ExtractExt(D.FileName)))
                                    Print "Adding entry: " + D.FileName + " (approved by the ext filter)"
                                    AddGadgetItem lastlist, D.FileName
                                     Else
                                    'Print " Disapproved: "+D.FileName+"  ("+Upper(ExtractExt(D.FileName))+" is not in the filterlist"+")"
                                    EndIf
                                End Rem*/
                                            Ok = true;//                                                                                                '               '
                                            if (OnlyAllowExtList != null && OnlyAllowExtList.Length>0) Ok = Ok && ListContains(OnlyAllowExtList, qstr.Upper(qstr.ExtractExt(DE.Entry)));//'DebugLog "    Ext Check: "+D.FileName+" >>> "+Ok
                                            if (OnlyAllowPathList != null && OnlyAllowPathList.Length>0) Ok = Ok && ListContains(OnlyAllowPathList, qstr.Upper(qstr.ExtractDir(DE.Entry))); //'DebugLog "    Dir Check: "+D.FileName+" >>> "+Ok
                                            if (OnlyAllowPrefix != "") Ok = Ok && qstr.Prefixed(DE.Entry, OnlyAllowPrefix);
                                            CRASH($"Entry: {DE.Entry} / {Ok}");
                                            if (Ok)
                                            {
                                                regmc(DE.Entry); //CurrentListStore.AppendValues(DE.Entry); //AddGadgetItem lastlist,D.FileName
                                                                                         //                                    'DebugLog "Added to list: "+D.filename
                                            }
                                            //else
                                            //{
                                                //'DebugLog "     Rejected: "+D.filename
                                            //}//EndIf
                                        }//Next

                                        break;
                                    case "@db": // Please note, I just translated the code from BlitzMax as literally as I could.
                                        if (CurrentMC == null) { CRASH("No list to add database items to in line #" + linecount); return false; }
                                        // Print "Importing database: " + Trim(Right(TL, Len(TL) - 4))
                                        var sf = TL.Substring(4, TL.Length - 4);
                                        if (!(sf.Substring(0, 1) == "/" || sf.Substring(0, 1) == @"\" || sf.Substring(0, 2) == ":"))
                                        {
                                            sf = System.IO.Path.GetDirectoryName(filename) + "/" + sf;
                                        }
                                        var sdb = System.IO.File.ReadAllLines(sf);
                                        var readrec = false;
                                        foreach (string l in sdb)
                                        {
                                            if (l.ToUpper() == "[RECORDS]")
                                            {
                                                readrec = true;
                                            }
                                            else if (l.Length > 0 && l.Substring(0, 1) == "[")
                                            {
                                                readrec = false;
                                            }
                                            if (l.Length >= 5 && l.Substring(0, 5) == "Rec: " && readrec) regmc(l.Substring(4, l.Length - 4)); //CurrentListStore.AppendValues(l.Substring(4, l.Length - 4));
                                        }
                                        break;
                                    case "@noextfilter":
                                        OnlyAllowExt = "";
                                        OnlyAllowExtList = null;
                                        break;
                                    case "@nopathfilter":
                                    case "@nodirfilter":
                                        OnlyAllowPath = "";
                                        OnlyAllowPathList = null;
                                        break;
                                    case "@extfilter":
                                        OnlyAllowExt = qstr.Right(TL, qstr.Len(TL) - qstr.Len("@extfilter ")).ToUpper().Trim();
                                        OnlyAllowExtList = OnlyAllowExt.Split(',');
                                        break;
                                    case "@pathfilter":
                                        OnlyAllowPath = qstr.Right(TL, qstr.Len(TL) - qstr.Len("@pathfilter ")).ToUpper().Trim();
                                        OnlyAllowPathList = OnlyAllowPath.Split(',');
                                        break;
                                    case "@dirfilter":
                                        OnlyAllowPath = qstr.Right(TL, qstr.Len(TL) - qstr.Len("@dirfilter ")).ToUpper().Trim();
                                        OnlyAllowPathList = OnlyAllowPath.Split(',');
                                        break;
                                    case "@prefix":
                                        OnlyAllowPrefix = qstr.Right(TL, qstr.Len(TL) - qstr.Len("@prefix ")).Trim().ToUpper();
                                        break;
                                    default:
                                        CRASH("I do not understand: " + TL);
                                        return false;
                                        //break;
                                }
                                break;
                            case "Records":
                                TL = L.Trim();

                                if (qstr.Upper(qstr.Left(TL, 4)) == "REC:")
                                {
                                    if (recs.ContainsKey(qstr.Right(TL, TL.Length - 4).Trim())) //MapContains(recs, Upper(Trim(Right(TL, Len(TL)-4))))
                                    {
                                        switch (QuickGTK.Proceed("Duplicate record definition:\n\n" + qstr.Upper(qstr.MyTrim(qstr.Right(TL, qstr.Len(TL) - 4))) + "\n\nShall I merge the data with the existing record?"))
                                        {
                                            case -1:
                                                //Print "Kill program by user's request";
                                                return false; //End; 
                                            case 0:
                                                //Print "Destroying the old"
                                                TRec = new MyRecord();
                                                recs[qstr.Right(TL, TL.Length - 4).Trim()] = TRec;
                                                break;
                                            case 1:
                                                //Print "Merging!"
                                                TRec = recs[qstr.Right(TL, TL.Length - 4).Trim()]; //StringMap(MapValueForKey(Recs, Upper(Trim(Right(TL, Len(TL) - 4)))))
                                                                                            //For Local k$= EachIn MapKeys(TRec) Print K+" = " + TRec.Value(K) Next ' debug line
                                                break;
                                        } //End Select
                                }
                                else
                                {
                                    TRec = new MyRecord(); //New StringMap
                                    recs[qstr.Right(TL, TL.Length - 4).Trim()] = TRec;
                                }
                                } else if (TL.IndexOf('=') != -1) {
                            if (TRec == null) { CRASH("Definition without starting a record first in line #" + linecount + "~n~n" + L); return false; }
                            SL = TL.Split('=');
                                    for (int slak = 0; slak < SL.Length; slak++) SL[slak] = qstr.MyTrim(SL[slak]);
                                    if (!fields.ContainsKey(SL[0]))
                                    {
                                        if (RemoveNonExistent)
                                        {
                                            switch (QuickGTK.Proceed("Field does not exist ~q" + SL[0] + "~q in line " + linecount + "~n~nRemove this Field?"))
                                            {
                                                case -1: return false;
                                                case 0: TRec.value[SL[0]] = SL[1]; break;
                                                case 1: break; //Print "Field " + SL[0] + " has been removed from the database!"
                                            }//End Select
                                        }
                                        else
                                        {
                                            CRASH("Field does not exist ~q" + SL[0] + "~q in line " + linecount);
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        TRec.value[SL[0]] = SL[1];
                                    }
                                } else {
                                    CRASH("Syntax error in " + linecount + "~n~n" + L);
                                    return false;
                                }
                                break;
                        }
                    }
                }
            }            
            // Make sure MCs are properly finalized
            if (CurrentMC != null) CurrentMC.Model = CurrentListStore;
            // Record list updated?
            UpdateRecView();
            return ret;
        }

        public static void Save(string filename){
            // Save database itself           
            MySave.filename = filename;
            QOpen.SaveString(filename,MySave.XBase());
            // Loop for the export drivers
            foreach(string drv in MainClass.exportdrivers.Keys){
                // QuickGTK.Info("Exporting to " + drv); // debug
                // Export the entire database
                var ok = false;
                if (basexport.ContainsKey(drv) && basexport[drv] != "")
                {
                    ok = true;
                    var pth = Path.GetDirectoryName(basexport[drv]);
                    if (!Directory.Exists(pth)) {
                        ok = QuickGTK.Confirm($"Directory {pth} does not exist for exporting to {drv}.\n\nDo you want to create it?");
                        if (ok) Directory.CreateDirectory(pth);
                    }
                }
                if (ok) { QOpen.SaveString($"{basexport[drv]}.{MainClass.exportext[drv]}", MainClass.exportdrivers[drv].XBase()); }
                // Export record by record
                ok = false;
                if (recexport.ContainsKey(drv) && recexport[drv] != "")
                {
                    ok = true;
                    if (!Directory.Exists(recexport[drv]))
                    {
                        ok = QuickGTK.Confirm($"Directory {recexport[drv]} does not exist for exporting to {drv}.\n\nDo you want to create it?");
                        if (ok) Directory.CreateDirectory(recexport[drv]);
                    }
                }
                if (ok) { 
                    foreach(string recID in Record.Keys){
                        var expfile = $"{recexport[drv]}/{recID}.{MainClass.exportext[drv]}";
                        if (Record[recID].MODIFIED || (!File.Exists(expfile))) 
                        {
                            Console.WriteLine($"Exporting {recID} to {drv}.");
                            QOpen.SaveString(expfile, MainClass.exportdrivers[drv].XRecord(recID, true));
                        }
                        else
                        {
                            Console.WriteLine($" Skipping {recID} to {drv}. Record not modified and exported file exists");
                        }
                    }
                }
            }
            // Reset MODIFED
            foreach (string recID in Record.Keys) Record[recID].MODIFIED = false;
        }

        public void DataBase(){

        }
    }
}


/* Below is the ORIGINAL BlitzMax code of the database parser.
   It has to serve to help me a bit to set this up all well in C#

For L=EachIn LF
    linecount:+1
    TL = Trim(L)
    If Trim(L) And Left(Trim(L),1)<>"#" And Left(Trim(L),2)<>"//" And Left(Trim(L),2)<>"--" ' No empty lines and no comments either!
        If Left(TL,1)="[" And Right(TL,1)="]"
            If Upper(TL)="[SYSTEM]" 
                Chunk="System"
            ElseIf Upper(Left(TL,6))="[PAGE:"
                Chunk = "Structure"
                cpage:+1
                pagey=0
                Panels[cpage] = CreatePanel(0,0,tw,th,Tabber)
                HideGadget Panels[cpage]
                AddGadgetItem tabber,Mid(TL,7,Len(TL)-7)
                pagename = Trim(Mid(TL,7,Len(TL)-7))
            ElseIf Upper(TL)="[RECORDS]"
                Chunk = "Records"
            ElseIf Upper(TL)="[DEFAULT]"
                chunk = "Default"
            ElseIf Upper(Left(TL,7))="[ALLOW:"
                chunk = "Allow"
                AllowList = New TList
                MapInsert allow,Trim(Mid(TL,8,Len(TL)-8)),allowlist
                Print "Start Allow: "+Trim(Mid(TL,8,Len(TL)-8))
            Else 
                Error "Unknown [ ] definition: "+TL 
                EndIf
        Else
            Select Chunk
                Case "System"
                    If TL.find("=")=-1 error("Invalid line: "+TL)
                    SL = TL.split("=")
                    Select Upper(Trim(SL[0]))
                        Case "OUTPUTLUAREC"
                            OutputLuaRec = Trim(SL[1])
                        Case "OUTPUTLUABASE"
                            OutputLuaBase = Trim(SL[1])
                        Case "OUTPUTPYTHONREC"
                            outputpythonrec = Trim(SL[1])
                        Case "OUTPUTPYTHONBASE"
                            outputpythonbase = Trim(SL[1])
                        Case "OUTPUTMYSQLBASE"
                            outputmysqlbase = Trim(SL[1])
                        Case "LICENSE"
                            outlicense = Trim(SL[1])    
                        Case "AUTOOUTPUT"
                            AutoOutput = Trim(Upper(SL[1]))="YES" Or Trim(Upper(SL[1]))="TRUE"
                        Case "REMOVENONEXISTENTFIELDS"
                            RemoveNonExistent = Trim(Upper(SL[1]))="YES" Or Trim(Upper(SL[1]))="TRUE"
                        Default 
                            Error"Unknown variable: "+Trim(SL[0])
                        End Select
                Case "Structure"
                    TTL = Replace(TL,"~t"," ")
                    Repeat
                    TL = TTL
                    TTL = Replace(TTL,"  "," ")
                    Until TTL=TL                    
                    If TL.find(" ")=-1 And Left(tl,6).tolower()<>"strike" error "Invalid line: "+TL+"~nline #"+Linecount
                    SL = TL.split(" ")
                    If Left(SL[0],1)<>"@" And Left(tl,6).tolower()<>"strike" And Left(tl,4).tolower()<>"info"
                        CreateLabel Lower(SL[0]),0,pagey,250,15,panels[cpage]
                        CreateLabel SL[1],250,pagey,250,15,panels[cpage]
                        If MapContains(fields,SL[1]) error "Duplicate field: "+SL[1]
                        MapInsert fields,SL[1],SL[0]
                        MapInsert fieldonpage,SL[1],pagename
                    ElseIf Len(SL)>1
                        SL[1] = Replace(SL[1],"\space"," ")
                        EndIf                                   
                    Select Lower(SL[0])
                        Case "strike"
                            CreateLabel("---",0,pagey,ClientWidth(panels[cpage]),25,panels[cpage],LABEL_SEPARATOR)
                            pagey:+25
                        Case "info"
                              CreateLabel Right(TL,Len(TL)-5),0,pagey,1000,25,panels[cpage]
                            pagey:+25
                        Case "string"
                            MapInsert recgadget,SL[1],CreateTextField(500,pagey,500,25,panels[cpage])
                            pagey:+25
                        Case "int","double"
                            MapInsert recgadget,SL[1],CreateTextField(500,pagey,250,25,panels[cpage])
                            pagey:+25
                        Case "color"
                            CreateLabel "R:",500,Pagey,50,25,panels[cpage]
                            MapInsert RecGadget,SL[1]+".Red",CreateTextField(550,pagey,50,25,Panels[cpage])
                            CreateLabel "G:",610,Pagey,50,25,panels[cpage]
                            MapInsert RecGadget,SL[1]+".Green",CreateTextField(660,pagey,50,25,Panels[cpage])
                            CreateLabel "B:",720,Pagey,50,25,panels[cpage]
                            MapInsert RecGadget,SL[1]+".Blue",CreateTextField(770,pagey,50,25,Panels[cpage])
                            MapInsert RecGadget,SL[1]+".Pick",CreateButton("Pick",880,pagey,80,25,Panels[cpage])
                            MapInsert MapColor,GadField(SL[1]+".Pick"),SL[1]
                            SetGadgetColor GadField(SL[1]+".Red")  ,255,180,180
                            SetGadgetColor GadField(SL[1]+".Green"),180,255,180
                            SetGadgetColor GadField(SL[1]+".Blue") ,180,180,255
                            pagey:+25
                        Case "bool"
                            MapInsert RecGadget,SL[1]+".Panel",CreatePanel(500,pagey,400,25,Panels[cpage])
                            MapInsert RecGadget,SL[1]+".True",CreateButton("True",0,0,200,25,gadfield(SL[1]+".Panel"),Button_radio)
                            MapInsert RecGadget,SL[1]+".False",CreateButton("False",200,0,200,25,gadfield(SL[1]+".Panel"),Button_radio)
                            pagey:+25
                        Case "mc"
                            MapInsert recgadget,SL[1],CreateComboBox(500,pagey,500,25,panels[cpage])
                            lastlist = gadfield(SL[1])
                            pagey:+25
                        Case "@noextfilter"
                            OnlyAllowExt = ""
                            OnlyAllowExtList = Null 
                        Case "@nopathfilter","@nodirfilter"
                            OnlyAllowPath = ""
                            OnlyAllowPathList = Null
                        Case "@extfilter"
                            OnlyAllowExt = Upper(Trim(Right(TL,Len(TL)-Len("@extfilter "))))
                            OnlyAllowExtList = ListFromArray(OnlyAllowExt.split(","))   
                        Case "@pathfilter"
                            OnlyAllowPath = Upper(Trim(Right(TL,Len(TL)-Len("@pathfilter "))))
                            OnlyAllowPathList = ListFromArray(OnlyAllowpath.split(",")) 
                        Case "@dirfilter"
                            OnlyAllowPath = Upper(Trim(Right(TL,Len(TL)-Len("@dirfilter "))))
                            OnlyAllowPathList = ListFromArray(OnlyAllowPath.split(","))
                        Case "@prefix"                          
                            OnlyAllowPrefix = Upper(Trim(Right(TL,Len(TL)-Len("@prefix "))))
                        Case "@db"  
                                If Not lastlist error "No list to add this item to in line #"+linecount
                            Print "Importing database: "+Trim(Right(TL,Len(TL)-4))
                            For Local l$=EachIn Listfile("Databases/"+Trim(Right(TL,Len(TL)-4)))
                                Local readrec=False
                                If Upper(l)="[RECORDS]" 
                                    readrec=True
                                ElseIf Left(l,1)="["
                                    readrec=False
                                EndIf   
                                If Prefixed(l,"Rec: ")  AddGadgetItem lastlist,Trim(Right(l,Len(l)-4))
                            Next
                        Case "@f"
                            If Not lastlist error "No list to add this item to in line #"+linecount
                            Print "Importing JCR: "+Trim(Right(TL,Len(TL)-3))
                            Local JD:TJCRDir = JCR_Dir(Trim(Right(TL,Len(TL)-3)))
                            Ok = True
                            If Not JD error "JCR could not read file: "+Trim(Right(TL,Len(TL)-3))
                            'For Local F$=EachIn OnlyAllowPathlist DebugLog "Dir Allowed: "+f; Next
                            'For Local F$=EachIn OnlyAllowextlist  DebugLog "Ext Allowed: "+F; Next
                            For Local D:TJCREntry=EachIn MapValues(JD.Entries)
                                  Rem Old and inefficient
                                If Not OnlyAllowExtList
                                    Print "Adding entry: "+D.FileName
                                    AddGadgetItem lastlist,D.FileName
                                ElseIf ListContains(OnlyAllowExtList,Upper(ExtractExt(D.FileName)   ))
                                    Print "Adding entry: "+D.FileName+" (approved by the ext filter)"
                                    AddGadgetItem lastlist,D.FileName
                                    Else
                                    'Print " Disapproved: "+D.FileName+"  ("+Upper(ExtractExt(D.FileName))+" is not in the filterlist"+")"
                                    EndIf
                                End Rem
                                Ok = True                                                                                                '               '
                                If OnlyAllowExtList  Ok = ok And ListContains(OnlyAllowExtList ,Upper(ExtractExt(D.FileName))) 'DebugLog "    Ext Check: "+D.FileName+" >>> "+Ok
                                If OnlyAllowPathList Ok = Ok And ListContains(OnlyAllowPathList,Upper(ExtractDir(D.FileName))) 'DebugLog "    Dir Check: "+D.FileName+" >>> "+Ok
                                If OnlyAllowprefix   Ok = Ok And Prefixed(D.Filename,onlyallowprefix)
                                If OK Then 
                                    AddGadgetItem lastlist,D.FileName
                                    'DebugLog "Added to list: "+D.filename
                                    Else
                                    'DebugLog "     Rejected: "+D.filename
                                    EndIf
                                Next
                        Case "@i"
                            If Not lastlist error "No list to add this item to in line #"+linecount
                            AddGadgetItem lastlist,SL[1]
                        Default error "Unknown type '"+SL[0]+"' in line #"+linecount
                        End Select
                Case "Records"
                    TL = Trim(L)
                    If Upper(Left(TL,4))="REC:"
                        If MapContains(recs,Upper(Trim(Right(TL,Len(TL)-4))))
                            Select Proceed("Duplicate record definition:~n~n"+Upper(Trim(Right(TL,Len(TL)-4)))+"~n~nShall I merge the data with the existing record?")
                                Case -1 
                                    Print "Kill program by user's request"
                                    End
                                Case 0
                                    Print "Destroying the old"
                                    TRec = New StringMap
                                    MapInsert Recs,Upper(Trim(Right(TL,Len(TL)-4))),TRec
                                Case 1
                                    Print "Merging!"
                                    TRec = StringMap(MapValueForKey(Recs,   Upper(Trim(Right(TL,Len(TL)-4)))))
                                    For Local k$=EachIn MapKeys(TRec) Print K+" = "+TRec.Value(K) Next ' debug line
                            End Select
                        Else    
                            TRec = New StringMap
                            MapInsert Recs,Upper(Trim(Right(TL,Len(TL)-4))),TRec
                        EndIf
                    ElseIf TL.Find("=")<>"="
                        If Not TRec error "Definition without starting a record first in line #"+linecount+"~n~n"+L
                        SL = TL.split("=")
                        For Local slak=0 Until Len(SL) SL[slak]=Trim(SL[slak]) Next
                        If Not MapContains(fields,sl[0]) 
                            If RemoveNonExistent 
                                Select Proceed("Field does not exist ~q"+SL[0]+"~q in line "+linecount+"~n~nRemove this Field?")
                                    Case -1 End
                                    Case  0 MapInsert TRec,SL[0],SL[1]
                                    Case  1 Print "Field "+SL[0]+" has been removed from the database!"
                                    End Select
                                Else
                                error "Field does not exist ~q"+SL[0]+"~q in line "+linecount
                                EndIf
                            Else    
                            MapInsert TRec,SL[0],SL[1]
                            EndIf
                    Else
                        error "Syntax error in "+linecount+"~n~n"+L
                        EndIf
                Case "Allow"
                    TL = Trim(L)
                    ListAddLast allowlist,TL; SortList allowlist
                    Print "= Added: "+TL 
                Case "Default"
                    TL = Trim(L)
                    If TL.Find("=")<>"="
                        SL = TL.split("=")
                        For Local slak=0 Until Len(SL) SL[slak]=Trim(SL[slak]) Next
                        If Not MapContains(fields,sl[0]) error "Field does not exist ~q"+SL[0]+"~q in line "+linecount
                        MapInsert DefaultValues,SL[0],SL[1]
                    Else
                        error "Syntax error in "+linecount+"~n~n"+L
                        EndIf   
                Default
                    error "Internal error!~n~nUnknown Chunk!~n~n"+linecount+"/"+L   
                End Select      
            EndIf
        EndIf
    Next

 */
