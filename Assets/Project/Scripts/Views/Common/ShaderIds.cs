using UnityEngine;

namespace SelfBulletting.Views.Common
{
    /// <summary>
    /// Shader property ids the views write through a <see cref="MaterialPropertyBlock"/>.
    /// </summary>
    public static class ShaderIds
    {
        public static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    }
}
