namespace ShipsPort.Model.Ships;

public interface IShipFactory
{
    public IShip CreateShip(ShipType type);
}