using UnityEngine;
using MonoChrome.Events;
using MonoChrome;
using System.Collections.Generic;

namespace MonoChrome.Core
{
    /// <summary>
    /// 핵심 UI 매니저 - 게임 상태에 따른 UI 자동 관리
    /// 레거시 호환성을 위한 CoreUIManager 구현
    /// </summary>
    public class CoreUIManager : MonoBehaviour
    {
        [Header("UI 시스템 설정")]
        [SerializeField] private bool _enableDebugLogs = true;
        [SerializeField] private bool _autoManageUI = true;
        
        // 현재 상태
        private bool _isInitialized = false;
        
        private void Awake()
        {
            Initialize();
        }
        
        private void OnEnable()
        {
            SubscribeToEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        
        /// <summary>
        /// 초기화
        /// </summary>
        private void Initialize()
        {
            if (_isInitialized) return;
            
            LogDebug("CoreUIManager 초기화 중...");
            
            _isInitialized = true;
            LogDebug("CoreUIManager 초기화 완료");
        }
        
        /// <summary>
        /// 이벤트 구독
        /// </summary>
        private void SubscribeToEvents()
        {
            if (!_autoManageUI) return;
            
            // 게임 상태 변경 이벤트 구독
            GameStateMachine.OnStateChanged += OnGameStateChanged;
            
            LogDebug("CoreUIManager 이벤트 구독 완료");
        }
        
        /// <summary>
        /// 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            // 게임 상태 변경 이벤트 구독 해제
            GameStateMachine.OnStateChanged -= OnGameStateChanged;
            
            LogDebug("CoreUIManager 이벤트 구독 해제 완료");
        }
        
        /// <summary>
        /// 게임 상태 변경 처리
        /// </summary>
        private void OnGameStateChanged(GameStateMachine.GameState previousState, GameStateMachine.GameState newState)
        {
            LogDebug($"게임 상태 변경 감지: {previousState} -> {newState}");
            
            // 상태에 따른 UI 자동 전환
            switch (newState)
            {
                case GameStateMachine.GameState.MainMenu:
                    ShowMainMenuUI();
                    break;
                case GameStateMachine.GameState.CharacterSelection:
                    ShowCharacterSelectionUI();
                    break;
                case GameStateMachine.GameState.Dungeon:
                    ShowDungeonUI();
                    break;
                case GameStateMachine.GameState.Combat:
                    ShowCombatUI();
                    break;
                case GameStateMachine.GameState.GameOver:
                    ShowGameOverUI();
                    break;
                case GameStateMachine.GameState.Victory:
                    ShowVictoryUI();
                    break;
            }
        }
        
        #region UI 표시 메서드들
        /// <summary>
        /// 메인 메뉴 UI 표시
        /// </summary>
        private void ShowMainMenuUI()
        {
            LogDebug("메인 메뉴 UI 표시");
            DungeonEvents.UIEvents.RequestPanelShow("MainMenuPanel");
        }
        
        /// <summary>
        /// 캐릭터 선택 UI 표시
        /// </summary>
        private void ShowCharacterSelectionUI()
        {
            LogDebug("캐릭터 선택 UI 표시");
            DungeonEvents.UIEvents.RequestPanelShow("CharacterSelectionPanel");
        }
        
        /// <summary>
        /// 던전 UI 표시
        /// </summary>
        private void ShowDungeonUI()
        {
            LogDebug("던전 UI 표시");
            DungeonEvents.UIEvents.RequestPanelShow("DungeonPanel");
        }
        
        /// <summary>
        /// 전투 UI 표시
        /// </summary>
        private void ShowCombatUI()
        {
            LogDebug("전투 UI 표시");
            DungeonEvents.UIEvents.RequestPanelShow("CombatUI");
        }
        
        /// <summary>
        /// 게임 오버 UI 표시
        /// </summary>
        private void ShowGameOverUI()
        {
            LogDebug("게임 오버 UI 표시");
            DungeonEvents.UIEvents.RequestPanelShow("GameOverPanel");
        }
        
        /// <summary>
        /// 승리 UI 표시
        /// </summary>
        private void ShowVictoryUI()
        {
            LogDebug("승리 UI 표시");
            DungeonEvents.UIEvents.RequestPanelShow("VictoryPanel");
        }
        #endregion
        
        #region 공개 API (레거시 호환성)
        /// <summary>
        /// 던전 UI 업데이트 (레거시 호환성)
        /// </summary>
        public void UpdateDungeonUI()
        {
            LogDebug("던전 UI 업데이트 요청");
            DungeonEvents.UIEvents.RequestDungeonUIUpdate();
        }
        
        /// <summary>
        /// 던전 맵 업데이트 (레거시 호환성)
        /// </summary>
        public void UpdateDungeonMap(List<DungeonNode> nodes, int currentIndex)
        {
            LogDebug($"던전 맵 업데이트: {nodes.Count}개 노드, 현재 인덱스 {currentIndex}");
            DungeonEvents.UIEvents.RequestDungeonMapUpdate(nodes, currentIndex);
        }
        
        /// <summary>
        /// 패널 전환 이벤트 처리 (레거시 호환성)
        /// </summary>
        public void OnPanelSwitched(string panelName)
        {
            LogDebug($"패널 전환: {panelName}");
            DungeonEvents.UIEvents.RequestPanelShow(panelName);
        }
        #endregion
        
        #region 상태 및 디버깅
        /// <summary>
        /// 초기화 상태 확인
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// 디버그 로그
        /// </summary>
        private void LogDebug(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[CoreUIManager] {message}");
            }
        }
        
        /// <summary>
        /// 상태 리포트 생성
        /// </summary>
        [ContextMenu("Generate CoreUIManager Status Report")]
        public void GenerateStatusReport()
        {
            LogDebug("=== CoreUIManager 상태 리포트 ===");
            LogDebug($"초기화됨: {_isInitialized}");
            LogDebug($"자동 UI 관리: {_autoManageUI}");
            LogDebug($"디버그 로그: {_enableDebugLogs}");
            
            var gameStateMachine = GameStateMachine.Instance;
            if (gameStateMachine != null)
            {
                LogDebug($"현재 게임 상태: {gameStateMachine.CurrentState}");
            }
            else
            {
                LogDebug("GameStateMachine을 찾을 수 없음");
            }
            
            LogDebug("==============================");
        }
        #endregion
    }
}