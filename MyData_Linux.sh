#!/bin/sh

# This script should run MyData in Linux *if* the monoframe work has been installed.
# When you've taken this file from the repository, make sure it's in the same folder as were MyData.exe is located and it *should* work.
# Please note that I just copied stuff from the Mac script used in the Mac Application bundle. That should work, but full guarantees there are not. As I do not have full access to Linux right now, I could not test this out!



# fetch the path relative to the launch point where this shell script exists. (taken from macpack)
APP_PATH=`echo $0 | awk '{split($0,patharr,"/"); idx=1; while(patharr[idx+3] != "") { if (patharr[idx] != "/") {printf("%s/", patharr[idx]); idx++ }} }'`
if test x"$APP_PATH" == "x"; then
  APP_PATH="../../" 
fi

ASSEMBLY=MyData.exe
MONO=`which mono`

MONO_FRAMEWORK_PATH=/Library/Frameworks/Mono.framework/Versions/Current
export DYLD_FALLBACK_LIBRARY_PATH=$APP_PATH/Contents/MacOS:$MONO_FRAMEWORK_PATH/lib:/lib:/usr/lib

source /etc/profile

cd "$APP_PATH/Contents/MacOS"
exec -a "MyData" "$MONO" --debug $ASSEMBLY $@

