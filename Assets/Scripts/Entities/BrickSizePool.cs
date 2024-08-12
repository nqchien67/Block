using UnityEngine;

namespace Entities
{
	public class BrickSizePool : MonoBehaviour
	{
		public static BrickSizePool Instance;
		public int _maxSize;

		[SerializeField] private int startSize;
		[SerializeField] private int biggestSize;
		[SerializeField] private int scoreAmountToIncreaseSize;

		private void Awake()
		{
			Instance = this;
			_maxSize = startSize;
		}

		public void ChangeSizeByScore(int score)
		{
			if (_maxSize >= biggestSize) return;
			_maxSize = startSize + score / scoreAmountToIncreaseSize;
			_maxSize = Mathf.Min(_maxSize, biggestSize);
		}
	}
}