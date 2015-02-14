namespace MHSX2
{
    public enum SexType
    {
        COMON = 0,
        MAN,
        WOMAN,
    }

    public enum JobType
    {
        COMON = 0,
        KNIGHT,
        GUNNER
    }

    public enum EquipKind
    {
        Weapon = 0,
        Head,
        Body,
        Arm,
        Waist,
        Leg,
        NumOfEquipKind
    }

    public enum ElementType
    {
        Fire = 0,
        Water,
        Thunder,
        Ice,
        Dragon,
        NumOfElementKind
    }

    public enum SkillSearchType
    {
        AND = 0,
        OR
    }

    public enum SearchClassState
    {
        Searching,
        Waiting,
        Finished
    }

    public enum SearchClassRequestResult
    {
        None,
        Sorry,
        Start
    }

    public enum JewelryDataTagInfo
    {
        Solo,
        Plus,
        Minus,
        Flat
    }

    public enum ItemClassType { Equip, Jewelry }

    public enum SlotType { Normal, SP, CuffP, CuffS }

}