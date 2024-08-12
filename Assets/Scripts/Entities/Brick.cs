using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Entities
{
	public class Brick : MonoBehaviour
	{
		[SerializeField] private Sprite[] sprites;
		[SerializeField] private Sprite breakSprite;

		public Collider2D col;
		public float _paddingLeft;
		public float _paddingRight;

		public int _size;
		public Vector2Int[] cells;
		private float _maxPosX;

		private float _minPosX;

		private GameGrid gameGrid;
		private Transform graphic;
		private Rigidbody2D rb;

		private Tween scaleTween;
		private SpriteRenderer spriteRenderer;

		public Vector2 Position
		{
			get => transform.position;
			set => transform.position = value;
		}

		private void Awake()
		{
			graphic = transform.Find("Graphic");
			spriteRenderer = graphic.GetComponent<SpriteRenderer>();
			col = graphic.GetComponent<Collider2D>();
			rb = GetComponent<Rigidbody2D>();
		}

		private void Start()
		{
			spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
		}

		public void Initialize(Vector2Int startCell, int size, GameGrid gameGrid)
		{
			this.gameGrid = gameGrid;
			Position = gameGrid.CellToWorldPosition(startCell);
			SetSize(size);
			SetCells(startCell);
			PlaySpawnAnimation();
		}


		public void AlignToCell()
		{
			Vector2Int cell = gameGrid.WorldToCellPosition(Position);
			ChangeCells(cell);
			Position = gameGrid.CellToWorldPosition(cell);
		}

		public void Break()
		{
			foreach (var cell in cells) gameGrid.Grid[cell.x, cell.y] = null;

			StartCoroutine(BreakAnimation());
		}

		private IEnumerator BreakAnimation()
		{
			spriteRenderer.sprite = breakSprite;
			graphic.DOScale(1.4f, 0.2f);
			yield return spriteRenderer.DOFade(0, 0.5f).WaitForCompletion();

			Destroy(gameObject);
		}

		public void SetSize(int _size)
		{
			this._size = _size;

			var newSize = spriteRenderer.size;
			newSize.x = _size * gameGrid.cellSizeX;
			spriteRenderer.size = newSize;

			var localPos = graphic.localPosition;
			localPos.x += (_size - 1) * gameGrid.cellSizeX / 2f;
			graphic.localPosition = localPos;
			_paddingLeft = gameGrid.cellSizeX / 2f;
			_paddingRight = newSize.x - gameGrid.cellSizeX * 0.5f;
		}

		public void SetCells(Vector2Int startCell)
		{
			cells = new Vector2Int[_size];
			for (int i = 0; i < _size; i++)
			{
				cells[i] = new Vector2Int(startCell.x + i, startCell.y);
				gameGrid.Grid[startCell.x + i, startCell.y] = this;
			}
		}

		public void MoveCellY(int newY)
		{
			Vector2Int startCell = cells[0];
			ChangeCells(new Vector2Int(startCell.x, newY));
		}

		public void PlayMoveUpAnimation()
		{
			transform.DOMoveY(gameGrid.CellToWorldPosition(cells[0]).y, gameGrid.actionTime);
		}

		public float PlayFallAnimation()
		{
			float endPos = gameGrid.CellToWorldPosition(cells[0]).y;
			int rowCount = Mathf.Abs(gameGrid.WorldToCellPosition(Position).y - cells[0].y);
			float time = Mathf.Sqrt(2 * rowCount * gameGrid.cellSizeY / -Physics2D.gravity.y);
			transform.DOMoveY(endPos, time * 0.9f)
				.SetEase(Ease.InCubic)
				.OnComplete(() => gameGrid.brickFellCount++);

			return time;
		}

		private void ChangeCells(Vector2Int newStartCell)
		{
			for (int i = 0; i < _size; i++) gameGrid.Grid[cells[i].x, cells[i].y] = null;

			for (int i = 0; i < _size; i++)
			{
				cells[i] = new Vector2Int(newStartCell.x + i, newStartCell.y);
				gameGrid.Grid[newStartCell.x + i, newStartCell.y] = this;
			}
		}


		private void PlaySpawnAnimation()
		{
			var newPos = graphic.localPosition;
			newPos.y = -0.5f;
			graphic.localPosition = newPos;
			graphic.localScale = new Vector3(1, 0.8f, 1);
			spriteRenderer.color = new Color(1, 1, 1, 0);

			DOTween.Sequence()
				.Join(graphic.DOScaleY(1, gameGrid.actionTime))
				.Join(graphic.DOLocalMoveY(0, gameGrid.actionTime))
				.Join(spriteRenderer.DOFade(1, gameGrid.actionTime))
				.OnComplete(() => col.isTrigger = false);
		}


		public bool IsBellowEmpty()
		{
			foreach (var c in cells)
			{
				int bellowY = c.y - 1;
				if (bellowY < 0 || gameGrid.Grid[c.x, bellowY] != null)
					return false;
			}

			return true;
		}
	}
}