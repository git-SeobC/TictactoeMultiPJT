using System;
using System.Threading;
using TMPro.EditorUtilities;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;


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

    private SquareState[,] _board = new SquareState[3, 3];
    private SquareState _currentTurnState = SquareState.Cross;
    private GameOverState _gameOverState = GameOverState.NotOver;

    // 보드의 좌표, SquareState를 전달
    public event Action<int, int, SquareState> OnBoardChanged;
    // GameOverUI를 표현할 이벤트
    public event Action<GameOverState> OnGameEnded;
    public event Action<SquareState> OnTurnChanged;

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
        //NetworkManager.Singleton.OnconnectionEvent += (networkManager, ConnenctionEventData) =>
        {

        };
    }

    bool circleTurn = false;
    public void PlayMarker(int _x, int _y)
    {
        if (_gameOverState != GameOverState.NotOver)
        {
            return;
        }
        if (_board[_y, _x] != SquareState.None)
        {
            Logger.Info($"해당 위치에 두기 실패");
            return;
        }

        _board[_y, _x] = _currentTurnState;

        OnBoardChanged?.Invoke(_x, _y, _board[_y, _x]);

        _gameOverState = TestGameOver();
        if (_gameOverState != GameOverState.NotOver)
        {
            OnGameEnded?.Invoke(_gameOverState);
            return;
        }

        if (_currentTurnState == SquareState.Cross) _currentTurnState = SquareState.Circle;
        else _currentTurnState = SquareState.Cross;
        OnTurnChanged?.Invoke(_currentTurnState);
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
}
