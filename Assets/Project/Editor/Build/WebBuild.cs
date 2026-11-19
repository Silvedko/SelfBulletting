using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SelfBulletting.Editor.Build
{
    /// <summary>
    /// Web build for GitHub Pages. Pages serves files as they are and cannot send a Content-Encoding header,
    /// so the build is gzipped with the decompression fallback: the loader unpacks the files in the browser.
    /// The page is the Portrait template (<c>Assets/WebGLTemplates/Portrait</c>): a 9:16 column on a desktop,
    /// the whole screen on a phone. The active build profile or platform (Android) is restored after the build.
    /// </summary>
    internal static class WebBuild
    {
        private const string MenuPath = "SelfBulletting/Build/Web (GitHub Pages)";
        private const string DefaultOutputPath = "Builds/WebGL";
        private const string OutputArgument = "-buildOutput";
        private const string Template = "PROJECT:Portrait";

        // Default canvas size: the 9:16 of the UI reference resolution (1080×1920), halved.
        private const int CanvasWidth = 540;
        private const int CanvasHeight = 960;

        [MenuItem(MenuPath)]
        private static void BuildFromMenu()
        {
            if (Build(DefaultOutputPath))
            {
                EditorUtility.RevealInFinder(Path.Combine(DefaultOutputPath, "index.html"));
            }
        }

        /// <summary>
        /// Batch mode entry point. The output folder comes from <c>-buildOutput</c>, <c>Builds/WebGL</c> by default:
        /// <c>unity run . -- -executeMethod SelfBulletting.Editor.Build.WebBuild.BuildFromCommandLine</c>.
        /// </summary>
        public static void BuildFromCommandLine()
        {
            bool succeeded = Build(GetArgument(OutputArgument) ?? DefaultOutputPath);
            EditorApplication.Exit(succeeded ? 0 : 1);
        }

        private static bool Build(string outputPath)
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
            {
                Debug.LogError($"[{nameof(WebBuild)}] Web Build Support is not installed for this Unity version. "
                    + "If it was installed while the Editor was open, restart the Editor.");
                return false;
            }

            BuildProfile previousProfile = BuildProfile.GetActiveBuildProfile();
            BuildTarget previousTarget = EditorUserBuildSettings.activeBuildTarget;

            ApplyPlayerSettings();

            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = BuildOptions.None,
            };

            DateTime startedUtc = DateTime.UtcNow;
            BuildSummary summary = BuildPipeline.BuildPlayer(options).summary;

            // The report can say Succeeded when the web post-process step threw (e.g. a module installed
            // while the Editor was open), so the build counts only if it has written a new page.
            string page = Path.Combine(outputPath, "index.html");
            bool succeeded = summary.result == BuildResult.Succeeded
                && File.Exists(page)
                && File.GetLastWriteTimeUtc(page) >= startedUtc;
            string result = succeeded ? "Succeeded" : "Failed";
            string message = $"[{nameof(WebBuild)}] {result} ({summary.result}): {summary.totalSize / (1024f * 1024f):F1} MB, "
                + $"{summary.totalErrors} errors, {summary.totalTime:mm\\:ss} -> {Path.GetFullPath(outputPath)}";

            if (succeeded)
            {
                Debug.Log(message);
            }
            else
            {
                if (summary.result == BuildResult.Succeeded)
                {
                    message += ". The web post-process step wrote no page: if Web Build Support was installed "
                        + "while the Editor was open, restart the Editor.";
                }

                Debug.LogError(message);
            }

            RestorePlatform(previousProfile, previousTarget);
            SavePlayerSettings();
            return succeeded;
        }

        /// <summary>
        /// The Input System adds its actions to the preloaded assets for the build and takes them out after it,
        /// but the settings file is written in between. Saving again makes the file match the Editor.
        /// </summary>
        private static void SavePlayerSettings()
        {
            EditorUtility.SetDirty(Unsupported.GetSerializedAssetInterfaceSingleton(nameof(PlayerSettings)));
            AssetDatabase.SaveAssets();
        }

        private static void RestorePlatform(BuildProfile profile, BuildTarget target)
        {
            // The build leaves the Editor on WebGL even when the active profile still reads as the old one.
            if (EditorUserBuildSettings.activeBuildTarget != target)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(target), target);
            }

            if (profile != null && BuildProfile.GetActiveBuildProfile() != profile)
            {
                BuildProfile.SetActiveBuildProfile(profile);
            }
        }

        private static void ApplyPlayerSettings()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;

            // Hashed names: after a redeploy the browser cannot mix a cached old file with the new ones.
            PlayerSettings.WebGL.nameFilesAsHashes = true;
            PlayerSettings.WebGL.template = Template;
            PlayerSettings.defaultWebScreenWidth = CanvasWidth;
            PlayerSettings.defaultWebScreenHeight = CanvasHeight;
            AssetDatabase.SaveAssets();
        }

        private static string GetArgument(string name)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(arguments, name);
            return index >= 0 && index + 1 < arguments.Length ? arguments[index + 1] : null;
        }
    }
}
