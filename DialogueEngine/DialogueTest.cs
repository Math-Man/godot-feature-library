using System.Collections.Generic;
using Godot;
using GodotFeatureLibrary.Events;
using GodotFeatureLibrary.GameInput;

namespace GodotFeatureLibrary.DialogueEngine;

public partial class DialogueTest : Node
{
    private int _currentQuoteIndex;
    private static readonly Curve SimpleCurve;
    private static readonly Curve PurposefulCurve;
    private static readonly Curve FinalCurve;

    // Character colors
    private static readonly Color CamillaColor = new(0.9f, 0.7f, 0.5f);   // warm peach
    private static readonly Color CassildaColor = new(0.4f, 0.55f, 0.9f); // soft blue
    private static readonly Color StrangerColor = new(0.8f, 0.75f, 0.2f); // sickly yellow

    static DialogueTest()
    {
        SimpleCurve = new Curve();
        SimpleCurve.AddPoint(new Vector2(0, 0));
        SimpleCurve.AddPoint(new Vector2(1, 1));

        PurposefulCurve = new Curve();
        PurposefulCurve.AddPoint(new Vector2(0, 0));
        PurposefulCurve.AddPoint(new Vector2(0.4f, 0.2f));
        PurposefulCurve.AddPoint(new Vector2(1, 1));

        FinalCurve = new Curve();
        FinalCurve.AddPoint(new Vector2(0, 0));
        FinalCurve.AddPoint(new Vector2(0.7f, 0.5f));
        FinalCurve.AddPoint(new Vector2(1, 1));
    }

    private readonly List<DialogueEvent> _quotes =
    [
        new("You, sir, should unmask", DialogueMode.Dialogue, PurposefulCurve, 3f, 1f, "Camilla", CamillaColor),
        new("Indeed?", DialogueMode.Dialogue, SimpleCurve, 1.5f, 0.5f, "Stranger", StrangerColor),
        new("Indeed, it's time. We all have laid aside disguise but you.", DialogueMode.Dialogue, PurposefulCurve, 4f, 1.5f, "Cassilda", CassildaColor),
        new("I wear no mask.", DialogueMode.Dialogue, SimpleCurve, 2f, 2f, "Stranger", StrangerColor),
        new("No mask? No mask!", DialogueMode.Dialogue, FinalCurve, 2f, 1f, "Camilla", CamillaColor),
        new("...", DialogueMode.Dialogue, SimpleCurve, 1f),
    ];

    public override void _Process(double delta)
    {
        if (!Input.IsActionJustPressed(InputMapping.INTERACTION_PRIMARY)) return;
        EventBus.Instance.Publish(_quotes[_currentQuoteIndex % _quotes.Count]);
        _currentQuoteIndex++;
    }
}