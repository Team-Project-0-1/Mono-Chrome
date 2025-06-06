namespace MonoChrome.Core
{
    /// <summary>
    /// UI 컨트롤러 인터페이스 - UI 시스템의 추상화
    /// </summary>
    public interface IUIController
    {
        void ShowPanel(string panelName);
        void OnNodeSelected(int nodeIndex);
        void OnNewGameRequested();
        void OnDungeonEnterRequested();
        void OnMainMenuRequested();
        void OnRoomActivityCompleted();
    }
}