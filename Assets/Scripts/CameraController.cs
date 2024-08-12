using UnityEngine;

public class CameraController : MonoBehaviour
{
	public static CameraController Instance;
	public static Camera Cam;

	[SerializeField] private float rotationMultiplier = 7.5f;
	[SerializeField] private Vector2 followOffset;

	[SerializeField] private float camSmoothing = 0.2f;
	private Vector3 currentVelocity = Vector3.zero;

	private Vector3 originPos;
	private float shakeFadeTime;
	private float shakePower;
	private float shakeRotation;

	private float shakeTimeRemaining;
	private Transform target;

	public Vector2 BottomLeft => Cam.ViewportToWorldPoint(Vector2.zero);
	public Vector2 TopRight => Cam.ViewportToWorldPoint(Vector2.one);

	private void Awake()
	{
		Cam = GetComponent<Camera>();

		Instance = this;
	}

	private void Start()
	{
		originPos = transform.position;
	}

	private void Update()
	{
		if (!(target is null))
		{
			originPos.x = target.position.x;
			originPos.y = target.position.y;
			originPos += (Vector3)followOffset;
		}

		transform.position =
			Vector3.SmoothDamp(transform.position, originPos, ref currentVelocity, camSmoothing);
	}

	private void LateUpdate()
	{
		ShakeHandle();
	}

	private void ShakeHandle()
	{
		if (shakeTimeRemaining > 0)
		{
			shakeTimeRemaining -= Time.deltaTime;

			float xAmount = Random.Range(-1f, 1f) * shakePower;
			float yAmount = Random.Range(-1f, 1f) * shakePower;

			transform.position += new Vector3(xAmount, yAmount, 0f);

			shakePower = Mathf.MoveTowards(shakePower, 0f, shakeFadeTime * Time.deltaTime);
			shakeRotation = Mathf.MoveTowards(shakeRotation, 0f, shakeFadeTime * rotationMultiplier * Time.deltaTime);
		}

		transform.rotation = Quaternion.Euler(0f, 0f, shakeRotation * Random.Range(-1f, 1f));
	}

	public void StartShake(float length, float shakePower)
	{
		if (shakePower > this.shakePower)
		{
			shakeTimeRemaining = length;
			this.shakePower = shakePower;
			shakeFadeTime = shakePower / length;
			shakeRotation = shakePower * rotationMultiplier;
		}
	}

	public void StartFollow(Transform target)
	{
		this.target = target;
	}

	public void StopFollow()
	{
		target = null;
	}

	public static Vector2 GetMousePosition()
	{
		return Cam.ScreenToWorldPoint(Input.mousePosition);
	}

	public static bool MouseRaycast(out RaycastHit2D hit)
	{
		Vector2 mousePosition = Input.mousePosition;

		Ray ray = Cam.ScreenPointToRay(mousePosition);
		hit = Physics2D.Raycast(ray.origin, ray.direction);

		return hit;
	}
}