#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MonoChrome
{
    /// <summary>
    /// 캐릭터 데이터 ScriptableObject 생성 편의 도구
    /// </summary>
    public class CharacterDataSOCreator : EditorWindow
    {
        private string baseFolderPath = "Assets/Resources/Characters";
        
        [MenuItem("Tools/MonoChrome/Create Character Data ScriptableObjects")]
        public static void ShowWindow()
        {
            GetWindow<CharacterDataSOCreator>("캐릭터 데이터 ScriptableObject 생성기");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("캐릭터 데이터 ScriptableObject 생성 도구", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            baseFolderPath = EditorGUILayout.TextField("캐릭터 데이터 저장 경로", baseFolderPath);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("리소스 폴더 생성"))
            {
                CreateResourceFolder();
            }
            
            EditorGUILayout.Space();
            
            GUILayout.Label("플레이어 캐릭터 생성", EditorStyles.boldLabel);
            
            if (GUILayout.Button("청각 플레이어 캐릭터 생성"))
            {
                CreateAuditoryPlayerCharacter();
            }
            
            if (GUILayout.Button("후각 플레이어 캐릭터 생성"))
            {
                CreateOlfactoryPlayerCharacter();
            }
            
            if (GUILayout.Button("촉각 플레이어 캐릭터 생성"))
            {
                CreateTactilePlayerCharacter();
            }
            
            if (GUILayout.Button("영적 플레이어 캐릭터 생성"))
            {
                CreateSpiritualPlayerCharacter();
            }
            
            EditorGUILayout.Space();
            
            GUILayout.Label("몬스터 캐릭터 생성", EditorStyles.boldLabel);
            
            if (GUILayout.Button("일반 몬스터 생성"))
            {
                CreateNormalMonster();
            }
            
            if (GUILayout.Button("루멘 리퍼 몬스터 생성"))
            {
                CreateLumenReaperMonster();
            }
            
            if (GUILayout.Button("엘리트 몬스터 생성"))
            {
                CreateEliteMonster();
            }
        }
        
        private void CreateResourceFolder()
        {
            System.IO.Directory.CreateDirectory(baseFolderPath);
            AssetDatabase.Refresh();
            
            // 세부 폴더 생성
            System.IO.Directory.CreateDirectory($"{baseFolderPath}/Player");
            System.IO.Directory.CreateDirectory($"{baseFolderPath}/Enemy");
            
            AssetDatabase.Refresh();
            
            Debug.Log("캐릭터 데이터 리소스 폴더가 생성되었습니다.");
        }
        
        #region 플레이어 캐릭터 생성
        private void CreateAuditoryPlayerCharacter()
        {
            // 패턴 세트 로드
            PatternSetSO patternSet = FindPatternSet("PlayerAuditorySet");
            
            // 캐릭터 데이터 생성
            CharacterDataSO characterData = CreateCharacterDataSO(
                "PlayerAuditory",
                "소리를 듣는 자",
                "청각을 통해 세상을 인식하고, 진동을 증폭시켜 공격하는 능력을 가진 생존자입니다.",
                CharacterType.Player,
                SenseType.Auditory,
                100, 10, 5, 3,
                StatusEffectType.Amplify,
                StatusEffectType.Resonance,
                patternSet,
                "기합",
                "모든 동전을 다시 던지고 증폭 스택을 1 증가시킵니다.",
                ActiveSkillType.RethrowAll
            );
            
            Debug.Log("청각 플레이어 캐릭터가 생성되었습니다.");
        }
        
        private void CreateOlfactoryPlayerCharacter()
        {
            // 패턴 세트 로드
            PatternSetSO patternSet = FindPatternSet("PlayerOlfactorySet");
            
            // 캐릭터 데이터 생성
            CharacterDataSO characterData = CreateCharacterDataSO(
                "PlayerOlfactory",
                "냄새를 맡는 자",
                "후각을 통해 세상을 인식하고, 적을 표식하여 추적하는 능력을 가진 생존자입니다.",
                CharacterType.Player,
                SenseType.Olfactory,
                90, 12, 4, 3,
                StatusEffectType.Mark,
                StatusEffectType.Bleed,
                patternSet,
                "속임수",
                "동전 1개를 선택해 뒤집고 표식 스택을 3 증가시킵니다.",
                ActiveSkillType.FlipOne
            );
            
            Debug.Log("후각 플레이어 캐릭터가 생성되었습니다.");
        }
        
        private void CreateTactilePlayerCharacter()
        {
            // 패턴 세트 로드
            PatternSetSO patternSet = FindPatternSet("PlayerTactileSet");
            
            // 캐릭터 데이터 생성
            CharacterDataSO characterData = CreateCharacterDataSO(
                "PlayerTactile",
                "만지는 자",
                "촉각을 통해 세상을 인식하고, 적의 공격을 감지하여 반격하는 능력을 가진 생존자입니다.",
                CharacterType.Player,
                SenseType.Tactile,
                110, 8, 8, 3,
                StatusEffectType.Counter,
                StatusEffectType.Crush,
                patternSet,
                "불괴",
                "동전 1개를 선택해 고정하고 반격 스택을 3 증가시킵니다.",
                ActiveSkillType.LockOne
            );
            
            Debug.Log("촉각 플레이어 캐릭터가 생성되었습니다.");
        }
        
        private void CreateSpiritualPlayerCharacter()
        {
            // 패턴 세트 로드
            PatternSetSO patternSet = FindPatternSet("PlayerSpiritualSet");
            
            // 캐릭터 데이터 생성
            CharacterDataSO characterData = CreateCharacterDataSO(
                "PlayerSpiritual",
                "영혼을 보는 자",
                "영적 감각을 통해 세상을 인식하고, 저주와 봉인으로 적을 약화시키는 능력을 가진 생존자입니다.",
                CharacterType.Player,
                SenseType.Spiritual,
                80, 11, 3, 3,
                StatusEffectType.Curse,
                StatusEffectType.Seal,
                patternSet,
                "주문 배치",
                "동전 2개의 위치를 교환하고 저주 스택을 2 증가시킵니다.",
                ActiveSkillType.SwapTwo
            );
            
            Debug.Log("영적 플레이어 캐릭터가 생성되었습니다.");
        }
        #endregion
        
        #region 몬스터 캐릭터 생성
        private void CreateNormalMonster()
        {
            // 패턴 세트 로드
            PatternSetSO patternSet = FindPatternSet("NormalMonsterSet");
            
            // 캐릭터 데이터 생성
            CharacterDataSO characterData = CreateCharacterDataSO(
                "NormalMonster",
                "그림자 괴물",
                "빛을 먹는 일반적인 괴물입니다. 약한 공격력과 방어력을 가지고 있습니다.",
                CharacterType.Normal,
                SenseType.Auditory, // 몬스터는 감각 타입이 무의미
                50, 5, 3, 0,
                StatusEffectType.None,
                StatusEffectType.None,
                patternSet
            );
            
            Debug.Log("일반 몬스터가 생성되었습니다.");
        }
        
        private void CreateLumenReaperMonster()
        {
            // 패턴 세트 로드
            PatternSetSO patternSet = FindPatternSet("LumenReaperSet");
            
            // 캐릭터 데이터 생성
            CharacterDataSO characterData = CreateCharacterDataSO(
                "LumenReaper",
                "루멘 리퍼",
                "빛을 '수확'하는 특수한 괴물입니다. 표식을 남기고 추적하는 능력을 가지고 있습니다.",
                CharacterType.Normal,
                SenseType.Auditory, // 몬스터는 감각 타입이 무의미
                80, 8, 3, 0,
                StatusEffectType.Mark,
                StatusEffectType.Bleed,
                patternSet
            );
            
            Debug.Log("루멘 리퍼 몬스터가 생성되었습니다.");
        }
        
        private void CreateEliteMonster()
        {
            // 패턴 세트 로드
            PatternSetSO patternSet = FindPatternSet("EliteMonsterSet");
            
            // 캐릭터 데이터 생성
            CharacterDataSO characterData = CreateCharacterDataSO(
                "EliteMonster",
                "어둠의 집행자",
                "빛을 먹는 강력한 괴물입니다. 일반 괴물보다 강한 공격력과 방어력을 가지고 있습니다.",
                CharacterType.Elite,
                SenseType.Auditory, // 몬스터는 감각 타입이 무의미
                100, 10, 7, 0,
                StatusEffectType.Amplify,
                StatusEffectType.Counter,
                patternSet
            );
            
            Debug.Log("엘리트 몬스터가 생성되었습니다.");
        }
        #endregion
        
        #region 유틸리티 메서드
        private CharacterDataSO CreateCharacterDataSO(string assetName, string characterName, string description,
            CharacterType characterType, SenseType senseType, int maxHealth, int attackPower, int defensePower,
            int skillCooldown, StatusEffectType primaryEffectType, StatusEffectType secondaryEffectType,
            PatternSetSO patternSet, string skillName = null, string skillDescription = null,
            ActiveSkillType skillType = ActiveSkillType.RethrowAll)
        {
            string folderPath = GetCharacterFolderPath(characterType);
            string fullPath = System.IO.Path.Combine(folderPath, $"{assetName}.asset");
            
            // 에셋 생성
            CharacterDataSO characterDataSO = ScriptableObject.CreateInstance<CharacterDataSO>();
            
            // 기본 정보 설정
            characterDataSO.characterName = characterName;
            characterDataSO.description = description;
            characterDataSO.characterType = characterType;
            characterDataSO.senseType = senseType;
            
            // 기본 스탯 설정
            characterDataSO.maxHealth = maxHealth;
            characterDataSO.baseAttackPower = attackPower;
            characterDataSO.baseDefensePower = defensePower;
            characterDataSO.skillCooldown = skillCooldown;
            
            // 상태 효과 설정
            characterDataSO.primaryEffectType = primaryEffectType;
            characterDataSO.secondaryEffectType = secondaryEffectType;
            
            // 패턴 설정
            characterDataSO.patternSet = patternSet;
            
            // 액티브 스킬 설정 (플레이어만 해당)
            if (characterType == CharacterType.Player && !string.IsNullOrEmpty(skillName))
            {
                characterDataSO.skillName = skillName;
                characterDataSO.skillDescription = skillDescription;
                characterDataSO.skillType = skillType;
            }
            
            // 에셋 저장
            AssetDatabase.CreateAsset(characterDataSO, fullPath);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"캐릭터 데이터 에셋 생성: {fullPath}");
            
            return characterDataSO;
        }
        
        private string GetCharacterFolderPath(CharacterType characterType)
        {
            // 기본 경로
            string folderPath = baseFolderPath;
            
            // 캐릭터 타입 기반 경로 설정
            if (characterType == CharacterType.Player)
            {
                folderPath = System.IO.Path.Combine(folderPath, "Player");
            }
            else
            {
                folderPath = System.IO.Path.Combine(folderPath, "Enemy");
            }
            
            return folderPath;
        }
        
        private PatternSetSO FindPatternSet(string setName)
        {
            string[] guids = AssetDatabase.FindAssets($"t:PatternSetSO {setName}");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<PatternSetSO>(path);
            }
            
            Debug.LogWarning($"패턴 세트를 찾을 수 없습니다: {setName}");
            return null;
        }
        #endregion
    }
}
#endif