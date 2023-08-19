// Lic:
// My Data
// Simplistic Database
// 
// 
// 
// (c) Jeroen P. Broks, 2018, 2021, 2023
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
// 
// Please note that some references to data like pictures or audio, do not automatically
// fall under this licenses. Mostly this is noted in the respective files.
// 
// Version: 23.08.19
// EndLic

using System;
using System.Diagnostics;
using System.Text;
using TrickyUnits;
using TrickyUnits.GTK;

namespace MyData
{
	abstract class Export
	{
		public Export()
		{
			MKL.Version("MyData For C# - export.cs","23.08.19");
			MKL.Lic    ("MyData For C# - export.cs","GNU General Public License 3");
		}

		abstract public string XRecord(string recname = "", bool addreturn = false);
		abstract public string XBase();
		virtual public string XClass(string cln) { return ""; }
		
		

		public string eol
		{
			get
			{
				if (!MyDataBase.sys.ContainsKey("EOL")) return "\n";
				switch (MyDataBase.sys["EOL"].ToUpper())
				{
					case "DOS":
					case "WINDOWS":
						// return "\r\n"; // Fuck Windows
						return "\n";
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
			var ret = new StringBuilder("");
			var recs = false;
			var white = 0;
			foreach (string L in data)
			{
				var TL = L.ToUpper().Trim();
				if (qstr.Left(TL, 1) == "[" && qstr.Right(TL, 1) == "]") recs = TL == "[RECORDS]";
				if (!recs) {
					ret.Append( $"{L}{eol}");
					if (TL == "") white++; else white = 0;
				}
			}
			if (white < 3) for (int i = 0; i < 3; i++) ret.Append("\n"); //ret.Append( eol);
			var today = DateTime.Today;
			ret.Append( $"[RECORDS]{eol}# The code below has been generated by MyData.{eol}# Best is not to alter this data unless you know what you are doing!{eol}# Also adding comments here is pretty senseless!{eol}# Data last generated {today.Day}/{today.Month}/{today.Year}{eol}{eol}");
			foreach (string recid in MyDataBase.Record.Keys)
			{
				var rec = MyDataBase.Record[recid];
				ret.Append( $"{eol}Rec: {recid.ToUpper()}{eol}");
				foreach (string key in rec.value.Keys) ret.Append( $"\t{key} = {rec.value[key]}{eol}");
			}
			ret.Append( $"{eol}");
			return ret.ToString();
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
				var v = "";
				if (!MyDataBase.Record[recname].value.ContainsKey(k)) {
					Console.Beep();
					Debug.WriteLine($"Record {recname} has no field named {k}!");
				} else
					v = MyDataBase.Record[recname].value[k];
				if (!(MyDataBase.RemoveNonExistent && (v == "" || (MyDataBase.fields[k] == "bool" && v.ToUpper() != "TRUE")))){
					switch (MyDataBase.fields[k])
					{
						case "string":
						case "mc":
							byte[] bytes = System.Text.Encoding.ASCII.GetBytes(MyDataBase.Record[recname].value[k]);
							ret += "\"";
							foreach (byte b in bytes)
							{
								if ((b > 31 && b < 128) && b!='"') { ret += qstr.Chr(b); }
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
								if ((b > 31 && b < 128) && b!='"') { lin += qstr.Chr(b); }
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
								if ((b > 31 && b < 128) && b!='"') { lin += qstr.Chr(b); }
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
								if ((b > 31 && b < 128) && b!='"') { lin += qstr.Chr(b); }
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


	class ExportNIL : Export {

		string header {
			get {
				var today = DateTime.Today;            
				return $"// File generated by MyData on {today.DayOfWeek.ToString()} {today.Day}/{today.Month}/{today.Year}\n// License: {MyDataBase.License}{eol}{eol}";
			}
		}

		public override string XRecord(string recname = "", bool addreturn = false) {
			var ret = "";
			var retname = recname; if (retname != "") retname = $"_{retname}";
			if (addreturn) { ret = header + $"do\n\n\ntable ret{retname}\nret{retname}={'{'}\n"; }
			foreach (string k in MyDataBase.Record[recname].value.Keys) {
				if (!addreturn) ret += "\t";
				ret += $"\t[\"{k}\"] = ";
				var v = MyDataBase.Record[recname].value[k];
				if (!(MyDataBase.RemoveNonExistent && (v == "" || (MyDataBase.fields[k] == "bool" && v.ToUpper() != "TRUE")))) {
					switch (MyDataBase.fields[k]) {
						case "string":
						case "mc":
							byte[] bytes = System.Text.Encoding.ASCII.GetBytes(MyDataBase.Record[recname].value[k]);
							ret += "\"";
							foreach (byte b in bytes) {
								if ((b > 31 && b < 128) && b != '"') { ret += qstr.Chr(b); } else {
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
					ret += ",\n"; //+ eol;
				}
			}
			if (addreturn) {
				ret += "}\n\n";
				ret += $"return ret{retname}\n\n\nend";
			}
			return ret;
		}

		public override string XBase() {
			var ret = $"{header}\ndo\n\n\ntable ret\nret = " + "{\n";
			foreach (string recID in MyDataBase.Record.Keys) {
				ret += $"\t[\"{recID}\"] = " + "{" + $"{eol}{XRecord(recID)}{eol}\t" + "},\n";
			}
			ret += $"\n" + "}" + $"\n\nreturn ret\n\n\n\n\nend";
			return ret;
		}
	}

	class ExportNeil : Export {

		private string classname = "ClassNameNotYetDefined";
		string header {
			get {
				var today = DateTime.Today;
				return $"// File generated by MyData on {today.DayOfWeek.ToString()} {today.Day}/{today.Month}/{today.Year}\n// License: {MyDataBase.License}\n\n";
			}
		}

		public override string XRecord(string recname = "", bool addreturn = false) {
			var ret = new StringBuilder($"// Record: {recname}\n");
			if (addreturn) {
				ret.Append(header);
				ret.Append($"//do\n\tvar ret = new MyData_{classname}()\n");
				ret.Append("\n\n\tInit\n");
			} else {
				ret.Append("do\n");
			ret.Append($"\tvar ret = new MyData_{classname}()\n");
			}
			foreach(string k in MyDataBase.fields.Keys) {
				var v = "";
				if (MyDataBase.Record[recname].value.ContainsKey(k)) v = MyDataBase.Record[recname].value[k];
				else {
					Console.Beep();
					Debug.WriteLine($"Export.Neil: Record {recname} has no field name {k}!");
				}
				ret.Append($"\t\tret.{k} = ");
				switch (MyDataBase.fields[k]) {
					case "string":
					case "mc":
						//byte[] bytes = System.Text.Encoding.ASCII.GetBytes(MyDataBase.Record[recname].value[k]);
						var bytes = System.Text.Encoding.ASCII.GetBytes(v);
						ret.Append("\"");
						foreach (byte b in bytes) {
							if ((b > 31 && b < 128) && b != '"') { ret.Append(qstr.Chr(b)); } else {
								ret.Append("\\" + qstr.Right("00" + Convert.ToString(b, 8), 3));
							}
						}
						ret.Append("\"");
						break;
					case "int":
					case "double":
						if (MyDataBase.Record[recname].value[k].Trim() == "")
							ret.Append("0");
						else
							ret.Append(MyDataBase.Record[recname].value[k]);
						break;
					case "bool":
						if (!MyDataBase.Record[recname].value.ContainsKey(k)) MyDataBase.Record[recname].value[k] = "FALSE";
						if (MyDataBase.Record[recname].value[k].ToUpper() == "TRUE") ret.Append("true"); else ret.Append("false");
						break;
					case "date":
						var ds = MyDataBase.Record[recname].value[k].Split('/');
						ret.Append("{" + $" day={ds[0]}, month={ds[1]}, year ={ds[2]} " + "}");
						break;
					case "time":
						ds = MyDataBase.Record[recname].value[k].Split(':');
						ret.Append("{" + $" hour={ds[0]}, minute={ds[1]}, second ={ds[2]} " + "}");
						break;
					case "color":
						ds = MyDataBase.Record[recname].value[k].Split(',');
						ret.Append("{" + $" red={ds[0]}, green={ds[1]}, blue ={ds[2]} " + "}");
						break;
					default:
						ret.Append("nil");
						TrickyUnits.GTK.QuickGTK.Info($"I do not know how to deal with type {MyDataBase.fields[k]}");
						break;
				}
				ret.Append("\n");
			}
			if (addreturn) {
				//ret.Append("\t\treturn ret\nend");
				ret.Append("\n\n\tend\n\treturn ret\n//end\n");
			}

			return $"{ret}";
		}

		public override string XBase() {
			var ret = new StringBuilder( $"{header}\n\n\n\ntable retbase\n\n//Init\n\n");
			var today = DateTime.Today;
			ret.Append(XClass($"{qstr.md5($"MyData_{MyDataBase.License}{today.DayOfWeek}") }_{ qstr.md5($"MyData_{today.DayOfWeek}")}")+"\n\nInit\n");
			foreach (string recID in MyDataBase.Record.Keys) {
				//ret.Append( $"\t[\"{recID}\"] = " + "{" + $"{eol}{XRecord(recID)}{eol}\t" + "},\n");
				ret.Append(XRecord(recID)+"\n");
				ret.Append($"\tretbase[\"{recID}\"] = ret\nend\n\n");
			}
			ret.Append( $"\n" +  $"\n\nend\n\nreturn retbase\n\n\n\n\n//end");
			return $"{ret}";
		}

		override public string XClass(string cln) {
			var today = DateTime.Today;
			//classname = $"{qstr.md5($"Mydata_{MyDataBase.License}{today.DayOfWeek}") }_{ qstr.md5($"MyData_{today.DayOfWeek}")}";
			classname = $"MyDataClass_{cln}";
			var ret = new StringBuilder($"class MyData_{classname}\n");
			foreach (var dk in MyDataBase.defaults.Keys) Console.WriteLine($"Default {dk} = {MyDataBase.defaults[dk]}");
			foreach(var n in MyDataBase.fields.Keys) {
				var t = MyDataBase.fields[n];
				var nu = n.ToUpper();
				switch (t) {
					case "info":
					case "strike":
						break; // Nothing I need here!
					case "string":
					case "mc":
						ret.Append($"\tstring {n}");
						if (MyDataBase.defaults.ContainsKey(nu)) ret.Append($" = \"{qstr.SafeString(MyDataBase.defaults[nu])}\"");
						ret.Append("\n");
						break;
					case "int":
						ret.Append($"\tint {n}");
						if (MyDataBase.defaults.ContainsKey(nu)) ret.Append($" = {qstr.ToInt(MyDataBase.defaults[nu])}");
						ret.Append("\n");
						break;
					case "bool":
					case "boolean":
						ret.Append($"\tbool {n}");
						if (MyDataBase.defaults.ContainsKey(nu)) ret.Append($" = {MyDataBase.defaults[nu].ToLower()}");
						ret.Append("\n");
						break;
					default:
						QuickGTK.Error($"Unknown field type \"{n}\"! Field ignored and further exports will lead to errors in Neil!");
						break;
				}
			}
			ret.Append("end\n");
			return $"{ret}";
		}

	}

	class ExportGINIESource : Export {
		string header => $"# GINIE File generated {DateTime.Now}";

		void ClassIt(GINIE g,string classname="class") {
			foreach (var n in MyDataBase.fields.Keys) {
				var t = MyDataBase.fields[n];
				var nu = n.ToUpper();
				switch (t) {
					case "info":
					case "strike":
						break; // Nothing I need here!
					case "string":
					case "mc":
						g[classname, n] = "String";
						break;
					case "int":
						g[classname, n] = "Int";
						break;
					case "bool":
					case "boolean":
						g[classname, n] = "Boolean";
						break;
					default:
						QuickGTK.Error($"Unknown field type \"{n}\"! Field ignored and further exports will lead to errors in Neil!");
						break;
				}
			}
		}

		void RecIt(GINIE g, string rec, MyRecord mr) {
			var v = "";
			foreach (var k in MyDataBase.fields.Keys) {
				if (mr.value.ContainsKey(k)) v = mr.value[k];
				g[$"Rec:{rec}", k] = v;
			}
		}

		public override string XBase() {
			var output = GINIE.Empty();
			ClassIt(output);
			foreach (var IT in MyDataBase.Record) RecIt(output, IT.Key, IT.Value);
			return $"{header}\n\n{output.ToSource()}";
		}

		public override string XRecord(string recname = "", bool addreturn = false) {
			var output = GINIE.Empty();
			RecIt(output, recname, MyDataBase.Record[recname]);
			return $"{header}\n\n{output.ToSource()}";
		}

        public override string XClass(string cln) {
			var output = GINIE.Empty();
			ClassIt(output);
			return $"{header}\n\n{output.ToSource()}";
		}
    }

}