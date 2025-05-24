using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace MonoChrome.Editor
{
    /// <summary>
    /// 포트폴리오 품질을 위한 완전한 게임 데이터 생성 도구
    /// ScriptableObject 기반 데이터 관리 시스템 구축
    /// </summary>
    public class GameDataGenerator : EditorWindow
    {
        #region Window Setup
        [MenuItem("MonoChrome/Game Data Generator")]
        public static void ShowWindow()
        {
            GameDataGenerator window = GetWindow<GameDataGenerator>();
            window.titleContent = new GUIContent("Game Data Generator");
            window.minSize = new Vector2(500, 600);
            window.Show();
        }
        #endregion
        
        #region GUI Variables
        private bool _createCharacters = true;
        private bool _createPatterns = true;
        private bool _createManagers = true;
        private bool _createPrefabs = true;
        private bool _overwriteExisting = false;
        
        private Vector2 _scrollPosition;
        private GUIStyle _headerStyle;
        private GUIStyle _buttonStyle;
        #endregion
        
        #region GUI Drawing
        private void OnGUI()
        {
            InitializeStyles();
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            DrawHeader();
            DrawOptions();
            DrawActions();
            DrawStatus();
            
            EditorGUILayout.EndScrollView();
        }
        
        private void InitializeStyles()
        {
            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(EditorStyles.boldLabel);
                _headerStyle.fontSize = 16;
                _headerStyle.alignment = TextAnchor.MiddleCenter;
            }
            
            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button);
                _buttonStyle.fontSize = 12;
                _buttonStyle.fixedHeight = 30;
            }
        }
        
        private void DrawHeader()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("MONOCHROME: the Eclipse", _headerStyle);
            EditorGUILayout.LabelField("포트폴리오 품질 게임 데이터 생성기", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(20);
            
            EditorGUILayout.HelpBox(
                "이 도구는 ScriptableObject 기반의 완전한 게임 데이터를 생성합니다.\n" +
                "캐릭터, 패턴, 매니저, UI 프리팹을 모두 자동으로 생성하여\n" +
                "포트폴리오 품질의 데이터 관리 시스템을 구축합니다.", 
                MessageType.Info
            );
            GUILayout.Space(10);
        }
        
        private void DrawOptions()
        {
            EditorGUILayout.LabelField("생성 옵션", EditorStyles.boldLabel);
            
            _createCharacters = EditorGUILayout.Toggle("캐릭터 데이터 생성", _createCharacters);
            _createPatterns = EditorGUILayout.Toggle("패턴 데이터 생성", _createPatterns);
            _createManagers = EditorGUILayout.Toggle("매니저 데이터 생성", _createManagers);
            _createPrefabs = EditorGUILayout.Toggle("UI 프리팹 생성", _createPrefabs);
            
            GUILayout.Space(10);
            _overwriteExisting = EditorGUILayout.Toggle("기존 파일 덮어쓰기", _overwriteExisting);
            GUILayout.Space(20);
        }
        
        private void DrawActions()
        {
            EditorGUILayout.LabelField("실행", EditorStyles.boldLabel);
            
            if (GUILayout.Button("모든 데이터 생성", _buttonStyle))
            {
                GenerateAllData();
            }
            
            GUILayout.Space(10);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("캐릭터만 생성"))
            {
                GenerateCharacterData();
            }
            if (GUILayout.Button("패턴만 생성"))
            {
                GeneratePatternData();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("매니저만 생성"))
            {
                GenerateManagerData();
            }
            if (GUILayout.Button("프리팹만 생성"))
            {
                GenerateUIPrefabs();
            }
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(20);
        }
        
        private void DrawStatus()
        {
            EditorGUILayout.LabelField("상태", EditorStyles.boldLabel);
            
            if (GUILayout.Button("생성된 데이터 확인"))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>("Assets/ScriptableObjects");
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            
            if (GUILayout.Button("Resources 폴더 확인"))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>("Assets/Resources");
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
        }
        #endregion
        
        #region Data Generation
        /// <summary>
        /// 모든 데이터 생성
        /// </summary>
        private void GenerateAllData()
        {
            Debug.Log("GameDataGenerator: 모든 데이터 생성 시작");
            
            try
            {
                CreateDirectories();
                
                if (_createCharacters)
                    GenerateCharacterData();
                
                if (_createPatterns)
                    GeneratePatternData();
                
                if (_createManagers)
                    GenerateManagerData();
                    
                if (_createPrefabs)
                    GenerateUIPrefabs();
                
                AssetDatabase.Refresh();
                Debug.Log("GameDataGenerator: 모든 데이터 생성 완료!");
                
                EditorUtility.DisplayDialog("완료", "모든 게임 데이터가 성공적으로 생성되었습니다!", "확인");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"GameDataGenerator: 데이터 생성 중 오류 발생 - {ex.Message}");
                EditorUtility.DisplayDialog("오류", $"데이터 생성 중 오류가 발생했습니다:\n{ex.Message}", "확인");
            }
        }
        
        /// <summary>
        /// 디렉토리 생성
        /// </summary>
        private void CreateDirectories()
        {
            string[] directories = {
                "Assets/ScriptableObjects",
                "Assets/ScriptableObjects/Characters",
                "Assets/ScriptableObjects/Characters/Players",
                "Assets/ScriptableObjects/Characters/Enemies",
                "Assets/ScriptableObjects/Patterns",
                "Assets/ScriptableObjects/Patterns/Basic",
                "Assets/ScriptableObjects/Patterns/Auditory",
                "Assets/ScriptableObjects/Patterns/Olfactory",
                "Assets/ScriptableObjects/Patterns/Tactile",
                "Assets/ScriptableObjects/Patterns/Spiritual",
                "Assets/ScriptableObjects/Managers",
                "Assets/Resources",
                "Assets/Prefabs",
                "Assets/Prefabs/UI",
                "Assets/Prefabs/UI/Combat"
            };
            
            foreach (string dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    Debug.Log($"GameDataGenerator: 폴더 생성 - {dir}");
                }
            }
        }
        
        /// <summary>
        /// 캐릭터 데이터 생성
        /// </summary>
        private void GenerateCharacterData()
        {
            Debug.Log("GameDataGenerator: 캐릭터 데이터 생성 시작");
            
            // 플레이어 캐릭터들
            CreatePlayerCharacter("김훈희", SenseType.Auditory, "청각으로 세상을 느끼는 엔지니어", 80, 5, 5, "기합", "동전 전체 재던지기 + 증폭 스택 +1");
            CreatePlayerCharacter("신제우", SenseType.Olfactory, "후각으로 추적하는 사냥꾼", 70, 6, 4, "속임수", "동전 1개 뒤집기 + 연격 +3");
            CreatePlayerCharacter("곽장환", SenseType.Tactile, "촉각으로 전투하는 무술가", 75, 2, 8, "불괴", "동전 1개 고정 유지 + 반격 +3");
            CreatePlayerCharacter("박재석", SenseType.Spiritual, "영적 시야로 보는 수행자", 70, 6, 4, "주문 배치", "동전 2개 위치 교환 + 저주 +2");
            
            // 일반 적들
            CreateEnemyCharacter("루멘 리퍼", CharacterType.Normal, "빛을 거두는 은밀한 수확자", 60, 6, 3, StatusEffectType.Mark, StatusEffectType.Bleed);
            CreateEnemyCharacter("약탈자1", CharacterType.Normal, "폐허 속 격동의 시기에 살아남은 약탈자", 80, 3, 5, StatusEffectType.Amplify, StatusEffectType.Resonance);
            CreateEnemyCharacter("들개", CharacterType.Normal, "한 때 우리의 반려, 지금은 굶주린 사냥꾼", 45, 8, 2, StatusEffectType.Counter, StatusEffectType.Crush);
            
            // 미니보스
            CreateEnemyCharacter("그림자 수호자", CharacterType.MiniBoss, "어둠 속에서 영역을 지키는 수호자", 120, 10, 6, StatusEffectType.Curse, StatusEffectType.Seal);
            
            // 보스
            CreateEnemyCharacter("검은 심연", CharacterType.Boss, "모든 빛을 삼키는 최종 포식자", 200, 15, 8, StatusEffectType.Curse, StatusEffectType.Seal);
            
            Debug.Log("GameDataGenerator: 캐릭터 데이터 생성 완료");
        }
        
        /// <summary>
        /// 플레이어 캐릭터 생성
        /// </summary>
        private void CreatePlayerCharacter(string name, SenseType sense, string desc, int hp, int atk, int def, string skillName, string skillDesc)
        {
            string path = $"Assets/ScriptableObjects/Characters/Players/{name}.asset";
            if (!_overwriteExisting && File.Exists(path)) return;
            
            CharacterDataSO character = ScriptableObject.CreateInstance<CharacterDataSO>();
            character.characterName = name;
            character.description = desc;
            character.characterType = CharacterType.Player;
            character.senseType = sense;
            character.maxHealth = hp;
            character.baseAttackPower = atk;
            character.baseDefensePower = def;
            character.primaryEffectType = GetPrimaryEffect(sense);
            character.secondaryEffectType = GetSecondaryEffect(sense);
            character.skillName = skillName;
            character.skillDescription = skillDesc;
            character.skillType = GetSkillType(sense);
            
            AssetDatabase.CreateAsset(character, path);
            Debug.Log($"GameDataGenerator: 플레이어 캐릭터 생성 - {name}");
        }
        
        /// <summary>
        /// 적 캐릭터 생성
        /// </summary>
        private void CreateEnemyCharacter(string name, CharacterType type, string desc, int hp, int atk, int def, StatusEffectType primary, StatusEffectType secondary)
        {
            string folder = type == CharacterType.Normal ? "Enemies" : "Enemies";
            string path = $"Assets/ScriptableObjects/Characters/{folder}/{name}.asset";
            if (!_overwriteExisting && File.Exists(path)) return;
            
            CharacterDataSO character = ScriptableObject.CreateInstance<CharacterDataSO>();
            character.characterName = name;
            character.description = desc;
            character.characterType = type;
            character.maxHealth = hp;
            character.baseAttackPower = atk;
            character.baseDefensePower = def;
            character.primaryEffectType = primary;
            character.secondaryEffectType = secondary;
            character.isEnemyBoss = type != CharacterType.Normal;
            
            AssetDatabase.CreateAsset(character, path);
            Debug.Log($"GameDataGenerator: 적 캐릭터 생성 - {name} ({type})");
        }
        
        /// <summary>
        /// 패턴 데이터 생성
        /// </summary>
        private void GeneratePatternData()
        {
            Debug.Log("GameDataGenerator: 패턴 데이터 생성 시작");
            
            int patternId = 1;
            
            // 기본 패턴들
            CreatePattern("앞면 2연", patternId++, PatternType.Consecutive2, true, true, 2, 0, "기본적인 공격 패턴", "Basic");
            CreatePattern("뒷면 2연", patternId++, PatternType.Consecutive2, false, false, 0, 2, "기본적인 방어 패턴", "Basic");
            CreatePattern("앞면 3연", patternId++, PatternType.Consecutive3, true, true, 4, 0, "강력한 공격 패턴", "Basic");
            CreatePattern("뒷면 3연", patternId++, PatternType.Consecutive3, false, false, 0, 4, "강력한 방어 패턴", "Basic");
            CreatePattern("앞면 4연", patternId++, PatternType.Consecutive4, true, true, 6, 0, "매우 강력한 공격", "Basic");
            CreatePattern("뒷면 4연", patternId++, PatternType.Consecutive4, false, false, 0, 6, "매우 강력한 방어", "Basic");
            CreatePattern("앞면 5연", patternId++, PatternType.Consecutive5, true, true, 8, 0, "최강 공격", "Basic");
            CreatePattern("뒷면 5연", patternId++, PatternType.Consecutive5, false, false, 0, 8, "최강 방어", "Basic");
            CreatePattern("앞면 유일", patternId++, PatternType.AllOfOne, true, true, 10, 0, "완전 공격", "Basic");
            CreatePattern("뒷면 유일", patternId++, PatternType.AllOfOne, false, false, 0, 10, "완전 방어", "Basic");
            CreatePattern("각성", patternId++, PatternType.Alternating, true, true, 5, 3, "균형잡힌 교대 패턴", "Basic");
            
            // 감각별 특수 패턴들
            GenerateSenseSpecificPatterns(ref patternId);
            
            Debug.Log("GameDataGenerator: 패턴 데이터 생성 완료");
        }
        
        /// <summary>
        /// 감각별 특수 패턴 생성
        /// </summary>
        private void GenerateSenseSpecificPatterns(ref int patternId)
        {
            // 청각 패턴
            CreatePattern("진동 타격", patternId++, PatternType.Consecutive2, true, true, 3, 0, "적에게 피해 + 증폭 +1", "Auditory");
            CreatePattern("잔향 방어", patternId++, PatternType.Consecutive2, false, false, 0, 3, "다음 턴 기본 공격 시 증폭 추가 +2", "Auditory");
            CreatePattern("연쇄 타격", patternId++, PatternType.Consecutive3, true, true, 5, 0, "증폭 1 소모 후 적 2연타", "Auditory");
            CreatePattern("공진", patternId++, PatternType.AllOfOne, true, true, 0, 0, "증폭 전부 소모 → 수치 x2 만큼 피해", "Auditory");
            
            // 후각 패턴
            CreatePattern("수리검 투척", patternId++, PatternType.Consecutive2, true, true, 2, 0, "2회 공격 + 적에게 표식 1", "Olfactory");
            CreatePattern("후각 제어", patternId++, PatternType.Consecutive2, false, false, 0, 2, "방어 +1, 적의 표식 수치 유지", "Olfactory");
            CreatePattern("궤적 분열", patternId++, PatternType.Consecutive3, true, true, 3, 0, "피해 1 × 3회. 표식 보유 시 마지막 타격 +1", "Olfactory");
            CreatePattern("절멸", patternId++, PatternType.Alternating, true, true, 5, 0, "표식 있는 적에게 5회 공격 후 표식 초기화", "Olfactory");
            
            // 촉각 패턴
            CreatePattern("반격 자세", patternId++, PatternType.Consecutive2, false, false, 0, 2, "다음 턴에 자신에게 반격 +2", "Tactile");
            CreatePattern("분쇄타", patternId++, PatternType.Consecutive3, true, true, 5, 0, "강력한 공격 + 분쇄 +1", "Tactile");
            CreatePattern("저리 꺼져", patternId++, PatternType.Consecutive4, false, false, 0, 4, "자신에게 즉시 반격 +4", "Tactile");
            
            // 영혼 패턴
            CreatePattern("저주", patternId++, PatternType.Consecutive2, true, true, 2, 0, "공격 + 적에게 저주 +2", "Spiritual");
            CreatePattern("봉인", patternId++, PatternType.Consecutive3, true, true, 3, 0, "공격 + 봉인 +1", "Spiritual");
            CreatePattern("침식의 낫", patternId++, PatternType.Consecutive2, true, true, 1, 0, "1회 공격 + 적에게 저주 2", "Spiritual");
            CreatePattern("수확", patternId++, PatternType.Consecutive4, true, true, 0, 0, "현재 부여한 저주만큼 봉인", "Spiritual");
        }
        
        /// <summary>
        /// 패턴 생성
        /// </summary>
        private void CreatePattern(string name, int id, PatternType type, bool value, bool isAttack, int atkBonus, int defBonus, string desc, string category)
        {
            string path = $"Assets/ScriptableObjects/Patterns/{category}/{name}.asset";
            if (!_overwriteExisting && File.Exists(path)) return;
            
            PatternSO pattern = ScriptableObject.CreateInstance<PatternSO>();
            pattern.patternName = name;
            pattern.id = id;
            pattern.patternType = type;
            pattern.patternValue = value;
            pattern.isAttack = isAttack;
            pattern.attackBonus = atkBonus;
            pattern.defenseBonus = defBonus;
            pattern.description = desc;
            pattern.specialEffect = desc;
            
            // 감각 타입 설정
            if (category != "Basic")
            {
                SenseType senseType = GetSenseTypeFromCategory(category);
                pattern.applicableSenseTypes = new SenseType[] { senseType };
            }
            
            AssetDatabase.CreateAsset(pattern, path);
        }
        
        /// <summary>
        /// 매니저 데이터 생성
        /// </summary>
        private void GenerateManagerData()
        {
            Debug.Log("GameDataGenerator: 매니저 데이터 생성 시작");
            
            // CharacterDataManager 생성
            CreateCharacterDataManager();
            
            // PatternDataManager 생성
            CreatePatternDataManager();
            
            Debug.Log("GameDataGenerator: 매니저 데이터 생성 완료");
        }
        
        /// <summary>
        /// CharacterDataManager 생성
        /// </summary>
        private void CreateCharacterDataManager()
        {
            string path = "Assets/Resources/CharacterDataManager.asset";
            if (!_overwriteExisting && File.Exists(path)) return;
            
            CharacterDataManager manager = ScriptableObject.CreateInstance<CharacterDataManager>();
            AssetDatabase.CreateAsset(manager, path);
            
            Debug.Log("GameDataGenerator: CharacterDataManager 생성 완료");
            Debug.Log("수동 작업 필요: 인스펙터에서 캐릭터 데이터들을 할당하세요.");
        }
        
        /// <summary>
        /// PatternDataManager 생성
        /// </summary>
        private void CreatePatternDataManager()
        {
            string path = "Assets/Resources/PatternDataManager.asset";
            if (!_overwriteExisting && File.Exists(path)) return;
            
            PatternDataManager manager = ScriptableObject.CreateInstance<PatternDataManager>();
            AssetDatabase.CreateAsset(manager, path);
            
            Debug.Log("GameDataGenerator: PatternDataManager 생성 완료");
            Debug.Log("수동 작업 필요: 인스펙터에서 패턴 데이터들을 할당하세요.");
        }
        
        /// <summary>
        /// UI 프리팹 생성
        /// </summary>
        private void GenerateUIPrefabs()
        {
            Debug.Log("GameDataGenerator: UI 프리팹 생성 시작");
            
            // 이 부분은 실제로는 Scene에서 수동으로 만들어야 하지만
            // 가이드라인을 제공합니다.
            Debug.Log("UI 프리팹은 다음과 같이 구성하세요:");
            Debug.Log("1. CoinPrefab: Image + CoinUI 컴포넌트");
            Debug.Log("2. PatternButtonPrefab: Button + Text + PatternButtonUI 컴포넌트");
            Debug.Log("3. StatusEffectPrefab: Image + Text + StatusEffectUI 컴포넌트");
            Debug.Log("4. CombatUI Canvas: CombatUIController 컴포넌트와 모든 정적 UI 요소들");
            
            Debug.Log("GameDataGenerator: UI 프리팹 가이드 완료");
        }
        #endregion
        
        #region Helper Methods
        private StatusEffectType GetPrimaryEffect(SenseType sense) => sense switch
        {
            SenseType.Auditory => StatusEffectType.Amplify,
            SenseType.Olfactory => StatusEffectType.Mark,
            SenseType.Tactile => StatusEffectType.Counter,
            SenseType.Spiritual => StatusEffectType.Curse,
            _ => StatusEffectType.Amplify
        };
        
        private StatusEffectType GetSecondaryEffect(SenseType sense) => sense switch
        {
            SenseType.Auditory => StatusEffectType.Resonance,
            SenseType.Olfactory => StatusEffectType.Bleed,
            SenseType.Tactile => StatusEffectType.Crush,
            SenseType.Spiritual => StatusEffectType.Seal,
            _ => StatusEffectType.Resonance
        };
        
        private ActiveSkillType GetSkillType(SenseType sense) => sense switch
        {
            SenseType.Auditory => ActiveSkillType.RethrowAll,
            SenseType.Olfactory => ActiveSkillType.FlipOne,
            SenseType.Tactile => ActiveSkillType.LockOne,
            SenseType.Spiritual => ActiveSkillType.SwapTwo,
            _ => ActiveSkillType.RethrowAll
        };
        
        private SenseType GetSenseTypeFromCategory(string category) => category switch
        {
            "Auditory" => SenseType.Auditory,
            "Olfactory" => SenseType.Olfactory,
            "Tactile" => SenseType.Tactile,
            "Spiritual" => SenseType.Spiritual,
            _ => SenseType.Auditory
        };
        #endregion
    }
}
