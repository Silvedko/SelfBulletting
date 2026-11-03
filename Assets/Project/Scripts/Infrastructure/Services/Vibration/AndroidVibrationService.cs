using System;
using UnityEngine;

namespace SelfBulletting.Infrastructure.Services.Vibration
{
    /// <summary>
    /// Short pulses through android.os.VibrationEffect (API 26+, the project's minimum).
    /// Handheld.Vibrate cannot do this: it has a fixed long duration.
    /// </summary>
    public sealed class AndroidVibrationService : IVibrationService, IDisposable
    {
        private const int DefaultAmplitude = -1;

        private readonly AndroidJavaObject _vibrator;
        private readonly AndroidJavaClass _vibrationEffect;

        public AndroidVibrationService()
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                _vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            }

            _vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");
        }

        public void Pulse(int milliseconds)
        {
            try
            {
                using (AndroidJavaObject effect = _vibrationEffect.CallStatic<AndroidJavaObject>(
                           "createOneShot", (long)milliseconds, DefaultAmplitude))
                {
                    _vibrator.Call("vibrate", effect);
                }
            }
            catch (AndroidJavaException)
            {
                // A long buzz is better than none. This call is also what makes Unity add
                // the VIBRATE permission to the Android manifest, which VibrationEffect needs too.
                Handheld.Vibrate();
            }
        }

        public void Dispose()
        {
            _vibrator?.Dispose();
            _vibrationEffect?.Dispose();
        }
    }
}
