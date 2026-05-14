# TLD Director Mode 🎥

A powerful, cinematic camera and video capturing mod for *The Long Dark*, powered by MelonLoader. 
Perfect for content creators, modders, and anyone who wants to take perfect tracking shots, screenshots, or seamless looping WebM videos directly inside the game!

## ⚠️ Requirements
* **MelonLoader** installed for The Long Dark.
* **FFmpeg**: This mod relies on FFmpeg to compile the captured PNG frames into a WebM video. 
  * Download `ffmpeg.exe` and place it directly inside your game's root directory (the same folder as `tld.exe`).

## 🛠️ Features
* **Save Camera Points:** Manually save exact camera positions and rotations around the map.
* **Cinematic Mode:** Teleport seamlessly between your saved points to preview your shots.
* **Batch Recording:** Let the mod automatically visit all your saved points and record high-quality videos one by one.
* **Seamless Looping:** Built-in FFmpeg scripting can automatically crossfade the start and end of your clips to create perfect looping WebM videos.
* **LOD Enforcement:** Forces high-quality Level of Detail (LOD) models for the best visual output.

## 🎮 How to Use / Controls

All captured data and videos are stored in `[GameRootDirectory]/DirectorModeData`.

### Setting up Shots
1. Walk or fly (with Developer Console) to the exact spot you want to capture.
2. Press **Up Arrow** / **Down Arrow** to manually assign an ID to your current shot (e.g., `Shot_0`, `Shot_1`). Check the MelonLoader console to see your selected ID.
3. Press **F7** to save the current position, rotation, and Shot ID. 
   *(This data is saved to `DirectorModeData/DirectorPoints.txt`)*

### Previewing Shots
1. Press **F9** to toggle **Cinematic Mode**. Your character controls will be locked, and you will be teleported to your first saved point.
2. Use **Right Arrow** and **Left Arrow** to cycle through all your saved points.
3. Press **F9** again to return to normal gameplay (you will be teleported back to where you originally were).

### Recording Video
While in **Cinematic Mode** (F9), use the following hotkeys to start capturing frames (the game will lock to 24fps while recording):

* **F11**: Record the *current* point as a standard 7-second WebM video.
* **F12**: Record the *current* point as a *looping* WebM video (crossfaded).
* **F6**: Start a **Batch Record**. The mod will automatically visit *every* saved point, wait for cloth/wind physics to settle, record a standard video, and compile it.
* **F10**: Start a **Batch Record** for *looping* videos.

*Note: The mod captures frames as PNGs first, then uses FFmpeg to convert them to WebM in the background. Please wait for the MelonLoader console to say "[FFmpeg] DONE" before closing the game.*
