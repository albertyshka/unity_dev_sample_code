using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Loader;
using Model.Stuff;
using NReco.Csv;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions;

namespace Settings.Season
{
	public class CSVResourceSeasonSettingsProvider : ISeasonSettingsProvider, IDisposable
	{
		private bool _isReady;
		private readonly Subject<bool> _isReadyChangesStream = new Subject<bool>();
		private readonly Dictionary<string, SeasonSettings> _settings = new Dictionary<string, SeasonSettings>();

		public void Initialize(params object[] args)
		{
			if (_isReady) return;

			LoadSettings();
			IsReady = true;
		}

		public bool IsReady
		{
			get => _isReady;
			private set
			{
				if (value == _isReady) return;
				_isReady = value;
				Assert.IsTrue(_isReady);
				if (_isReadyChangesStream.HasObservers)
				{
					_isReadyChangesStream.OnNext(_isReady);
					_isReadyChangesStream.OnCompleted();
				}
			}
		}

		public IObservable<bool> IsReadyChangesStream => _isReadyChangesStream;

		public Task<bool> Update()
		{
			throw new NotImplementedException("Reserved.");
		}

		IReadOnlyDictionary<string, SeasonSettings> ISeasonSettingsProvider.Settings => _settings;

		void IDisposable.Dispose()
		{
			_isReadyChangesStream.Dispose();
		}

		private void LoadSettings()
		{
			var csvContentStrings = Resources.Load<TextAsset>("Data\\Seasons\\seasons").text;
			using (var textReader = new StringReader(csvContentStrings))
			{
				var csvReader = new CsvReader(textReader);
				csvReader.Read(); //Skip Header
				while (csvReader.Read())
				{
					var name = string.Empty;
					DateTimeOffset startDate = DateTimeOffset.MinValue;
					DateTimeOffset endDate = DateTimeOffset.MaxValue;
					var catPower = 0;
					var stars = 0;
					var description = string.Empty;
					var goldPresetID = 0;
					var platinumPresetID = 0;
					var upgradePresetID = 0;
					var introImageName = string.Empty;
					var iconName = string.Empty;
					var windowImageName = string.Empty;
					var subsGoldSalePercent = 0;
					var subsPlatinumSalePercent = 0;

					for (var i = 0; i < csvReader.FieldsCount; ++i)
					{
						switch (i)
						{
							case 0: // NAME_INDEX
								name = csvReader[i];
								break;
							case 1: // START_DATE_INDEX
								startDate = DateTimeParser.ParseToDateTimeOffset(csvReader[i]);
								break;
							case 2: // END_DATE_INDEX
								endDate = DateTimeParser.ParseToDateTimeOffset(csvReader[i]);
								break;
							case 3: // CAT_POWER_INDEX
								catPower = int.TryParse(csvReader[i], out var catPowerValue) ? catPowerValue : catPower;
								break;
							case 4: // STARS_INDEX
								stars = int.TryParse(csvReader[i], out var starsValue) ? starsValue : stars;
								break;
							case 5: // DESCRIPTION_INDEX
								description = csvReader[i];
								break;
							case 6: // SUBSCIPTION_GOLD_PRESET_INDEX
								goldPresetID = int.TryParse(csvReader[i], out var goldPresetIDValue) ? 
									goldPresetIDValue : goldPresetID;
								break;
							case 7: // SUBSCIPTION_PLATINUM_PRESET_INDEX
								platinumPresetID = int.TryParse(csvReader[i], out var platinumPresetIDValue) ? 
									platinumPresetIDValue : platinumPresetID;
								break;
							case 8: // SUBSCIPTION_UPGRADE_PRESET_INDEX
								upgradePresetID = int.TryParse(csvReader[i], out var upgradePresetIDValue) ? 
									upgradePresetIDValue : upgradePresetID;
								break;
							case 9: // INTRO_IMAGE_INDEX
								introImageName = csvReader[i];
								break;
							case 10: // ICON_INDEX
								iconName = csvReader[i];
								break;
							case 11: // WINDOW_INDEX
								windowImageName = csvReader[i];
								break;
							case 16: // SUBS_GOLD_SALE_PERCENT_INDEX
								subsGoldSalePercent = int.TryParse(csvReader[i], out var subsGoldSalePercentValue) ? 
									subsGoldSalePercentValue : subsGoldSalePercent;
								break;
							case 17: // SUBS_PLATINUM_SALE_PERCENT_INDEX
								subsPlatinumSalePercent = int.TryParse(csvReader[i], out var subsPlatinumSalePercentValue) ? 
									subsPlatinumSalePercentValue : subsPlatinumSalePercent;
								break;
						}
					}

					if (_settings.ContainsKey(name))
					{
						Debug.LogErrorFormat("Season with the name {0} already exists.", name);
						continue;
					}

					var record = new SeasonSettings(name, startDate, endDate, catPower, stars, description,
						goldPresetID, platinumPresetID, upgradePresetID, introImageName, iconName, windowImageName,
						subsGoldSalePercent, subsPlatinumSalePercent);
					_settings.Add(name, record);
				}
			}
		}
	}
}