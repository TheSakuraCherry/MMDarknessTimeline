namespace MMDarkness
{
    public interface IDirector : IData
    {
        float Length { get; }

        void SaveToAssets();
    }
}