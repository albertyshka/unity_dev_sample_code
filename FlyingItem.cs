using System.Collections.Generic;
using Firebase.Extensions;
using Loader;
using Model;
using Model.Stuff;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(FlyingItemDrag))]
public class FlyingItem : MonoBehaviour, IPointerClickHandler, IReserving
{
	[SerializeField] private SpriteRenderer _itemSprite;
	[SerializeField] private SpriteRenderer _shadowSprite;

	private FlyingItemDrag _flyingItemDrag = null;
	private string _spawnItemName = null;

	public void Init(ItemData itemData, int sortingOrder)
	{
		itemData.Image.GetSprite().ContinueWithOnMainThread(task => _itemSprite.sprite = task.Result);
		_itemSprite.sortingOrder = sortingOrder;
		_shadowSprite.sortingOrder = sortingOrder;

		// из тап реварда берем первый item (он там должен быть единственный)
		var dropRewardResult = WorldRewards.CompileDropReward(itemData.DropReward.Get(), null, null);
		_spawnItemName = ((StuffItem)dropRewardResult.Stuff[0])?.Name;

		_flyingItemDrag = GetComponent<FlyingItemDrag>();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!_flyingItemDrag.IsDragging())
		{
			SpawnItem();
		}
	}

	private void SpawnItem()
	{
		var nearestEmptyCells = new List<Field>();
		var point = World.tileGrid.GetCurrentFieldCoords(transform);

		var itemData = ItemLoader.Instance.Get(_spawnItemName);

		if (itemData == null)
		{
			Destroy(gameObject);
			return;
		}

		World.tileGrid.CollectNearestEmptyCells(nearestEmptyCells, point[0], point[1], 1, 100, itemData);

		if (nearestEmptyCells.Count == 0)
		{
			World.uiManager.ShowAlert("no_room");
			return;
		}

		var cell = nearestEmptyCells[0];
		var instance = Instantiate(itemData.GetPrefab(), transform.parent);
		var itemComponent = instance.GetComponent<Item>();
		instance.SetActive(false);

		itemComponent.Init(itemData);
		instance.transform.position = transform.position;
		cell.ReserveField(itemComponent, this);

		itemComponent.CanInteract = false;
		itemComponent.DropOn(cell).ContinueWithOnMainThread(task => itemComponent.CanInteract = true);
		Destroy(gameObject);
	}

	public void OnCancelReserve()
	{

	}
}
