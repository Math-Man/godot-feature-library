# Event Bus 

Simple event-bus pattern. Useful for passing information between unconnected scenes.

## Dependencies:
None

Usage:

 1. Define an event class:
    public class BattleOverEvent
    {
        public int WinningTeam { get; }
        public BattleOverEvent(int winningTeam) => WinningTeam = winningTeam;
    }

 2. Subscribe to events:
    EventBus.Instance.Subscribe<BattleOverEvent>(e =>
    {
        BattleOver(PLAYER_TEAM == e.WinningTeam);
    });

 3. Publish events:
    EventBus.Instance.Publish(new BattleOverEvent(winningTeam));
