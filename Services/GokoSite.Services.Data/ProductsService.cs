using GokoSite.Data;
using GokoSite.Data.Models.Products;
using GokoSite.Services.Data.Contracts;
using GokoSite.Web.ViewModels.Products;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GokoSite.Services.Data
{
    public class ProductsService : IProductsService
    {
        private readonly ApplicationDbContext db;
        private readonly IHostingEnvironment hostingEnvironment;

        public ProductsService(ApplicationDbContext db, IHostingEnvironment hostingEnvironment)
        {
            this.db = db;
            this.hostingEnvironment = hostingEnvironment;
        }

        public async Task AddProduct(AddProductInputModel input)
        {
            var filename = this.UploadFile(input);

            var product = new Product()
            {
                Name = input.Name,
                Description = input.Description,
                DownloadLink = input.DownloadLink,
                MainPhoto = filename,
            };

            await this.db.Products.AddAsync(product);
            await this.db.SaveChangesAsync();
        }

        public async Task<ProductDetailsViewModel> GetProduct(string id)
        {
            var product = await this.db.Products.FirstOrDefaultAsync(p => p.ProductId == id);

            return new ProductDetailsViewModel()
            {
                Name = product.Name,
                Description = product.Description,
                DownloadLink = product.DownloadLink,
                MainImage = product.MainPhoto,
            };
        }

        public async Task<ICollection<ProductViewModel>> GetProducts()
        {
            return this.db.Products.Select(p => new ProductViewModel()
            {
                ProductId = p.ProductId,
                Name = p.Name,
                MainImage = p.MainPhoto,
            }).ToList();
        }

        private string UploadFile(AddProductInputModel input)
        {
            string fileName = null;
            if (input.MainImage != null)
            {
                string uploadDir = Path.Combine(this.hostingEnvironment.WebRootPath, "productImages");
                fileName = Guid.NewGuid().ToString() + "-" + input.MainImage.FileName;
                string filePath = Path.Combine(uploadDir, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    input.MainImage.CopyTo(fileStream);
                }
            }

            return fileName;
        }
    }
}
