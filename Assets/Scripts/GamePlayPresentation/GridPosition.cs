using UnityEngine;

// 기능 : 마우스 버튼을 Unity 애플리케이션에 전달한다.
public class GridPosition : MonoBehaviour
{
    // Ctrl + Shift + M -> Visual studio Unity Event Method Check

    [SerializeField] private int _x;
    [SerializeField] private int _y;

    private void OnMouseDown()
    {
        //Logger.Info($"({_x}, {_y}) clicked.");

        // _x와 _y를 GameManager에게 전달해야 한다.
        // 1. GameManager의 메소드를 호출하는 방식
        GameManager.Instance.PlayMarkerRpc(_x, _y);
        // 입력 요청의 단계를 거치도록 수정
        //GameManager.Instance.ReqValidatePlayMarkerRpc(_x, _y, SquareState.Cross);
    }
}