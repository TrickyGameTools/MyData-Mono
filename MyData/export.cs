// Lic:
// 	MyData for C#
// 	Exporter
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
using TrickyUnits;
namespace MyData
{
    abstract class Export
    {
        public Export()
        {
            MKL.Version("MyData For C# - export.cs","18.09.13");
            MKL.Lic    ("MyData For C# - export.cs","GNU General Public License 3");
        }

        abstract public string XRecord(string recname = "", bool addreturn = false);
        abstract public string XBase();

        public string eol
        {
            get
            {
                if (!MyDataBase.sys.ContainsKey("EOL")) return "\n";
                switch (MyDataBase.sys["EOL"].ToUpper())
                {
                    case "DOS":
                    case "WINDOWS":
                        return "\r\n";
                    case "UNIX":
                    case "LINUX":
                    case "MAC":
                    case "MACOSX":
                        return "\n";
                    default:
                        TrickyUnits.GTK.QuickGTK.Error($"I do not know EOL type {MyDataBase.sys["EOL"]}, so reverting back to Unix format.");
                        MyDataBase.sys["EOL"] = "UNIX"; // Won't alter the database files, but it will prevent the message to pop up over and over and over and over and over etc.
                        return "\n";
                }
            }
        }
    }

    class ExportMyData : Export
    {

        public string filename;

        override public string XRecord(string recname = "", bool addreturn = false) { TrickyUnits.GTK.QuickGTK.Error("Something is wrong in the export zone!"); return "?"; }
        override public string XBase()
        {
            var data = System.IO.File.ReadAllLines(filename);
            var ret = "";
            var recs = false;
            var white = 0;
            foreach (string L in data)
            {
                var TL = L.ToUpper().Trim();
                if (qstr.Left(TL, 1) == "[" && qstr.Right(TL, 1) == "]") recs = TL == "[RECORDS]";
                if (!recs) {
                    ret += $"{L}{eol}";
                    if (TL == "") white++; else white = 0;
                }
            }
            if (white < 3) for (int i = 0; i < 3; i++) ret += eol;
            ret += $"[RECORDS]{eol}# The code below has been generated by MyData.{eol}# Best is not to alter this data unless you know what you are doing!{eol}# Also adding comments here is pretty senseless!";
            foreach (string recid in MyDataBase.Record.Keys)
            {
                var rec = MyDataBase.Record[recid];
                ret += $"{eol}Rec: {recid.ToUpper()}{eol}";
                foreach (string key in rec.value.Keys) ret += $"\t{key} = {rec.value[key]}{eol}";
            }
            ret += $"{eol}";
            return ret;
        }

    }


    class ExportLua : Export
    {

        string header
        {
            get
            {
                var today = DateTime.Today;
                return $"-- File generated by MyData on {today.DayOfWeek.ToString()} {today.Day}/{today.Month}/{today.Year}{eol}-- License: {MyDataBase.License}{eol}{eol}";
            }
        }

        public override string XRecord(string recname = "", bool addreturn = false)
        {
            var ret = "";
            if (addreturn) { ret = header + "local ret={" + eol; }
            foreach (string k in MyDataBase.Record[recname].value.Keys)
            {
                if (!addreturn) ret += "\t";
                ret += $"\t[\"{k}\"] = ";
                if (!(MyDataBase.RemoveNonExistent && (k == "" || (MyDataBase.fields[k] == "bool" && k.ToUpper() != "TRUE")))){
                    switch (MyDataBase.fields[k])
                    {
                        case "string":
                        case "mc":
                            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(MyDataBase.Record[recname].value[k]);
                            ret += "\"";
                            foreach (byte b in bytes)
                            {
                                if (b > 31 && b < 128) { ret += qstr.Chr(b); }
                                else
                                {
                                    ret += "\\" + qstr.Right("00" + Convert.ToString(b, 8), 3);
                                }
                            }
                            ret += "\"";
                            break;
                        case "int":
                        case "double":
                            ret += MyDataBase.Record[recname].value[k];
                            break;
                        case "bool":
                            if (MyDataBase.Record[recname].value[k].ToUpper() == "TRUE") ret += "true"; else ret += "false";
                            break;

                    }
                    ret += "," + eol;
                }
            }
            if (addreturn)
            {
                ret += "}\n\nreturn ret\n\n";
            }
            return ret;
        }

        public override string XBase()
        {
            var ret = $"{header}{eol}local ret = " + "{" + eol;
            foreach (string recID in MyDataBase.Record.Keys)
            {
                ret += $"\t[\"{recID}\"] = " + "{" + $"{eol}{XRecord(recID)}{eol}\t" + "}," + eol;
            }
            ret += $"{eol}" + "}" + $"{eol}{eol}return ret{eol}{eol}";
            return ret;
        }
    }

    class ExportXML : Export
    {

        string header
        {
            get
            {
                var today = DateTime.Today;
                return $"<? xml version=\"1.0\" encoding =\"utf - 8\" ?>{eol}{eol}<!--\n\tGenerated by MyData File generated by MyData on {today.DayOfWeek.ToString()} {today.Day}/{today.Month}/{today.Year}{eol}\tLicense: {MyDataBase.License}{eol}{eol}-->{eol}{eol}";
            }
        }
        override public string XRecord(string recname = "", bool addreturn = false)
        {
            var ret = "";
            if (addreturn) ret += header;
            foreach (string k in MyDataBase.Record[recname].value.Keys)
            {
                var val = MyDataBase.Record[recname].value[k];
                if (!(MyDataBase.RemoveNonExistent && (k == "" || (MyDataBase.fields[k] == "bool" && k.ToUpper() != "TRUE"))))
                {
                    if (!addreturn) ret += "\t";
                    ret += $"<{k}>{val}</{k}>{eol}";
                }
            }
            return ret;
        }

        override public string XBase()
        {
            var ret = header;
            foreach (string rID in MyDataBase.Record.Keys)
            {
                ret += $"<{rID}>{eol}{XRecord(rID, false)}</{rID}>{eol}";
            }
            return ret;
        }
    }



    class ExportYAML : Export
    {

        string header
        {
            get
            {
                var today = DateTime.Today;
                return $"# Generated by MyData File generated by MyData on {today.DayOfWeek.ToString()} {today.Day}/{today.Month}/{today.Year}{eol}# tLicense: {MyDataBase.License}{eol}{eol}";
            }
        }
        override public string XRecord(string recname = "", bool addreturn = false)
        {
            var ret = "";
            if (addreturn) ret += header;
            foreach (string k in MyDataBase.Record[recname].value.Keys)
            {
                var val = MyDataBase.Record[recname].value[k];
                if (!(MyDataBase.RemoveNonExistent && (k == "" || (MyDataBase.fields[k] == "bool" && k.ToUpper() != "TRUE"))))
                {
                    if (!addreturn) ret += "\t";
                    ret += $"{k} : {val}{eol}";
                }
            }
            return ret;
        }

        override public string XBase()
        {
            var ret = header;
            foreach (string rID in MyDataBase.Record.Keys)
            {
                ret += $"{rID} :{eol}{XRecord(rID, false)}";
            }
            return ret;
        }
    }

    class ExportJSON : Export
    {

        public override string XRecord(string recname = "", bool addreturn = false)
        {
            var t = ""; if (!addreturn) t = "\t";
            var ret = t + "{" + eol;
            var first = true;
            foreach (string k in MyDataBase.Record[recname].value.Keys)
            {
                var ok = !(MyDataBase.RemoveNonExistent && (k == "" || (MyDataBase.fields[k] == "bool" && k.ToUpper() != "TRUE")));
                var lin = $"\t{t}\"{k}\" : ";
                if (!ok) { lin += "null"; }
                else
                {
                    switch (MyDataBase.fields[k])
                    {
                        case "string":
                        case "mc":
                            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(MyDataBase.Record[recname].value[k]);
                            lin += "\"";
                            foreach (byte b in bytes)
                            {
                                if (b > 31 && b < 128) { lin += qstr.Chr(b); }
                                else
                                {
                                    lin += "\\" + qstr.Right("00" + Convert.ToString(b, 8), 3);
                                }
                            }
                            lin += "\"";
                            break;
                        case "int":
                        case "double":
                            lin += MyDataBase.Record[recname].value[k];
                            break;
                        case "bool":
                            if (MyDataBase.Record[recname].value[k].ToUpper() == "TRUE") lin += "true"; else lin += "false";
                            break;
                    }
                }
                if (ok || (!MyDataBase.sys.ContainsKey("NONULL") || MyDataBase.sys["NONULL"].ToUpper() != "TRUE"))
                {
                    if (!first) ret += $",{eol}"; first = false;
                    ret += lin;
                }

            }
            ret += "}" + eol;
            return ret;
        }

        public override string XBase()
        {
            var ret = "{" + eol;
            var first = true;
            foreach (string rID in MyDataBase.Record.Keys)
            {
                if (!first) ret += $",{eol}"; first = false;
                ret += $"\t\"{rID}\" : {XRecord(rID, false)}";
            }
            ret += "}" + eol + eol;
            return ret;

        }
    }

    class ExportPython : Export
    {

        string header
        {
            get
            {
                var today = DateTime.Today;
                return $"# File generated by MyData on {today.DayOfWeek.ToString()} {today.Day}/{today.Month}/{today.Year}{eol}# License: {MyDataBase.License}{eol}{eol}";
            }
        }

        public override string XRecord(string recname = "", bool addreturn = false)
        {
            var vr = "";
            var t = ""; if (!addreturn) { t = "\t"; } else { vr = $"{header}{eol}{eol}MyRec = "; }
            var ret = t + vr + "{" + eol;
            var first = true;
            foreach (string k in MyDataBase.Record[recname].value.Keys)
            {
                var ok = !(MyDataBase.RemoveNonExistent && (k == "" || (MyDataBase.fields[k] == "bool" && k.ToUpper() != "TRUE")));
                var lin = $"\t{t}\"{k}\" : ";
                if (!ok) { lin += "null"; }
                else
                {
                    switch (MyDataBase.fields[k])
                    {
                        case "string":
                        case "mc":
                            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(MyDataBase.Record[recname].value[k]);
                            lin += "\"";
                            foreach (byte b in bytes)
                            {
                                if (b > 31 && b < 128) { lin += qstr.Chr(b); }
                                else
                                {
                                    lin += "\\" + qstr.Right("00" + Convert.ToString(b, 8), 3);
                                }
                            }
                            lin += "\"";
                            break;
                        case "int":
                        case "double":
                            lin += MyDataBase.Record[recname].value[k];
                            break;
                        case "bool":
                            if (MyDataBase.Record[recname].value[k].ToUpper() == "TRUE") lin += "True"; else lin += "False";
                            break;
                    }
                }
                if (ok || (!MyDataBase.sys.ContainsKey("NONULL") || MyDataBase.sys["NONULL"].ToUpper() != "TRUE"))
                {
                    if (!first) ret += $",{eol}"; first = false;
                    ret += lin;
                }

            }
            ret += "}" + eol;
            return ret;
        }

        public override string XBase()
        {
            var ret = header+eol+eol+"MyData = {" + eol;
            var first = true;
            foreach (string rID in MyDataBase.Record.Keys)
            {
                if (!first) ret += $",{eol}"; first = false;
                ret += $"\t\"{rID}\" : {XRecord(rID, false)}";
            }
            ret += "}" + eol + eol;
            return ret;

        }
    }
    class ExportPHP : Export
    {

        string header
        {
            get
            {
                var today = DateTime.Today;
                return $"// File generated by MyData on {today.DayOfWeek.ToString()} {today.Day}/{today.Month}/{today.Year}{eol}// License: {MyDataBase.License}{eol}{eol}";
            }
        }

        public override string XRecord(string recname = "", bool addreturn = false)
        {
            var vr = "";
            var t = ""; if (!addreturn) { t = "\t"; } else { vr = $"<?{eol}{header}{eol}{eol}$MyRec = "; }
            var ret = t + vr + "array (" + eol;
            var first = true;
            foreach (string k in MyDataBase.Record[recname].value.Keys)
            {
                var ok = !(MyDataBase.RemoveNonExistent && (k == "" || (MyDataBase.fields[k] == "bool" && k.ToUpper() != "TRUE")));
                var lin = $"\t{t}\"{k}\" => ";
                if (!ok) { lin += "null"; }
                else
                {
                    switch (MyDataBase.fields[k])
                    {
                        case "string":
                        case "mc":
                            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(MyDataBase.Record[recname].value[k]);
                            lin += "\"";
                            foreach (byte b in bytes)
                            {
                                if (b > 31 && b < 128) { lin += qstr.Chr(b); }
                                else
                                {
                                    lin += "\\" + qstr.Right("00" + Convert.ToString(b, 8), 3);
                                }
                            }
                            lin += "\"";
                            break;
                        case "int":
                        case "double":
                            lin += MyDataBase.Record[recname].value[k];
                            break;
                        case "bool":
                            if (MyDataBase.Record[recname].value[k].ToUpper() == "TRUE") lin += "true"; else lin += "false";
                            break;
                    }
                }
                if (ok)
                {
                    if (!first) ret += $",{eol}"; first = false;
                    ret += lin;
                }

            }
            ret += ")" ;
            if (addreturn) ret+=$";{eol}?>{eol}";
            return ret;
        }

        public override string XBase()
        {
            var ret = "<? "+header + eol + eol + "$MyData = array" + eol;
            var first = true;
            foreach (string rID in MyDataBase.Record.Keys)
            {
                if (!first) ret += $",{eol}"; first = false;
                ret += $"\t\"{rID}\" => {XRecord(rID, false)}";
            }
            ret += "); ?>" + eol + eol;
            return ret;

        }
    }

    class ExportGINI : Export
    {
        string header
        {
            get
            {
                var today = DateTime.Today;
                return $"[rem]{eol}File generated by MyData on {today.DayOfWeek.ToString()} {today.Day}/{today.Month}/{today.Year}{eol}License: {MyDataBase.License}{eol}{eol}";
            }
        }



        public override string XRecord(string recname = "", bool addreturn = false)
        {
            var ret = "";
            if (addreturn) ret += $"{header}{eol}[vars]{eol}";
            foreach (string k in MyDataBase.Record[recname].value.Keys)
            {
                var val = MyDataBase.Record[recname].value[k];
                if (!(MyDataBase.RemoveNonExistent && (k == "" || (MyDataBase.fields[k] == "bool" && k.ToUpper() != "TRUE"))))
                {
                    if (!addreturn) ret += $"{recname.ToUpper()}.";
                    ret += $"{k.ToUpper()}={val}{eol}";
                }
            }
            return ret;
        }

        override public string XBase()
        {
            var ret = $"{header}{eol}{eol}[vars]{eol}";
            foreach (string rID in MyDataBase.Record.Keys)
            {
                ret += $"{XRecord(rID, false)}";
            }
            return ret;
        }
    }

}

