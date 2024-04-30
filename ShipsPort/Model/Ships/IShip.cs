namespace ShipsPort.Model.Ships;

public enum ShipType
{
    First, Second, Third, Fourth
}

public interface IShip : ICloneable, IEquatable<IShip>
{
    public Guid Id { get; set; }
    public ShipType Type { get; }
    public int LoadingTime { get; }
    public int LoadingTimeInterval { get; }
    public int WaitingTime { get; set; }
}