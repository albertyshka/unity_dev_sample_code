#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
using Zenject;
using System.Threading.Tasks;

namespace Model.Generated.Addressables
{
	[CreateAssetMenu(fileName = "DownloadableConfigsLibrary", menuName = "Generated/Downloadable Configs Library")]
	public class DownloadableConfigsLibrary : ScriptableObjectInstaller<DownloadableConfigsLibrary>,
		IDownloadableConfigsLibrary
	{
#pragma warning disable 649
		[SerializeField]
		private List<DownloadableConfigInfoRecord> _configsMap = new List<DownloadableConfigInfoRecord>();
#pragma warning restore 649

		public override void InstallBindings()
		{
			Container.Bind(typeof(IDownloadableConfigsLibrary)).FromInstance(this).AsSingle();
		}

		public async Task<TextAsset> GetConfigByName(string configName)
		{
			AssetReference assetReference = _configsMap
				.Where(x => x.ConfigName == configName)
				.Select(x => x.ConfigReference)
				.First();

			TextAsset result = new TextAsset("-");

			if (assetReference != null)
			{
				if (assetReference.OperationHandle.IsValid())
				{
					if (assetReference.OperationHandle.IsDone)
					{
						result = assetReference.Asset as TextAsset;
					}
					else
					{
						result = await assetReference.OperationHandle.Task as TextAsset;
						if (!result)
						{
							Debug.LogErrorFormat("Can't load Atlas from Addressables bundle: {0}.", configName);
						}
					}
				}
				else
				{
					result = await assetReference.LoadAssetAsync<TextAsset>().Task;
					if (!result)
					{
						Debug.LogErrorFormat("Can't load Atlas from Addressables bundle: {0}.", configName);
					}
				}
			}

			return result;
		}

		public AssetReference GetConfigAssetReferenceByName(string configName)
		{
			return _configsMap
				.Where(x => x.ConfigName == configName)
				.Select(x => x.ConfigReference)
				.First();
		}
	}

	[Serializable]
	public class DownloadableConfigInfoRecord
	{
#pragma warning disable 649
		[SerializeField] private string _configName;
		[SerializeField] private AssetReference _configAssetReference;
#pragma warning restore 649

		public string ConfigName => _configName;
		public AssetReference ConfigReference => _configAssetReference;
	}
}