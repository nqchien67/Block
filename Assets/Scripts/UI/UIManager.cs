using System;
using System.Collections;
using Assets.Scripts.UI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class UIManager : MonoBehaviour
	{
		public static UIManager Instance;
		[SerializeField] private Text notificationText;
		[SerializeField] private Text comboText;

		public int score;

		private Transform gameOverScreen;

		private Coroutine notiTextBlinkRoutine;
		private Text scoreText;

		private void Awake()
		{
			Instance = this;

			var scoreBox = transform.Find("Score");
			scoreText = scoreBox != null
				? scoreBox.GetComponentInChildren<Text>()
				: transform.Find("ScoreText").GetComponent<Text>();

			gameOverScreen = transform.Find("GameOverScreen");
		}

		private void Start()
		{
			if (comboText != null)
			{
				Color color = comboText.color;
				color.a = 0;
				comboText.color = color;
			}
		}

		private void OnDisable()
		{
			scoreText.transform.DOKill();
			transform.DOKill();
			notificationText.DOKill();
		}

		private void DisplayGameOverBoard()
		{
			gameOverScreen.gameObject.SetActive(true);
			gameOverScreen.GetComponent<GameOverBoard>().SetHighScore(GetOrSetHighScore(score));
		}

		public void IncreaseScore(int amount)
		{
			score += amount;
			scoreText.DOKill();
			scoreText.DOCounter(int.Parse(scoreText.text), score, 0.3f, false).OnComplete(() =>
			{
				scoreText.transform.DOKill();
				const float newScale = 1.2f;
				scoreText.transform.DOScale(new Vector3(newScale, newScale, 1), 0.1f).SetLoops(2, LoopType.Yoyo);
			});
		}

		public IEnumerator IncreaseNumberToward(int startNumber, int endNumber, Action<int> actionPerFrame,
			int waitFrame = 1, Action onComplete = null)
		{
			int currentNumber = startNumber;
			while (currentNumber < endNumber)
			{
				currentNumber += 1;
				actionPerFrame?.Invoke(currentNumber);

				for (int i = 0; i < waitFrame; i++)
					yield return null;
			}

			onComplete?.Invoke();
		}

		private int GetOrSetHighScore(int currentScore)
		{
			int highScore = 0;
			if (PlayerPrefs.HasKey("HighScore"))
				highScore = PlayerPrefs.GetInt("HighScore");

			if (currentScore > highScore)
			{
				highScore = currentScore;
				PlayerPrefs.SetInt("HighScore", highScore);
				PlayerPrefs.Save();
			}

			return highScore;
		}

		public void GameOver()
		{
			DisplayGameOverBoard();
		}

		public void SetNotificationText(string text, Color? color = null, bool blink = false)
		{
			if (notiTextBlinkRoutine != null)
			{
				StopCoroutine(notiTextBlinkRoutine);
				notiTextBlinkRoutine = null;
			}

			notificationText.gameObject.SetActive(text != "");

			notificationText.text = text;
			if (color.HasValue)
				notificationText.color = color.Value;

			if (blink)
				notiTextBlinkRoutine = StartCoroutine(BlinkTextEffect());
		}

		private IEnumerator BlinkTextEffect()
		{
			yield return notificationText.DOFade(1, 0.1f).WaitForCompletion();
			yield return notificationText.transform.DOScale(new Vector3(1.2f, 1.2f, 1), 0.2f)
				.SetLoops(2, LoopType.Yoyo).WaitForCompletion();
		}
	}
}