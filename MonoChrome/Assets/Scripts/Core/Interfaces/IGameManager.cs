using UnityEngine;

namespace MonoChrome.Core
{
    /// <summary>
    /// 게임 매니저 인터페이스 - 의존성 주입과 테스트를 위한 추상화
    /// </summary>
    public interface IGameManager
    {
        bool IsInitialized { get; }
        bool SystemsReady { get; }
        GameStateMachine.GameState CurrentState { get; }
        
        void StartNewGame();
        void SelectCharacter(string characterName);
        void EnterDungeon();
        void StartCombat(string enemyType = "약탈자1", CharacterType characterType = CharacterType.Normal);
        void ReturnToMainMenu();
        void QuitGame();
    }
}