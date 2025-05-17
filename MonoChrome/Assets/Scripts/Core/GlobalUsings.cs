// GlobalUsings.cs
// 이 파일은 프로젝트의 네임스페이스 구조를 문서화하기 위한 파일입니다.
// Unity 프로젝트가 C# 9.0을 사용하고 있어 global using 지시문을 사용할 수 없습니다.

using UnityEngine;

namespace MonoChrome.Core
{
    /// <summary>
    /// 프로젝트 네임스페이스 구조 문서화 클래스
    /// 모든 스크립트는 다음 네임스페이스 구조를 따릅니다.
    /// </summary>
    public static class NamespaceGuide
    {
        /*
        네임스페이스 구조:
        
        - MonoChrome: 최상위 네임스페이스 (공통 열거형, 상수 등)
        - MonoChrome.Core: 핵심 시스템 클래스 (GameManager, 인터페이스 등)
        - MonoChrome.Characters: 캐릭터 관련 클래스
        - MonoChrome.Combat: 전투 시스템 관련 클래스
        - MonoChrome.Dungeon: 던전 생성 및 탐험 관련 클래스
        - MonoChrome.StatusEffects: 상태 효과 관련 클래스
        - MonoChrome.UI: UI 관련 클래스
        - MonoChrome.Utils: 유틸리티 클래스 및 확장 메서드
        
        각 네임스페이스는 해당 도메인에 특화된 기능과 책임이 있어야 합니다.
        네임스페이스 간 의존성은 최소화해야 하며, 가능한 한 단방향 의존성을 유지해야 합니다.
        
        일반적인 using 지시문으로 필요한 네임스페이스를 각 파일에서 명시적으로 참조해야 합니다:
        
        using MonoChrome;  // 열거형 및 공통 타입
        using MonoChrome.Core;  // 핵심 게임 시스템
        using MonoChrome.Characters;  // 캐릭터 관련 클래스
        using MonoChrome.Combat;  // 전투 관련 클래스
        using MonoChrome.Dungeon;  // 던전 관련 클래스
        using MonoChrome.StatusEffects;  // 상태 효과 관련 클래스
        using MonoChrome.UI;  // UI 관련 클래스
        using MonoChrome.Utils;  // 유틸리티 및 확장 메서드
        */
    }
}