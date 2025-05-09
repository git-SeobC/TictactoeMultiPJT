using Unity.Netcode;
using UnityEngine;


/// <summary>
/// 보드의 상태를 애플리케이션에 출력한다.
/// </summary>
public class GameVisualManager : NetworkBehaviour
{
    // 인스턴싱을 위한 프리팹 등록
    [SerializeField] private GameObject _crossMarkerPrefab;
    [SerializeField] private GameObject _circleMarkerPrefab;

    private void Start()
    {
        GameManager.Instance.OnBoardChanged += CreateMarkerRpc;
    }

    [Rpc(SendTo.Server)]
    private void CreateMarkerRpc(int _x, int _y, SquareState boardState)
    {
        switch (boardState)
        {
            case SquareState.Cross:
                Instantiate(_crossMarkerPrefab, GetWorldPositionFromCoordinate(_x, _y), Quaternion.identity);
                break;
            case SquareState.Circle:
                Instantiate(_circleMarkerPrefab, GetWorldPositionFromCoordinate(_x, _y), Quaternion.identity);
                break;
            default:
                Logger.Error($"잘못된 값이 입력되었습니다. {(int)boardState}");
                break;
        }
    }

    private Vector2 GetWorldPositionFromCoordinate(int x, int y)
    {
        int worldX = -3 + 3 * x;
        int worldY = 3 - 3 * y;

        return new Vector2(worldX, worldY);
    }

}
