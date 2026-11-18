using System;
using System.Collections.Generic;
using SelfBulletting.Configs.Modes;
using UnityEngine;

namespace SelfBulletting.Gameplay.Modes
{
    /// <summary>
    /// The config of every game mode, one each. Fails at start-up when a mode has no config or two of them, or when
    /// the modes do not lose at the same critical size, so a broken asset never surfaces halfway through a round.
    /// </summary>
    public sealed class ModeConfigs
    {
        private readonly Dictionary<GameMode, ModeConfig> _configs = new Dictionary<GameMode, ModeConfig>();

        public ModeConfigs(IEnumerable<ModeConfig> configs)
        {
            if (configs == null)
            {
                throw new ArgumentNullException(nameof(configs), "Assign the mode configs.");
            }

            foreach (ModeConfig config in configs)
            {
                Add(config);
            }

            foreach (GameMode mode in Enum.GetValues(typeof(GameMode)))
            {
                if (!_configs.ContainsKey(mode))
                {
                    throw new ArgumentException($"No config for the {mode} mode.", nameof(configs));
                }
            }

            EnsureSameCriticalSize(nameof(configs));
        }

        public ModeConfig Get(GameMode mode) => _configs[mode];

        private void Add(ModeConfig config)
        {
            if (config == null)
            {
                throw new ArgumentException("A mode config slot is empty.", nameof(config));
            }

            if (_configs.ContainsKey(config.Mode))
            {
                throw new ArgumentException($"Two configs for the {config.Mode} mode.", nameof(config));
            }

            _configs.Add(config.Mode, config);
        }

        // Win and lose are the same in every mode: a clear path wins, a blocked one at the critical size loses.
        private void EnsureSameCriticalSize(string paramName)
        {
            ModeConfig first = null;
            foreach (ModeConfig config in _configs.Values)
            {
                if (first == null)
                {
                    first = config;
                    continue;
                }

                if (!Mathf.Approximately(first.CriticalPercent, config.CriticalPercent))
                {
                    throw new ArgumentException(
                        $"The {config.Mode} mode loses at {config.CriticalPercent}% of R0 and the {first.Mode} mode at " +
                        $"{first.CriticalPercent}%: every mode must have the same critical size.", paramName);
                }
            }
        }
    }
}
