using ShipsPort.Model.Ships;

namespace ShipsPort.Model.Harbours;

public class HarbourStatistics
{
    public int QueueSize { get; set; }
    
    public int StormsAmount { get; set; }
    public double AverageStormsInterval { get; set; }
    public double AverageLoadingTime { get; set; }

    public List<float> StormsIntervals = [];
    public int ShipsInStormTime { get; set; } = 0;
    public int ShipsLoaded { get; set; } = 0;
    public int ShipsArrived { get; set; } = 0;
    public float AverageWaitingTime { get; set; }
    public List<int> WaitingTimes { get; set; } = [];
    public List<int> LoadingTimes { get; set; } = [];
    public Dictionary<int, float> LoadingProbability { get; set; } = [];
    public Dictionary<int, int> StormsTimes { get; set; } = [];

    public Dictionary<ShipType, double> ShipsRate { get; set; } = [];
}