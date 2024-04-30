using ShipsPort.Model.Ships;

namespace ShipsPort.Model.Harbours;

public interface IPier : IEquatable<IPier>
{
    public Guid Id { get; set; }
    public IShip? Ship { get; set; }
    public int Time { get; set; }

    public void Set(IShip ship, int time);
}