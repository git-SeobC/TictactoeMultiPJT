using System;
using UnityEngine;

public class HUDUI : MonoBehaviour
{
    [SerializeField] private GameObject _circleIndicatorArrow;
    [SerializeField] private GameObject _crossIndicatorArrow;
    void Start()
    {
        _circleIndicatorArrow.SetActive(false);
        _crossIndicatorArrow.SetActive(false);
        GameManager.Instance.OnTurnChanged += ChangeIndicator;
        //GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
    }

    private void GameManager_OnGameStarted(object sender, System.EventArgs e)
    {
        //if (GameManager.Instance.GetLocalPlayerType() == SquareState.Cross)
        //{
        //    crossYouTextGameObject.SetActive(true);
        //}
        //else
        //{
        //    circleYouTextGameObject.SetActive(true);
        //}
        //playerCrossScoreTextMesh.text = "0";
        //playerCircleScoreTextMesh.text = "0";

        //UpdateCurrentArrow();
    }

    private void ChangeIndicator(SquareState currentTurn)
    {
        switch (currentTurn)
        {
            case SquareState.None:
                _circleIndicatorArrow.SetActive(false);
                _crossIndicatorArrow.SetActive(false);
                break;
            case SquareState.Cross:
                _circleIndicatorArrow.SetActive(false);
                _crossIndicatorArrow.SetActive(true);
                break;
            case SquareState.Circle:
                _circleIndicatorArrow.SetActive(true);
                _crossIndicatorArrow.SetActive(false);
                break;
            default:
                throw new ArgumentOutOfRangeException($"{(int)currentTurn}");
        }
    }
}
