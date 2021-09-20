using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Model.Generated.Addressables
{
	public interface IDownloadableConfigsLibrary
	{
		/// <summary>
		/// �������� ���������� � ������� �� ����� �������.
		/// </summary>
		/// <param name="spriteName">��� �������.</param>
		/// <returns>���������� � �������.</returns>
		public Task<TextAsset> GetConfigByName(string configName);

		public AssetReference GetConfigAssetReferenceByName(string configName);
	}
}