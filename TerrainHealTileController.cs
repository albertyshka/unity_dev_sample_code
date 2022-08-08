using Model.Terrain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using DG.Tweening;
using Common;
using Terrain.View;
using UniRx;
using Terrain.Decorator;
using System;

namespace Terrain.Controller
{
	public class TerrainHealTileController : TerrainTileController
	{
#pragma warning disable 649
		[Inject] private readonly TerrainEffects _terrainEffects;
		[Inject] private readonly IPerformanceManager _performanceManager;
		[Inject] private readonly ITerrainModel _terrainModel;
		[Inject] private readonly DecoratorManager _decoratorManager;
		[Inject] private readonly ITerrain _terrain;
		[Inject] private readonly DiContainer _container;
#pragma warning restore 649

		private readonly float ANIMATION_TIME = 1.5f;
		private readonly Vector2 HEAL_FLY_TIME_RANGE = new Vector2(1f, 7f);
		private readonly Vector2 HEAL_TRAJECTORY_AMPLITUDE_RANGE = new Vector2(0.5f, 3);
		private readonly Vector2 HEAL_MIN_MAX_DISTANCE_RANGE = new Vector2(3, 10);

		private Tween _tween;

		protected override ITerrainTileModel Model => View.Model;

		/// <summary>
		/// Функция показывает эффект лечения тайла.
		/// </summary>
		/// <param name="sourceCoords">Координаты тайла, с которого летит сердечко.</param>
		/// <param name="healCount">Число, на которое пытаются уменьшить мертвость тайла.</param>
		/// <returns>Количество мертвости, на которое земля пролечилась.</returns>
		public int ShowHealEffect(Vector3Int sourceCoords, int healCount)
		{
			var isHighPerformance = _performanceManager.PerformanceState.Value == Performance.High;
			var healPrefab = isHighPerformance ? _terrainEffects.TerrainHealHeartWithTrail : _terrainEffects.TerrainHealHeart;

			var healingHeart = _container
					.InstantiatePrefabForComponent<TerrainEffectView>(healPrefab, transform.parent);

			var sourceTile = _terrainModel.GetTileAt(sourceCoords);
			healingHeart.Position = sourceTile.Position;

			_decoratorManager.TileEffects.TryGetValueOrCreate(Model, out var tileEffect);

			if (isHighPerformance)
			{
				var seconds = ANIMATION_TIME;
				var distanceDelta = (sourceCoords - Model.Position).magnitude
					.Remap(HEAL_MIN_MAX_DISTANCE_RANGE.x, HEAL_MIN_MAX_DISTANCE_RANGE.y, 0, 1);
				distanceDelta = Mathf.Clamp01(distanceDelta);
				var secondsMult = Mathf.Lerp(HEAL_FLY_TIME_RANGE.x, HEAL_FLY_TIME_RANGE.y, distanceDelta) *
								  UnityEngine.Random.Range(0.8f, 1.2f);
				var offset = Mathf.Lerp(HEAL_TRAJECTORY_AMPLITUDE_RANGE.x, HEAL_TRAJECTORY_AMPLITUDE_RANGE.y, distanceDelta);

				var curveIndex = UnityEngine.Random.Range(0, _terrainEffects.HealTrajectoryCurves.Count);
				var curve = _terrainEffects.HealTrajectoryCurves[curveIndex];

				tileEffect.AddExplicitDeadCount(healCount);
				_tween = healingHeart.IsoObject.DoMoveExt(Model.Position, seconds * secondsMult, curve, offset).OnComplete(() =>
				{
					Destroy(healingHeart.gameObject);

					tileEffect.AddExplicitDeadCount(-healCount);
					if (Model.DeadCount.Value > 0)
					{
						_terrain.GetTileController<TileDeadProgressController>(Model).ShowDeadness();
					}
					else
					{
						// show heal effect
						var healingParticles = _container.InstantiatePrefabForComponent<TerrainEffectView>(
							_terrainEffects.TerrainTileHealParticles, transform.parent);
						healingParticles.Position = new Vector3(Model.Position.x, Model.Position.y, 1);
						
						Destroy(healingParticles.gameObject, 5f);
					}
				});
			}
			else
			{
				tileEffect.AddExplicitDeadCount(healCount);
				DOTween.To(() => healingHeart.Position, p => healingHeart.Position = p, Model.Position, 1f).OnComplete(() =>
				{
					Destroy(healingHeart.gameObject);

					tileEffect.AddExplicitDeadCount(-healCount);
					if (Model.DeadCount.Value > 0)
					{
						_terrain.GetTileController<TileDeadProgressController>(Model).ShowDeadness();
					}
				});
			}

			return Mathf.Min(Model.DeadCount.Value, healCount);
		}
	}
}

