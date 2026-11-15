using UnityEngine;

namespace SelfBulletting.Views.Common
{
    /// <summary>
    /// Shader property ids the views write through a <see cref="MaterialPropertyBlock"/>.
    /// </summary>
    public static class ShaderIds
    {
        public static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        /// <summary>How far a fire is put out: 0 — burning, 1 — all steam (the "SelfBulletting/Fire" shader).</summary>
        public static readonly int Douse = Shader.PropertyToID("_Douse");
    }
}
