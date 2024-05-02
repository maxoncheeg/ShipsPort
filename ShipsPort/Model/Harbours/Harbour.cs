using System.Runtime.InteropServices.ComTypes;
using ShipsPort.Model.Ships;

namespace ShipsPort.Model.Harbours;

public enum TimeAction
{
    StormBegin,
    StormEnd,
    ShipArrival,
    ShipLoaded,
    ShipAfterStorm,
    FourthShipArrival
}

public class Harbour : IHarbour
{
    private readonly HarbourOptions _options;

    private int _initialHours = 0;
    private List<double> _amounts;
    private bool _isStorming = false;
    private readonly Random _random = new();
    private Queue<IShip> _queue = [];
    private IShipFactory _shipFactory;
    private List<IPier> _piers;

    private List<Tuple<int, TimeAction>> _timeLine = [];
    private Dictionary<IPier, Tuple<int, TimeAction>> _timeLinePiers = [];

    private Dictionary<IShip, int> _fourthShips = [];

    public IReadOnlyCollection<IShip> Piers { get; }
    public event EventHandler<StormArgs>? StormStarted;

    public Harbour(IShipFactory shipFactory, HarbourOptions options)
    {
        _options = options;
        _shipFactory = shipFactory;

        _piers = [];
        for (int i = 0; i < _options.PiersAmount; i++) _piers.Add(new Pier());

        for (int i = 0; i < _options.FourthShipAmount; i++) _fourthShips.Add(_shipFactory.CreateShip(ShipType.Fourth), 0);
    }

    public Task<HarbourStatistics> Open(int hours)
    {
        return Task.Run(() =>
        {
            _initialHours = hours;
            var stats = new HarbourStatistics();

            _amounts = [0, 0, 0, 0];

            int nextShipTime = _random.Next(_options.ArrivalMin, _options.ArrivalMax);
            int nextStormTime = (int)Math.Round(-_options.StormEx * Math.Log(_random.NextDouble()));
            int totalStormTime = nextStormTime;

            AddIntoTimeline(new(nextShipTime, TimeAction.ShipArrival));
            AddIntoTimeline(new(nextStormTime, TimeAction.StormBegin));

            foreach (var ship in _fourthShips)
                AddIntoTimeline(new(0, TimeAction.FourthShipArrival));

            int t = 0;
            while (t < hours)
            {
                var tuple = _timeLine.First();
                _timeLine.RemoveAt(0);
                t = tuple.Item1;
                if (t > hours) break;

                IPier pier;
                switch (tuple.Item2)
                {
                    case TimeAction.ShipArrival:
                        var newShip = AddShipToQueue();
                        nextShipTime = _random.Next(_options.ArrivalMin, _options.ArrivalMax);
                        newShip.WaitingTime = t;
                        if (_options.PrintSteps) Console.WriteLine($"{t}: прибыл корабль типа {newShip.Type}");
                        stats.ShipsArrived++;

                        AddIntoTimeline(new(t + nextShipTime, TimeAction.ShipArrival));
                        break;
                    case TimeAction.StormBegin:
                        var stormTime = _random.Next(_options.StormMin, _options.StormMax);
                        totalStormTime += stormTime;
                        _isStorming = true;

                        if (_options.PrintSteps) Console.WriteLine($"{t}: начался шторм на {stormTime} часов");
                        stats.StormsAmount++;
                        stats.StormsTimes.Add(t, t + stormTime);

                        AddIntoTimeline(new(t + stormTime, TimeAction.StormEnd));
                        break;
                    case TimeAction.StormEnd:
                        nextStormTime = (int)Math.Round(-_options.StormEx * Math.Log(_random.NextDouble()));
                        _isStorming = false;

                        if (_options.PrintSteps) Console.WriteLine($"{t}: шторм закончился");
                        stats.StormsIntervals.Add(nextStormTime);

                        AddIntoTimeline(new(t + nextStormTime, TimeAction.StormBegin));
                        break;
                    case TimeAction.ShipLoaded:
                        pier = _timeLinePiers.First(p => Equals(p.Value, tuple)).Key;

                        stats.ShipsLoaded++;
                        if (stats.LoadingProbability.ContainsKey(t))
                            stats.LoadingProbability[t] = (float)stats.ShipsLoaded / stats.ShipsArrived;
                        else stats.LoadingProbability.Add(t, (float)stats.ShipsLoaded / stats.ShipsArrived);

                        _timeLinePiers.Remove(pier);
                        if (_isStorming)
                        {
                            int stormEnd = _timeLine.First(line => line.Item2 == TimeAction.StormEnd).Item1;
                            Tuple<int, TimeAction> newTuple = new(stormEnd, TimeAction.ShipAfterStorm);
                            AddIntoTimeline(newTuple);
                            _timeLinePiers.Add(pier, newTuple);
                        }
                        else
                        {
                            if (pier.Ship is { Type: ShipType.Fourth } fourthShip)
                            {
                                _fourthShips[fourthShip] = t + _random.Next(_options.FourthShipArrivalMin,
                                    _options.FourthShipArrivalMax);
                                AddIntoTimeline(new(_fourthShips[fourthShip], TimeAction.FourthShipArrival));
                                
                            }
                            if (_options.PrintSteps) Console.WriteLine(
                                $"{t}: корабль {pier.Ship.Type} уплыл из {_piers.IndexOf(pier) + 1} пирс");
                            pier.Ship = null;
                        }


                        break;
                    case TimeAction.ShipAfterStorm:
                        pier = _timeLinePiers.First(p => Equals(p.Value, tuple)).Key;

                        if (pier.Ship is { Type: ShipType.Fourth } fourthStormShip)
                        {
                            _fourthShips[fourthStormShip] = t + _random.Next(_options.FourthShipArrivalMin,
                                _options.FourthShipArrivalMax);
                            AddIntoTimeline(new(_fourthShips[fourthStormShip], TimeAction.FourthShipArrival));
                        }
                        if (_options.PrintSteps) Console.WriteLine(
                            $"{t}: корабль {pier.Ship.Type} уплыл из {_piers.IndexOf(pier) + 1} пирс");
                        pier.Ship = null;

                        _timeLinePiers.Remove(pier);

                        break;
                    case TimeAction.FourthShipArrival:
                        var ship = _fourthShips.First(pair => pair.Value == t).Key;
                        _queue.Enqueue(ship);
                        _fourthShips[ship] = -1;

                        if (_options.PrintSteps) Console.WriteLine($"{t}: прибыл корабль типа {ShipType.Fourth}");
                        stats.ShipsArrived++;
                        break;
                }


                if (_queue.Count > 0)
                {
                    var empties = _piers.Where(p => p.Ship == null).ToList();
                    if (empties.Count > 0)
                    {
                        pier = empties[(int)Math.Floor(_random.NextDouble() * empties.Count)];
                        GetShipFromQueue(pier, t, stats);
                        Tuple<int, TimeAction> newTuple = new(t + pier.Time, TimeAction.ShipLoaded);
                        AddIntoTimeline(newTuple);

                        _timeLinePiers.Add(pier, newTuple);
                    }
                }
            }

            stats.AverageLoadingTime = (float)stats.LoadingTimes.Sum() / stats.LoadingTimes.Count;
            stats.QueueSize = _queue.Count;
            stats.ShipsRate = new Dictionary<ShipType, double>()
            {
                { ShipType.First, _amounts[0] / _amounts.Sum() }, { ShipType.Second, _amounts[1] / _amounts.Sum() },
                { ShipType.Third, _amounts[2] / _amounts.Sum() }, { ShipType.Fourth, _amounts[3] / _amounts.Sum() }
            };
            stats.AverageStormsInterval = (float)totalStormTime / stats.StormsAmount;

            return stats;
        });
    }

    private void GetShipFromQueue(IPier pier, int realTime, HarbourStatistics stats)
    {
        IShip ship = _queue.Dequeue();

        switch (ship.Type)
        {
            case ShipType.First:
                _amounts[0]++;
                break;
            case ShipType.Second:
                _amounts[1]++;
                break;
            case ShipType.Third:
                _amounts[2]++;
                break;
            case ShipType.Fourth:
                _amounts[3]++;
                break;
        }

        pier.Set(ship,
            ship.LoadingTime + _random.Next(-ship.LoadingTimeInterval, ship.LoadingTimeInterval + 1));
        
        stats.LoadingTimes.Add(pier.Time);
        stats.WaitingTimes.Add(realTime - ship.WaitingTime);

        if (_options.PrintSteps)
            Console.WriteLine(
                $"{realTime}: корабль {ship.Type} встал в {_piers.IndexOf(pier) + 1} пирс c временем {pier.Time}");
    }

    private void AddIntoTimeline(Tuple<int, TimeAction> tuple)
    {
        //if (tuple.Item1 > _initialHours) return;

        if (_timeLine.Count > 0 && _timeLine.FirstOrDefault(t => t.Item1 > tuple.Item1) is { } first)
            _timeLine.Insert(_timeLine.IndexOf(first), tuple);
        else
            _timeLine.Add(tuple);
    }

    private IShip AddShipToQueue()
    {
        double p = _random.NextDouble();
        ShipType type = ShipType.First;

        double rate = 0;
        foreach (var pair in _options.ShipsRate)
        {
            rate += pair.Value;
            type = pair.Key;
            if (p <= rate) break;

        }

        var ship = _shipFactory.CreateShip(type);
        _queue.Enqueue(ship);

        return ship;
    }
}