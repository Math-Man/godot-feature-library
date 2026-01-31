// Usage:
//
// 1. Define an event class:
//    public class BattleOverEvent
//    {
//        public int WinningTeam { get; }
//        public BattleOverEvent(int winningTeam) => WinningTeam = winningTeam;
//    }
//
// 2. Subscribe to events:
//    EventBus.Instance.Subscribe<BattleOverEvent>(e =>
//    {
//        BattleOver(PLAYER_TEAM == e.WinningTeam);
//    });
//
// 3. Publish events:
//    EventBus.Instance.Publish(new BattleOverEvent(winningTeam));

using System;
using System.Collections.Generic;
using Godot;

namespace GodotFeatureLibrary.EventBus.EventBus;

public partial class EventBus : Node, IEventBus
{
    public static EventBus Instance { get; private set; }

    private readonly Dictionary<Type, List<Delegate>> _handlers = new();
    
    public override void _Ready()
    {
        Instance = this;
    }
    
    public void Publish<T>(T @event) where T : class
    {
        var eventType = typeof(T);
        if (_handlers.ContainsKey(eventType))
        {
            foreach (var handler in _handlers[eventType])
            {
                ((Action<T>)handler)(@event);
            }
        }
    }

    public void Subscribe<T>(Action<T> handler) where T : class
    {
        var eventType = typeof(T);
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Delegate>();
        }
        _handlers[eventType].Add(handler);
    }
    
}