using AP.LIB.Common.CST;
using AP.LIB.Common.CST.Exceptions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AP.LIB.Store.UnitTest")]
namespace AP.LIB.Store.CST
{


    public class Store : IStore
    {
        private readonly IJsonValidator _jsonValidator;
        private readonly JObject _catalogAsObject;


        public JObject CatalogAsObject { get; set; }
        public Store()
        {
            _jsonValidator = new JsonValidator();
            _catalogAsObject = new JObject();
        }

        public Store(IJsonValidator jsonValidator)
        {
            _jsonValidator = jsonValidator;
        }

        public Store(string jsonFilePath)
        {
            Import(jsonFilePath);
        }
        internal double GetPriceForOneSetOfBook(double discount, NameQuantity basketByName)
        {
            double totalPrice = 0.0;
            var catalogTag = CatalogAsObject[StoreDataConstant.CATALOG].SingleOrDefault(q => q[StoreDataConstant.NAME].Value<string>() == basketByName.Name);
            if (catalogTag == null)
            {
                return 0.0;
            }
            if (Quantity(basketByName.Name) < basketByName.Quantity)
            {
                throw new NotEnoughInventoryException();
            }

            for (int i = 0; i < basketByName.Quantity; i++)
            {
                if (i == 0 && basketByName.Quantity > 1)
                {
                    totalPrice = catalogTag[StoreDataConstant.PRICE].Value<double>() * (1-discount);
                }
                else if( i == 0 && basketByName.Quantity == 1)
                {
                    totalPrice = catalogTag[StoreDataConstant.PRICE].Value<double>();
                }
                else
                {
                    totalPrice += catalogTag[StoreDataConstant.PRICE].Value<double>();
                }
      
            }
            //Updating the Quantity
            catalogTag[StoreDataConstant.QUANTITY] = Quantity(basketByName.Name) - basketByName.Quantity;
            return totalPrice;
        }

        /// <summary>
        /// Get the total Prices of list of Name quantity associated to a category
        /// </summary>
        /// <param name="discount">discount of category</param>
        /// <param name="basketByNames"> list of Name and quantity of basket element of category</param>
        /// <returns> total price of list of Name and quantity of category</returns>
        internal double GetPriceForDifferentsSetOfBooksInTheSameCategory(double discount, List<NameQuantity> basketByNames)
        {
            double totalPrice = 0.0;

            for (int i = 0; i < basketByNames.Count; i++)
            {
                if (i == 0 && basketByNames.Count > 1 && basketByNames[i].Quantity == 1)
                {
                    totalPrice = GetPriceForOneSetOfBook(discount, basketByNames[i]) * (1-discount);
                }
                else
                {
                    totalPrice += GetPriceForOneSetOfBook(discount, basketByNames[i]);
                }
             
            }

            return totalPrice;
        }

        /// <summary>
        /// Get Total Prices of Basket Command
        /// </summary>
        /// <param name="basketByCategories"> the collection of basket per categories</param>
        /// <returns></returns>
        internal double GetPriceForTotalBaskets(Dictionary<string, List<NameQuantity>> basketByCategories)
        {
            var totalPrice = 0.0;
            foreach (var elt in basketByCategories)
            {
                var discountTag = CatalogAsObject[StoreDataConstant.CATEGORY].SingleOrDefault(q => q[StoreDataConstant.NAME].Value<string>() == elt.Key);
                if (discountTag != null)
                {
                    double discount = discountTag[StoreDataConstant.DISCOUNT].Value<double>();
                    totalPrice += GetPriceForDifferentsSetOfBooksInTheSameCategory(discount, elt.Value);
                }
            }
            return totalPrice;
        }

        /// <summary>
        /// Fill the dictionary which mapping a categorie to list of Name and quantity of basket element
        /// </summary>
        /// <param name="basketByNames">the dictionary which mapping a categorie to list of Name and quantity of basket element</param>
        /// <returns>the dictionary which mapping a categorie to list of Name and quantity of basket element</returns>
        internal Dictionary<string, List<NameQuantity>> LoadBasketCategoriesDictionary(params string[] basketByNames)
        {
            var nameByQuantities = basketByNames.GroupBy(n => n).Select(np => new NameQuantity { Name = np.Key, Quantity = np.Count() }).ToList();

            var catalogs = CatalogAsObject[StoreDataConstant.CATALOG].ToList();

            var categoriesOfBasket = from e1 in catalogs
                                     join e2 in nameByQuantities
                                         on e1[StoreDataConstant.NAME].Value<string>() equals e2.Name
                                     select
                                     (e1[StoreDataConstant.CATEGORY].Value<string>(), e2.Name, e2.Quantity);

            var categoryOfBasket = from e1 in catalogs
                                   join e2 in basketByNames
                                       on e1[StoreDataConstant.NAME].Value<string>() equals e2
                                   select
                                   (e1[StoreDataConstant.CATEGORY].Value<string>());

            var mapBasketCategories = new Dictionary<string, List<NameQuantity>>();

            foreach (var cat in categoryOfBasket.Distinct())
            {
                mapBasketCategories.Add(cat, categoriesOfBasket.Where(e => e.Item1 == cat).Select(np => new NameQuantity { Name = np.Name, Quantity = np.Quantity }).ToList<NameQuantity>());
            }

            return mapBasketCategories;
        }

        /// <summary>
        /// Calculate the total price of the Basket course
        /// </summary>
        /// <param name="basketByNames"></param>
        /// <returns> the total price</returns>
        public double Buy(params string[] basketByNames)
        {
            double totalPrice = 0.0;
            if (basketByNames != null && basketByNames.Any())
            {
                var mapBasketToCategories = LoadBasketCategoriesDictionary(basketByNames);

                if (mapBasketToCategories != null && mapBasketToCategories.Any())
                {
                 totalPrice += GetPriceForTotalBaskets(mapBasketToCategories);
                }
            }

            return totalPrice;
        }
        /// <summary>
        /// Import the json Object from the json file path
        /// </summary>
        /// <param name="catalogAsJson">json file path catalog</param>
        public void Import(string catalogAsJson)
        {
            if (string.IsNullOrEmpty(catalogAsJson))
            {
                throw new BadRequestException(StoreDataConstant.BAD_REQUEST_CODE);
            }

            if (!File.Exists(catalogAsJson))
            {
                throw new FileNotFoundException();
            }

            CatalogAsObject = _jsonValidator.IsJsonValid(catalogAsJson);

            if (CatalogAsObject == null)
            {
                throw new CatalogParsingtException(StoreDataConstant.CATALOG_PARSING_ERRROR_CODE);
            }

        }

        /// <summary>
        /// Return the quantity of the book's name 
        /// </summary>
        /// <param name="name"> name of given book</param>
        /// <returns> the quantity of books corresponding to the name</returns>
        public int Quantity(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new NotEnoughInventoryException();
            }
            if (CatalogAsObject != null)
            {
                var tag = CatalogAsObject[StoreDataConstant.CATALOG].SingleOrDefault(q => q[StoreDataConstant.NAME].Value<string>() == name);
                if (tag != null)
                {
                    return tag[StoreDataConstant.QUANTITY].Value<int>();
                }
            }
            return 0;
        }
    }
}
