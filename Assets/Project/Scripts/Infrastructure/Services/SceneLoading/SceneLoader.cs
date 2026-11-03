using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SelfBulletting.Infrastructure.Services.SceneLoading
{
    /// <summary>
    /// Loads content scenes additively on top of the always-loaded Boot scene.
    /// Unloading a scene destroys everything it owns, which is how every replay starts from scratch.
    /// </summary>
    public sealed class SceneLoader
    {
        public async UniTask LoadAsync(string sceneName, IProgress<float> progress, CancellationToken cancellationToken)
        {
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                progress?.Report(1f);
                return;
            }

            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive)
                .ToUniTask(progress, cancellationToken: cancellationToken);

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        }

        public async UniTask UnloadAsync(string sceneName, CancellationToken cancellationToken)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.isLoaded)
            {
                return;
            }

            await SceneManager.UnloadSceneAsync(scene).ToUniTask(cancellationToken: cancellationToken);
            await Resources.UnloadUnusedAssets().ToUniTask(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Every content scene has exactly one entry point on one of its root objects.
        /// </summary>
        public TEntryPoint FindEntryPoint<TEntryPoint>(string sceneName) where TEntryPoint : Component
        {
            foreach (GameObject root in SceneManager.GetSceneByName(sceneName).GetRootGameObjects())
            {
                if (root.TryGetComponent(out TEntryPoint entryPoint))
                {
                    return entryPoint;
                }
            }

            throw new InvalidOperationException($"{typeof(TEntryPoint).Name} is not found in scene {sceneName}.");
        }
    }
}
