using helpers.Exceptions;
using SharedLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedLib.Providers.CategoryProvider
{
    public abstract class BaseConfigurationProvider
    {
        public const string InvalidCategoryIdMessage = "Specified Category Id Not Found";
        public const string InvalidPackageIdMessage = "Specified Package Id Not Found";
        public const string NullCategoriesMessage = "No categories Found";
        protected List<Category> inMemoryCategoryList;
        public static string InvalidProductIdMessage = "Invalid Product ID";

        public virtual async Task<Category> GetCategoryById(string categoryId, double amount)
        {
            var selectedCategory = inMemoryCategoryList.FirstOrDefault(category => category.CategoryId == categoryId && (category.MinAmount <= amount && category.MaxAmount >= amount));
            if (selectedCategory == null) throw new CustomException(InvalidCategoryIdMessage);
            return selectedCategory;
        }
        public virtual async Task<Product> GetProductById(long productId)
        {
            var correspondingCategory = inMemoryCategoryList.FirstOrDefault(category => category.Products.Any(product => product.Id == productId));
            if (correspondingCategory == null) throw new CustomException(InvalidProductIdMessage);
            var selectedProduct = correspondingCategory.Products.FirstOrDefault(product => product.Id == productId);
            if (selectedProduct == null) throw new CustomException(InvalidProductIdMessage);

            return selectedProduct;
        }
        public virtual async Task<Category> GetCategoryByProductId(long productId)
        {
            var correspondingCategory = inMemoryCategoryList.FirstOrDefault(category => category.Products != null && category.Products.Any(product => product.Id == productId));
            if (correspondingCategory == null) throw new CustomException(InvalidProductIdMessage);

            return correspondingCategory;
        }

        public virtual async Task<Category> GetCategoryByProductAmount(double amount)
        {
            Category correspondingCategory = null;
            correspondingCategory = inMemoryCategoryList.FirstOrDefault(category => category.MinAmount <= amount && category.MaxAmount >= amount && category.IsSpecial);
            if (correspondingCategory == null) throw new CustomException(InvalidCategoryIdMessage);
            return correspondingCategory;
        }

        public virtual async Task<Category> GetCategoryByProductAmount(double amount, string categoryNameStart)
        {
            Category correspondingCategory = null;
            correspondingCategory = inMemoryCategoryList.FirstOrDefault(category => category.CategoryId.ToLower().StartsWith(categoryNameStart.ToLower()) && category.MinAmount <= amount && category.MaxAmount >= amount && category.IsSpecial);

            if (correspondingCategory == null) throw new CustomException(InvalidCategoryIdMessage);

            return correspondingCategory;
        }

        public virtual async Task<bool> HasSpecialCategory(double amount)
        {
            var allSpecialCategories = inMemoryCategoryList.Where(category => category.IsSpecial);
            if (allSpecialCategories.Count() == 0) return false;
            var correspondingCategory = allSpecialCategories.FirstOrDefault(category => category.MinAmount <= amount && category.MaxAmount >= amount && category.IsSpecial);
            if (correspondingCategory == null || correspondingCategory.Products == null || correspondingCategory.Products.Count == 0) return false;
            return true;
        }
        public virtual async Task<bool> HasSpecialCategory(double amount, string categoryNameStart)
        {

            var allSpecialCategories = inMemoryCategoryList.Where(category => category.IsSpecial);
            if (allSpecialCategories == null || !allSpecialCategories.Any()) return false;

            var correspondingCategory = allSpecialCategories
                                            .FirstOrDefault(category =>
                                                        category.CategoryId.ToLower().StartsWith(categoryNameStart.ToLower()) &&
                                                        category.MinAmount <= amount && category.MaxAmount >= amount &&
                                                        category.IsSpecial);

            if (correspondingCategory == null || correspondingCategory.Products == null || correspondingCategory.Products.Count == 0)
                return false;

            return true;
        }
        public virtual async Task<Product> GetProductByProductAmount(double amount)
        {
            var correspondingCategory = inMemoryCategoryList.FirstOrDefault(category => category.MinAmount == amount && category.MaxAmount == amount);
            if (correspondingCategory == null || correspondingCategory.Products == null || !correspondingCategory.Products.Any()) throw new InvalidOperationException(InvalidProductIdMessage);
            var correspondingProduct = correspondingCategory.Products.First();
            return correspondingProduct;
        }
        public virtual async Task<Product> GetProductByCategoryIdAndAmount(long categoryId, double amount)
        {
            var correspondingCategory = inMemoryCategoryList.FirstOrDefault(category => category.MinAmount == amount && category.MaxAmount == amount);

            if (correspondingCategory == null || correspondingCategory.Products == null || !correspondingCategory.Products.Any()) throw new InvalidOperationException(InvalidProductIdMessage);
            var correspondingProduct = correspondingCategory.Products.First();
            return correspondingProduct;
        }
        public virtual async Task<List<Category>> GetCategories()
        {
            if (inMemoryCategoryList == null || !inMemoryCategoryList.Any()) throw new CustomException(NullCategoriesMessage);

            return inMemoryCategoryList;
        }
        public virtual async Task<(Category Category, Product Product)> GetProductWithCategory(string categoryId, double amountInCedis)
        {
            Category category = null;
            if (string.IsNullOrEmpty(categoryId))
            {
                category = await GetCategoryByProductAmount(amountInCedis, "");
            }
            else
            {
                if (categoryId.ToLower().Contains("special"))
                {
                    var categoryArray = categoryId.Split("_");
                    if (await HasSpecialCategory(amountInCedis, (categoryArray[0] != null ? categoryArray[0] : "")))
                        category = await GetCategoryByProductAmount(amountInCedis, (categoryArray[0] != null ? categoryArray[0] : ""));
                    else
                        category = await GetCategoryById(categoryId, amountInCedis);
                }
                else
                {
                    category = await GetCategoryById(categoryId, amountInCedis);
                }
            }

            Product product = category.Products.FirstOrDefault(c => amountInCedis >= c.MinAmount && amountInCedis <= c.MaxAmount);

            return (category, product);
        }



    }
}
