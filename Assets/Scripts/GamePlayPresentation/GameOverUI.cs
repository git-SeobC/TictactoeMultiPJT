using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textUI;

    void Start()
    {
        GameManager.Instance.OnGameEnded += OnGameEnded;
        gameObject.SetActive(false);
    }

    private void OnGameEnded(GameOverState state)
    {
        gameObject.SetActive(true);

        if (state == GameOverState.Tie)
        {
            _textUI.text = "Tie!";
        }
        else
        {
            _textUI.text = $"{state} is winner.";
        }
    }
}
