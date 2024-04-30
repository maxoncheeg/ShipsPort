namespace ShipsPort.Model.Ships;

public class Ship(ShipType type, int loadingTime, int interval) : IShip
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ShipType Type { get; } = type;
    public int LoadingTime { get; } = loadingTime;
    public int LoadingTimeInterval { get; } = interval;
    public int WaitingTime { get; set; } = 0;


    public object Clone()
    {
        return new Ship(Type, LoadingTime, LoadingTimeInterval);
    }

    public bool Equals(IShip? other)
    {
        return other != null && other.Id == Id;
    }
}