using System.Collections.Generic;
using MonoChrome.Systems.Dungeon;

namespace MonoChrome.Core
{
    /// <summary>
    /// 던전 컨트롤러 인터페이스 - 던전 시스템의 추상화
    /// </summary>
    public interface IDungeonController
    {
        List<DungeonNode> CurrentDungeonNodes { get; }
        DungeonNode CurrentNode { get; }
        int CurrentNodeIndex { get; }
        
        void GenerateNewDungeon(int stageIndex);
        void MoveToNode(int nodeIndex);
    }
}