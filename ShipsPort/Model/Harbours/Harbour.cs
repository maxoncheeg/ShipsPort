using System.Runtime.InteropServices.ComTypes;
using ShipsPort.Model.Ships;

namespace ShipsPort.Model.Harbours;

public class Harbour : IHarbour
{
    private readonly HarbourOptions _options;

    private List<double> _amounts;
    private bool _isStorming = false;
    private readonly Random _random = new();
    private Queue<IShip> _queue = [];
    private IShipFactory _shipFactory;
    private List<IPier> _piers;

    private Dictionary<IShip, int> _fourthShips = [];

    public IReadOnlyCollection<IShip> Piers { get; }
    public event EventHandler<StormArgs>? StormStarted;

    public Harbour(IShipFactory shipFactory, HarbourOptions options)
    {
        _options = options;
        _shipFactory = shipFactory;

        _piers = [];
        for (int i = 0; i < _options.PiersAmount; i++) _piers.Add(new Pier());

        for (int i = 0; i < _options.FourthShipAmount; i++) _fourthShips.Add(_options.FourthShip.Clone() as IShip, 1);
    }

    public Task<HarbourStatistics> Open(int hours)
    {
        return Task.Run(() =>
        {
            var stats = new HarbourStatistics();

            int h = hours;
            int nextShipTime = _random.Next(_options.ArrivalMin, _options.ArrivalMax);
            int nextStormTime = (int)Math.Round(-_options.StormEx * Math.Log(_random.NextDouble()));
            int stormTime = 0;
            int stormsAmount = 0;
            int totalStormTime = nextStormTime;
            int totalWaitingTime = 0;

            // foreach (var pair in _fourthShips)
            //      _fourthShips[pair.Key] = _random.Next(_options.FourthShipArrivalMin, _options.FourthShipArrivalMax);
            //

            _amounts = [0, 0, 0, 0];

            while (hours-- > 0)
            {
                int realTime = h - hours;

                foreach (var pair in _fourthShips)
                    if (_fourthShips[pair.Key] > 0 && --_fourthShips[pair.Key] <= 0)
                    {
                        stats.ShipsArrived++;
                        _queue.Enqueue(pair.Key);
                    }

                if (--nextShipTime == 0)
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

                    if (_options.PrintSteps) Console.WriteLine($"{realTime}: прибыл корабль типа {type}");

                    _queue.Enqueue(_shipFactory.CreateShip(type));
                    stats.ShipsArrived++;
                    nextShipTime = _random.Next(_options.ArrivalMin, _options.ArrivalMax);
                }

                if (_isStorming && --stormTime <= 0)
                {
                    totalStormTime += nextStormTime = (int)Math.Round(-_options.StormEx * Math.Log(_random.NextDouble()));
                    stats.StormsIntervals.Add(nextStormTime);
                    if (_options.PrintSteps) Console.WriteLine($"{realTime}: шторм закончился");
                    _isStorming = false;
                }

                if (!_isStorming && --nextStormTime <= 0)
                {
                    stormTime = _random.Next(_options.StormMin, _options.StormMax);
                    if (_options.PrintSteps) Console.WriteLine($"{realTime}: начался шторм на {stormTime} часов");
                    _isStorming = true;
                    stormsAmount++;
                    stats.StormsTimes.Add(h - hours, h - hours + stormTime);
                }

                foreach (var pier in _piers)
                    if (pier.Ship != null)
                    {
                        if (--pier.Time <= 0)
                        {
                            if (!_isStorming)
                            {
                                if (_options.PrintSteps)
                                    Console.WriteLine(
                                        $"{realTime}: корабль {pier.Ship.Type} уплыл из {_piers.IndexOf(pier) + 1} пирс");

                                // stats.WaitingTimes.Add(pier.Ship.WaitingTime);
                                pier.Ship = null;
                                stats.ShipsLoaded++;
                                if (stats.LoadingProbability.ContainsKey(h - hours))
                                    stats.LoadingProbability[h - hours] = (float)stats.ShipsLoaded / stats.ShipsArrived;
                                else
                                    stats.LoadingProbability.Add(h - hours,
                                        (float)stats.ShipsLoaded / stats.ShipsArrived);
                            }
                            else
                            {
                                stats.ShipsInStormTime++;
                                if (_options.PrintSteps)
                                    Console.WriteLine(
                                        $"{realTime}: корабль {pier.Ship.Type} хотел уплыть из {_piers.IndexOf(pier) + 1} пирса, но ШТОРМ!!!");
                            }

                            if (!_isStorming && _queue.Count > 0)
                            {
                                GetShipFromQueue(pier, realTime, stats);
                            }
                        }
                    }
                    else
                    {
                        if (!_isStorming && _queue.Count > 0)
                        {
                            GetShipFromQueue(pier, realTime, stats);
                        }
                    }

                foreach (var pair in _fourthShips)
                {
                    if (_piers.FirstOrDefault(p => pair.Key.Equals(p.Ship)) == null && pair.Value <= 0)

                        _fourthShips[pair.Key] =
                            _random.Next(_options.FourthShipArrivalMin, _options.FourthShipArrivalMax);
                }

                // foreach (var ship in _queue)
                //     ship.WaitingTime++;
            }

            //stats.AverageWaitingTime = (float)stats.WaitingTimes.Sum() / stats.WaitingTimes.Count;

            stats.AverageLoadingTime = (float)stats.LoadingTimes.Sum() / stats.LoadingTimes.Count;
            stats.QueueSize = _queue.Count;
            stats.ShipsRate = new Dictionary<ShipType, double>()
            {
                { ShipType.First, _amounts[0] / _amounts.Sum() }, { ShipType.Second, _amounts[1] / _amounts.Sum() },
                { ShipType.Third, _amounts[2] / _amounts.Sum() }, { ShipType.Fourth, _amounts[3] / _amounts.Sum() }
            };
            stats.StormsAmount = stormsAmount;
            stats.AverageStormsInterval = (float)totalStormTime / stormsAmount;

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

        if (_options.PrintSteps)
            Console.WriteLine(
                $"{realTime}: корабль {ship.Type} встал в {_piers.IndexOf(pier) + 1} пирс c временем {pier.Time}");
    }
}