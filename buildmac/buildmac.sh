#!/bin/sh

MACBUNDLE=MyData_Test1.app

echo Mac application creation script
echo ===============================

if [ -d "$MACBUNDLE" ]; then
   echo "Destroying old version";
   rm -Rv "$MACBUNDLE"
fi

if [ "$1" != "skipcompile" ]; then
   echo "Compiling MyData"
   msbuild "-p:Configuration=Release;WarningLevel=2" ../MyData
fi


echo "Creating bundle folders"
mkdir "$MACBUNDLE"
mkdir "$MACBUNDLE/Contents"
mkdir "$MACBUNDLE/Contents/MacOS"
mkdir "$MACBUNDLE/Contents/Resources"


echo "Copying info"
cp -v "Needed/Info.plist" "$MACBUNDLE/Contents"

echo "Copying icon"
cp -v "Needed/Icon.icns" "$MACBUNDLE/Contents/Resources/MyData.icns"

echo "Copying binaries"
cp -v "../MyData/bin/Release/"* "$MACBUNDLE/Contents/MacOS"

echo "Copying startup file"
cp -v "Needed/RunShell.sh" "$MACBUNDLE/Contents/MacOS/MyData"
chmod +x "$MACBUNDLE/Contents/MacOS/MyData"

echo "If no errors (I said ERRORS not WARNINGS!!!) everything SHOULD be in order"
echo "In that case there should be a MyData.app bundle in this very folder."
echo "You can drag it into your /Applications folder, if you like ;)"
