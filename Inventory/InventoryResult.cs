namespace GodotFeatureLibrary.Inventory;

public class InventoryResult
{
    public bool Success { get; }
    public int QuantityAffected { get; }
    public int QuantityRemaining { get; }
    public string Error { get; }

    private InventoryResult(bool success, int quantityAffected, int quantityRemaining, string error)
    {
        Success = success;
        QuantityAffected = quantityAffected;
        QuantityRemaining = quantityRemaining;
        Error = error;
    }

    public static InventoryResult Ok(int quantityAffected, int quantityRemaining = 0)
        => new(true, quantityAffected, quantityRemaining, null);

    public static InventoryResult Partial(int quantityAffected, int quantityRemaining)
        => new(true, quantityAffected, quantityRemaining, null);

    public static InventoryResult Fail(string error)
        => new(false, 0, 0, error);
}
