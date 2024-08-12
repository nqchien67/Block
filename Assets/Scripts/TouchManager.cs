using Entities;
using UnityEngine;

public class TouchManager : MonoBehaviour
{
	[SerializeField] private LayerMask brickLayerMask;
	private Vector2 _leftClampPos;

	private Vector2 _offset;
	private Vector2 _rightClampPos;
	private Brick draggingBrick;
	private bool isDragging;

	private Vector2Int startMoveCell;

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) && GameGrid.Instance._canDrag)
			OnMouseClick();
		else if (isDragging && Input.GetMouseButtonUp(0)) OnMouseRelease();

		if (isDragging) OnMouseDragging();
	}

	private void OnMouseClick()
	{
		var mousePosition = CameraController.GetMousePosition();
		Collider2D hit = Physics2D.OverlapPoint(mousePosition, brickLayerMask);
		if (!hit) return;
		isDragging = true;
		draggingBrick = hit.GetComponentInParent<Brick>();

		draggingBrick.col.enabled = false;
		SetClampPositions();
		_offset = draggingBrick.Position - mousePosition;
		startMoveCell = draggingBrick.cells[0];
	}

	private void OnMouseRelease()
	{
		isDragging = false;
		draggingBrick.AlignToCell();
		draggingBrick.col.enabled = true;
		if (startMoveCell != draggingBrick.cells[0])
			GameGrid.Instance.EndDrag();

		draggingBrick = null;
	}

	private void OnMouseDragging()
	{
		Vector2 mousePos = CameraController.GetMousePosition() + _offset;
		Vector2 position = draggingBrick.Position;
		position.x = GetClampPosX(mousePos.x);

		draggingBrick.Position = position;
		Debug.DrawLine(draggingBrick.Position, _leftClampPos, Color.green);
		Debug.DrawLine(draggingBrick.Position, _rightClampPos, Color.blue);
	}

	public void SetClampPositions()
	{
		RaycastHit2D hitLeft = Physics2D.Raycast(draggingBrick.Position, Vector2.left, 10);
		RaycastHit2D hitRight = Physics2D.Raycast(draggingBrick.Position, Vector2.right, 10);
		_leftClampPos = hitLeft.point;
		_rightClampPos = hitRight.point;
	}

	private float GetClampPosX(float x)
	{
		return Mathf.Clamp(x, _leftClampPos.x + draggingBrick._paddingLeft,
			_rightClampPos.x - draggingBrick._paddingRight);
	}
}