namespace GodotFeatureLibrary.DialogueEngine;

public enum DialogueMode
{
    Narration, // non-interruptible, auto-dismiss after curve completes         
    Dialogue,  // interruptible, sticky (waits for input to dismiss)               
    Cutscene  // non-interruptible, sticky (waits for input after curve completes)            
}