namespace Model.Interface
{
    public interface ISelectable
    {
        public string GetName();
        public string GetDescription();
        // for indestructible things (if any?) this is allowed to return blank (or null maybe??)
        public string GetHitPointString();
    }
}
