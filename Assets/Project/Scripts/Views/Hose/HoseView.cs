using System;
using System.Collections.Generic;
using SelfBulletting.Gameplay.Hose;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Views.Water;
using UnityEngine;

namespace SelfBulletting.Views.Hose
{
    /// <summary>
    /// The Hose stream: a droplet of the player's water for every drop in the air, stretched along its flight, and a
    /// small splash wherever a drop hits. The droplets are copies of one template made before the round and reused,
    /// so nothing is instantiated while the water flows.
    /// </summary>
    public sealed class HoseView : MonoBehaviour, IHoseView
    {
        private const float DiameterPerRadius = 2f;

        [Tooltip("Sphere of 1 m diameter with the water drop material, child of this object. It stays hidden; " +
            "the droplets are its copies.")]
        [SerializeField] private Transform _dropTemplate;

        [Tooltip("Splash where a drop hits. Optional.")]
        [SerializeField] private WaterSplashEffect _splash;

        [Tooltip("How many times longer than wide a droplet looks along its flight.")]
        [SerializeField, Min(1f)] private float _stretch = 1.6f;

        private readonly List<Transform> _drops = new List<Transform>();
        private int _shownCount;

        public void Prepare(int maxDrops)
        {
            while (_drops.Count < maxDrops)
            {
                Transform drop = Instantiate(_dropTemplate, transform);
                drop.name = $"{_dropTemplate.name}_{_drops.Count}";
                drop.gameObject.SetActive(false);
                _drops.Add(drop);
            }
        }

        public void ShowDrops(ReadOnlySpan<WaterDrop> drops)
        {
            int count = Mathf.Min(drops.Length, _drops.Count);
            for (int i = 0; i < count; i++)
            {
                Place(_drops[i], drops[i]);
            }

            HideFrom(count);
        }

        public void PlaySplash(Vector3 center, Vector3 groundPoint, float dropRadius)
        {
            if (_splash != null)
            {
                _splash.PlayDrop(center, groundPoint, dropRadius);
            }
        }

        public void Hide()
        {
            HideFrom(0);
        }

        private void Awake()
        {
            _dropTemplate.gameObject.SetActive(false);
        }

        private void Place(Transform view, in WaterDrop drop)
        {
            if (!view.gameObject.activeSelf)
            {
                view.gameObject.SetActive(true);
            }

            float diameter = drop.Radius * DiameterPerRadius;
            view.SetPositionAndRotation(drop.Center, Quaternion.LookRotation(drop.Direction));
            view.localScale = new Vector3(diameter, diameter, diameter * _stretch);
        }

        private void HideFrom(int first)
        {
            for (int i = first; i < _shownCount; i++)
            {
                _drops[i].gameObject.SetActive(false);
            }

            _shownCount = first;
        }
    }
}
