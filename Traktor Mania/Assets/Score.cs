using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    [SerializeField] private Slider progressBar;

    private int progress = 0;

    public void AddPoint()
    {
        progress++;
        progressBar.value = progress;
    }

    public void ChangeMaxValue(int value)
    {
        progressBar.maxValue = value;
    }
}
