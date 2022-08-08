using Model.Counter;
using Model.Terrain;
using Settings.Quest;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Zenject;

namespace Model.Quest
{
    public static class QuestModelExtensions
    {
		/// <summary>
		/// Метод возвращает сеттингсы цели квеста.
		/// </summary>
		/// <param name="questModel">Квест.</param>
		/// <param name="questSettingsProvider">Источник списка целей квеста из конфигов. (откуда будем брать цели квестов).</param>
		/// <returns>Цель квеста или null, если цель квеста не найдена.</returns>
		public static QuestAimSettings GetAimSettings(this IQuestModel questModel, IQuestSettingsProvider questSettingsProvider)
		{
            Assert.IsTrue(questSettingsProvider.IsReady);
            var aimId = questSettingsProvider.QuestSettings[questModel.TextId].AimId;

            return questSettingsProvider.QuestAimSettings.TryGetValue(aimId, out var aim) ? aim : null;
        }

		/// <summary>
		/// Метод возвращает сеттингсы квеста.
		/// </summary>
		/// <param name="questModel">Квест.</param>
		/// <param name="questSettingsProvider">Источник списка целей квеста из конфигов. (откуда будем брать цели квестов).</param>
		/// <returns>Цель квеста или null, если цель квеста не найдена.</returns>
		public static QuestSettings GetSettings(this IQuestModel questModel, IQuestSettingsProvider questSettingsProvider)
		{
			Assert.IsTrue(questSettingsProvider.IsReady);
			return questSettingsProvider.QuestSettings.TryGetValue(questModel.TextId, out var settings) ? settings : null;
		}

		/// <summary>
		/// Метод, который пытается увеличить счетчик квестов (прогресс квестов увеличивается).
		/// </summary>
		/// <param name="questModels">Модели квестов.</param>
		/// <param name="container">DI контейнер.</param>
		/// <param name="questAimType">Тип цели квеста, у которого увеличиваем прогресс.</param>
		/// <param name="args">Агрументы увеличения прогресса.</param>
		/// <param name="value">На сколько увеличить прогресс.</param>
		public static void ProgressQuests(this IEnumerable<QuestModel> questModels, DiContainer container, 
			QuestAimType questAimType, object[] args, int value = 1)
		{
			var questSettingsProvider = container.Resolve<IQuestSettingsProvider>();

			questModels
				.Where(q => q.GetAimSettings(questSettingsProvider).Type == questAimType)
				.ForEach(q => q.Progress(container, q.GetAimSettings(questSettingsProvider), args, value));
		}

		/// <summary>
		/// Метод, который увеличивает счетчик квеста (прогресс квеста увеличивается),
		/// если аргументы удовлетворяют условиям.
		/// </summary>
		/// <param name="questModel">Модель квеста.</param>
		/// <param name="container">DI контейнер.</param>
		/// <param name="questAimSettings">Сеттингсы цели квеста.</param>
		/// <param name="args">Аргументы, которые проверяются при попытке увеличить прогресс квеста.</param>
		/// <param name="value">На сколько увеличить прогресс.</param>
		/// <returns>True - прогресс квеста успешно увеличен, иначе false.</returns>
		public static bool Progress(this QuestModel questModel, DiContainer container, 
			QuestAimSettings questAimSettings, object[] args, int value = 1)
		{
			if (questAimSettings == null || !questAimSettings.CanProgress(container, args)) return false;

			questModel.Count.Value += value;

			// если есть каунтер, в который неообходимо записывать прогресс квеста, записываем его
			if (string.IsNullOrEmpty(questAimSettings.CounterName) && questAimSettings.CounterName != "-")
			{
				var counterModel = container.Resolve<CounterModel>();
				counterModel.IncrementCounterValue(questAimSettings.CounterName, value, true);
			}

			return true;
		}
	}
}