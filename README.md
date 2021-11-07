CommandLine for Unity
=
A command line prompt for Unity projects.

Features
-
- Auto-completion: navigate suggestions with Tab and Shift-Tab.
- History: lookup the history with Up and Down arrow keys.
- Syntax highlighting. 
- Very easy to use: just add a `[Command]` attribute to a field, property or method. 
- Works both for static and non-static members.
- Works for multiple `MonoBehaviour`s in a scene.
- Command names are generated automatically.
- Partial name matching.
- Unary and binary operators: `!`,`~`,`+`,`-`,`*`,`/`,`%`.
- Variable and command interpolation: `MyCommand $Variable` or `MyCommand ${MyOtherCommand 1 2 "hello"}`

Usage
-
    [Command]
    public static void MyCommand() {
        Debug.Log("Hello from CommandLine!");
    }