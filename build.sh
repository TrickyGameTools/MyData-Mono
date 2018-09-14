#!/bin/sh

echo MyData building script
echo ======================
echo This script will call the compiler from a unix termnal. 
echo Please ignore any warnings especially about unreachable code.
echo If the building tools deems the build "succeeded", all should be fine.
echo This script was tested with Microsoft \(R\) Build Engine version 15.7.224.30163 \(xplat-master/d375bb6e Sat Jun 30 05:26:28 EDT 2018\) for Mono


msbuild /p:Config=Release MyData.sln
