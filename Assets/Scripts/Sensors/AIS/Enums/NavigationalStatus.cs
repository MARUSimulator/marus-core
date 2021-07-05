namespace Labust.Sensors.AIS
{
    public enum NavigationStatus
    {
        UnderWayUsingEngine = 0,
        AtAnchor = 1,
        NotUnderCommand = 2,
        RestrictedManeuverability = 3,
        ConstrainedByHerDraught = 4,
        Moored = 5,
        Aground = 6,
        EngagedInFishing = 7,
        UnderWaySailing = 8,
        ReservedForFutureAmendmentOfNavigationalStatusForHsc = 9,
        ReservedForFutureAmendmentOfNavigationalStatusForWig = 10,
        ReservedForFutureUse1 = 11,
        ReservedForFutureUse2 = 12,
        ReservedForFutureUse3 = 13,
        AisSartIsActive = 14,
        NotDefined = 15
    }
}
