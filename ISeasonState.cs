using Model.Quest;
using Persistence.State;
using Persistence.State.Quest;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Model.Season
{
    public interface ISeasonState
    {
        /// <summary>
		/// Название сезона.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Время создания сезона.
		/// </summary>
		DateTimeOffset CreateTime { get; }

		/// <summary>
		/// Было ли показано окно анонса сезона, и перешел ли юзер в окно сезона после этого.
		/// </summary>
		IReadOnlyReactiveProperty<bool> AnnounceShown { get; }

		/// <summary>
		/// Тип подписки на сезон.
		/// </summary>
		IReadOnlyReactiveProperty<SeasonSubscriptionType> SeasonSubsType { get; }

		/// <summary>
		/// Список квестов поля.
		/// </summary>
		IReadOnlyReactiveCollection<IQuestModel> Quests { get; }

		/// <summary>
		/// Квесты, за которые юзер получил бесплатную награду.
		/// </summary>
		IReadOnlyReactiveCollection<string> FreeRewardClaimedQuests { get; }

		/// <summary>
		/// Квесты, за которые юзер получил награду по золотой подписке.
		/// </summary>
		IReadOnlyReactiveCollection<string> GoldRewardClaimedQuests { get; }

		/// <summary>
		/// Квесты, за которые юзер получил награду по платиновой подписке.
		/// </summary>
		IReadOnlyReactiveCollection<string> PlatinumRewardClaimedQuests { get; }
	}
}
