using ShipsPort.Model.Ships;

namespace ShipsPort.Model.Harbours;

public interface IHarbour
{
    public IReadOnlyCollection<IShip> Piers { get; }

    public event EventHandler<StormArgs> StormStarted;
    //public event Action<IShip> 

    public Task<HarbourStatistics> Open(int hours);
}