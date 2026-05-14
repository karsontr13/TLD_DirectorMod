# TLD Director Mode 🎥

A powerful, cinematic camera and video capturing mod for *The Long Dark*, powered by MelonLoader. 
Perfect for content creators, modders, and anyone who wants to take perfect tracking shots, screenshots, or seamless looping MP4/WebM videos directly inside the game!
If you encounter any errors, please let me know.

> ⚠️ **IMPORTANT: ABOUT IN-GAME LAG DURING RECORDING**
> You may experience massive FPS drops or lag in your game while actively recording. **This is completely normal!** It happens because the mod is rapidly capturing high-quality frames per second (based on your configured FPS setting). 
> 
> **Do not panic!** The FPS drop will instantly resolve once the recording is complete, and your final compiled video will be 100% perfectly smooth and stutter-free!

## ⚠️ Requirements
* **MelonLoader** installed for The Long Dark.
* **FFmpeg**: This mod relies on FFmpeg to compile the captured PNG frames into a video. 
* Download `ffmpeg.exe` and place it directly inside your game's root directory (the same folder as `tld.exe`).
 
* ## ⚠️ Installation (IMPORTANT)

This mod strictly requires **FFmpeg** to compile the captured frames into a final video. Please follow the steps below carefully:

**Mod Installation:**
1. Ensure you have [MelonLoader](https://melonwiki.xyz/#/) installed for The Long Dark.
2. Download the `TLD_DirectorMode.dll` file and place it inside your game's `Mods` folder.

**FFmpeg Installation (Required):**
1. Download the latest Windows release of FFmpeg. *(You can search for "FFmpeg download Windows" or download the "essentials" build from sites like gyan.dev/FFmpeg).*
2. Open the downloaded `.zip` or `.7z` archive.
3. Navigate into the `bin` folder inside the archive and find the **`ffmpeg.exe`** file.
4. Extract and copy this **`ffmpeg.exe`** file **directly into the root folder of The Long Dark** (the same directory where `tld.exe` is located).

*(If you do not place ffmpeg.exe in the correct folder, the mod will capture screenshots but will fail to compile them into a video!)*

## 🛠️ Features
* **Save Camera Points:** Manually save exact camera positions and rotations around the map.
* **Cinematic Mode:** Teleport seamlessly between your saved points to preview your shots.
* **Batch Recording:** Let the mod automatically visit all your saved points and record high-quality videos one by one.
* **Seamless Looping:** Built-in FFmpeg scripting can automatically crossfade the start and end of your clips to create perfect looping videos.
* **Full Customization:** Choose your preferred video format (MP4 or WebM), target FPS, capture duration, fade length, **video resolution** (from 720p up to 4K or Native), and **compression quality** (Low to Ultra) directly via the `UserData/MelonPreferences.cfg` file.
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
While in **Cinematic Mode** (F9), use the following hotkeys to start capturing frames *(the game will lock to your configured FPS, default 24fps, while recording)*:

* **F11**: Record the *current* point as a standard video (default 7 seconds).
* **F12**: Record the *current* point as a *looping* video (crossfaded).
* **F6**: Start a **Batch Record**. The mod will automatically visit *every* saved point, wait for cloth/wind physics to settle, record standard videos, and compile them.
* **F10**: Start a **Batch Record** for *looping* videos.
* **Backspace**: If you accidentally start recording or a batch capture, press this key to abort the operation immediately. (The mod will clean up temp files and stop capturing).

*Note: The mod captures frames as PNGs first, then uses FFmpeg to convert them to your chosen format (MP4 or WebM) in the background. Please wait for the MelonLoader console to say "[FFmpeg] DONE" before closing the game.*
