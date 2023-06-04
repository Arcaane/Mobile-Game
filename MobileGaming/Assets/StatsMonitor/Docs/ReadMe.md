# Stats Monitor for Unity

Thank you for purchasing Stats Monitor! Stats Monitor is a highly customizable In-game FPS counter for Unity projects that displays a number of performance-related statistics and a graph useful for game performance testing.

[Support Forum](http://forum.unity3d.com/threads/stats-monitor-performance-stats-graph-for-unity3d.351112/)

### USAGE

#### Adding Stats Monitor to a Scene

To ensure that Stats Monitor is added correctly to a scene you should use the menu ```GameObject/Create Other/Stats Monitor```
or the Context Menu ``Create Other/Stats Monitor``.

When running the game, Stats Monitor will appear by default in the top-right corner of the screen.

#### Using the Touch Controls

Stats Monitor can be controlled on supported mobile touch display devices with the following touch combinations.

**Toggling between inactive and active state:**

> Touch the display with two fingers and then tap once with a third finger.

**Switching between the different positions:**

> Tap three times with one finger on the Stats Monitor widget while it is visible.
 
**Switching between the different layout styles:**

> Touch the Stats Monitor widget with one finger and then tap three times outside the widget with a second finger.

Please note that while these are the default controls, the number of touches/taps can be changed in the Stats Monitor component inspector.

#### Using the Component Inspector

All parameters of Stats Monitor can be adjusted in the component inspector. The actual Stats Monitor script is wrapped into a container object so you will find the component inspector on the wrapped child object named ```StatsMonitor```.

**HINT**: You can adjust all parameters while the game is running and then store your adjustments to be permanent by right- clicking on the Stats Monitor script name in the components inspector and choose ```Copy Component``` from the context menu. After that, stop your game, again right-click on the script name and choose ```Paste Component Values``` from the context menu.

### CHANGELOG

V1.3.7

- Fixed graph GC issue.
- Fixed FPS graph bug.
- Reduced string allocation.
- Object counting is now disabled by default.
- Changed included docs to markdown format.

V1.3.6

- Fixes crashing on recent Unity versions.
- Renamed several parameters for more clarity.
- Garbage Collection monitoring can now be toggled and is off by default.
- Setting Object Count Interval to 0 will disable object counting.
- Several internal changes and improvements.

V1.3.5

- Small performance improvement.

V1.3.4

- Fixed: Profiler import issue.
- Fixed: control shortcuts not working in editor when set to any mobile platform.
- Fixed: Keep Alive behavior issues.
- Change: Stats Monitor will now completely remove itself from the scene hierarchy if not kept alive.
- Added: New aligments for Stats Monitor widget: Middle Right and Middle Left.

V1.3.3

- Replaced deprecated scene load code.
- Added Widget getter in StatsMonitor class.
- Moved public enums to StatsMonitor namespace.

V1.3.2

- Several internal code optimizations to make the package overall more lightweight and sealed. Most methods except for public API have been made internal and all code is now in the same namespace.
- Added Themes option: allows to quickly pick one of four preset styling themes.
- Optimized graph drawing.
- When set to RenderMode: Screen Space Camera, Stats Monitor will automatically set its canvas plane distance to the camera’s near clipping plane ensuring that it is front-most and not getting hidden behind other objects or being out of the camera’s view distance.
- Added check to ensure Stats Monitor is in scene root (it will move it to scene root if it’s not).
- Added button in inspector to quickly open params panel.

V1.3.1

- Added Render Mode option to render Stats Monitor either as Screen Space Overlay (default) or as Screen Space Camera. The second option may be more preferable depending on your game's display setup and looks more clean in the editor.
- Added descriptions of what the stats values mean. This can be found in the inspector panel of Stats Monitor.
- Made the GC Blip default color more obvious.
- Fixed view scaling bug that resulted when switching between different view styles in Camera render mode.

### FEATURES OVERVIEW

**FPS Stats** - Displays detailed information related to the framerate:
- The current frames per second.
- The last lowest framerate (MIN).
- The last highest framerate (MAX).
- Calculated average framerate (AVG), with configurable average samples count.
- The milliseconds it took for the game to render a frame (MS).

**Memory Stats** - Displays memory-related stats:
- _Total Memory_: Shows the total private memory amount which was reserved by the OS for the game. This memory can’t be used by other applications as long as the game is running.
- _Allocated Memory_: Shows the amount of private memory that is actually used by the game. While the Total Memory value is a ‘pool’ of reserved memory, the game doesn’t necessarily use all of it all the time. Allocated memory is what the game currently uses.
- _Mono Memory_: Shows the amount of memory that is used by Mono objects. This includes all the objects created by scripts.

**SysInfo Stats** - Displays information related to the hardware the game is running on:
- Operating System details.
- The CPU and CPU cores.
- GPU, Video RAM andRender API information. - System RAM.
- Screen size and window dimensions.

**Performance Graph** - The graph section logs FPS- and memory stats over time and is helpful in tracking FPS drops and memory spikes. It draws graphs for FPS, MS, Total Mem, Alloc Mem, and Mono Mem.

**Widget Styles** - Stats Monitor can be switched between four different styles:
- _Minimal_: Displays a small FPS counter only.
- _Stats Only_: Displays only the textual FPS/Mem stats panel.
- _Standard_: Displays the FPS/Mem stats panel as well as the graph panel.
- _Full_: Displays the FPS/Mem stats panel, the graph panel, and the SysInfo panel.

Switch between different widget positions via hotkey (Upper Left, Upper Right, Lower Right, etc.)

Switch between different widget styles via hot key.

**Customizable hot keys** for activating/deactivating the widget, switching between different widget positions, and switching between different styles (see Widget Styles feature). These functions can be configured to be triggered by a single key or additionally by a modifier key. This allows you to configure key combinations like for example SHIFT+Backquote or LCTRL+Backquote.

**Modes** - Stats Monitor can be switched between three different operating modes:
- _Active_: The stats monitor is visible and measures performance statistics.
- _Inactive_: The monitor is not visible and doesn’t measure anything. In this mode the monitor wont do anything except for checking for hot key input.
- _Passive_: In this mode the stats monitor is not displayed but still measures performance in the background.

**Framerate Throttling** - Allows you to set Stats Monitor to run the game at a specified maximum framerate. Useful for checking how the game performs at a specific framerate. Note that as long as throttling is enabled the game will disable VSync.

**Keep Alive** - While enabled, prevents Stats Monitor from being destroyed on level load.

**Configurable update interval** between 0.01ms and 10 seconds.

**Component Inspector** - All options are configurable in the Stats Monitor component inspector.

**Customizable Look** - Allows for customising font, two different font sizes (FPS counter and all other text), background gradient and transparency, text/graph colors, graph background color and transparency, padding, spacing, and graph height. Also included is the default-used font 'TerminalStats'.

**Light-weight** - Stats Monitor has been developed for being a minimal, unobtrusive but versatile tool that doesn’t consume any noticeable CPU cycles. The UI uses Unity’s modern UI system and is very responsive.

**FPS Warning and Critical Thresholds** - Two additional colors and FPS threshold values can be set to indicate framerates at very low ranges. These colors are also reflected in the graph.

**Touch Controls** - Use Stats Monitor on supported touch display devices and easily toggle it on/off or switch between different positions and layout styles.

**Auto-scaling** - By default Stats Monitor will scale its display depending on the device screen DPI. This keeps the widget at a readable size on high resolutions, like for example on Retina displays. Optionally the scaling can be set manually.

**Easy Integration** - Simply create the Stats Monitor from the _GameObject/Create Other_ menu and it's ready to use.

**Clean Code** - Written 100% in cleanest, well-optimized C# code.
