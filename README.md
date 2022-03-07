# UnityModExtension

Visual Studio 2019 Extension that provides a button command under the Debug top level menu.

The button starts the specified target executable and launch the debugger on a process that listens to the given port. (Default 55555)

This extension was made for BepInEx plugins for Unity games in mind.

For this extension to work properly you also need : 
- The [Tools for Unity Extension](https://docs.microsoft.com/en-us/visualstudio/gamedev/unity/get-started/getting-started-with-visual-studio-tools-for-unity?pivots=windows) installed.
- [A debug version of the mono runtime dll that is used by the unity game](https://github.com/dnSpyEx/dnSpy/wiki/Debugging-Unity-Games)
