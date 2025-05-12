using System;
using Unity.Netcode;
using Unity.VisualScripting.YamlDotNet.Serialization;

/// <summary>
/// 틱텍토 게임을 진행 -> 비즈니스 로직 담당 -> 핵심 묘듈
/// - 에플리케이션을 여러 계층(수준)으로 나눈다.
/// ㄴ 입출력과 가까울수록 저수준, 입출력과 멀어질수록 고수준
/// ㄴ 저수준이 고수준에 의존하게 만들어야 한다.
/// </summary>
/// 

// 순서대로 O X를 착수하는 게임
// 보드를 메모리에 표현
public enum SquareState
{
    None,
    Cross,
    Circle
}

public enum GameOverState
{
    NotOver,
    Cross,
    Circle,
    Tie
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    // NetworkVariable는 읽기와 쓰기 권한이 있고 클라이언트는 읽기 권한만, 쓰기는 서버가 갖는다.
    private NetworkVariable<SquareState> currentPlayablePlayerType = new NetworkVariable<SquareState>();
    private NetworkVariable<SquareState> _currentTurnState = new();
    private SquareState _localPlayerType = SquareState.None; // 각 클라이언트 타입 구분

    private SquareState[,] _board = new SquareState[3, 3];
    private NetworkVariable<GameOverState> _gameOverState = new();

    // 보드의 좌표, SquareState를 전달
    public event Action<int, int, SquareState> OnBoardChanged;
    // GameOverUI를 표현할 이벤트
    public event Action<GameOverState> OnGameEnded;
    public event Action<SquareState> OnTurnChanged;

    public event EventHandler OnGameStarted;
    public event EventHandler OnCurrentPlayablePlayerTypeChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _currentTurnState.OnValueChanged += (previousState, newState) =>
        {
            OnTurnChanged?.Invoke(newState);
            Logger.Info($"{previousState} => {newState}");
        };

        NetworkManager.Singleton.OnConnectionEvent += (networkManager, ConnectionEventData) =>
        {
            Logger.Info($"Client {ConnectionEventData.ClientId} {ConnectionEventData.EventType}");
            if (networkManager.ConnectedClients.Count == 2)
            {
                StartGame();
            }
        };
    }

    public override void OnNetworkSpawn()
    {
        //Debug.Log("OnNetworkSpawn : " + NetworkManager.Singleton.LocalClientId);
        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            _localPlayerType = SquareState.Cross;
        }
        else
        {
            _localPlayerType = SquareState.Circle;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        currentPlayablePlayerType.OnValueChanged += (SquareState oldPlayerType, SquareState newPlayerType) =>
        {
            OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            // Start Game
            currentPlayablePlayerType.Value = SquareState.Cross;
            TriggerOnGameStartedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }


    private bool CanPlayMarker(int x, int y, SquareState localPlayerType)
    {
        return _gameOverState.Value != GameOverState.NotOver &&
            localPlayerType == _currentTurnState.Value &&
            _board[y, x] == SquareState.None;
    }

    public void StartGame()
    {
        if (IsHost)
        {
            _localPlayerType = SquareState.Cross;
            _currentTurnState.Value = SquareState.Cross; // 주의! 현재 차례 상태를 바꾸는 쓰기 권한은 호스트만 가능하도록
        }
        else
        {
            _localPlayerType = SquareState.Circle;
        }
    }

    // 서버에게 입력에 대해 요청하는 메소드
    // 입력 : 좌표 값
    // 출력 : X
    [Rpc(SendTo.Server)]
    public void ReqValidatePlayMarkerRpc(int x, int y, SquareState localPlayerType)
    {
        Logger.Info($"{nameof(ReqValidatePlayMarkerRpc)} {x}, {y}, {localPlayerType}");

        if (CanPlayMarker(x, y, localPlayerType) == false)
        {
            return;
        }

        // 서버만 바뀜
        OnBoardChanged?.Invoke(x, y, localPlayerType);
        _board[y, x] = localPlayerType;


        if (_currentTurnState.Value == SquareState.Cross)
        {
            _currentTurnState.Value = SquareState.Circle;
        }
        else if (_currentTurnState.Value == SquareState.Circle)
        {
            _currentTurnState.Value = SquareState.Cross;
        }
    }

    //[Rpc(SendTo.Server)]
    public void PlayMarkerRpc(int _x, int _y)
    {
        // 서버에게 입력이 유효한지 요청
        if (CanPlayMarker(_x, _y, _localPlayerType))
        {
            ReqValidatePlayMarkerRpc(_x, _y, _localPlayerType);
        }
        else return;

        //if (playerType != currentPlayablePlayerType.Value) return;
        //if (_gameOverState != GameOverState.NotOver) return;
        //if (_board[_y, _x] != SquareState.None)
        //{
        //    Logger.Info($"해당 위치에 두기 실패");
        //    return;
        //}

        //_board[_y, _x] = currentPlayablePlayerType.Value;


        //_gameOverState = TestGameOver();
        //if (_gameOverState != GameOverState.NotOver)
        //{
        //    OnGameEnded?.Invoke(_gameOverState);
        //    return;
        //}

        //switch (currentPlayablePlayerType.Value)
        //{
        //    default:
        //    case SquareState.Cross:
        //        currentPlayablePlayerType.Value = SquareState.Circle;
        //        break;
        //    case SquareState.Circle:
        //        currentPlayablePlayerType.Value = SquareState.Cross;
        //        break;
        //}

        //OnTurnChanged?.Invoke(currentPlayablePlayerType.Value);
    }


    GameOverState TestGameOver()
    {
        // 가로 검사
        for (int y = 0; y < 3; y++)
        {
            if (_board[y, 0] != SquareState.None &&
                _board[y, 0] == _board[y, 1] && _board[y, 1] == _board[y, 2])
            {
                if (_board[y, 0] == SquareState.Cross)
                {
                    return GameOverState.Cross;
                }
                else if (_board[y, 0] == SquareState.Circle)
                {
                    return GameOverState.Circle;
                }
            }
        }
        // 세로 검사
        for (int x = 0; x < 3; x++)
        {
            if (_board[0, x] != SquareState.None &&
                _board[0, x] == _board[1, x] && _board[1, x] == _board[2, x])
            {
                if (_board[0, x] == SquareState.Cross)
                {
                    return GameOverState.Cross;
                }
                else if (_board[0, x] == SquareState.Circle)
                {
                    return GameOverState.Circle;
                }
            }
        }
        // 대각 검사
        if (_board[0, 0] != SquareState.None && _board[0, 0] == _board[1, 1] && _board[1, 1] == _board[2, 2])
        {
            if (_board[0, 0] == SquareState.Cross)
            {
                return GameOverState.Cross;
            }
            else if (_board[0, 0] == SquareState.Circle)
            {
                return GameOverState.Circle;
            }
        }
        if (_board[2, 0] != SquareState.None && _board[2, 0] == _board[1, 1] && _board[1, 1] == _board[0, 2])
        {
            if (_board[2, 0] == SquareState.Cross)
            {
                return GameOverState.Cross;
            }
            else if (_board[2, 0] == SquareState.Circle)
            {
                return GameOverState.Circle;
            }
        }
        // 무승부 = 모든 칸이 다 차면
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                if (_board[y, x] == SquareState.None)
                {
                    return GameOverState.NotOver;
                }
            }
        }
        return GameOverState.Tie;
    }

    public void CheckGame()
    {
        #region 하드코딩
        //가로 판정
        if (_board[0, 0] != SquareState.None && _board[0, 0] == _board[0, 1] && _board[0, 1] == _board[0, 2])
        {
            Logger.Info($"{_board[0, 0]}이/가 승리하였습니다");
            return;
        }
        else if (_board[1, 0] != SquareState.None && _board[1, 0] == _board[1, 1] && _board[1, 1] == _board[1, 2])
        {
            Logger.Info($"{_board[1, 0]}이/가 승리하였습니다");
            return;
        }
        else if (_board[2, 0] != SquareState.None && _board[2, 0] == _board[2, 1] && _board[2, 1] == _board[2, 2])
        {
            Logger.Info($"{_board[2, 0]}이/가 승리하였습니다");
            return;
        }
        // 세로 판정
        else if (_board[0, 0] != SquareState.None && _board[0, 0] == _board[1, 0] && _board[1, 0] == _board[2, 0])
        {
            Logger.Info($"{_board[2, 0]}이/가 승리하였습니다");
            return;
        }
        else if (_board[0, 1] != SquareState.None && _board[0, 1] == _board[1, 1] && _board[1, 1] == _board[2, 1])
        {
            Logger.Info($"{_board[2, 0]}이/가 승리하였습니다");
            return;
        }
        else if (_board[0, 2] != SquareState.None && _board[0, 2] == _board[1, 2] && _board[1, 2] == _board[2, 2])
        {
            Logger.Info($"{_board[2, 0]}이/가 승리하였습니다");
            return;
        }
        // 대각 판정
        else if (_board[0, 0] != SquareState.None && _board[0, 0] == _board[1, 1] && _board[1, 1] == _board[2, 2])
        {
            Logger.Info($"{_board[0, 0]}이/가 승리하였습니다");
            return;
        }
        else if (_board[2, 0] != SquareState.None && _board[2, 0] == _board[1, 1] && _board[1, 1] == _board[0, 2])
        {
            Logger.Info($"{_board[2, 0]}이/가 승리하였습니다");
            return;
        }
        #endregion
    }

    public SquareState GetLocalPlayerType()
    {
        return _localPlayerType;
    }
}
