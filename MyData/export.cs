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
// Version: 18.09.09
// EndLic
﻿using System;
using TrickyUnits;
namespace MyData
{
    public abstract class export
    {
         export()
        {
            MKL.Version("MyData For C# - export.cs","18.09.09");
            MKL.Lic    ("MyData For C# - export.cs","GNU General Public License 3");
        }

        abstract public string XRecord(string varname="", bool addreturn=false);
        abstract public string XBase(string varname);
    }
}
