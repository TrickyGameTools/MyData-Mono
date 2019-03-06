# Build Mac

This directory will contain some scripts and stuff which will transform a MyData binary into a full application bundle.
A few notes, though:

- The application will require Mono to be installed, or this simply will not work.
- This script will assume you just cloned the github repository and kept its original setup
- The script will try to compile MyData first, although if you add the parameter "skipcompile" it won't happen. The script will however check the existence.
