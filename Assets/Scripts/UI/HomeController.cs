using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
	public class HomeController : MonoBehaviour
	{
		[SerializeField] private Transform guideScreen;
		[SerializeField] private float guideDisplayTime = 0.5f;

		private Transform board;
		private float endAlpha;
		private Image guideScreenImg;
		private Transform homeBtn;

		private Transform logo;

		private void Awake()
		{
			logo = transform.Find("Logo");
			homeBtn = guideScreen.Find("HomeBtn");
			board = guideScreen.Find("Board");

			if (guideScreen != null)
			{
				guideScreenImg = guideScreen.GetComponent<Image>();
				endAlpha = guideScreenImg.color.a;
				HideGuide();
			}
		}

		private void Start()
		{
			Vector2 middleTopScreen = Camera.main.ViewportToWorldPoint(Vector2.up);
			Vector3 endPos = logo.position;
			logo.position = new Vector2(endPos.x, endPos.y + middleTopScreen.y + 3);

			logo.DOMove(endPos, 0.2f).OnComplete(() =>
			{
				logo.DOScale(new Vector3(1.1f, 0.85f, 1), 0.08f).SetLoops(2, LoopType.Yoyo)
					.SetEase(Ease.OutCubic);
				logo.DOMove(endPos + 0.3f * Vector3.down, 0.08f).OnComplete(() =>
				{
					logo.DOMove(endPos, 0.15f).SetEase(Ease.OutBack);
				});
			});
		}

		private void OnDisable()
		{
			logo.DOKill();
		}

		public void Play()
		{
			SceneManager.LoadScene("Game");
		}

		public void ShowGuide()
		{
			guideScreen.gameObject.SetActive(true);
			StartCoroutine(Display());
		}

		public void HideGuide()
		{
			guideScreenImg.color = new Color(guideScreenImg.color.r, guideScreenImg.color.g, guideScreenImg.color.b, 0);
			board.localScale = new Vector3(0, 0, 1);
			homeBtn.localScale = new Vector3(0, 0, 1);
			guideScreen.gameObject.SetActive(false);
		}

		private IEnumerator Display()
		{
			float elapsedTime = 0;
			while (elapsedTime < guideDisplayTime / 2)
			{
				float alpha = Mathf.Lerp(0, endAlpha, elapsedTime / (guideDisplayTime / 2));
				guideScreenImg.color = new Color(guideScreenImg.color.r, guideScreenImg.color.g, guideScreenImg.color.b,
					alpha);

				elapsedTime += Time.deltaTime;
				yield return null;
			}

			elapsedTime = 0;
			while (elapsedTime < guideDisplayTime / 2)
			{
				float scale = Mathf.Lerp(0, 1, elapsedTime / (guideDisplayTime / 2));
				board.localScale = new Vector3(scale, scale, 1);
				homeBtn.localScale = new Vector3(scale, scale, 1);

				elapsedTime += Time.deltaTime;
				yield return null;
			}
		}
	}
}