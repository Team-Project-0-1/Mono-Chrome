using System;
using System.Collections.Generic;
using UnityEngine;
using MonoChrome;

namespace MonoChrome.Events
{
    /// <summary>
    /// 던전 관련 이벤트들 - 시스템 간 느슨한 결합을 위한 메시징
    /// </summary>
    public static class DungeonEvents
    {
        /// <summary>던전 생성 요청 이벤트</summary>
        public static event Action<int> OnDungeonGenerationRequested;
        
        /// <summary>던전 생성 완료 이벤트</summary>
        public static event Action<List<DungeonNode>, int> OnDungeonGenerated;
        
        /// <summary>노드 이동 요청 이벤트</summary>
        public static event Action<int> OnNodeMoveRequested;
        
        /// <summary>노드 이동 완료 이벤트</summary>
        public static event Action<DungeonNode> OnNodeMoveCompleted;
        
        /// <summary>방 활동 완료 이벤트</summary>
        public static event Action OnRoomActivityCompleted;

        // 이벤트 발행 메서드들
        public static void RequestDungeonGeneration(int stageIndex) 
            => OnDungeonGenerationRequested?.Invoke(stageIndex);
        
        public static void NotifyDungeonGenerated(List<DungeonNode> nodes, int currentIndex) 
            => OnDungeonGenerated?.Invoke(nodes, currentIndex);
        
        public static void RequestNodeMove(int nodeIndex) 
            => OnNodeMoveRequested?.Invoke(nodeIndex);
        
        public static void NotifyNodeMoveCompleted(DungeonNode node) 
            => OnNodeMoveCompleted?.Invoke(node);
        
        public static void NotifyRoomActivityCompleted() 
            => OnRoomActivityCompleted?.Invoke();

        /// <summary>모든 던전 이벤트 구독 해제</summary>
        public static void ClearAllSubscriptions()
        {
            OnDungeonGenerationRequested = null;
            OnDungeonGenerated = null;
            OnNodeMoveRequested = null;
            OnNodeMoveCompleted = null;
            OnRoomActivityCompleted = null;
        }
    }

    /// <summary>
    /// UI 관련 이벤트들
    /// </summary>
    public static class UIEvents
    {
        /// <summary>패널 표시 요청 이벤트</summary>
        public static event Action<string> OnPanelShowRequested;
        
        /// <summary>던전 맵 업데이트 요청 이벤트</summary>
        public static event Action<List<DungeonNode>, int> OnDungeonMapUpdateRequested;
        
        /// <summary>플레이어 상태 업데이트 요청 이벤트</summary>
        public static event Action<int, int> OnPlayerStatusUpdateRequested;
        
        /// <summary>던전 UI 업데이트 요청 이벤트</summary>
        public static event Action OnDungeonUIUpdateRequested;

        // 이벤트 발행 메서드들
        public static void RequestPanelShow(string panelName) 
            => OnPanelShowRequested?.Invoke(panelName);
        
        public static void RequestDungeonMapUpdate(List<DungeonNode> nodes, int currentIndex) 
            => OnDungeonMapUpdateRequested?.Invoke(nodes, currentIndex);
        
        public static void RequestPlayerStatusUpdate(int currentHealth, int maxHealth) 
            => OnPlayerStatusUpdateRequested?.Invoke(currentHealth, maxHealth);
        
        public static void RequestDungeonUIUpdate()
            => OnDungeonUIUpdateRequested?.Invoke();

        /// <summary>모든 UI 이벤트 구독 해제</summary>
        public static void ClearAllSubscriptions()
        {
            OnPanelShowRequested = null;
            OnDungeonMapUpdateRequested = null;
            OnPlayerStatusUpdateRequested = null;
            OnDungeonUIUpdateRequested = null;
        }
    }

    /// <summary>
    /// 전투 관련 이벤트들
    /// </summary>
    public static class CombatEvents
    {
        /// <summary>전투 시작 요청 이벤트</summary>
        public static event Action<string, CharacterType> OnCombatStartRequested;
        
        /// <summary>전투 초기화 완료 이벤트</summary>
        public static event Action OnCombatInitialized;
        
        /// <summary>전투 종료 요청 이벤트</summary>
        public static event Action<bool> OnCombatEndRequested; // bool: isVictory
        
        /// <summary>전투 종료 이벤트</summary>
        public static event Action<bool> OnCombatEnded; // bool: isVictory

        // 이벤트 발행 메서드들
        public static void RequestCombatStart(string enemyType, CharacterType type) 
            => OnCombatStartRequested?.Invoke(enemyType, type);
        
        public static void NotifyCombatInitialized() 
            => OnCombatInitialized?.Invoke();
        
        public static void RequestCombatEnd(bool isVictory)
            => OnCombatEndRequested?.Invoke(isVictory);
        
        public static void NotifyCombatEnded(bool isVictory) 
            => OnCombatEnded?.Invoke(isVictory);

        /// <summary>모든 전투 이벤트 구독 해제</summary>
        public static void ClearAllSubscriptions()
        {
            OnCombatStartRequested = null;
            OnCombatInitialized = null;
            OnCombatEndRequested = null;
            OnCombatEnded = null;
        }
    }
}