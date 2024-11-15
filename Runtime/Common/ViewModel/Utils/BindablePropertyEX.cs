namespace MMDarkness
{
    public static class BindablePropertyEX
    {
        public static IBindableProperty<T> AsBindableProperty<T>(this IBindableProperty property)
        {
            return property as IBindableProperty<T>;
        }
    }
}