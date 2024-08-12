using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Entities
{
	public class GameGrid : MonoBehaviour
	{
		public static GameGrid Instance;
		[SerializeField] private Text canDrag;

		public int _numbOfColumn = 4;
		public int _numbOfRow = 3;

		public float cellSizeX;
		public float cellSizeY;

		[SerializeField] private GameObject brickPrefab;
		public float actionTime;

		public bool _canDrag;
		public int brickFellCount;

		[SerializeField] private bool _drawGizmos = true;

		private readonly List<Brick> _fallBricks = new List<Brick>();

		private readonly Vector2Int[] directions =
		{
			new Vector2Int(0, 1),
			new Vector2Int(0, -1),
			new Vector2Int(-1, 0),
			new Vector2Int(1, 0),
			new Vector2Int(-1, 1),
			new Vector2Int(-1, -1),
			new Vector2Int(1, 1),
			new Vector2Int(1, -1)
		};

		private bool _isLost;

		public Brick[,] Grid;
		private float maxAdjacentLength;

		private void Awake()
		{
			Instance = this;
			Grid = new Brick[_numbOfColumn, _numbOfRow];
		}

		private void Start()
		{
			StartCoroutine(AddStartRow(2));
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.A))
				StartCoroutine(AddNewRow());
		}

		private void OnDrawGizmos()
		{
			if (!Application.isPlaying || !_drawGizmos) return;

			for (var i = 0; i < _numbOfColumn; i++)
			for (var j = 0; j < _numbOfRow; j++)
			{
				Gizmos.color = Grid[i, j] switch
				{
					null => Color.white,
					_ => Color.blue
				};

				Gizmos.DrawSphere(CellToWorldPosition(new Vector2Int(i, j)), 0.1f);
			}
		}

		private IEnumerator AddStartRow(int numbOfRows)
		{
			for (int i = 0; i < numbOfRows; i++) yield return StartCoroutine(AddNewRow());

			_canDrag = true;
		}

		private IEnumerator AddNewRow()
		{
			yield return PushRowsUpOne();

			if (_isLost)
			{
				yield return new WaitForSeconds(actionTime * 2);
				GameManager.Instance.GameOver();
			}
			else
			{
				yield return SpawnNewRowAtBottom();
				yield return StartCoroutine(FallBrickRoutine());
			}
		}

		private YieldInstruction PushRowsUpOne()
		{
			for (int y = _numbOfRow - 1; y >= 0; y--)
			for (int x = 0; x < _numbOfColumn; x++)
			{
				var brick = Grid[x, y];
				if (brick == null)
					continue;

				if (y + 1 > _numbOfRow - 1)
				{
					_isLost = true;
					return null;
				}

				brick.MoveCellY(y + 1);
				brick.PlayMoveUpAnimation();
			}

			return new WaitForSeconds(actionTime);
		}

		private YieldInstruction SpawnNewRowAtBottom()
		{
			bool anySpawned = false;
			for (int i = 0; i < _numbOfColumn; i++)
			{
				if (ShouldNotSpawn(i))
					continue;

				int cellLeft = _numbOfColumn - i;
				int maxSize = cellLeft < BrickSizePool.Instance._maxSize ? cellLeft : BrickSizePool.Instance._maxSize;
				int sizeUse = Random.Range(1, maxSize + 1);
				SpawnBrick(new Vector2Int(i, 0), sizeUse);
				anySpawned = true;
			}

			return new WaitForSeconds(actionTime);

			bool ShouldNotSpawn(int i)
			{
				if (!anySpawned && i > _numbOfColumn * 0.7f)
					return false;
				return Random.value < 0.6f || Grid[i, 0] != null;
			}
		}

		private void SpawnBrick(Vector2Int startCell, int size)
		{
			Brick brick = Instantiate(brickPrefab).GetComponent<Brick>();
			brick.Initialize(startCell, size, this);
		}

		private IEnumerator FallBrickRoutine()
		{
			_fallBricks.Clear();
			for (int i = 0; i < _numbOfRow; i++)
			for (int y = 0; y < _numbOfRow; y++)
			{
				List<Brick> checkedRowBricks = new List<Brick>();
				for (int x = 0; x < _numbOfColumn; x++) CheckBrickFall(new Vector2Int(x, y), checkedRowBricks);

				checkedRowBricks.Clear();
			}

			if (_fallBricks.Count > 0)
			{
				yield return StartCoroutine(PlayBrickFallAnimation());
			}

			yield return StartCoroutine(CheckFullRowsRoutine());
		}

		private IEnumerator PlayBrickFallAnimation()
		{
			float maxWaitTime = 0;

			foreach (var b in _fallBricks)
			{
				float fallTime = b.PlayFallAnimation();
				if (maxWaitTime < fallTime)
					maxWaitTime = fallTime;

				yield return null;
			}

			brickFellCount = 0;
			float shouldEndTime = Time.time + maxWaitTime + 0.02f;

			while (brickFellCount < _fallBricks.Count && Time.time < shouldEndTime)
			{
				yield return null;
			}
		}

		private void CheckBrickFall(Vector2Int cell, List<Brick> checkedRowBricks)
		{
			if (Grid[cell.x, cell.y] == null) return;
			Brick b = Grid[cell.x, cell.y];
			if (checkedRowBricks.Contains(b)) return;

			if (b.IsBellowEmpty())
			{
				b.MoveCellY(cell.y - 1);
				_fallBricks.Add(b);
			}

			checkedRowBricks.Add(b);
		}

		private IEnumerator CheckFullRowsRoutine()
		{
			bool fallBrickAtComplete = false;
			for (int y = 0; y < _numbOfRow; y++)
			{
				bool isFull = true;
				for (int x = 0; x < _numbOfColumn; x++)
				{
					if (Grid[x, y] != null) continue;
					isFull = false;
					break;
				}

				if (!isFull) continue;

				yield return new WaitForSeconds(actionTime);
				GameManager.Instance.GainScore(10);
				for (int x = 0; x < _numbOfColumn; x++)
				{
					Brick brick = Grid[x, y];
					x += brick._size - 1;
					brick.Break();
					fallBrickAtComplete = true;
				}
			}

			if (fallBrickAtComplete)
			{
				yield return StartCoroutine(FallBrickRoutine());
			}
			else
			{
				yield return StartCoroutine(CheckShouldAddMoreRow());
				yield return null;
			}
		}

		private IEnumerator CheckShouldAddMoreRow()
		{
			bool rowEmpty = true;
			for (int x = 0; x < _numbOfColumn; x++)
			{
				if (Grid[x, 1] == null) continue;
				rowEmpty = false;
				break;
			}

			if (rowEmpty)
				yield return StartCoroutine(AddNewRow());
		}

		public void EndDrag()
		{
			_canDrag = false;
			StartCoroutine(FallThenAddNewRow());
		}

		private IEnumerator FallThenAddNewRow()
		{
			yield return StartCoroutine(FallBrickRoutine());
			yield return StartCoroutine(AddNewRow());
			_canDrag = true;
		}

		public bool IsOutsideBound(int x, int y)
		{
			return x < 0 || x >= _numbOfColumn || y < 0 || y >= _numbOfRow;
		}

		public bool IsOutsideBound(Vector2Int cell)
		{
			return IsOutsideBound(cell.x, cell.y);
		}


		private Vector3 GetClampedMousePosition(List<Vector3> convertedPos)
		{
			Vector3 mousePosition = CameraController.GetMousePosition();

			Vector3 direction = mousePosition - convertedPos.Last();
			Vector3 clampedPosition = convertedPos.Last() + Vector3.ClampMagnitude(direction, maxAdjacentLength);
			return clampedPosition;
		}

		private bool AreCellsAdjacent(Vector2Int cellA, Vector2Int cellB)
		{
			int dx = Mathf.Abs(cellA.x - cellB.x);
			int dy = Mathf.Abs(cellA.y - cellB.y);

			return dx <= 1 && dy <= 1 && dx + dy > 0;
		}

		public Vector2 CellToWorldPosition(Vector2Int cellPos)
		{
			var position = new Vector2(cellPos.x * cellSizeX, cellPos.y * cellSizeY);
			position += (Vector2)transform.position + 0.5f * new Vector2(cellSizeX, cellSizeY);
			return position;
		}

		public Vector2Int WorldToCellPosition(Vector2 worldPos)
		{
			Vector2 relativePosition = worldPos - (Vector2)transform.position;
			int cellX = Mathf.FloorToInt(relativePosition.x / cellSizeX);
			int cellY = Mathf.FloorToInt(relativePosition.y / cellSizeY);
			return new Vector2Int(cellX, cellY);
		}
	}
}