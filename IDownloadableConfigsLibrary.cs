using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Model.Generated.Addressables
{
	public interface IDownloadableConfigsLibrary
	{
		/// <summary>
		/// Получить информацию о конфиге по имени конфига.
		/// </summary>
		/// <param name="spriteName">Имя конфига.</param>
		/// <returns>Информация о конфиге.</returns>
		public Task<TextAsset> GetConfigByName(string configName);

		public AssetReference GetConfigAssetReferenceByName(string configName);
	}
}