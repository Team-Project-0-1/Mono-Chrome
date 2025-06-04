using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MonoChrome.Core
{
    /// <summary>
    /// 빠른 UI 구조 자동 생성 도구
    /// 포트폴리오용: 빠른 프로토타이핑과 UI 구조 생성을 위한 유틸리티
    /// </summary>
    public class QuickUIBuilder : MonoBehaviour
    {
        [Header("UI Creation Settings")]
        [SerializeField] private bool _createDebugButtons = true;
        [SerializeField] private bool _overwriteExisting = false;
        
        [ContextMenu("Create Basic UI Structure")]
        public void CreateBasicUIStructure()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("QuickUIBuilder: Canvas not found! Creating new Canvas...");
                canvas = CreateCanvas();
            }

            Debug.Log("QuickUIBuilder: Creating basic UI structure...");

            // 캐릭터 선택 패널 생성
            CreateCharacterSelectionPanel(canvas.transform);
            
            // 던전 패널 생성
            CreateDungeonPanel(canvas.transform);
            
            // 기타 패널들 생성
            CreateOtherPanels(canvas.transform);
            
            // 디버그 버튼 생성
            if (_createDebugButtons)
            {
                CreateDebugButtons(canvas.transform);
            }
            
            Debug.Log("QuickUIBuilder: Basic UI structure created successfully!");
        }
        
        /// <summary>
        /// Canvas 생성
        /// </summary>
        private Canvas CreateCanvas()
        {
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // EventSystem 확인 및 생성
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            
            return canvas;
        }
        
        /// <summary>
        /// 캐릭터 선택 패널 생성
        /// </summary>
        private void CreateCharacterSelectionPanel(Transform parent)
        {
            // 기존 패널 확인
            Transform existing = parent.Find("CharacterSelectionPanel");
            if (existing != null && !_overwriteExisting)
            {
                Debug.Log("QuickUIBuilder: CharacterSelectionPanel already exists, skipping...");
                return;
            }
            
            if (existing != null)
                DestroyImmediate(existing.gameObject);
            
            GameObject panel = new GameObject("CharacterSelectionPanel");
            panel.transform.SetParent(parent, false);
            
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            // 배경 이미지 추가
            Image backgroundImage = panel.AddComponent<Image>();
            backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // 제목 텍스트 추가
            CreateTitleText(panel.transform, "캐릭터 선택", new Vector2(0.5f, 0.85f));
            
            // 캐릭터 버튼들 생성
            string[] characters = { "김훈희", "신제우", "곽장환", "박재석" };
            string[] descriptions = { 
                "소리로 세상을 듣는 자 (초보자용)",
                "냄새로 추적하는 자 (숙련자용)", 
                "손끝으로 바람을 읽는 자 (고급자용)",
                "영적인 시야를 보는 자 (고급자용)"
            };
            
            for (int i = 0; i < characters.Length; i++)
            {
                CreateCharacterButton(panel.transform, i + 1, characters[i], descriptions[i]);
            }
            
            panel.SetActive(false); // 기본적으로 비활성화
            Debug.Log("QuickUIBuilder: CharacterSelectionPanel created");
        }
        
        /// <summary>
        /// 캐릭터 버튼 생성
        /// </summary>
        private void CreateCharacterButton(Transform parent, int index, string characterName, string description)
        {
            GameObject button = new GameObject($"CharacterButton{index}");
            button.transform.SetParent(parent, false);
            
            RectTransform rect = button.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.75f - (index - 1) * 0.15f);
            rect.anchorMax = new Vector2(0.5f, 0.75f - (index - 1) * 0.15f);
            rect.sizeDelta = new Vector2(400, 80);
            
            Button btn = button.AddComponent<Button>();
            Image img = button.AddComponent<Image>();
            img.color = new Color(0.2f, 0.3f, 0.5f, 0.8f);
            
            // 호버 효과를 위한 ColorBlock 설정
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(0.3f, 0.4f, 0.7f, 1f);
            colors.pressedColor = new Color(0.1f, 0.2f, 0.4f, 1f);
            btn.colors = colors;
            
            // 캐릭터 이름 텍스트
            GameObject nameTextObj = new GameObject("CharacterName");
            nameTextObj.transform.SetParent(button.transform, false);
            
            RectTransform nameRect = nameTextObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.5f);
            nameRect.anchorMax = new Vector2(1, 1f);
            nameRect.offsetMin = new Vector2(10, 0);
            nameRect.offsetMax = new Vector2(-10, -5);
            
            TextMeshProUGUI nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
            nameText.text = characterName;
            nameText.fontSize = 24;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.fontStyle = FontStyles.Bold;
            
            // 설명 텍스트
            GameObject descTextObj = new GameObject("CharacterDescription");
            descTextObj.transform.SetParent(button.transform, false);
            
            RectTransform descRect = descTextObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0);
            descRect.anchorMax = new Vector2(1, 0.5f);
            descRect.offsetMin = new Vector2(10, 5);
            descRect.offsetMax = new Vector2(-10, 0);
            
            TextMeshProUGUI descText = descTextObj.AddComponent<TextMeshProUGUI>();
            descText.text = description;
            descText.fontSize = 16;
            descText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            descText.alignment = TextAlignmentOptions.Center;
        }
        
        /// <summary>
        /// 던전 패널 생성
        /// </summary>
        private void CreateDungeonPanel(Transform parent)
        {
            // 기존 패널 확인
            Transform existing = parent.Find("DungeonPanel");
            if (existing != null && !_overwriteExisting)
            {
                Debug.Log("QuickUIBuilder: DungeonPanel already exists, skipping...");
                return;
            }
            
            if (existing != null)
                DestroyImmediate(existing.gameObject);
            
            GameObject panel = new GameObject("DungeonPanel");
            panel.transform.SetParent(parent, false);
            
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            // 배경 이미지 추가
            Image backgroundImage = panel.AddComponent<Image>();
            backgroundImage.color = new Color(0.05f, 0.05f, 0.1f, 0.9f);
            
            // 제목 텍스트 추가
            CreateTitleText(panel.transform, "다음 방을 선택하세요", new Vector2(0.5f, 0.85f));
            
            // 방 선택 버튼들 생성
            string[] roomTypes = { "전투", "이벤트", "상점" };
            string[] roomDescs = { 
                "적과 마주쳐 전투를 벌입니다.",
                "특별한 이벤트가 발생합니다.",
                "아이템을 구매할 수 있습니다."
            };
            Color[] roomColors = {
                new Color(0.6f, 0.2f, 0.2f, 0.8f), // 빨간색 (전투)
                new Color(0.2f, 0.4f, 0.6f, 0.8f), // 파란색 (이벤트)
                new Color(0.2f, 0.6f, 0.2f, 0.8f)  // 녹색 (상점)
            };
            
            for (int i = 0; i < roomTypes.Length; i++)
            {
                CreateRoomButton(panel.transform, i + 1, roomTypes[i], roomDescs[i], roomColors[i]);
            }
            
            // 상단에 스테이지 정보 표시
            CreateStageInfoPanel(panel.transform);
            
            panel.SetActive(false);
            Debug.Log("QuickUIBuilder: DungeonPanel created");
        }
        
        /// <summary>
        /// 방 선택 버튼 생성
        /// </summary>
        private void CreateRoomButton(Transform parent, int index, string roomType, string description, Color buttonColor)
        {
            GameObject button = new GameObject($"RoomButton{index}");
            button.transform.SetParent(parent, false);
            
            RectTransform rect = button.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.65f - (index - 1) * 0.2f);
            rect.anchorMax = new Vector2(0.5f, 0.65f - (index - 1) * 0.2f);
            rect.sizeDelta = new Vector2(350, 100);
            
            Button btn = button.AddComponent<Button>();
            Image img = button.AddComponent<Image>();
            img.color = buttonColor;
            
            // 호버 효과
            ColorBlock colors = btn.colors;
            colors.highlightedColor = Color.Lerp(buttonColor, Color.white, 0.3f);
            colors.pressedColor = Color.Lerp(buttonColor, Color.black, 0.3f);
            btn.colors = colors;
            
            // 방 타입 텍스트
            GameObject typeTextObj = new GameObject("RoomType");
            typeTextObj.transform.SetParent(button.transform, false);
            
            RectTransform typeRect = typeTextObj.AddComponent<RectTransform>();
            typeRect.anchorMin = new Vector2(0, 0.6f);
            typeRect.anchorMax = new Vector2(1, 1f);
            typeRect.offsetMin = new Vector2(10, 0);
            typeRect.offsetMax = new Vector2(-10, -5);
            
            TextMeshProUGUI typeText = typeTextObj.AddComponent<TextMeshProUGUI>();
            typeText.text = roomType;
            typeText.fontSize = 28;
            typeText.color = Color.white;
            typeText.alignment = TextAlignmentOptions.Center;
            typeText.fontStyle = FontStyles.Bold;
            
            // 설명 텍스트
            GameObject descTextObj = new GameObject("RoomDescription");
            descTextObj.transform.SetParent(button.transform, false);
            
            RectTransform descRect = descTextObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0);
            descRect.anchorMax = new Vector2(1, 0.6f);
            descRect.offsetMin = new Vector2(10, 5);
            descRect.offsetMax = new Vector2(-10, 0);
            
            TextMeshProUGUI descText = descTextObj.AddComponent<TextMeshProUGUI>();
            descText.text = description;
            descText.fontSize = 18;
            descText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            descText.alignment = TextAlignmentOptions.Center;
        }
        
        /// <summary>
        /// 스테이지 정보 패널 생성
        /// </summary>
        private void CreateStageInfoPanel(Transform parent)
        {
            GameObject infoPanel = new GameObject("StageInfoPanel");
            infoPanel.transform.SetParent(parent, false);
            
            RectTransform rect = infoPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.9f);
            rect.anchorMax = new Vector2(1, 1f);
            rect.offsetMin = new Vector2(20, 0);
            rect.offsetMax = new Vector2(-20, -10);
            
            Image backgroundImage = infoPanel.AddComponent<Image>();
            backgroundImage.color = new Color(0, 0, 0, 0.5f);
            
            // 스테이지 정보 텍스트
            GameObject stageTextObj = new GameObject("StageInfoText");
            stageTextObj.transform.SetParent(infoPanel.transform, false);
            
            RectTransform stageRect = stageTextObj.AddComponent<RectTransform>();
            stageRect.anchorMin = Vector2.zero;
            stageRect.anchorMax = Vector2.one;
            stageRect.offsetMin = new Vector2(10, 0);
            stageRect.offsetMax = new Vector2(-10, 0);
            
            TextMeshProUGUI stageText = stageTextObj.AddComponent<TextMeshProUGUI>();
            stageText.text = "스테이지 1 - 방 1";
            stageText.fontSize = 20;
            stageText.color = Color.white;
            stageText.alignment = TextAlignmentOptions.Center;
        }
        
        /// <summary>
        /// 제목 텍스트 생성
        /// </summary>
        private void CreateTitleText(Transform parent, string title, Vector2 anchorPosition)
        {
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(parent, false);
            
            RectTransform rect = titleObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorPosition;
            rect.anchorMax = anchorPosition;
            rect.sizeDelta = new Vector2(600, 60);
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = title;
            titleText.fontSize = 36;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
        }
        
        /// <summary>
        /// 기타 패널들 생성
        /// </summary>
        private void CreateOtherPanels(Transform parent)
        {
            string[] panelNames = { "CombatPanel", "EventPanel", "ShopPanel", "RestPanel", "GameOverPanel", "VictoryPanel" };
            
            foreach (string panelName in panelNames)
            {
                // 기존 패널 확인
                Transform existing = parent.Find(panelName);
                if (existing != null && !_overwriteExisting)
                {
                    continue;
                }
                
                if (existing != null)
                    DestroyImmediate(existing.gameObject);
                
                GameObject panel = new GameObject(panelName);
                panel.transform.SetParent(parent, false);
                
                RectTransform rect = panel.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                
                // 배경 추가
                Image backgroundImage = panel.AddComponent<Image>();
                backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
                
                // 패널 이름 표시
                CreateTitleText(panel.transform, panelName.Replace("Panel", ""), new Vector2(0.5f, 0.5f));
                
                panel.SetActive(false);
            }
            
            Debug.Log("QuickUIBuilder: Other panels created");
        }
        
        /// <summary>
        /// 디버그 버튼들 생성
        /// </summary>
        private void CreateDebugButtons(Transform parent)
        {
            GameObject debugPanel = new GameObject("DebugPanel");
            debugPanel.transform.SetParent(parent, false);
            
            RectTransform rect = debugPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0.3f, 0.3f);
            rect.offsetMin = new Vector2(10, 10);
            rect.offsetMax = new Vector2(-10, -10);
            
            Image backgroundImage = debugPanel.AddComponent<Image>();
            backgroundImage.color = new Color(0, 0, 0, 0.7f);
            
            // 디버그 제목
            CreateTitleText(debugPanel.transform, "Debug", new Vector2(0.5f, 0.9f));
            
            // 메인 메뉴 버튼
            CreateDebugButton(debugPanel.transform, "MainMenu", new Vector2(0.5f, 0.7f), () => {
                Debug.Log("QuickUIBuilder: Loading MainMenu scene");
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            });
            
            // 캐릭터 선택 버튼
            CreateDebugButton(debugPanel.transform, "Character", new Vector2(0.5f, 0.5f), () => {
                if (CoreGameManager.Instance != null)
                    CoreGameManager.Instance.ChangeState(CoreGameManager.GameState.CharacterSelection);
            });
            
            // 던전 버튼
            CreateDebugButton(debugPanel.transform, "Dungeon", new Vector2(0.5f, 0.3f), () => {
                if (CoreGameManager.Instance != null)
                    CoreGameManager.Instance.ChangeState(CoreGameManager.GameState.Dungeon);
            });
            
            Debug.Log("QuickUIBuilder: Debug buttons created");
        }
        
        /// <summary>
        /// 디버그 버튼 생성
        /// </summary>
        private void CreateDebugButton(Transform parent, string buttonText, Vector2 anchorPosition, System.Action onClick)
        {
            GameObject button = new GameObject($"Debug{buttonText}Button");
            button.transform.SetParent(parent, false);
            
            RectTransform rect = button.AddComponent<RectTransform>();
            rect.anchorMin = anchorPosition;
            rect.anchorMax = anchorPosition;
            rect.sizeDelta = new Vector2(120, 30);
            
            Button btn = button.AddComponent<Button>();
            Image img = button.AddComponent<Image>();
            img.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            
            // 텍스트 추가
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(button.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = buttonText;
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            
            // 이벤트 연결
            btn.onClick.AddListener(() => onClick?.Invoke());
        }
        
        #region Context Menu Actions
        [ContextMenu("Create Character Selection Only")]
        public void CreateCharacterSelectionOnly()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                canvas = CreateCanvas();
            }
            
            CreateCharacterSelectionPanel(canvas.transform);
            Debug.Log("QuickUIBuilder: Character selection panel created");
        }
        
        [ContextMenu("Create Dungeon Panel Only")]
        public void CreateDungeonPanelOnly()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                canvas = CreateCanvas();
            }
            
            CreateDungeonPanel(canvas.transform);
            Debug.Log("QuickUIBuilder: Dungeon panel created");
        }
        
        [ContextMenu("Clear All UI")]
        public void ClearAllUI()
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;
            
            string[] panelNames = { 
                "CharacterSelectionPanel", "DungeonPanel", "CombatPanel", 
                "EventPanel", "ShopPanel", "RestPanel", 
                "GameOverPanel", "VictoryPanel", "DebugPanel" 
            };
            
            foreach (string panelName in panelNames)
            {
                Transform panel = canvas.transform.Find(panelName);
                if (panel != null)
                {
                    DestroyImmediate(panel.gameObject);
                }
            }
            
            Debug.Log("QuickUIBuilder: All UI panels cleared");
        }
        #endregion
    }
}