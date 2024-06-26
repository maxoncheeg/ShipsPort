﻿using ShipsPort.Model.Ships;

namespace ShipsPort.Model.Harbours;

public class Pier : IPier
{
    private int _time = 0;

    public IShip? Ship { get; set; } = null;

    public int Time
    {
        get => _time;
        set { _time = value <= 0 ? 0 : value; }
    } 

    public void Set(IShip ship, int time)
    {
        Ship = ship;
        Time = time;
    }
}