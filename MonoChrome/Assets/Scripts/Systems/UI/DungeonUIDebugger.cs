using UnityEngine;
using MonoChrome.Systems.Dungeon;

namespace MonoChrome
{
    /// <summary>
    /// DungeonUI 디버깅을 위한 헬퍼 스크립트
    /// </summary>
    public class DungeonUIDebugger : MonoBehaviour
    {
        [Header("디버깅 옵션")]
        [SerializeField] private bool autoActivateOnStart = true;
        
        private void Start()
        {
            if (autoActivateOnStart)
            {
                Invoke(nameof(ActivateDungeonUI), 1f);
            }
        }
        
        [ContextMenu("Activate Dungeon UI")]
        public void ActivateDungeonUI()
        {
            Debug.Log("DungeonUIDebugger: Activating Dungeon UI");
            
            // 1. CharacterSelectionPanel 비활성화
            GameObject charPanel = GameObject.Find("CharacterSelectionPanel");
            if (charPanel != null)
            {
                charPanel.SetActive(false);
            }
            
            // 2. DungeonPanel 활성화
            GameObject dungeonPanel = GameObject.Find("DungeonPanel");
            if (dungeonPanel != null)
            {
                dungeonPanel.SetActive(true);
                Debug.Log("DungeonUIDebugger: DungeonPanel activated");
            }
            
            // 3. GameManager 상태 변경
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameManager.GameState.Dungeon);
            }
            
            // 4. 던전 생성
            Invoke(nameof(GenerateTestDungeon), 0.5f);
        }
        
        public void GenerateTestDungeon()
        {
            DungeonManager dm = FindObjectOfType<DungeonManager>();
            if (dm != null)
            {
                dm.GenerateNewDungeon(0);
            }
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ActivateDungeonUI();
            }
        }
    }
}
