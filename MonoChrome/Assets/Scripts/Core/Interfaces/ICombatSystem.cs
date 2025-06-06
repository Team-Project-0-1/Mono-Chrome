using System.Collections.Generic;
using MonoChrome.Systems.Combat;

namespace MonoChrome.Core
{
    /// <summary>
    /// 전투 시스템 인터페이스 - 전투 로직의 추상화
    /// </summary>
    public interface ICombatSystem
    {
        bool IsCombatActive { get; }
        bool IsPlayerTurn { get; }
        int TurnCount { get; }
        
        void ExecutePlayerPattern(Pattern pattern);
        void UseActiveSkill();
    }
}