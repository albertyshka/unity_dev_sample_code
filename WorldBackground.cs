using System.Collections;
using UnityEngine;
using DG.Tweening;
using Zenject;
using Common;
using UniRx;
using System;
using Random = UnityEngine.Random;
using System.Collections.Generic;

public class WorldBackground : MonoBehaviour
{
#pragma warning disable 649
    [Inject] private readonly IPerformanceManager _performanceManager;
#pragma warning restore 649

    public Sprite[] cloudSprites;

    private readonly Vector3 OFFSET_MOVE_DIRECTION = new Vector3(-1f, -0.5f, 0f);
    private const float OFFSET_MAGNITUDE = 1000f;
    private readonly Vector2 VELOCITY = new Vector2(30f, 90f);
    private readonly int AMOUNT = 20;
    private readonly Vector2 DELAY_BETWEEN_SPAWN = new Vector2(4f, 10f);
    private readonly Vector2 SCALE = new Vector2(100, 400);

    private IDisposable _performanceHandler;
    private List<GameObject> _instantiatedClouds = new List<GameObject>();

    private Coroutine _cloudGenerator = null;

    void Start()
    {
        transform.localPosition = World.tileGrid.transform.localPosition;

		_performanceHandler = _performanceManager.PerformanceState.Subscribe(performance =>
		{
			switch (performance)
			{
				case Performance.High:
                    if (cloudSprites == null || cloudSprites.Length == 0)
                    {
                        Debug.LogWarning("You didn't provide sprites for background effect");
                    }
                    else
                    {
                        _cloudGenerator = StartCoroutine(CloudGenerator());
                    }
                    break;
				case Performance.Lo:
                    DOTween.Kill(this);

                    if (_cloudGenerator != null)
                    {
                        StopCoroutine(_cloudGenerator);
                    }

                    // если на уровне уже были какие-то облака, медленно уводим их в фейд и удаляем
                    if (_instantiatedClouds.Count != 0)
					{
						foreach (var gO in _instantiatedClouds)
						{
                            var spR = gO.GetComponent<SpriteRenderer>();

                            DOTween.Sequence()
                                .SetId(this)
                                .Append(spR.DOFade(0, 2.0f))
                                .OnComplete(() => Destroy(gO));
						}
                        _instantiatedClouds.Clear();
                    }
                    break;
				default:
					throw new NotSupportedException();
			}
		});
	}

    private GameObject InstantiateCloudPrefab(Vector3 position, Sprite cloudSprite, int index)
	{
        var cloud = new GameObject(cloudSprite.name);
        cloud.transform.parent = transform;
        cloud.transform.localPosition = position;

        var scale = Random.Range(SCALE.x, SCALE.y);
        cloud.transform.localScale = new Vector3(scale, scale, scale);

        var spR = cloud.AddComponent<SpriteRenderer>();
        spR.sprite = cloudSprite;
        spR.sortingOrder = index;

        var color = spR.color;
        color.a = 0;
        spR.color = color;

        return cloud;
    }

    private Vector3[] GeneratePoints(Vector3[] corners, Vector3 moveDirection, float offsetMagnitude)
	{
        Vector3[] result = new Vector3[4]; // start + offset, start, end, end + offset

        var rightBorder = corners[2][0];
        var bottomBorder = corners[2][1];

        var startPoint = corners[1]; // правый верхний угол
        startPoint[1] = Random.Range(0f, bottomBorder); // расположить рандомно по высоте грида
        Vector3 endPoint = startPoint + Mathf.Max(Mathf.Abs(rightBorder), Mathf.Abs(bottomBorder)) * moveDirection;

        result[0] = startPoint + (moveDirection * -offsetMagnitude);
        result[1] = startPoint;
        result[2] = endPoint;
        result[3] = endPoint + (moveDirection * offsetMagnitude);

        return result;
    }

    private void StartMoving(GameObject cloud, Vector3[] movePoints, float velocity, Vector3 offsetMoveDirection, float offsetMagnitude)
	{
        var offsetMoveDuration = (offsetMoveDirection * -offsetMagnitude).magnitude / velocity;
        var spR = cloud.GetComponent<SpriteRenderer>();

        DOTween.Sequence()
            .SetId(this)
            .Append(cloud.transform.DOLocalMove(movePoints[1], offsetMoveDuration, true).SetEase(Ease.Linear))
            .Join(spR.DOFade(1, offsetMoveDuration))
            .Append(cloud.transform.DOLocalMove(movePoints[2], (movePoints[2] - movePoints[1]).magnitude / velocity, true).SetEase(Ease.Linear))
            .Append(cloud.transform.DOLocalMove(movePoints[3], offsetMoveDuration, true).SetEase(Ease.Linear))
            .Join(spR.DOFade(0, offsetMoveDuration))
            .SetLoops(-1);
    }

    IEnumerator CloudGenerator()
	{
        for (int i = 0; i < AMOUNT; i++)
        {
            var corners = World.tileGrid.GetGridCorners();
            var movePoints = GeneratePoints(corners, OFFSET_MOVE_DIRECTION, OFFSET_MAGNITUDE);
            var velocity = Random.Range(VELOCITY.x, VELOCITY.y);
            var delayBetween = Random.Range(DELAY_BETWEEN_SPAWN.x, DELAY_BETWEEN_SPAWN.y); 

            var cloud = InstantiateCloudPrefab(movePoints[0], cloudSprites[Random.Range(0, cloudSprites.Length - 1)], i);
            _instantiatedClouds.Add(cloud);
            StartMoving(cloud, movePoints, velocity, OFFSET_MOVE_DIRECTION, OFFSET_MAGNITUDE);

            yield return new WaitForSeconds(delayBetween);
            //Debug.Log($"corners {corners[0]} {corners[1]} {corners[2]} {corners[3]} \n movePoints {movePoints[0]} {movePoints[1]} {movePoints[2]} {movePoints[3]}");
        }
	}

	private void OnDestroy()
	{
        _performanceHandler.Dispose();
        DOTween.Kill(this);

        if (_cloudGenerator != null)
		{
            StopCoroutine(_cloudGenerator);
		}
    }

}
