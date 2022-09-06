namespace ProductWeb.Services.IService
{
    public interface IShoppingCartService<T>
    {
        void IncrementCount(T shoppingCart, int count);
        void DecrementCount(T shoppingCart, int count);

        void Save();
        void Add(T shoppingCart);
    }

}
