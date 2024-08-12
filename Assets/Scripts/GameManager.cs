using System;
using System.Collections;
using Entities;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	private int score;

	private UIManager ui;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		ui = UIManager.Instance;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
			Replay();
	}


	public void GameOver()
	{
		StopAllCoroutines();
		UIManager.Instance.GameOver();
	}

	public void GainScore(int amount)
	{
		ui.IncreaseScore(amount);
		BrickSizePool.Instance.ChangeSizeByScore(ui.score);
	}

	public void GainScore(int sequenceCount, int number)
	{
		CameraController.Instance.StartShake(0.01f * sequenceCount, 0.003f * sequenceCount);
		GainScore(sequenceCount * Mathf.RoundToInt(number / 2f));
	}

	public void Replay()
	{
		SceneManager.LoadScene("Game");
	}

	public void GoHome()
	{
		SceneManager.LoadScene("Home");
	}

	public IEnumerator WaitForSeconds(float seconds, Action action)
	{
		yield return new WaitForSeconds(seconds);
		action.Invoke();
	}

	public IEnumerator WaitForFrames(int numFrames, Action action)
	{
		for (int i = 0; i < numFrames; i++)
			yield return null;

		action.Invoke();
	}

	public IEnumerator WaitForEndOfFrame(Action action)
	{
		yield return new WaitForEndOfFrame();
		action.Invoke();
	}
}