using Il2Cpp;
using MelonLoader;
using MelonLoader.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[assembly: MelonInfo(typeof(TLD_DirectorMode.DirectorMain), "TLD Director Mode", "1.3.0", "KarsonTR")]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace TLD_DirectorMode
{
    public enum VideoFormat { WebM, MP4 }
    public enum VideoResolution { Res_720p, Res_1080p, Res_1440p, Res_4K, Native }
    public enum VideoQuality { Low, Medium, High, Ultra }

    public class DirectorMain : MelonMod
    {
        private static bool isCinematicMode = false;
        private static bool isCapturing = false;
        private static bool isBatchMode = false;
        private static bool isConverting = false;
        private static int currentPointIndex = 0;
        private static int currentShotId = 0;

        private static string baseDir = Path.Combine(MelonEnvironment.GameRootDirectory, "DirectorModeData");
        private static string configPath = Path.Combine(baseDir, "DirectorPoints.txt");

        private static List<Vector3> posList = new List<Vector3>();
        private static List<Quaternion> rotList = new List<Quaternion>();
        private static List<int> idList = new List<int>();
        private static List<string> targetSceneList = new List<string>();

        private Vector3 originalPlayerPos;
        private Quaternion originalPlayerRot;
        private const float playerHeightOffset = 1.7f;
        private static GameObject magicPlatform;

        // --- MELON PREFERENCES ---
        private static MelonPreferences_Category configCategory;
        private static MelonPreferences_Entry<int> prefFPS;
        private static MelonPreferences_Entry<int> prefDuration;
        private static MelonPreferences_Entry<float> prefFadeDuration;
        private static MelonPreferences_Entry<VideoFormat> prefFormat;
        private static MelonPreferences_Entry<VideoResolution> prefResolution; // YENİ
        private static MelonPreferences_Entry<VideoQuality> prefQuality; // YENİ

        public override void OnInitializeMelon()
        {
            if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);

            configCategory = MelonPreferences.CreateCategory("TLD_DirectorMode", "Director Mode Settings");
            prefFPS = configCategory.CreateEntry<int>("FPS", 24, "Capture FPS", "Video framerate (e.g., 24, 30, 60)");
            prefDuration = configCategory.CreateEntry<int>("Duration", 7, "Capture Duration (Seconds)", "How long each video should be");
            prefFadeDuration = configCategory.CreateEntry<float>("FadeDuration", 2.0f, "Loop Fade Duration (Seconds)", "Crossfade duration for looping videos");
            prefFormat = configCategory.CreateEntry<VideoFormat>("Format", VideoFormat.MP4, "Video Format", "Choose between MP4 and WebM");

            // Çözünürlük ve Kalite ayarları
            prefResolution = configCategory.CreateEntry<VideoResolution>("Resolution", VideoResolution.Res_1080p, "Video Resolution", "Output resolution (Native uses your screen size)");
            prefQuality = configCategory.CreateEntry<VideoQuality>("Quality", VideoQuality.High, "Video Quality", "Affects file size and visual fidelity");

            MelonLogger.Msg("TLD Director Mode v1.3.0 Initialized! Quality settings loaded.");
        }

        private string GetCurrentSceneName()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            return sceneName.Replace("_SANDBOX", "").Replace("_DLC01", "").Replace("_WILDLIFE", "");
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                currentShotId++;
                MelonLogger.Msg($"[Director Mode] Target Shot ID: Shot_{currentShotId}");
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) && currentShotId > 0)
            {
                currentShotId--;
                MelonLogger.Msg($"[Director Mode] Target Shot ID: Shot_{currentShotId}");
            }

            if (Input.GetKeyDown(KeyCode.F7) && !isCapturing) SaveCurrentPoint();
            if (Input.GetKeyDown(KeyCode.F9) && !isCapturing) ToggleCinematicMode();

            if (isCinematicMode && !isCapturing && !isConverting)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow)) NextPoint();
                else if (Input.GetKeyDown(KeyCode.LeftArrow)) PreviousPoint();

                if (Input.GetKeyDown(KeyCode.F12)) StartCaptureFlow(currentPointIndex, true, false);
                if (Input.GetKeyDown(KeyCode.F11)) StartCaptureFlow(currentPointIndex, false, false);
                if (Input.GetKeyDown(KeyCode.F10)) StartCaptureFlow(0, true, true);
                if (Input.GetKeyDown(KeyCode.F6)) StartCaptureFlow(0, false, true);

                QualitySettings.lodBias = 100f;
            }
        }

        private void RefreshPointsList()
        {
            if (!File.Exists(configPath)) return;
            posList.Clear(); rotList.Clear(); idList.Clear(); targetSceneList.Clear();

            foreach (var line in File.ReadAllLines(configPath))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split('|');
                var p = parts[0].Split(','); var r = parts[1].Split(',');

                posList.Add(new Vector3(float.Parse(p[0]), float.Parse(p[1]), float.Parse(p[2])));
                rotList.Add(Quaternion.Euler(new Vector3(float.Parse(r[0]), float.Parse(r[1]), float.Parse(r[2]))));

                int shotId = 0;
                if (parts.Length > 2) int.TryParse(parts[2], out shotId);
                idList.Add(shotId);

                string sName = (parts.Length > 3) ? parts[3] : "Global";
                targetSceneList.Add(sName);
            }
        }

        private void StartCaptureFlow(int startIndex, bool isLooping, bool batch)
        {
            RefreshPointsList();
            if (posList.Count == 0) return;

            isBatchMode = batch;
            currentPointIndex = startIndex;
            MelonCoroutines.Start(PrepareAndCaptureRoutine(isLooping));
        }

        private IEnumerator PrepareAndCaptureRoutine(bool isLooping)
        {
            GoToPoint(currentPointIndex);
            yield return new WaitForSeconds(4.0f);
            MelonCoroutines.Start(CaptureClipRoutine(isLooping));
        }

        private void SaveCurrentPoint()
        {
            var cam = GameManager.GetMainCamera();
            if (cam == null) return;

            string sceneName = GetCurrentSceneName();
            string dataLine = $"{cam.transform.position.x},{cam.transform.position.y},{cam.transform.position.z}|{cam.transform.rotation.eulerAngles.x},{cam.transform.rotation.eulerAngles.y},{cam.transform.rotation.eulerAngles.z}|{currentShotId}|{sceneName}\n";

            File.AppendAllText(configPath, dataLine);
            MelonLogger.Msg($"[Director Mode] Point Shot_{currentShotId} Saved! (Scene: {sceneName})");
            currentShotId++;
        }

        private void ToggleCinematicMode()
        {
            isCinematicMode = !isCinematicMode;
            if (isCinematicMode)
            {
                RefreshPointsList();
                if (posList.Count == 0)
                {
                    MelonLogger.Warning("[Director Mode] No points saved! Press F7 to save points first.");
                    isCinematicMode = false;
                    return;
                }

                originalPlayerPos = GameManager.GetPlayerTransform().position;
                originalPlayerRot = GameManager.GetPlayerTransform().rotation;
                currentPointIndex = 0;
                GoToPoint(currentPointIndex);
                MelonLogger.Msg(">>> CINEMATIC MODE ENABLED <<<");
            }
            else
            {
                if (magicPlatform != null) { UnityEngine.Object.Destroy(magicPlatform); magicPlatform = null; }
                GameManager.GetPlayerManagerComponent().TeleportPlayer(originalPlayerPos, originalPlayerRot);

                var cc = GameManager.GetPlayerTransform().GetComponent<CharacterController>();
                if (cc != null) cc.enabled = true;
                MelonLogger.Msg(">>> CINEMATIC MODE DISABLED <<<");
            }
        }

        private void GoToPoint(int index)
        {
            var pm = GameManager.GetPlayerManagerComponent();
            if (pm == null) return;

            Vector3 targetPos = new Vector3(posList[index].x, posList[index].y - playerHeightOffset, posList[index].z);

            if (magicPlatform == null)
            {
                magicPlatform = GameObject.CreatePrimitive(PrimitiveType.Cube);
                UnityEngine.Object.Destroy(magicPlatform.GetComponent<MeshRenderer>());
                magicPlatform.transform.localScale = new Vector3(3f, 0.1f, 3f);
            }
            magicPlatform.transform.position = new Vector3(targetPos.x, targetPos.y - 0.05f, targetPos.z);

            pm.TeleportPlayer(targetPos, rotList[index]);

            var cam = GameManager.GetMainCamera();
            if (cam != null) cam.transform.rotation = rotList[index];

            var cc = GameManager.GetPlayerTransform().GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            foreach (var lod in GameObject.FindObjectsOfType<LODGroup>()) { lod.ForceLOD(0); }
        }

        private void NextPoint() { currentPointIndex = (currentPointIndex + 1) % posList.Count; GoToPoint(currentPointIndex); }
        private void PreviousPoint() { currentPointIndex = (currentPointIndex - 1 + posList.Count) % posList.Count; GoToPoint(currentPointIndex); }

        private IEnumerator CaptureClipRoutine(bool isLooping)
        {
            isCapturing = true;

            int currentFPS = prefFPS.Value;
            int currentDuration = prefDuration.Value;
            VideoFormat currentFormat = prefFormat.Value;
            VideoResolution currentRes = prefResolution.Value;
            VideoQuality currentQual = prefQuality.Value;

            int totalFrames = currentFPS * currentDuration;
            Time.captureFramerate = currentFPS;

            bool isNight = GameManager.GetTimeOfDayComponent()?.IsNight() ?? false;
            int actualShotId = idList[currentPointIndex];
            string sceneName = targetSceneList[currentPointIndex];

            string sceneDir = Path.Combine(baseDir, sceneName);
            if (!Directory.Exists(sceneDir)) Directory.CreateDirectory(sceneDir);

            string extension = currentFormat == VideoFormat.MP4 ? ".mp4" : ".webm";
            string nightSuffix = isNight ? "_Night" : "";
            string finalVideoName = $"Shot_{actualShotId}{nightSuffix}{extension}";
            string finalVideoPath = Path.Combine(sceneDir, finalVideoName);

            string clipDir = Path.Combine(baseDir, $"Temp_{sceneName}_{actualShotId}_{DateTime.Now.Ticks}");
            Directory.CreateDirectory(clipDir);

            MelonLogger.Msg($"[RECORDING] Started: {finalVideoName} ({currentFPS} FPS, {currentRes}, {currentQual}) ...");

            for (int f = 0; f < totalFrames; f++)
            {
                yield return new WaitForEndOfFrame();
                Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                tex.Apply();
                File.WriteAllBytes(Path.Combine(clipDir, $"frame_{f:D05}.png"), ImageConversion.EncodeToPNG(tex).ToArray());
                UnityEngine.Object.Destroy(tex);
            }

            Time.captureFramerate = 0;
            isCapturing = false;
            isConverting = true;

            Task.Run(() => ConvertToVideo(clipDir, finalVideoPath, isLooping, currentFPS, currentDuration, prefFadeDuration.Value, currentFormat, currentRes, currentQual));

            if (isBatchMode)
            {
                while (isConverting) { yield return null; }
                currentPointIndex++;
                if (currentPointIndex < posList.Count)
                {
                    MelonLogger.Msg($"[BATCH RECORD] Proceeding to point {currentPointIndex + 1}/{posList.Count}...");
                    MelonCoroutines.Start(PrepareAndCaptureRoutine(isLooping));
                }
                else
                {
                    isBatchMode = false;
                    currentPointIndex = 0;
                    GoToPoint(0);
                    MelonLogger.Msg(">>> BATCH RECORDING COMPLETE <<<");
                }
            }
        }

        private void ConvertToVideo(string clipDir, string finalVideoPath, bool isLooping, int fps, int duration, float fadeDuration, VideoFormat format, VideoResolution res, VideoQuality qual)
        {
            try
            {
                string ffmpegPath = Path.Combine(MelonEnvironment.GameRootDirectory, "ffmpeg.exe");
                if (!File.Exists(ffmpegPath))
                {
                    MelonLogger.Error("[FFmpeg] ffmpeg.exe not found in game root directory!");
                    return;
                }

                string inputPattern = Path.Combine(clipDir, "frame_%05d.png");
                float cutPoint = duration - fadeDuration;
                var culture = System.Globalization.CultureInfo.InvariantCulture;

                string cutStr = cutPoint.ToString("F1", culture);
                string durStr = duration.ToString("F1", culture);
                string fadeStr = fadeDuration.ToString("F1", culture);

                // Çözünürlük ayarını (Scale) belirle
                string scaleStr = "";
                switch (res)
                {
                    case VideoResolution.Res_720p: scaleStr = "scale=1280:-1"; break;
                    case VideoResolution.Res_1080p: scaleStr = "scale=1920:-1"; break;
                    case VideoResolution.Res_1440p: scaleStr = "scale=2560:-1"; break;
                    case VideoResolution.Res_4K: scaleStr = "scale=3840:-1"; break;
                    case VideoResolution.Native: scaleStr = "copy"; break; // Native ise boyutlandırma yapma
                }

                // Kalite ayarını (CRF) belirle (Düşük CRF = Yüksek Kalite)
                int crfValue = 18; // Default High
                switch (qual)
                {
                    case VideoQuality.Low: crfValue = 28; break;
                    case VideoQuality.Medium: crfValue = 23; break;
                    case VideoQuality.High: crfValue = 18; break;
                    case VideoQuality.Ultra: crfValue = 12; break;
                }

                string filterComplex = "";
                if (isLooping)
                {
                    string scaleFilter = scaleStr == "copy" ? "" : $",{scaleStr}";
                    filterComplex = $"[0:v]split[v1][v2];" +
                                    $"[v1]trim=start=0:end={cutStr},setpts=PTS-STARTPTS[main];" +
                                    $"[v2]trim=start={cutStr}:end={durStr},setpts=PTS-STARTPTS[end];" +
                                    $"[end][main]xfade=transition=fade:duration={fadeStr}:offset=0{scaleFilter}[out]";
                }
                else
                {
                    filterComplex = scaleStr == "copy" ? "" : $"[0:v]{scaleStr}[out]";
                }

                string filterArgs = string.IsNullOrEmpty(filterComplex) ? "" : $"-filter_complex \"{filterComplex}\" -map \"[out]\"";
                if (string.IsNullOrEmpty(filterComplex) && !isLooping) filterArgs = ""; // Native ve No-Loop durumu

                string codecArgs = "";
                if (format == VideoFormat.MP4)
                {
                    codecArgs = $"-c:v libx264 -preset slow -crf {crfValue} -pix_fmt yuv420p";
                }
                else
                {
                    // WebM için CRF ölçeği biraz farklıdır, oranı koruyoruz
                    int webmCrf = crfValue - 6;
                    if (webmCrf < 4) webmCrf = 4;
                    codecArgs = $"-c:v libvpx -crf {webmCrf} -b:v 0 -qmin 10 -qmax 50 -deadline good -cpu-used 0 -auto-alt-ref 1";
                }

                ProcessStartInfo videoInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = $"-framerate {fps} -i \"{inputPattern}\" {filterArgs} {codecArgs} -y \"{finalVideoPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(videoInfo)) { process.WaitForExit(); }

                if (Directory.Exists(clipDir)) Directory.Delete(clipDir, true);
                MelonLogger.Msg($"[FFmpeg] DONE: Video saved successfully to {finalVideoPath}");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[FFmpeg] Error occurred: {ex.Message}");
            }
            finally
            {
                isConverting = false;
            }
        }
    }
}