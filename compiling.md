# Compiling from source

This is explained from Visual Studio.

One should have one directory completely dedicated to my work... At least that's the safest way to go XD.
When you are on Linux or Mac, make sure this folder is on a native formatted device, as FAT32 or ExFAT will cause errors in Visual Studio. This is a bug in Visual Studio, unless they fix it, all I can do is make it known.

Open a terminal window and go to this dedicated directory and type the following commands
~~~shell
git clone https://github.com/TrickyGameTools/MyData-Mono.git MyData
git clone https://github.com/Tricky1975/trickyunits_csharp.git TrickyUnits
git clone https://github.com/Tricky1975/JCR6_Sharp.git JCR6
~~~
Please mind (especially when you are on Linux) that you use the same upper and lower cases I did

Next fire up Visual Studio and load the MyData.sln file in the MyData folder, and all files SHOULD be properly linked, so you can start compiling.


