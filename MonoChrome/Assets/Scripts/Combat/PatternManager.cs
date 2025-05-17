using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonoChrome.Combat
{
    /// <summary>
    /// 전투의 패턴(족보) 시스템을 관리하는 클래스
    /// 동전 결과에서 가능한 패턴을 결정하고 평가한다.
    /// </summary>
    public class PatternManager : MonoBehaviour
    {
        [Header("ScriptableObject 패턴 데이터")]
        [SerializeField] private List<PatternSO> patternDatabase;
        [SerializeField] private List<PatternSetSO> patternSets;
        [SerializeField] private bool useScriptableObjects = false; // 이 옵션이 켜져 있으면 ScriptableObject 기반 패턴 사용
        
        // 프로퍼티
        public bool UseScriptableObjects { get => useScriptableObjects; set => useScriptableObjects = value; }
        
        /// <summary>
        /// ScriptableObject 패턴 데이터베이스 초기화
        /// </summary>
        private void Awake()
        {
            if (patternDatabase == null)
            {
                patternDatabase = new List<PatternSO>();
            }
            
            if (patternSets == null)
            {
                patternSets = new List<PatternSetSO>();
            }
        }
        
        /// <summary>
        /// 캐릭터 유형에 맞는 패턴 세트 가져오기
        /// </summary>
        public PatternSetSO GetPatternSetForCharacter(CharacterType characterType, SenseType senseType = SenseType.Auditory)
        {
            if (patternSets == null || patternSets.Count == 0)
            {
                Debug.LogWarning("PatternManager: Pattern sets list is empty");
                return null;
            }
            
            foreach (PatternSetSO patternSet in patternSets)
            {
                if (patternSet.IsApplicableTo(characterType, senseType))
                {
                    return patternSet;
                }
            }
            
            Debug.LogWarning($"PatternManager: No pattern set found for {characterType} with sense {senseType}");
            return null;
        }
        
        /// <summary>
        /// ID로 패턴 ScriptableObject 찾기
        /// </summary>
        public PatternSO GetPatternSOById(int id)
        {
            if (patternDatabase == null || patternDatabase.Count == 0)
            {
                Debug.LogWarning("PatternManager: Pattern database is empty");
                return null;
            }
            
            return patternDatabase.Find(p => p.id == id);
        }
        
        /// <summary>
        /// ScriptableObject 기반으로 사용 가능한 패턴 목록 가져오기
        /// </summary>
        public List<Pattern> GetAvailablePatternsFromSO(CharacterType characterType, SenseType senseType = SenseType.Auditory)
        {
            List<Pattern> result = new List<Pattern>();
            
            if (!useScriptableObjects)
            {
                Debug.LogWarning("PatternManager: ScriptableObject mode is not enabled");
                return result;
            }
            
            PatternSetSO patternSet = GetPatternSetForCharacter(characterType, senseType);
            if (patternSet != null)
            {
                return patternSet.GetPatterns();
            }
            
            return result;
        }
        
        /// <summary>
        /// 주어진 동전 결과와 가능한 패턴 목록에서 사용 가능한 패턴을 결정
        /// </summary>
        /// <param name="coinResults">동전 결과</param>
        /// <param name="availablePatterns">캐릭터가 가진 패턴 목록</param>
        /// <returns>사용 가능한 패턴 목록</returns>
        public List<MonoChrome.Combat.Pattern> DetermineAvailablePatterns(List<bool> coinResults, List<MonoChrome.Combat.Pattern> availablePatterns)
        {
            if (coinResults == null || availablePatterns == null)
            {
                Debug.LogError("PatternManager: Null parameter in DetermineAvailablePatterns");
                return new List<Pattern>();
            }
            
            Debug.Log($"PatternManager: Determining patterns for {coinResults.Count} coins and {availablePatterns.Count} available patterns");
            
            List<Pattern> result = new List<MonoChrome.Combat.Pattern>();
            
            foreach (Pattern pattern in availablePatterns)
            {
                if (IsPatternPossible(coinResults, pattern))
                {
                    result.Add(pattern);
                    Debug.Log($"PatternManager: Pattern '{pattern.Name}' is possible");
                }
            }
            
            Debug.Log($"PatternManager: Found {result.Count} possible patterns");
            return result;
        }
        
        /// <summary>
        /// 주어진 동전 결과에 대해 사용 가능한 패턴을 ScriptableObject 기반으로 결정
        /// </summary>
        public List<Pattern> DetermineAvailablePatternsFromSO(List<bool> coinResults, CharacterType characterType, SenseType senseType = SenseType.Auditory)
        {
            if (!useScriptableObjects)
            {
                Debug.LogWarning("PatternManager: ScriptableObject mode is not enabled");
                return new List<Pattern>();
            }
            
            List<Pattern> availablePatterns = GetAvailablePatternsFromSO(characterType, senseType);
            return DetermineAvailablePatterns(coinResults, availablePatterns);
        }
        
        /// <summary>
        /// 특정 패턴이 현재 동전 상태에서 가능한지 확인
        /// </summary>
        /// <param name="coinResults">동전 결과</param>
        /// <param name="pattern">확인할 패턴</param>
        /// <returns>패턴 가능 여부</returns>
        private bool IsPatternPossible(List<bool> coinResults, Pattern pattern)
        {
            if (coinResults == null || pattern == null)
            {
                return false;
            }
            
            switch (pattern.PatternType)
            {
                case PatternType.Consecutive2:
                    return HasConsecutiveCoins(coinResults, 2, pattern.PatternValue);
                    
                case PatternType.Consecutive3:
                    return HasConsecutiveCoins(coinResults, 3, pattern.PatternValue);
                    
                case PatternType.Consecutive4:
                    return HasConsecutiveCoins(coinResults, 4, pattern.PatternValue);
                    
                case PatternType.Consecutive5:
                    return HasConsecutiveCoins(coinResults, 5, pattern.PatternValue);
                    
                case PatternType.AllOfOne:
                    return HasAllSameCoins(coinResults, pattern.PatternValue);
                    
                case PatternType.Alternating:
                    return HasAlternatingCoins(coinResults);
                    
                default:
                    Debug.LogWarning($"PatternManager: Unknown pattern type {pattern.PatternType}");
                    return false;
            }
        }
        
        /// <summary>
        /// 연속된 같은 면의 동전이 있는지 확인
        /// </summary>
        /// <param name="coinResults">동전 결과</param>
        /// <param name="count">필요한 연속 개수</param>
        /// <param name="isHeads">앞면(true) 또는 뒷면(false) 여부</param>
        /// <returns>연속 동전 존재 여부</returns>
        private bool HasConsecutiveCoins(List<bool> coinResults, int count, bool isHeads)
        {
            if (count > coinResults.Count)
            {
                return false;
            }
            
            int currentCount = 0;
            int maxCount = 0;
            
            foreach (bool coin in coinResults)
            {
                if (coin == isHeads)
                {
                    currentCount++;
                    maxCount = Mathf.Max(maxCount, currentCount);
                }
                else
                {
                    currentCount = 0;
                }
            }
            
            return maxCount >= count;
        }
        
        /// <summary>
        /// 모든 동전이 같은 면인지 확인
        /// </summary>
        /// <param name="coinResults">동전 결과</param>
        /// <param name="isHeads">앞면(true) 또는 뒷면(false) 여부</param>
        /// <returns>모든 동전 일치 여부</returns>
        private bool HasAllSameCoins(List<bool> coinResults, bool isHeads)
        {
            if (coinResults.Count == 0)
            {
                return false;
            }
            
            foreach (bool coin in coinResults)
            {
                if (coin != isHeads)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 동전이 교대로 나타나는지 확인 (앞뒤앞뒤... 또는 뒤앞뒤앞...)
        /// </summary>
        /// <param name="coinResults">동전 결과</param>
        /// <returns>교대 패턴 여부</returns>
        private bool HasAlternatingCoins(List<bool> coinResults)
        {
            if (coinResults.Count <= 1)
            {
                return false;
            }
            
            bool expected = coinResults[0];
            
            for (int i = 0; i < coinResults.Count; i++)
            {
                expected = !expected; // 다음에 기대하는 값은 반대값
                
                if (i % 2 == 1 && coinResults[i] != expected)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 연속 패턴의 길이 찾기
        /// </summary>
        /// <param name="coinResults">동전 결과</param>
        /// <param name="isHeads">앞면(true) 또는 뒷면(false) 여부</param>
        /// <returns>최대 연속 길이</returns>
        public int FindMaxConsecutiveLength(List<bool> coinResults, bool isHeads)
        {
            int currentCount = 0;
            int maxCount = 0;
            
            foreach (bool coin in coinResults)
            {
                if (coin == isHeads)
                {
                    currentCount++;
                    maxCount = Mathf.Max(maxCount, currentCount);
                }
                else
                {
                    currentCount = 0;
                }
            }
            
            return maxCount;
        }
        
        /// <summary>
        /// 주어진 패턴에 해당하는 동전 위치 찾기
        /// </summary>
        /// <param name="coinResults">동전 결과</param>
        /// <param name="pattern">찾을 패턴</param>
        /// <returns>패턴에 해당하는 동전 인덱스 목록</returns>
        public List<int> FindPatternCoinIndices(List<bool> coinResults, Pattern pattern)
        {
            List<int> indices = new List<int>();
            
            if (coinResults == null || pattern == null)
            {
                return indices;
            }
            
            switch (pattern.PatternType)
            {
                case PatternType.Consecutive2:
                case PatternType.Consecutive3:
                case PatternType.Consecutive4:
                case PatternType.Consecutive5:
                    int requiredCount = GetRequiredConsecutiveCount(pattern.PatternType);
                    FindConsecutiveIndices(coinResults, pattern.PatternValue, requiredCount, indices);
                    break;
                    
                case PatternType.AllOfOne:
                    // 모든 동전이 같은 면인 경우 모든 인덱스 추가
                    if (HasAllSameCoins(coinResults, pattern.PatternValue))
                    {
                        for (int i = 0; i < coinResults.Count; i++)
                        {
                            indices.Add(i);
                        }
                    }
                    break;
                    
                case PatternType.Alternating:
                    // 교대 패턴인 경우 모든 인덱스 추가
                    if (HasAlternatingCoins(coinResults))
                    {
                        for (int i = 0; i < coinResults.Count; i++)
                        {
                            indices.Add(i);
                        }
                    }
                    break;
            }
            
            return indices;
        }
        
        /// <summary>
        /// 연속된 동전 인덱스 찾기
        /// </summary>
        private void FindConsecutiveIndices(List<bool> coinResults, bool isHeads, int requiredCount, List<int> indices)
        {
            int currentCount = 0;
            int startIndex = -1;
            
            for (int i = 0; i < coinResults.Count; i++)
            {
                if (coinResults[i] == isHeads)
                {
                    if (currentCount == 0)
                    {
                        startIndex = i;
                    }
                    
                    currentCount++;
                    
                    if (currentCount == requiredCount)
                    {
                        // 패턴을 찾음 - 인덱스 추가
                        for (int j = 0; j < requiredCount; j++)
                        {
                            indices.Add(startIndex + j);
                        }
                        
                        // 인덱스를 찾았으므로 종료
                        return;
                    }
                }
                else
                {
                    currentCount = 0;
                    startIndex = -1;
                }
            }
        }
        
        /// <summary>
        /// 패턴 유형에 따른 필요한 연속 동전 개수 반환
        /// </summary>
        private int GetRequiredConsecutiveCount(PatternType patternType)
        {
            switch (patternType)
            {
                case PatternType.Consecutive2: return 2;
                case PatternType.Consecutive3: return 3;
                case PatternType.Consecutive4: return 4;
                case PatternType.Consecutive5: return 5;
                default: return 0;
            }
        }
    }
}