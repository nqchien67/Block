using System.Collections;
using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
	public class GameOverBoard : MonoBehaviour
	{
		[SerializeField] private float displayTime = 1;
		private Transform board;
		private float endAlpha;
		private Transform homeBtn;

		private Image image;
		private Transform replayBtn;

		private Text scoreText;

		private void Awake()
		{
			image = GetComponent<Image>();
			board = transform.Find("Board");

			endAlpha = image.color.a;
			image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
			board.localScale = new Vector3(0, 0, 1);

			scoreText = board.Find("Score").GetComponent<Text>();
			scoreText.text = "0";
		}

		private void OnEnable()
		{
			StartCoroutine(Display());
		}

		private IEnumerator Display()
		{
			float elapsedTime = 0;
			while (elapsedTime < displayTime / 2)
			{
				float alpha = Mathf.Lerp(0, endAlpha, elapsedTime / (displayTime / 2));
				image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);

				elapsedTime += Time.deltaTime;
				yield return null;
			}

			elapsedTime = 0;
			while (elapsedTime < displayTime / 2)
			{
				float scale = Mathf.Lerp(0, 1, elapsedTime / (displayTime / 2));
				board.localScale = new Vector3(scale, scale, 1);

				elapsedTime += Time.deltaTime;
				yield return null;
			}

			SetScore(UIManager.Instance.score);
		}

		private void SetScore(int score)
		{
			// StartCoroutine(UIManager.Instance.IncreaseNumberToward(0, score,
			// 	scorePerFrame => { scoreText.text = scorePerFrame.ToString(); }));
			scoreText.text = score.ToString();
			scoreText.DOCounter(0, score, 0.7f);
		}

		public void SetHighScore(int highScore)
		{
			board.Find("HighScore").GetComponent<Text>().text = highScore.ToString();
		}
	}
}