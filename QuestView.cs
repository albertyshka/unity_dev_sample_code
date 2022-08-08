using DG.Tweening;
using Loader;
using Loader.Progress;
using Model.Generated;
using Model.Generated.Addressables;
using Model.Quest;
using Settings.Quest;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Persistence.State.Quest;
using System;
using Translates;
using Base.WindowManager;
using UICanvas.Quests;
using UICanvas.WindowManager;

namespace Quests.View
{
	/// <summary>
	/// Класс отвечает за отображение (вью) модели квеста.
	/// </summary>
    public class QuestView : MonoBehaviour
    {
		protected readonly CompositeDisposable _disposables = new CompositeDisposable();
		private IDisposable _flagHandler;
		private bool _isFirstTime = true;

#pragma warning disable 649
		[Inject] private readonly IQuestModel _questModel;
		[Inject] private readonly IQuestSettingsProvider _questSettingsProvider;
		[Inject] private readonly IItemViewLibrary _itemViewLibrary;
		[Inject] private readonly IDownloadableItemsLibrary _downloadableItemsLibrary;
		[Inject] private readonly IProgressManager _progressManager;
		[Inject] private readonly ITranslateManager _translateManager;
		[Inject] private readonly IWindowManagerExt _windowManager;
		[Inject] private readonly DiContainer _container;
#pragma warning restore 649

		[Header("Image")]
		[SerializeField] protected Image _image;
		[SerializeField] protected GameObject _spinner;

		[Header("Text")]
		[SerializeField] private TextMeshProUGUI _countText;
		[SerializeField] private TextMeshProUGUI _targetCountText;
		[SerializeField] private Color _countTextIncompleteColor;
		[SerializeField] private Color _targetCountTextDefaultColor;
		[SerializeField] private Color _textCompleteColor;

		[Header("Flag")]
		[SerializeField] private AutoFlip _flag;

		private void Start()
		{
			UpdateImage();
			SubscribeToModelUpdates();
		}

		public void OpenInfo()
		{
			if (_windowManager.GetWindow(QuestInfoWindow.Id) == null)
			{
				_windowManager.ShowWindow(_container, QuestInfoWindow.Id, 
					new object[] { _questModel, _questSettingsProvider, transform.position });
				// TODO: доделать прогресс квеста
			}
		}

		/// <summary>
		/// Функция подписывает вью на обновление модели.
		/// </summary>
		private void SubscribeToModelUpdates()
		{
			var aimSettings = _questModel.GetAimSettings(_questSettingsProvider);
			_targetCountText.text = $"/{aimSettings.RequiredCount}";

			// обновлять прогресс квеста
			_questModel.Count
				.Subscribe(count => _countText.text = count.ToString())
				.AddTo(_disposables);

			// показывать флажок при обновлении, обновлять цвет текста и текст флажка
			_questModel.Status.Subscribe(status =>
			{
				switch (status)
				{
					case QuestStatus.INACTIVE:
						break;
					case QuestStatus.ACTIVE:

						// показать и скрыть флаг.
						var count = _questModel.Count.Value;
						var maxCount = aimSettings.RequiredCount;
						ShowFlag($"{Math.Min(count, maxCount)}/{maxCount}");

						// задать цвет текста.
						_countText.color = _countTextIncompleteColor;
						_targetCountText.color = _targetCountTextDefaultColor;
						break;
					case QuestStatus.FINISHED:

						// показать флаг и оставить его.
						_flag.ControledBook.SetRightPageText(_translateManager.Get("ms_quest_complete"));
						SetFlagActive(true);

						// задать цвет текста.
						_countText.color = _textCompleteColor;
						_targetCountText.color = _textCompleteColor;
						break;
				}
			})
			.AddTo(_disposables);
		}

		/// <summary>
		/// Функция обновляет картинку квеста.
		/// </summary>
		private async void UpdateImage()
		{
			_spinner.gameObject.SetActive(true);
			_image.gameObject.SetActive(false);

			var spriteName = _questModel.GetSettings(_questSettingsProvider).ImageName;
			var spriteHolder = string.IsNullOrEmpty(spriteName) || spriteName == "-"
							? null
							: new SpriteHolder(
								_itemViewLibrary.GetSpriteByName(spriteName),
								_downloadableItemsLibrary.GetSpriteByName(spriteName),
								_progressManager);

			_image.sprite = await spriteHolder.GetSprite();

			_spinner.gameObject.SetActive(false);
			_image.gameObject.SetActive(true);
		}

		/// <summary>
		/// Функция устанавливает на флаг текст и показывает его.
		/// Затем, через некоторое время скрывает его.
		/// </summary>
		/// <param name="text">Текст, который нужно установить на флаг.</param>
		private void ShowFlag(string text)
		{
			_flag.ControledBook.SetRightPageText(text);

			if (_flagHandler != null)
			{
				_disposables.Remove(_flagHandler);
			}

			if (_isFirstTime)
			{
				SetFlagActive(true);
				_isFirstTime = false;
			}

			SetFlagActive(true);

			_flagHandler = Observable.Timer(TimeSpan.FromSeconds(2), Scheduler.MainThread)
				.Subscribe(l =>
				{
					_disposables.Remove(_flagHandler);
					_flagHandler = null;
					SetFlagActive(false);

				}).AddTo(_disposables);
		}

		/// <summary>
		/// Функция показывает флаг или скрывает его.
		/// </summary>
		/// <param name="active">Показать или скрыть флаг.</param>
		private void SetFlagActive(bool active)
		{
			if (active)
			{
				_flag.FlipLeftPage();
			}
			else
			{
				_flag.FlipRightPage();
			}
		}

		private void OnDestroy()
		{
			_disposables.Dispose();
		}
	}
}