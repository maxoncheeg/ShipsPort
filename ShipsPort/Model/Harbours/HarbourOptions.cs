using ShipsPort.Model.Ships;

namespace ShipsPort.Model.Harbours;

public class HarbourOptions
{
    public bool PrintSteps { get; set; } = false;
    public int PiersAmount { get; set; } = 3;
    public int ArrivalMin { get; set; } = 1;
    public int ArrivalMax { get; set; } = 8;

    public int StormMin { get; set; } = 2;
    public int StormMax { get; set; } = 7;
    public int StormEx { get; set; } = 48;

    public Dictionary<ShipType, float> ShipsRate { get; set; } = new()
    {
        { ShipType.First, 0.25f }, 
        { ShipType.Second, 0.55f },
        { ShipType.Third, 0.2f }
    };

    public int FourthShipAmount { get; set; } = 5;
    public IShip FourthShip { get; set; } = new Ship(ShipType.Fourth, 21, 3);
    public int FourthShipArrivalMin { get; set; } = 216;
    public int FourthShipArrivalMax { get; set; } = 265;
}