using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonoChrome.Combat
{
    /// <summary>
    /// 전투에서 동전 시스템을 관리하는 클래스
    /// 동전 던지기, 동전 상태 변경 및 결과 추적을 담당한다.
    /// </summary>
    public class CoinManager : MonoBehaviour
    {
        [SerializeField] private int _coinCount = 5;
        
        // 동전 상태 배열 (true: 앞면, false: 뒷면)
        private bool[] _coinStates;
        
        // 고정된 동전 인덱스 (액티브 스킬에 의한 고정)
        private bool[] _lockedCoins;
        
        // Unity 이벤트 함수
        private void Awake()
        {
            InitializeCoinArrays();
        }
        
        /// <summary>
        /// 동전 배열 초기화
        /// </summary>
        private void InitializeCoinArrays()
        {
            _coinStates = new bool[_coinCount];
            _lockedCoins = new bool[_coinCount];
            
            // 초기화 - 모든 동전 잠금 해제
            for (int i = 0; i < _coinCount; i++)
            {
                _lockedCoins[i] = false;
            }
            
            Debug.Log($"CoinManager: Initialized arrays for {_coinCount} coins");
        }
        
        /// <summary>
        /// 모든 동전 던지기
        /// </summary>
        public void FlipCoins()
        {
            Debug.Log("CoinManager: Flipping coins");
            
            for (int i = 0; i < _coinCount; i++)
            {
                // 잠긴 동전은 던지지 않음
                if (!_lockedCoins[i])
                {
                    // 랜덤 동전 던지기 (true: 앞면, false: 뒷면)
                    _coinStates[i] = Random.value > 0.5f;
                    
                    // 결과 로그
                    Debug.Log($"CoinManager: Coin {i} flipped to {(_coinStates[i] ? "Heads" : "Tails")}");
                }
                else
                {
                    Debug.Log($"CoinManager: Coin {i} is locked, keeping as {(_coinStates[i] ? "Heads" : "Tails")}");
                }
            }
            
            // 모든 동전 잠금 해제 (턴 종료시 항상 해제됨)
            for (int i = 0; i < _coinCount; i++)
            {
                _lockedCoins[i] = false;
            }
        }
        
        /// <summary>
        /// 특정 동전 뒤집기
        /// </summary>
        /// <param name="index">뒤집을 동전의 인덱스</param>
        public void FlipCoin(int index)
        {
            if (index < 0 || index >= _coinCount)
            {
                Debug.LogError($"CoinManager: Invalid coin index: {index}");
                return;
            }
            
            // 동전 뒤집기
            _coinStates[index] = !_coinStates[index];
            
            Debug.Log($"CoinManager: Coin {index} manually flipped to {(_coinStates[index] ? "Heads" : "Tails")}");
        }
        
        /// <summary>
        /// 특정 동전 고정하기
        /// </summary>
        /// <param name="index">고정할 동전의 인덱스</param>
        public void LockCoin(int index)
        {
            if (index < 0 || index >= _coinCount)
            {
                Debug.LogError($"CoinManager: Invalid coin index: {index}");
                return;
            }
            
            // 동전 고정
            _lockedCoins[index] = true;
            
            Debug.Log($"CoinManager: Coin {index} locked");
        }
        
        /// <summary>
        /// 동전 위치 교환
        /// </summary>
        /// <param name="index1">첫 번째 동전 인덱스</param>
        /// <param name="index2">두 번째 동전 인덱스</param>
        public void SwapCoins(int index1, int index2)
        {
            if (index1 < 0 || index1 >= _coinCount || index2 < 0 || index2 >= _coinCount)
            {
                Debug.LogError($"CoinManager: Invalid coin indices: {index1}, {index2}");
                return;
            }
            
            // 동전 상태 교환
            bool temp = _coinStates[index1];
            _coinStates[index1] = _coinStates[index2];
            _coinStates[index2] = temp;
            
            // 잠금 상태도 교환
            bool tempLock = _lockedCoins[index1];
            _lockedCoins[index1] = _lockedCoins[index2];
            _lockedCoins[index2] = tempLock;
            
            Debug.Log($"CoinManager: Swapped coins {index1} and {index2}");
        }
        
        /// <summary>
        /// 모든 동전 재던지기
        /// </summary>
        public void RethrowAllCoins()
        {
            Debug.Log("CoinManager: Rethrowing all coins");
            
            // 모든 잠금 해제
            for (int i = 0; i < _coinCount; i++)
            {
                _lockedCoins[i] = false;
            }
            
            // 다시 던지기
            FlipCoins();
        }
        
        /// <summary>
        /// 모든 동전 반전 (앞면 <-> 뒷면)
        /// </summary>
        public void FlipAllCoins()
        {
            Debug.Log("CoinManager: Flipping all coins");
            
            for (int i = 0; i < _coinCount; i++)
            {
                // 잠긴 동전은 반전하지 않음
                if (!_lockedCoins[i])
                {
                    _coinStates[i] = !_coinStates[i];
                    Debug.Log($"CoinManager: Coin {i} flipped to {(_coinStates[i] ? "Heads" : "Tails")}");
                }
                else
                {
                    Debug.Log($"CoinManager: Coin {i} is locked, keeping as {(_coinStates[i] ? "Heads" : "Tails")}");
                }
            }
        }
        
        /// <summary>
        /// 동전 결과 가져오기
        /// </summary>
        /// <returns>동전 상태 배열</returns>
        public bool[] GetCoinStates()
        {
            return _coinStates;
        }
        
        /// <summary>
        /// 동전 결과를 리스트로 가져오기
        /// </summary>
        /// <returns>동전 상태 리스트</returns>
        public List<bool> GetCoinResults()
        {
            List<bool> results = new List<bool>();
            
            for (int i = 0; i < _coinCount; i++)
            {
                results.Add(_coinStates[i]);
            }
            
            return results;
        }
        
        /// <summary>
        /// 동전 개수 설정
        /// </summary>
        /// <param name="count">새 동전 개수</param>
        public void SetCoinCount(int count)
        {
            if (count <= 0)
            {
                Debug.LogError($"CoinManager: Invalid coin count: {count}");
                return;
            }
            
            _coinCount = count;
            InitializeCoinArrays();
            FlipCoins();
            
            Debug.Log($"CoinManager: Coin count set to {count}");
        }
        
        /// <summary>
        /// 동전 개수 가져오기
        /// </summary>
        /// <returns>현재 동전 개수</returns>
        public int GetCoinCount()
        {
            return _coinCount;
        }
        
        /// <summary>
        /// 앞면 동전 개수 계산
        /// </summary>
        /// <returns>앞면 동전 개수</returns>
        public int CountHeads()
        {
            int headCount = 0;
            
            for (int i = 0; i < _coinCount; i++)
            {
                if (_coinStates[i])
                {
                    headCount++;
                }
            }
            
            return headCount;
        }
        
        /// <summary>
        /// 뒷면 동전 개수 계산
        /// </summary>
        /// <returns>뒷면 동전 개수</returns>
        public int CountTails()
        {
            int tailCount = 0;
            
            for (int i = 0; i < _coinCount; i++)
            {
                if (!_coinStates[i])
                {
                    tailCount++;
                }
            }
            
            return tailCount;
        }
        
        /// <summary>
        /// 지정된 인덱스의 코인 상태를 설정합니다.
        /// </summary>
        /// <param name="index">코인 인덱스</param>
        /// <param name="isHeads">앞면(true) 또는 뒷면(false)</param>
        /// <param name="locked">잠금 여부</param>
        public void SetCoinState(int index, bool isHeads, bool locked = false)
        {
            if (index < 0 || index >= _coinCount)
            {
                Debug.LogError($"CoinManager: Invalid coin index: {index}");
                return;
            }
            
            _coinStates[index] = isHeads;
            _lockedCoins[index] = locked;
            
            Debug.Log($"CoinManager: Coin {index} set to {(isHeads ? "Heads" : "Tails")}, locked: {locked}");
        }
        
        /// <summary>
        /// 특정 인덱스의 동전 상태를 가져오기
        /// </summary>
        /// <param name="index">동전 인덱스</param>
        /// <returns>동전 상태 (앞면: true, 뒷면: false)</returns>
        public bool GetCoinState(int index)
        {
            if (index < 0 || index >= _coinCount)
            {
                Debug.LogError($"CoinManager: Invalid coin index: {index}");
                return false;
            }
            
            return _coinStates[index];
        }
        
        /// <summary>
        /// 특정 인덱스의 동전 잠금 상태 확인
        /// </summary>
        /// <param name="index">동전 인덱스</param>
        /// <returns>잠금 상태</returns>
        public bool IsCoinLocked(int index)
        {
            if (index < 0 || index >= _coinCount)
            {
                Debug.LogError($"CoinManager: Invalid coin index: {index}");
                return false;
            }
            
            return _lockedCoins[index];
        }
    }
}