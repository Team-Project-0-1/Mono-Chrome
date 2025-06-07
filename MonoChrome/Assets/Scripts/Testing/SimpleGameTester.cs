using UnityEngine;
using MonoChrome.Core;

namespace MonoChrome.Testing
{
    /// <summary>
    /// 간단한 게임 플로우 테스터
    /// 포트폴리오 시연용
    /// </summary>
    public class SimpleGameTester : MonoBehaviour
    {
        [Header("테스트 키 설정")]
        [SerializeField] private KeyCode testNewGameKey = KeyCode.F1;
        [SerializeField] private KeyCode testCharacterSelectKey = KeyCode.F2;
        [SerializeField] private KeyCode testCombatKey = KeyCode.F3;
        [SerializeField] private KeyCode testDungeonKey = KeyCode.F4;
        
        private void Start()
        {
            Debug.Log("SimpleGameTester 초기화 완료");
            Debug.Log("F1: 새게임, F2: 캐릭터선택, F3: 전투, F4: 던전");
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(testNewGameKey))
            {
                TestNewGame();
            }
            else if (Input.GetKeyDown(testCharacterSelectKey))
            {
                TestCharacterSelect();
            }
            else if (Input.GetKeyDown(testCombatKey))
            {
                TestCombat();
            }
            else if (Input.GetKeyDown(testDungeonKey))
            {
                TestDungeon();
            }
        }
        
        private void TestNewGame()
        {
            Debug.Log("🎮 새 게임 테스트");
            var gameManager = MasterGameManager.Instance;
            if (gameManager != null)
            {
                gameManager.StartNewGame();
            }
        }
        
        private void TestCharacterSelect()
        {
            Debug.Log("🎭 캐릭터 선택 테스트");
            var gameManager = MasterGameManager.Instance;
            if (gameManager != null)
            {
                gameManager.SelectCharacter("김훈희");
            }
        }
        
        private void TestCombat()
        {
            Debug.Log("⚔️ 전투 테스트");
            var gameManager = MasterGameManager.Instance;
            if (gameManager != null)
            {
                gameManager.StartCombat("약탈자1", CharacterType.Normal);
            }
        }
        
        private void TestDungeon()
        {
            Debug.Log("🏰 던전 테스트");
            var gameManager = MasterGameManager.Instance;
            if (gameManager != null)
            {
                gameManager.EnterDungeon();
            }
        }
    }
}
