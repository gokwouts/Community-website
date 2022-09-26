namespace GokoSite.Services.Data.Contracts
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using GokoSite.Web.ViewModels.Products;

    public interface IProductsService
    {
        public Task AddProduct(AddProductInputModel input);

        public Task<ICollection<ProductViewModel>> GetProducts();

        public Task<ProductDetailsViewModel> GetProduct(string id);
    }
}
