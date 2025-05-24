using UnityEngine;
using UnityEditor;
using System.IO;

namespace MonoChrome.Editor
{
    /// <summary>
    /// 게임 데이터 생성을 위한 에디터 툴
    /// 포트폴리오 품질을 위한 완전한 데이터 세트를 자동 생성합니다.
    /// </summary>
    public class GameDataCreator : EditorWindow
    {
        private bool _createCharacters = true;
        private bool _createPatterns = true;
        private bool _createManagers = true;
        private bool _overwriteExisting = false;

        [MenuItem("MonoChrome/Game Data Creator")]
        public static void ShowWindow()
        {
            GameDataCreator window = GetWindow<GameDataCreator>();
            window.titleContent = new GUIContent("Game Data Creator");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("MonoChrome 게임 데이터 생성기", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox("포트폴리오 품질의 완전한 게임 데이터를 생성합니다.", MessageType.Info);
            GUILayout.Space(10);

            _createCharacters = EditorGUILayout.Toggle("캐릭터 데이터 생성", _createCharacters);
            _createPatterns = EditorGUILayout.Toggle("패턴 데이터 생성", _createPatterns);
            _createManagers = EditorGUILayout.Toggle("데이터 매니저 생성", _createManagers);
            GUILayout.Space(10);

            _overwriteExisting = EditorGUILayout.Toggle("기존 파일 덮어쓰기", _overwriteExisting);
            
            GUILayout.Space(20);

            if (GUILayout.Button("모든 데이터 생성", GUILayout.Height(30)))
            {
                CreateAllGameData();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("ScriptableObjects 폴더에서 확인"))
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>("Assets/ScriptableObjects");
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
        }

        private void CreateAllGameData()
        {
            // 폴더 생성
            CreateDirectories();

            if (_createCharacters)
            {
                CreateCharacterData();
            }

            if (_createPatterns)
            {
                CreatePatternData();
            }

            if (_createManagers)
            {
                CreateDataManagers();
            }

            AssetDatabase.Refresh();
            Debug.Log("게임 데이터 생성 완료!");
        }

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
                "Assets/ScriptableObjects/Patterns/Enemy",
                "Assets/ScriptableObjects/Managers",
                "Assets/Resources"
            };

            foreach (string dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
        }

        private void CreateCharacterData()
        {
            // 플레이어 캐릭터 생성
            CreatePlayerCharacter("김훈희", SenseType.Auditory, "청각으로 세상을 느끼는 엔지니어");
            CreatePlayerCharacter("신제우", SenseType.Olfactory, "후각으로 추적하는 사냥꾼");
            CreatePlayerCharacter("곽장환", SenseType.Tactile, "촉각으로 전투하는 무술가");
            CreatePlayerCharacter("박재석", SenseType.Spiritual, "영적 시야로 보는 수행자");

            // 일반 적 생성
            CreateEnemyCharacter("루멘 리퍼", CharacterType.Normal, StatusEffectType.Mark, StatusEffectType.Bleed, 60, 6, 3);
            CreateEnemyCharacter("약탈자1", CharacterType.Normal, StatusEffectType.Amplify, StatusEffectType.Resonance, 80, 3, 5);
            CreateEnemyCharacter("들개", CharacterType.Normal, StatusEffectType.Counter, StatusEffectType.Crush, 45, 8, 2);

            // 미니보스 생성
            CreateEnemyCharacter("그림자 수호자", CharacterType.MiniBoss, StatusEffectType.Curse, StatusEffectType.Seal, 120, 10, 6);

            // 보스 생성
            CreateEnemyCharacter("검은 심연", CharacterType.Boss, StatusEffectType.Curse, StatusEffectType.Seal, 200, 15, 8);

            Debug.Log("캐릭터 데이터 생성 완료");
        }

        private void CreatePlayerCharacter(string name, SenseType senseType, string description)
        {
            string path = $"Assets/ScriptableObjects/Characters/Players/{name}.asset";
            if (!_overwriteExisting && File.Exists(path)) return;

            CharacterDataSO character = CreateInstance<CharacterDataSO>();
            character.characterName = name;
            character.description = description;
            character.characterType = CharacterType.Player;
            character.senseType = senseType;
            character.maxHealth = GetPlayerHealth(senseType);
            character.baseAttackPower = GetPlayerAttack(senseType);
            character.baseDefensePower = GetPlayerDefense(senseType);
            character.primaryEffectType = GetPrimaryEffect(senseType);
            character.secondaryEffectType = GetSecondaryEffect(senseType);
            character.skillName = GetSkillName(senseType);
            character.skillDescription = GetSkillDescription(senseType);
            character.skillType = GetSkillType(senseType);

            AssetDatabase.CreateAsset(character, path);
        }

        private void CreateEnemyCharacter(string name, CharacterType type, StatusEffectType primary, StatusEffectType secondary, int hp, int atk, int def)
        {
            string folder = type == CharacterType.Normal ? "Enemies" : type == CharacterType.MiniBoss ? "Enemies" : "Enemies";
            string path = $"Assets/ScriptableObjects/Characters/{folder}/{name}.asset";
            if (!_overwriteExisting && File.Exists(path)) return;

            CharacterDataSO character = CreateInstance<CharacterDataSO>();
            character.characterName = name;
            character.characterType = type;
            character.maxHealth = hp;
            character.baseAttackPower = atk;
            character.baseDefensePower = def;
            character.primaryEffectType = primary;
            character.secondaryEffectType = secondary;
            character.isEnemyBoss = type != CharacterType.Normal;

            AssetDatabase.CreateAsset(character, path);
        }

        private void CreatePatternData()
        {
            // 기본 패턴 생성
            CreateBasicPattern("앞면 2연", PatternType.Consecutive2, true, true, 2, 0, "기본적인 공격 패턴");
            CreateBasicPattern("뒷면 2연", PatternType.Consecutive2, false, false, 0, 2, "기본적인 방어 패턴");
            CreateBasicPattern("앞면 3연", PatternType.Consecutive3, true, true, 4, 0, "강력한 공격 패턴");
            CreateBasicPattern("뒷면 3연", PatternType.Consecutive3, false, false, 0, 4, "강력한 방어 패턴");

            // 감각별 특수 패턴 생성
            CreateSensePatterns();

            Debug.Log("패턴 데이터 생성 완료");
        }

        private void CreateBasicPattern(string name, PatternType type, bool value, bool isAttack, int atkBonus, int defBonus, string desc)
        {
            string path = $"Assets/ScriptableObjects/Patterns/Basic/{name}.asset";
            if (!_overwriteExisting && File.Exists(path)) return;

            PatternSO pattern = CreateInstance<PatternSO>();
            pattern.patternName = name;
            pattern.description = desc;
            pattern.id = GetNextPatternId();
            pattern.patternType = type;
            pattern.patternValue = value;
            pattern.isAttack = isAttack;
            pattern.attackBonus = atkBonus;
            pattern.defenseBonus = defBonus;

            AssetDatabase.CreateAsset(pattern, path);
        }

        private void CreateSensePatterns()
        {
            // 청각 패턴
            CreateSensePattern("진동 타격", SenseType.Auditory, PatternType.Consecutive2, true, true, 3, 0, "증폭 +1");
            CreateSensePattern("잔향 방어", SenseType.Auditory, PatternType.Consecutive2, false, false, 0, 3, "증폭 +2 (다음 턴)");

            // 후각 패턴
            CreateSensePattern("수리검 투척", SenseType.Olfactory, PatternType.Consecutive2, true, true, 2, 0, "표식 +1");
            CreateSensePattern("후각 제어", SenseType.Olfactory, PatternType.Consecutive2, false, false, 0, 2, "표식 유지");

            // 촉각 패턴
            CreateSensePattern("반격 자세", SenseType.Tactile, PatternType.Consecutive2, false, false, 0, 2, "반격 +2");
            CreateSensePattern("분쇄타", SenseType.Tactile, PatternType.Consecutive3, true, true, 5, 0, "분쇄 +1");

            // 영혼 패턴
            CreateSensePattern("저주", SenseType.Spiritual, PatternType.Consecutive2, true, true, 2, 0, "저주 +2");
            CreateSensePattern("봉인", SenseType.Spiritual, PatternType.Consecutive3, true, true, 3, 0, "봉인 +1");
        }

        private void CreateSensePattern(string name, SenseType sense, PatternType type, bool value, bool isAttack, int atkBonus, int defBonus, string effect)
        {
            string senseFolder = sense.ToString();
            string path = $"Assets/ScriptableObjects/Patterns/{senseFolder}/{name}.asset";
            if (!_overwriteExisting && File.Exists(path)) return;

            PatternSO pattern = CreateInstance<PatternSO>();
            pattern.patternName = name;
            pattern.description = effect;
            pattern.id = GetNextPatternId();
            pattern.patternType = type;
            pattern.patternValue = value;
            pattern.isAttack = isAttack;
            pattern.attackBonus = atkBonus;
            pattern.defenseBonus = defBonus;
            pattern.specialEffect = effect;
            pattern.applicableSenseTypes = new SenseType[] { sense };

            AssetDatabase.CreateAsset(pattern, path);
        }

        private void CreateDataManagers()
        {
            // CharacterDataManager 생성
            CreateCharacterDataManager();
            
            // PatternDataManager 생성
            CreatePatternDataManager();

            Debug.Log("데이터 매니저 생성 완료");
        }

        private void CreateCharacterDataManager()
        {
            string path = "Assets/Resources/CharacterDataManager.asset";
            if (!_overwriteExisting && File.Exists(path)) return;

            CharacterDataManager manager = CreateInstance<CharacterDataManager>();
            AssetDatabase.CreateAsset(manager, path);

            // 에디터에서 참조 설정 (실제로는 인스펙터에서 수동 설정 권장)
            Debug.Log("CharacterDataManager 생성됨. 인스펙터에서 캐릭터 데이터를 할당하세요.");
        }

        private void CreatePatternDataManager()
        {
            string path = "Assets/Resources/PatternDataManager.asset";
            if (!_overwriteExisting && File.Exists(path)) return;

            PatternDataManager manager = CreateInstance<PatternDataManager>();
            AssetDatabase.CreateAsset(manager, path);

            Debug.Log("PatternDataManager 생성됨. 인스펙터에서 패턴 데이터를 할당하세요.");
        }

        // 헬퍼 메서드들
        private int GetPlayerHealth(SenseType sense) => sense switch
        {
            SenseType.Auditory => 80,
            SenseType.Olfactory => 70,
            SenseType.Tactile => 75,
            SenseType.Spiritual => 70,
            _ => 75
        };

        private int GetPlayerAttack(SenseType sense) => sense switch
        {
            SenseType.Auditory => 5,
            SenseType.Olfactory => 6,
            SenseType.Tactile => 2,
            SenseType.Spiritual => 6,
            _ => 5
        };

        private int GetPlayerDefense(SenseType sense) => sense switch
        {
            SenseType.Auditory => 5,
            SenseType.Olfactory => 4,
            SenseType.Tactile => 8,
            SenseType.Spiritual => 4,
            _ => 5
        };

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

        private string GetSkillName(SenseType sense) => sense switch
        {
            SenseType.Auditory => "기합",
            SenseType.Olfactory => "속임수",
            SenseType.Tactile => "불괴",
            SenseType.Spiritual => "주문 배치",
            _ => "기본 스킬"
        };

        private string GetSkillDescription(SenseType sense) => sense switch
        {
            SenseType.Auditory => "동전 전체 재던지기 + 증폭 스택 +1",
            SenseType.Olfactory => "동전 1개 뒤집기 + 연격 +3",
            SenseType.Tactile => "동전 1개 고정 유지 + 반격 +3",
            SenseType.Spiritual => "동전 2개 위치 교환 + 저주 +2",
            _ => "기본 스킬"
        };

        private ActiveSkillType GetSkillType(SenseType sense) => sense switch
        {
            SenseType.Auditory => ActiveSkillType.RethrowAll,
            SenseType.Olfactory => ActiveSkillType.FlipOne,
            SenseType.Tactile => ActiveSkillType.LockOne,
            SenseType.Spiritual => ActiveSkillType.SwapTwo,
            _ => ActiveSkillType.RethrowAll
        };

        private static int _patternIdCounter = 1;
        private int GetNextPatternId() => _patternIdCounter++;
    }
}
