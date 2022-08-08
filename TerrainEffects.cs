using System.Collections;
using System.Collections.Generic;
using Terrain.View;
using UnityEngine;
using Zenject;

namespace Terrain
{
    [CreateAssetMenu(fileName = "Terrain Effects", menuName = "Terrain/Terrain Effects")]
    public class TerrainEffects : ScriptableObjectInstaller<TerrainEffects>
    {
        [SerializeField] private TerrainEffectView _terrainHealHeartWithTrail;
        [SerializeField] private TerrainEffectView _terrainHealHeart;
        [SerializeField] private GameObject _terrainTileHealParticles;
        [SerializeField] private List<AnimationCurve> _healTrajectoryCurves;

        /// <summary>
        /// Префаб сердца для эффекта лечения земли.
        /// Используется при высокой производительности.
        /// </summary>
        public TerrainEffectView TerrainHealHeartWithTrail => _terrainHealHeartWithTrail;

        /// <summary>
        /// Префаб сердца для эффекта лечения земли.
        /// Используется при низкой производительности.
        /// </summary>
        public TerrainEffectView TerrainHealHeart => _terrainHealHeart;

        /// <summary>
        /// Эффект полного излечения земли.
        /// </summary>
        public GameObject TerrainTileHealParticles => _terrainTileHealParticles;

        /// <summary>
        /// Анимационные кривые для рандомного полета сердечек к мертвой земле.
        /// </summary>
        public IReadOnlyList<AnimationCurve> HealTrajectoryCurves => _healTrajectoryCurves;

        public override void InstallBindings()
        {
            Container.Bind<TerrainEffects>().FromInstance(this).AsSingle();
        }
    }
}

