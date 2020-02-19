using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIManager : MonoBehaviour {
  public Image[] lifeImages;
  public int lifeCount;
  private int score;
  public TextMeshProUGUI scoreText;
  private static UIManager instance;
  private void Start() {
    instance = this;
    AddScore(0);
  }
  public static void RemoveLife() {
    instance.lifeImages[instance.lifeCount - 1].enabled = false;
    instance.lifeCount--;
  }
  public static void AddScore(int value) {
    instance.score += value;
    instance.scoreText.text = "Score: " + instance.score.ToString();
  }
}
