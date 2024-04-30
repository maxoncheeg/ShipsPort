namespace ShipsPort.Model.Ships;

public class ShipFactory : IShipFactory
{
    public IShip CreateShip(ShipType type)
    {
        return type switch
        {
            ShipType.First => new Ship(type, 18, 2),
            ShipType.Second => new Ship(type, 24, 3),
            ShipType.Third => new Ship(type, 35, 4)
        };
    }
}