// Lic:
// 	MyData for C#
// 	Exporter
// 	
// 	
// 	
// 	(c) Jeroen P. Broks, 2018, 2019, All rights reserved
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
// Version: 19.01.25
// EndLic

using System;
using TrickyUnits;
namespace MyData
{
    abstract class Export
    {
        public Export()
        {
            MKL.Version("MyData For C# - export.cs","19.01.25");
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
        public static int[] TimeSplit(string time,char sep=':'){
            var s = time.Split(sep);
            int[] ret = { 0, 0, 0 };
            try{
                for (int i = 0; i < ret.Length; i++) ret[i] = Int32.Parse(s[i]);
            } catch {
                ret[0] = 0; // Just a warning suppressor :P
            }
            return ret;
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
            var today = DateTime.Today;
            ret += $"[RECORDS]{eol}# The code below has been generated by MyData.{eol}# Best is not to alter this data unless you know what you are doing!{eol}# Also adding comments here is pretty senseless!{eol}# Data last generated {today.Day}/{today.Month}/{today.Year}{eol}{eol}";
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
                var v = MyDataBase.Record[recname].value[k];
                if (!(MyDataBase.RemoveNonExistent && (v == "" || (MyDataBase.fields[k] == "bool" && v.ToUpper() != "TRUE")))){
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
                            if (MyDataBase.Record[recname].value[k].Trim() == "")
                                ret += "0";
                            else                            
                                ret += MyDataBase.Record[recname].value[k];
                            break;
                        case "bool":
                            if (MyDataBase.Record[recname].value[k].ToUpper() == "TRUE") ret += "true"; else ret += "false";
                            break; 
                        case "date":
                            var ds = MyDataBase.Record[recname].value[k].Split('/');
                            ret += "{" + $" day={ds[0]}, month={ds[1]}, year ={ds[2]} " + "}";
                            break;
                        case "time":
                            ds = MyDataBase.Record[recname].value[k].Split(':');
                            ret += "{" + $" hour={ds[0]}, minute={ds[1]}, second ={ds[2]} " + "}";
                            break;
                        case "color":
                            ds = MyDataBase.Record[recname].value[k].Split(',');
                            ret += "{" + $" red={ds[0]}, green={ds[1]}, blue ={ds[2]} " + "}";
                            break;
                        default:
                            TrickyUnits.GTK.QuickGTK.Info($"I do not know how to deal with type {MyDataBase.fields[k]}");
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
                if (!(MyDataBase.RemoveNonExistent && (val == "" || (MyDataBase.fields[k] == "bool" && val.ToUpper() != "TRUE"))))
                {
                    if (!addreturn) ret += "\t";
                    switch (MyDataBase.fields[k])
                    {
                        case "date":
                            var ds = MyDataBase.Record[recname].value[k].Split('/');
                            ret += $"<{k}><day>{ds[0]}</day><month>{ds[1]}</month><year>{ds[2]}</year></{k}>{eol}";
                            break;
                        case "time":
                            ds = MyDataBase.Record[recname].value[k].Split(':');
                            ret += $"<{k}><hour>{ds[0]}</hour><minute>{ds[1]}</minute><second>{ds[2]}</second></{k}>{eol}";
                            break;
                        case "color":
                            ds = MyDataBase.Record[recname].value[k].Split(',');
                            ret += $"<{k}><red>{ds[0]}</red><green>{ds[1]}</green><blue>{ds[2]}</blue></{k}>{eol}";
                            break;
                        default:
                            ret += $"<{k}>{val}</{k}>{eol}";
                            break;
                    }
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
                if (!(MyDataBase.RemoveNonExistent && (val == "" || (MyDataBase.fields[k] == "bool" && val.ToUpper() != "TRUE"))))
                {
                    if (!addreturn) ret += "\t";
                    switch (MyDataBase.fields[k])
                    {
                        case "date":
                            var ds = val.Split('/');
                            ret += $"{k} : {eol}";
                            if (!addreturn) ret += "\t"; ret += $"\tday : {ds[0]}{eol}";
                            if (!addreturn) ret += "\t"; ret += $"\tmonth : {ds[1]}{eol}";
                            if (!addreturn) ret += "\t"; ret += $"\tyear : {ds[2]}{eol}";
                            break;
                        case "time":
                            ds = val.Split(':');
                            ret += $"{k} : {eol}";
                            if (!addreturn) ret += "\t"; ret += $"\thour : {ds[0]}{eol}";
                            if (!addreturn) ret += "\t"; ret += $"\tminute : {ds[1]}{eol}";
                            if (!addreturn) ret += "\t"; ret += $"\tsecond : {ds[2]}{eol}";
                            break;
                        case "color":
                            ds = val.Split(',');
                            ret += $"{k} : {eol}";
                            if (!addreturn) ret += "\t"; ret += $"\tred : {ds[0]}{eol}";
                            if (!addreturn) ret += "\t"; ret += $"\tgreen : {ds[1]}{eol}";
                            if (!addreturn) ret += "\t"; ret += $"\tblue : {ds[2]}{eol}";
                            break;
                        default:
                            ret += $"{k} : {val}{eol}";
                            break;
                    }
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
                var val = MyDataBase.Record[recname].value[k];
                var ok = !(MyDataBase.RemoveNonExistent && (val == "" || (MyDataBase.fields[k] == "bool" && val.ToUpper() != "TRUE")));
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
                        case "date":
                            var sd = MyDataBase.Record[recname].value[k].Split('/');
                            lin += "{"+$" \"day\" : {sd[0]}, \"month\" : {sd[1]}, \"year\" : {sd[2]} "+"}";
                            break;
                        case "time":
                            var si = TimeSplit(MyDataBase.Record[recname].value[k]);
                            lin += "{" + $" \"hour\" : {si[0]}, \"minute\" : {si[1]}, \"second\" : {si[2]} " + "}";
                            break;
                        case "color":
                            si = TimeSplit(MyDataBase.Record[recname].value[k],',');
                            lin += "{" + $" \"red\" : {si[0]}, \"green\" : {si[1]}, \"blue\" : {si[2]} " + "}";
                            break;
                        default:
                            TrickyUnits.GTK.QuickGTK.Error("I cannot handle type -> " + MyDataBase.fields[k]);
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
                var val = MyDataBase.Record[recname].value[k];
                var ok = !(MyDataBase.RemoveNonExistent && (val == "" || (MyDataBase.fields[k] == "bool" && val.ToUpper() != "TRUE")));
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
                        case "date":
                            var sd = MyDataBase.Record[recname].value[k].Split('/');
                            lin += "{" + $" \"day\" : {sd[0]}, \"month\" : {sd[1]}, \"year\" : {sd[2]} " + "}";
                            break;
                        case "time":
                            var si = TimeSplit(MyDataBase.Record[recname].value[k]); //MyDataBase.Record[recname].value[k].Split(':');
                            lin += "{" + $" \"hour\" : {si[0]}, \"minute\" : {si[1]}, \"second\" : {si[2]} " + "}";
                            break;
                        case "color":
                            si = TimeSplit(MyDataBase.Record[recname].value[k],',');
                            lin += "{" + $" \"red\" : {si[0]}, \"green\" : {si[1]}, \"blue\" : {si[2]} " + "}";
                            break;
                        default:
                            TrickyUnits.GTK.QuickGTK.Error("I cannot handle type -> " + MyDataBase.fields[k]);
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
                var val = MyDataBase.Record[recname].value[k];
                var ok = !(MyDataBase.RemoveNonExistent && (val == "" || (MyDataBase.fields[k] == "bool" && val.ToUpper() != "TRUE")));
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
                        case "date":
                            var sd = MyDataBase.Record[recname].value[k].Split('/');
                            lin += $" array( \"day\" => {sd[0]}, \"month\" => {sd[1]}, \"year\" => {sd[2]}) ";
                            break;
                        case "time":
                            //sd = MyDataBase.Record[recname].value[k].Split(':');
                            var si = TimeSplit(MyDataBase.Record[recname].value[k]);
                            lin += $"array( \"hour\" => {si[0]}, \"minute\" => {si[1]}, \"second\" => {si[2]} )";
                            break;
                        case "color":
                            //sd = MyDataBase.Record[recname].value[k].Split(':');
                            si = TimeSplit(MyDataBase.Record[recname].value[k],',');
                            lin += $"array( \"red\" => {si[0]}, \"green\" => {si[1]}, \"blue\" => {si[2]} )";
                            break;

                        default:
                            TrickyUnits.GTK.QuickGTK.Error("I cannot handle type -> " + MyDataBase.fields[k]);
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
                if (!(MyDataBase.RemoveNonExistent && (val == "" || (MyDataBase.fields[k] == "bool" && val.ToUpper() != "TRUE"))))
                {
                    if (!addreturn) ret += $"{recname.ToUpper()}.";
                    switch (MyDataBase.fields[k])
                    {
                        case "date":
                            var sd = MyDataBase.Record[recname].value[k].Split('/');
                            ret += $"{k.ToUpper()}.DAY={sd[0]}{eol}"; if (!addreturn) ret += $"{recname.ToUpper()}.";
                            ret += $"{k.ToUpper()}.MONTH={sd[1]}{eol}"; if (!addreturn) ret += $"{recname.ToUpper()}.";
                            ret += $"{k.ToUpper()}.YEAR={sd[2]}{eol}";
                            break;
                        case "time":
                            sd = MyDataBase.Record[recname].value[k].Split(':');
                            ret += $"{k.ToUpper()}.HOUR={sd[0]}{eol}"; if (!addreturn) ret += $"{recname.ToUpper()}.";
                            ret += $"{k.ToUpper()}.MINUTE={sd[1]}{eol}"; if (!addreturn) ret += $"{recname.ToUpper()}.";
                            ret += $"{k.ToUpper()}.SECOND={sd[2]}{eol}";
                            break;
                        case "color":
                            sd = MyDataBase.Record[recname].value[k].Split(',');
                            ret += $"{k.ToUpper()}.RED={sd[0]}{eol}"; if (!addreturn) ret += $"{recname.ToUpper()}.";
                            ret += $"{k.ToUpper()}.GREEN={sd[1]}{eol}"; if (!addreturn) ret += $"{recname.ToUpper()}.";
                            ret += $"{k.ToUpper()}.BLUE={sd[2]}{eol}";
                            break;
                        default:
                            ret += $"{k.ToUpper()}={val}{eol}";
                            break;
                    }
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

