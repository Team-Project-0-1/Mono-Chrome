using UnityEngine;
using UnityEditor;
using System.IO;
using MonoChrome.Setup;

namespace MonoChrome.Editor
{
    /// <summary>
    /// 포트폴리오 품질을 위한 게임 데이터 초기화 도구
    /// </summary>
    public static class DataInitializer
    {
        [MenuItem("MonoChrome/Initialize Game Data")]
        public static void InitializeGameData()
        {
            // Unified entry point
            GameInitializer.Initialize(createMasterGameManager: false, initializeGameData: true);
        }

        public static void GenerateGameData()
        {
            Debug.Log("Starting game data initialization...");

            // 폴더 생성
            CreateDirectories();

            // 캐릭터 데이터 생성
            CreateCharacterData();

            // 패턴 데이터 생성
            CreatePatternData();

            // 데이터 매니저 생성
            CreateDataManagers();

            AssetDatabase.Refresh();
            Debug.Log("Game data initialization completed!");
        }
        
        private static void CreateDirectories()
        {
            string[] directories = {
                "Assets/ScriptableObjects",
                "Assets/ScriptableObjects/Characters",
                "Assets/ScriptableObjects/Patterns",
                "Assets/Resources"
            };
            
            foreach (string dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    Debug.Log($"Created directory: {dir}");
                }
            }
        }
        
        private static void CreateCharacterData()
        {
            // 플레이어 캐릭터들
            CreatePlayerCharacter("김훈희", SenseType.Auditory, "청각으로 세상을 느끼는 엔지니어", 80, 5, 5);
            CreatePlayerCharacter("신제우", SenseType.Olfactory, "후각으로 추적하는 사냥꾼", 70, 6, 4);
            CreatePlayerCharacter("곽장환", SenseType.Tactile, "촉각으로 전투하는 무술가", 75, 2, 8);
            CreatePlayerCharacter("박재석", SenseType.Spiritual, "영적 시야로 보는 수행자", 70, 6, 4);
            
            // 적 캐릭터들
            CreateEnemyCharacter("루멘 리퍼", CharacterType.Normal, 60, 6, 3);
            CreateEnemyCharacter("약탈자1", CharacterType.Normal, 80, 3, 5);
            CreateEnemyCharacter("들개", CharacterType.Normal, 45, 8, 2);
            CreateEnemyCharacter("그림자 수호자", CharacterType.MiniBoss, 120, 10, 6);
            CreateEnemyCharacter("검은 심연", CharacterType.Boss, 200, 15, 8);
            
            Debug.Log("Character data created successfully");
        }
        
        private static void CreatePlayerCharacter(string name, SenseType sense, string desc, int hp, int atk, int def)
        {
            string path = $"Assets/ScriptableObjects/Characters/{name}.asset";
            if (File.Exists(path)) return;
            
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
            
            AssetDatabase.CreateAsset(character, path);
        }
        
        private static void CreateEnemyCharacter(string name, CharacterType type, int hp, int atk, int def)
        {
            string path = $"Assets/ScriptableObjects/Characters/{name}.asset";
            if (File.Exists(path)) return;
            
            CharacterDataSO character = ScriptableObject.CreateInstance<CharacterDataSO>();
            character.characterName = name;
            character.characterType = type;
            character.maxHealth = hp;
            character.baseAttackPower = atk;
            character.baseDefensePower = def;
            character.primaryEffectType = StatusEffectType.Mark;
            character.secondaryEffectType = StatusEffectType.Bleed;
            
            AssetDatabase.CreateAsset(character, path);
        }
        
        private static void CreatePatternData()
        {
            int patternId = 1;
            
            // 기본 패턴들
            CreatePattern("앞면 2연", patternId++, PatternType.Consecutive2, true, true, 2, 0, "기본 공격 패턴");
            CreatePattern("뒷면 2연", patternId++, PatternType.Consecutive2, false, false, 0, 2, "기본 방어 패턴");
            CreatePattern("앞면 3연", patternId++, PatternType.Consecutive3, true, true, 4, 0, "강력한 공격");
            CreatePattern("뒷면 3연", patternId++, PatternType.Consecutive3, false, false, 0, 4, "강력한 방어");
            CreatePattern("앞면 4연", patternId++, PatternType.Consecutive4, true, true, 6, 0, "매우 강력한 공격");
            CreatePattern("뒷면 4연", patternId++, PatternType.Consecutive4, false, false, 0, 6, "매우 강력한 방어");
            CreatePattern("앞면 5연", patternId++, PatternType.Consecutive5, true, true, 8, 0, "최강 공격");
            CreatePattern("뒷면 5연", patternId++, PatternType.Consecutive5, false, false, 0, 8, "최강 방어");
            CreatePattern("앞면 유일", patternId++, PatternType.AllOfOne, true, true, 10, 0, "완전 공격");
            CreatePattern("뒷면 유일", patternId++, PatternType.AllOfOne, false, false, 0, 10, "완전 방어");
            CreatePattern("각성", patternId++, PatternType.Alternating, true, true, 5, 3, "교대 패턴");
            
            Debug.Log("Pattern data created successfully");
        }
        
        private static void CreatePattern(string name, int id, PatternType type, bool value, bool isAttack, int atkBonus, int defBonus, string desc)
        {
            string path = $"Assets/ScriptableObjects/Patterns/{name}.asset";
            if (File.Exists(path)) return;
            
            PatternSO pattern = ScriptableObject.CreateInstance<PatternSO>();
            pattern.patternName = name;
            pattern.id = id;
            pattern.patternType = type;
            pattern.patternValue = value;
            pattern.isAttack = isAttack;
            pattern.attackBonus = atkBonus;
            pattern.defenseBonus = defBonus;
            pattern.description = desc;
            
            AssetDatabase.CreateAsset(pattern, path);
        }
        
        private static void CreateDataManagers()
        {
            // CharacterDataManager 생성
            string charManagerPath = "Assets/Resources/CharacterDataManager.asset";
            if (!File.Exists(charManagerPath))
            {
                CharacterDataManager charManager = ScriptableObject.CreateInstance<CharacterDataManager>();
                AssetDatabase.CreateAsset(charManager, charManagerPath);
                Debug.Log("CharacterDataManager created");
            }
            
            // PatternDataManager 생성
            string patternManagerPath = "Assets/Resources/PatternDataManager.asset";
            if (!File.Exists(patternManagerPath))
            {
                PatternDataManager patternManager = ScriptableObject.CreateInstance<PatternDataManager>();
                AssetDatabase.CreateAsset(patternManager, patternManagerPath);
                Debug.Log("PatternDataManager created");
            }
        }
        
        private static StatusEffectType GetPrimaryEffect(SenseType sense) => sense switch
        {
            SenseType.Auditory => StatusEffectType.Amplify,
            SenseType.Olfactory => StatusEffectType.Mark,
            SenseType.Tactile => StatusEffectType.Counter,
            SenseType.Spiritual => StatusEffectType.Curse,
            _ => StatusEffectType.Amplify
        };
        
        private static StatusEffectType GetSecondaryEffect(SenseType sense) => sense switch
        {
            SenseType.Auditory => StatusEffectType.Resonance,
            SenseType.Olfactory => StatusEffectType.Bleed,
            SenseType.Tactile => StatusEffectType.Crush,
            SenseType.Spiritual => StatusEffectType.Seal,
            _ => StatusEffectType.Resonance
        };
    }
}
