#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MonoChrome
{
    /// <summary>
    /// 기본 패턴 ScriptableObject 생성 편의 도구
    /// </summary>
    public class PatternSOCreator : EditorWindow
    {
        private string baseFolderPath = "Assets/Resources/Patterns";
        private string setsFolderPath = "Assets/Resources/PatternSets";
        
        [MenuItem("Tools/MonoChrome/Create Pattern ScriptableObjects")]
        public static void ShowWindow()
        {
            GetWindow<PatternSOCreator>("패턴 ScriptableObject 생성기");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("패턴 ScriptableObject 생성 도구", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            baseFolderPath = EditorGUILayout.TextField("패턴 저장 경로", baseFolderPath);
            setsFolderPath = EditorGUILayout.TextField("패턴 세트 저장 경로", setsFolderPath);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("리소스 폴더 생성"))
            {
                CreateResourceFolders();
            }
            
            EditorGUILayout.Space();
            
            GUILayout.Label("캐릭터 유형별 기본 패턴 생성", EditorStyles.boldLabel);
            
            if (GUILayout.Button("청각 캐릭터(증폭/공명) 패턴 생성"))
            {
                CreateAuditoryPatterns();
            }
            
            if (GUILayout.Button("후각 캐릭터(표식/출혈) 패턴 생성"))
            {
                CreateOlfactoryPatterns();
            }
            
            if (GUILayout.Button("촉각 캐릭터(반격/분쇄) 패턴 생성"))
            {
                CreateTactilePatterns();
            }
            
            if (GUILayout.Button("영적 캐릭터(저주/봉인) 패턴 생성"))
            {
                CreateSpiritualPatterns();
            }
            
            EditorGUILayout.Space();
            
            GUILayout.Label("몬스터 패턴 생성", EditorStyles.boldLabel);
            
            if (GUILayout.Button("일반 몬스터 패턴 생성"))
            {
                CreateNormalMonsterPatterns();
            }
            
            if (GUILayout.Button("엘리트 몬스터 패턴 생성"))
            {
                CreateEliteMonsterPatterns();
            }
            
            if (GUILayout.Button("루멘 리퍼 패턴 생성"))
            {
                CreateLumenReaperPatterns();
            }
            
            EditorGUILayout.Space();
            
            GUILayout.Label("패턴 세트 생성", EditorStyles.boldLabel);
            
            if (GUILayout.Button("모든 캐릭터 기본 패턴 세트 생성"))
            {
                CreateAllPatternSets();
            }
        }
        
        private void CreateResourceFolders()
        {
            System.IO.Directory.CreateDirectory(baseFolderPath);
            System.IO.Directory.CreateDirectory(setsFolderPath);
            
            AssetDatabase.Refresh();
            
            // 세부 폴더 생성
            System.IO.Directory.CreateDirectory($"{baseFolderPath}/Player");
            System.IO.Directory.CreateDirectory($"{baseFolderPath}/Enemy");
            System.IO.Directory.CreateDirectory($"{baseFolderPath}/Player/Auditory");
            System.IO.Directory.CreateDirectory($"{baseFolderPath}/Player/Olfactory");
            System.IO.Directory.CreateDirectory($"{baseFolderPath}/Player/Tactile");
            System.IO.Directory.CreateDirectory($"{baseFolderPath}/Player/Spiritual");
            System.IO.Directory.CreateDirectory($"{baseFolderPath}/Enemy/Normal");
            System.IO.Directory.CreateDirectory($"{baseFolderPath}/Enemy/Elite");
            System.IO.Directory.CreateDirectory($"{baseFolderPath}/Enemy/Boss");
            
            AssetDatabase.Refresh();
            
            Debug.Log("리소스 폴더가 생성되었습니다.");
        }
        
        #region 플레이어 패턴 생성
        private void CreateAuditoryPatterns()
        {
            // 앞면 패턴 - 공명 타격
            PatternSO pattern1 = CreatePatternSO("Auditory_Attack_ResonanceStrike", "공명 타격", "증폭 1 소모해서 공명 1 부여",
                PatternType.Consecutive2, true, true, 1, 0,
                new StatusEffectDataWrapper[]
                {
                    new StatusEffectDataWrapper { effectType = StatusEffectType.Resonance, magnitude = 1, duration = 2 }
                },
                new CharacterType[] { CharacterType.Player },
                new SenseType[] { SenseType.Auditory });
            
            // 앞면 패턴 - 진동 연쇄
            PatternSO pattern2 = CreatePatternSO("Auditory_Attack_VibrationChain", "진동 연쇄", "2연타 + 증폭 2 소모해서 공명 2 부여",
                PatternType.Consecutive3, true, true, 2, 0,
                new StatusEffectDataWrapper[]
                {
                    new StatusEffectDataWrapper { effectType = StatusEffectType.Resonance, magnitude = 2, duration = 2 }
                },
                new CharacterType[] { CharacterType.Player },
                new SenseType[] { SenseType.Auditory });
            
            // 뒷면 패턴 - 잔향 방어
            PatternSO pattern3 = CreatePatternSO("Auditory_Defense_EchoDefense", "잔향 방어", "진동을 일으켜 적의 공격 방어, 다음 턴 증폭 +2",
                PatternType.Consecutive2, false, false, 0, 2,
                new StatusEffectDataWrapper[]
                {
                    new StatusEffectDataWrapper { effectType = StatusEffectType.Amplify, magnitude = 2, duration = 1 }
                },
                new CharacterType[] { CharacterType.Player },
                new SenseType[] { SenseType.Auditory });
            
            Debug.Log("청각 캐릭터 패턴이 생성되었습니다.");
        }
        
        private void CreateOlfactoryPatterns()
        {
            // 앞면 패턴 - 추적
            PatternSO pattern1 = CreatePatternSO("Olfactory_Attack_Track", "추적", "1회 공격 + 적에게 표식 2 부여",
                PatternType.Consecutive2, true, true, 1, 0,
                new StatusEffectDataWrapper[]
                {
                    new StatusEffectDataWrapper { effectType = StatusEffectType.Mark, magnitude = 2, duration = 2 }
                },
                new CharacterType[] { CharacterType.Player },
                new SenseType[] { SenseType.Olfactory });
            
            // 앞면 패턴 - 수확
            PatternSO pattern2 = CreatePatternSO("Olfactory_Attack_Harvest", "수확", "표식 수치만큼 연속 공격, 표식 초기화",
                PatternType.Consecutive3, true, true, 2, 0,
                null,
                new CharacterType[] { CharacterType.Player },
                new SenseType[] { SenseType.Olfactory });
            pattern2.specialEffect = "표식 수치 + 공격력만큼 연속 공격";
            
            // 뒷면 패턴 - 은닉
            PatternSO pattern3 = CreatePatternSO("Olfactory_Defense_Conceal", "은닉", "방어 성공 시 적에게 출혈 부여",
                PatternType.Consecutive2, false, false, 0, 2,
                new StatusEffectDataWrapper[]
                {
                    new StatusEffectDataWrapper { effectType = StatusEffectType.Bleed, magnitude = 2, duration = 2 }
                },
                new CharacterType[] { CharacterType.Player },
                new SenseType[] { SenseType.Olfactory });
            
            Debug.Log("후각 캐릭터 패턴이 생성되었습니다.");
        }
        
        private void CreateTactilePatterns()
        {
            // 앞면 패턴 - 반격 공격
            PatternSO pattern1 = CreatePatternSO("Tactile_Attack_CounterAttack", "반격 공격", "공격 + 반격 1 부여",
                PatternType.Consecutive2, true, true, 2, 0,
                new StatusEffectDataWrapper[]
                {
                    new StatusEffectDataWrapper { effectType = StatusEffectType.Counter, magnitude = 1, duration = 2 }
                },
                new CharacterType[] { CharacterType.Player },
                new SenseType[] { SenseType.Tactile });
            
            // 뒷면 패턴 - 역동팔
            PatternSO pattern2 = CreatePatternSO("Tactile_Defense_DynamicArm", "역동팔", "방어 성공 시 적 방어력 분쇄",
                PatternType.Consecutive2, false, false, 0, 3,
                new StatusEffectDataWrapper[]
                {
                    new StatusEffectDataWrapper { effectType = StatusEffectType.Crush, magnitude = 1, duration = 2 }
                },
                new CharacterType[] { CharacterType.Player },
                new SenseType[] { SenseType.Tactile });
            
            Debug.Log("촉각 캐릭터 패턴이 생성되었습니다.");
        }
        
        private void CreateSpiritualPatterns()
        {
            // 앞면 패턴 - 저주 부여
            PatternSO pattern1 = CreatePatternSO("Spiritual_Attack_CurseBestow", "저주 부여", "공격 + 저주 2 부여",
                PatternType.Consecutive2, true, true, 1, 0,
                new StatusEffectDataWrapper[]
                {
                    new StatusEffectDataWrapper { effectType = StatusEffectType.Curse, magnitude = 2, duration = 2 }
                },
                new CharacterType[] { CharacterType.Player },
                new SenseType[] { SenseType.Spiritual });
            
            // 뒷면 패턴 - 봉인 방어
            PatternSO pattern2 = CreatePatternSO("Spiritual_Defense_SealDefense", "봉인 방어", "방어 + 적 동전 1개 봉인",
                PatternType.Consecutive2, false, false, 0, 2,
                new StatusEffectDataWrapper[]
                {
                    new StatusEffectDataWrapper { effectType = StatusEffectType.Seal, magnitude = 1, duration = 1 }
                },
                new CharacterType[] { CharacterType.Player },
                new SenseType[] { SenseType.Spiritual });
            
            Debug.Log("영적 캐릭터 패턴이 생성되었습니다.");
        }
        #endregion
        
        #region 몬스터 패턴 생성
        private void CreateNormalMonsterPatterns()
        {
            // 기본 공격 패턴
            PatternSO pattern1 = CreatePatternSO("Normal_Attack_Basic", "기본 공격", "적에게 기본 공격을 가한다",
                PatternType.Consecutive2, true, true, 1, 0,
                null,
                new CharacterType[] { CharacterType.Normal },
                null);
            
            // 방어 태세 패턴
            PatternSO pattern2 = CreatePatternSO("Normal_Defense_Stance", "방어 태세", "방어 자세를 취해 방어력 증가",
                PatternType.Consecutive2, false, false, 0, 2,
                null,
                new CharacterType[] { CharacterType.Normal },
                null);
            
            Debug.Log("일반 몬스터 패턴이 생성되었습니다.");
        }
        
        private void CreateEliteMonsterPatterns()
        {
            // 강화 공격 패턴
            PatternSO pattern1 = CreatePatternSO("Elite_Attack_Enhanced", "강화 공격", "강력한 일격을 가한다",
                PatternType.Consecutive4, true, true, 4, 0,
                null,
                new CharacterType[] { CharacterType.Elite },
                null);
            
            // 강화 방어 패턴
            PatternSO pattern2 = CreatePatternSO("Elite_Defense_Enhanced", "견고한 방어", "강력한 방어 자세로 방어력 크게 증가",
                PatternType.Consecutive3, false, false, 0, 4,
                null,
                new CharacterType[] { CharacterType.Elite },
                null);
            
            Debug.Log("엘리트 몬스터 패턴이 생성되었습니다.");
        }
        
        private void CreateLumenReaperPatterns()
        {
            // 추적 패턴
            PatternSO pattern1 = CreatePatternSO("LumenReaper_Attack_Track", "추적", "적에게 표식 2 부여",
                PatternType.Consecutive2, true, true, 1, 0,
                new StatusEffectDataWrapper[]
                {
                    new StatusEffectDataWrapper { effectType = StatusEffectType.Mark, magnitude = 2, duration = 2 }
                },
                new CharacterType[] { CharacterType.Normal, CharacterType.Elite },
                null);
            
            // 수확 패턴
            PatternSO pattern2 = CreatePatternSO("LumenReaper_Attack_Harvest", "수확", "표식 수치만큼 연속 공격, 표식 초기화",
                PatternType.Consecutive4, true, true, 3, 0,
                null,
                new CharacterType[] { CharacterType.Normal, CharacterType.Elite },
                null);
            pattern2.specialEffect = "표식 수치만큼 추가 공격";
            
            Debug.Log("루멘 리퍼 패턴이 생성되었습니다.");
        }
        #endregion
        
        #region 패턴 세트 생성
        private void CreateAllPatternSets()
        {
            // 청각 캐릭터 패턴 세트
            string[] auditoryPatternNames = new string[]
            {
                "Auditory_Attack_ResonanceStrike",
                "Auditory_Attack_VibrationChain",
                "Auditory_Defense_EchoDefense"
            };
            CreatePatternSet("PlayerAuditorySet", "청각 캐릭터 기본 패턴", CharacterType.Player, SenseType.Auditory, auditoryPatternNames);
            
            // 후각 캐릭터 패턴 세트
            string[] olfactoryPatternNames = new string[]
            {
                "Olfactory_Attack_Track",
                "Olfactory_Attack_Harvest",
                "Olfactory_Defense_Conceal"
            };
            CreatePatternSet("PlayerOlfactorySet", "후각 캐릭터 기본 패턴", CharacterType.Player, SenseType.Olfactory, olfactoryPatternNames);
            
            // 촉각 캐릭터 패턴 세트
            string[] tactilePatternNames = new string[]
            {
                "Tactile_Attack_CounterAttack",
                "Tactile_Defense_DynamicArm"
            };
            CreatePatternSet("PlayerTactileSet", "촉각 캐릭터 기본 패턴", CharacterType.Player, SenseType.Tactile, tactilePatternNames);
            
            // 영적 캐릭터 패턴 세트
            string[] spiritualPatternNames = new string[]
            {
                "Spiritual_Attack_CurseBestow",
                "Spiritual_Defense_SealDefense"
            };
            CreatePatternSet("PlayerSpiritualSet", "영적 캐릭터 기본 패턴", CharacterType.Player, SenseType.Spiritual, spiritualPatternNames);
            
            // 일반 몬스터 패턴 세트
            string[] normalMonsterPatternNames = new string[]
            {
                "Normal_Attack_Basic",
                "Normal_Defense_Stance"
            };
            CreatePatternSet("NormalMonsterSet", "일반 몬스터 기본 패턴", CharacterType.Normal, SenseType.Auditory, normalMonsterPatternNames);
            
            // 엘리트 몬스터 패턴 세트
            string[] eliteMonsterPatternNames = new string[]
            {
                "Elite_Attack_Enhanced",
                "Elite_Defense_Enhanced"
            };
            CreatePatternSet("EliteMonsterSet", "엘리트 몬스터 기본 패턴", CharacterType.Elite, SenseType.Auditory, eliteMonsterPatternNames);
            
            // 루멘 리퍼 패턴 세트
            string[] lumenReaperPatternNames = new string[]
            {
                "LumenReaper_Attack_Track",
                "LumenReaper_Attack_Harvest"
            };
            CreatePatternSet("LumenReaperSet", "루멘 리퍼 기본 패턴", CharacterType.Normal, SenseType.Auditory, lumenReaperPatternNames);
            
            Debug.Log("모든 패턴 세트가 생성되었습니다.");
        }
        #endregion
        
        #region 유틸리티 메서드
        private PatternSO CreatePatternSO(string assetName, string patternName, string description,
            PatternType patternType, bool patternValue, bool isAttack,
            int attackBonus, int defenseBonus,
            StatusEffectDataWrapper[] statusEffects = null,
            CharacterType[] characterTypes = null,
            SenseType[] senseTypes = null)
        {
            string folderPath = GetPatternFolderPath(characterTypes, senseTypes, isAttack);
            string fullPath = System.IO.Path.Combine(folderPath, $"{assetName}.asset");
            
            // 에셋 생성
            PatternSO patternSO = ScriptableObject.CreateInstance<PatternSO>();
            
            // 기본 정보 설정
            patternSO.patternName = patternName;
            patternSO.description = description;
            patternSO.id = GeneratePatternId(patternName); // 이름 기반 ID 생성
            
            // 패턴 속성 설정
            patternSO.patternType = patternType;
            patternSO.patternValue = patternValue;
            patternSO.isAttack = isAttack;
            
            // 캐릭터 요구사항 설정
            patternSO.applicableCharacterTypes = characterTypes;
            patternSO.applicableSenseTypes = senseTypes;
            
            // 효과 설정
            patternSO.attackBonus = attackBonus;
            patternSO.defenseBonus = defenseBonus;
            
            // 상태 효과 설정
            patternSO.statusEffects = statusEffects;
            
            // 에셋 저장
            AssetDatabase.CreateAsset(patternSO, fullPath);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"패턴 에셋 생성: {fullPath}");
            
            return patternSO;
        }
        
        private string GetPatternFolderPath(CharacterType[] characterTypes, SenseType[] senseTypes, bool isAttack)
        {
            // 기본 경로
            string folderPath = baseFolderPath;
            
            // 캐릭터 타입 기반 경로 설정
            if (characterTypes != null && characterTypes.Length > 0)
            {
                if (characterTypes[0] == CharacterType.Player)
                {
                    folderPath = System.IO.Path.Combine(folderPath, "Player");
                    
                    // 감각 타입 기반 경로 추가
                    if (senseTypes != null && senseTypes.Length > 0)
                    {
                        switch (senseTypes[0])
                        {
                            case SenseType.Auditory:
                                folderPath = System.IO.Path.Combine(folderPath, "Auditory");
                                break;
                            case SenseType.Olfactory:
                                folderPath = System.IO.Path.Combine(folderPath, "Olfactory");
                                break;
                            case SenseType.Tactile:
                                folderPath = System.IO.Path.Combine(folderPath, "Tactile");
                                break;
                            case SenseType.Spiritual:
                                folderPath = System.IO.Path.Combine(folderPath, "Spiritual");
                                break;
                        }
                    }
                }
                else
                {
                    folderPath = System.IO.Path.Combine(folderPath, "Enemy");
                    
                    switch (characterTypes[0])
                    {
                        case CharacterType.Normal:
                            folderPath = System.IO.Path.Combine(folderPath, "Normal");
                            break;
                        case CharacterType.Elite:
                            folderPath = System.IO.Path.Combine(folderPath, "Elite");
                            break;
                        case CharacterType.Boss:
                            folderPath = System.IO.Path.Combine(folderPath, "Boss");
                            break;
                    }
                }
            }
            
            return folderPath;
        }
        
        private int GeneratePatternId(string patternName)
        {
            // 간단한 해시 코드 생성 (고유한 ID를 위한 임시 방법)
            return patternName.GetHashCode() & 0x7FFFFFFF; // 양수로 변환
        }
        
        private void CreatePatternSet(string assetName, string setName, CharacterType characterType, 
            SenseType senseType, string[] patternAssetNames)
        {
            string fullPath = System.IO.Path.Combine(setsFolderPath, $"{assetName}.asset");
            
            // 에셋 생성
            PatternSetSO patternSetSO = ScriptableObject.CreateInstance<PatternSetSO>();
            
            // 기본 정보 설정
            patternSetSO.setName = setName;
            patternSetSO.targetCharacterType = characterType;
            patternSetSO.targetSenseType = senseType;
            
            // 패턴 목록 설정
            List<PatternSO> patterns = new List<PatternSO>();
            foreach (string patternName in patternAssetNames)
            {
                string patternPath = FindPatternAssetPath(patternName);
                if (!string.IsNullOrEmpty(patternPath))
                {
                    PatternSO pattern = AssetDatabase.LoadAssetAtPath<PatternSO>(patternPath);
                    if (pattern != null)
                    {
                        patterns.Add(pattern);
                    }
                    else
                    {
                        Debug.LogWarning($"패턴을 찾을 수 없습니다: {patternName}");
                    }
                }
            }
            
            patternSetSO.patterns = patterns.ToArray();
            
            // 에셋 저장
            AssetDatabase.CreateAsset(patternSetSO, fullPath);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"패턴 세트 에셋 생성: {fullPath}");
        }
        
        private string FindPatternAssetPath(string patternName)
        {
            string[] guids = AssetDatabase.FindAssets($"t:PatternSO {patternName}", new string[] { baseFolderPath });
            if (guids.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(guids[0]);
            }
            
            Debug.LogWarning($"PatternSO 에셋을 찾을 수 없습니다: {patternName}");
            return null;
        }
        #endregion
    }
}
#endif