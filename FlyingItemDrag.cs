using Common;
using System.Collections;
using System.Collections.Generic;
using Timers;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class FlyingItemDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	[SerializeField] private Transform _itemTransform;

	private readonly float DESTINATION_SCATTER = 100.0f;
	private readonly float WIGGLE_DELAY_SECONDS = 4.0f;
	private readonly float WIGGLE_DURATION = 12.0f;
	private readonly Vector3 WIGGLE_ROTATION_DEGREES = new Vector3(0.0f, 0.0f, 25.0f);

	private int _speed;
	private bool _wiggle;
	private bool isDragging;
	private Camera _mainCamera;
	private Vector3 _destination;
	private Tweener _moveTweener;
	private Sequence _wiggleSequence;

	private void Awake()
	{
		_mainCamera = MainCameraCache.MainCamera;
	}

	public void Init(Vector3 destination, RewardOptions options)
	{
		_speed = Random.Range(options.AutoAddData.MinSpeed, options.AutoAddData.MaxSpeed);
		_wiggle = options.AutoAddData.Wiggle;
		_destination += destination + new Vector3(Random.value * DESTINATION_SCATTER, -Random.value * DESTINATION_SCATTER, 1);
	}

	public void StartMove()
	{
		_moveTweener?.Kill();
		_wiggleSequence?.Kill();

		// запускаем анимацию перемещения
		var duration = (transform.localPosition - _destination).magnitude / _speed;

		var initialRotation = transform.rotation.eulerAngles;
		var rotationDelay = Random.value * WIGGLE_DELAY_SECONDS;

		if (_wiggle)
		{
			_wiggleSequence = DOTween.Sequence()
				.SetId(this)
				.AppendInterval(rotationDelay)
				.Append(_itemTransform.DORotate(initialRotation + WIGGLE_ROTATION_DEGREES, WIGGLE_DURATION).SetEase(Ease.Linear))
				.AppendInterval(rotationDelay)
				.SetLoops(-1, LoopType.Yoyo);
		}

		_moveTweener = transform.DOLocalMove(_destination, duration).OnComplete(() =>
		{
			_wiggleSequence?.Kill();
			Destroy(gameObject);
		});
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		isDragging = true;

		_moveTweener?.Kill();
		_wiggleSequence?.Kill();

		World.tileGrid.SelectFlyingItem(transform);
	}

	public void OnDrag(PointerEventData eventData)
	{
		Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, World.autoAdd.distance);
		Vector3 objPosition = _mainCamera.ScreenToWorldPoint(mousePosition);

		transform.position = objPosition;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		isDragging = false;

		StartMove();

		World.tileGrid.DeselectFlyingItem();
	}

	public bool IsDragging()
	{
		return isDragging;
	}
}
