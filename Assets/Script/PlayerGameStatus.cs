using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attribute{
    public int hp;
    public int shield;
    public int fire_ap;
    public int water_ap;
    public int air_ap;
    public int earth_ap;
    public int poison_ap;
    public int thunder_ap;
    public int ap;
}

public class PlayerGameStatus{
    public Attribute attribute;
    public List<int> cards;
}
