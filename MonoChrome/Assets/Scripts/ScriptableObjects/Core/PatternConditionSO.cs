using UnityEngine;

namespace MonoChrome
{
    /// <summary>
    /// 패턴 조건을 정의하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Pattern Condition", menuName = "MonoChrome/Combat/Pattern Condition")]
    public class PatternConditionSO : ScriptableObject
    {
        [Header("패턴 유형")]
        public PatternType patternType;

        [Header("위치 조건 (옵션)")]
        public bool usePositionConstraint;
        public int startIndex;
        public int endIndex;

        /// <summary>
        /// 패턴 검증 로직
        /// </summary>
        /// <param name="coinStates">동전 상태 배열</param>
        /// <param name="targetValue">목표 값 (true: 앞면, false: 뒷면)</param>
        /// <returns>패턴 일치 여부</returns>
        public bool Validate(bool[] coinStates, bool targetValue)
        {
            switch (patternType)
            {
                case PatternType.Consecutive2:
                    return ValidateConsecutive(coinStates, targetValue, 2);

                case PatternType.Consecutive3:
                    return ValidateConsecutive(coinStates, targetValue, 3);

                case PatternType.Consecutive4:
                    return ValidateConsecutive(coinStates, targetValue, 4);

                case PatternType.Consecutive5:
                    return ValidateConsecutive(coinStates, targetValue, 5);

                case PatternType.AllOfOne:
                    return ValidateAllSame(coinStates, targetValue);

                case PatternType.Alternating:
                    return ValidateAlternating(coinStates, targetValue);

                default:
                    return false;
            }
        }

        // n개 연속 동일한 면 패턴 검증
        private bool ValidateConsecutive(bool[] coinStates, bool targetValue, int count)
        {
            if (coinStates.Length < count) return false;

            int consecutiveCount = 0;
            int startPos = usePositionConstraint ? startIndex : 0;
            int endPos = usePositionConstraint ? endIndex : coinStates.Length - 1;

            for (int i = startPos; i <= endPos; i++)
            {
                if (coinStates[i] == targetValue)
                {
                    consecutiveCount++;
                    if (consecutiveCount >= count) return true;
                }
                else
                {
                    consecutiveCount = 0;
                }
            }

            return false;
        }

        // 모두 동일한 면인지 검증 (유일)
        private bool ValidateAllSame(bool[] coinStates, bool targetValue)
        {
            int startPos = usePositionConstraint ? startIndex : 0;
            int endPos = usePositionConstraint ? endIndex : coinStates.Length - 1;

            for (int i = startPos; i <= endPos; i++)
            {
                if (coinStates[i] != targetValue) return false;
            }
            return true;
        }

        // 면이 번갈아 가며 나타나는지 검증 (각성)
        private bool ValidateAlternating(bool[] coinStates, bool targetValue)
        {
            int startPos = usePositionConstraint ? startIndex : 0;
            int endPos = usePositionConstraint ? endIndex : coinStates.Length - 1;

            if (endPos - startPos < 2) return false;

            bool expected = targetValue;
            for (int i = startPos; i <= endPos; i++)
            {
                if (coinStates[i] != expected) return false;
                expected = !expected;
            }
            return true;
        }
    }
}
