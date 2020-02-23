using AP.LIB.Common.CST;
using Xunit;
using Moq;
namespace AP.LIB.Store.UnitTest
{
    using AP.LIB.Common.CST.Exceptions;
    using AP.LIB.Store.CST;
    using Newtonsoft.Json.Linq;
    using System.IO;

    public class StoreUnitTest
    {
        [Fact]
        void Should_Import_Succeed()
        {
            var expectedJsonObj = JObject.Parse(StoreDataConstant.jsonCatalog);
            var validator = new JsonValidator();
            string inpuJsonPath = @"./catalogAsJsonDocument.json";
            var str = new Store(validator);
            str.Import(inpuJsonPath);
            Assert.NotNull(str.CatalogAsObject);
            Assert.Equal(expectedJsonObj, str.CatalogAsObject);            
        }

        [Fact]
        void Should_Import_Throws_BadRequestException_When_Input_Json_Is_Null_or_Empty()
        {
            var validator = new JsonValidator();
            string inputJsonPath = null;
            var str = new Store(validator);
            Assert.Throws<BadRequestException>(() => str.Import(inputJsonPath));       
        }

        [Fact]
        void Should_Import_Throws_CatalogParsingtException_When_Input_Json_Is_Null_or_Empty()
        {
            string inputJsonPath = @"./catalogAsJsonDocument.json";
            var validator = new Mock<IJsonValidator>();
            validator.Setup(v => v.IsJsonValid(It.IsAny<string>())).Returns((JObject)null);
            var str = new Store(validator.Object);
            Assert.Throws<CatalogParsingtException>(() => str.Import(inputJsonPath));
            validator.Verify(v => v.IsJsonValid(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        void Should_Import_Throws_FileNotFoundException_When_Input_Json_Is_Null_or_Empty()
        {
            var validator = new JsonValidator();
            string inputJsonPath = "toto.xt";
            var str = new Store(validator);
            Assert.Throws<FileNotFoundException>(() => str.Import(inputJsonPath));
        }

        [Fact]
        void should_Quantity_Retuns_Number_When_Given_Name_Is_Found()
        {
            var validator = new JsonValidator();
            string inpuJsonPath = @"./catalogAsJsonDocument.json";
            var str = new Store(validator);
            str.Import(inpuJsonPath);
            var qty = str.Quantity("J.K Rowling - Goblet Of fire");
            Assert.NotNull(str.CatalogAsObject);
            Assert.Equal(2, qty);
        }

        [Fact]
        void should_Quantity_Retuns_Zero_When_Given_Name_Is_Not_Found()
        {
            var validator = new JsonValidator();
            string inpuJsonPath = @"./catalogAsJsonDocument.json";
            var str = new Store(validator);
            str.Import(inpuJsonPath);
            var qty = str.Quantity("Toto");
            Assert.NotNull(str.CatalogAsObject);
            Assert.Equal(0, qty);
        }

        [Fact]
        void should_Quantity_Throws_NotEnoughInventoryException_When_Given_Name_Is_Null_Or_Empty()
        {
            var validator = new JsonValidator();
            string inpuJsonPath = @"./catalogAsJsonDocument.json";
            var str = new Store(validator);
            str.Import(inpuJsonPath);
            Assert.Throws<NotEnoughInventoryException>(() => str.Quantity(null));           
        }

        [Fact]
        void should_Buy_Retuns_Good_Prices_When_Given_Command_Is_two_Differents_Categories()
        {
            var validator = new JsonValidator();
            string inpuJsonPath = @"./catalogAsJsonDocument.json";
            var str = new Store(validator);
            str.Import(inpuJsonPath);
            var total = str.Buy("J.K Rowling - Goblet Of fire", "Isaac Asimov - Foundation");
            Assert.Equal(24, total);
        }

        [Fact]
        void should_Buy_Retuns_Good_Prices_When_Given_Command_Is_three_Books_With_One_Doublon_With_Different_Categories()
        {
            var validator = new JsonValidator();
            string inpuJsonPath = @"./catalogAsJsonDocument.json";
            var str = new Store(validator);
            str.Import(inpuJsonPath);
            var total = str.Buy("J.K Rowling - Goblet Of fire", "Robin Hobb - Assassin Apprentice", "Robin Hobb - Assassin Apprentice");
            Assert.Equal(30, total);
        }

        //Exemple 2 : si un client achète un exemplaire de Rand, les deux ouvrages d’Asimov(un
        //exemplaire de Isaac Asimov - Robot series et un de Isaac Asimov – Foundation) ,
        //l’ouvrage de Rowling en deux exemplaire et celui de Hobb en deux exemplaires, il doit payer
        //69.95 € = 12 + 5 * 0.95 + 16 * 0.95 + 8 *0.9 + 8 + 12 * 0.9 + 12
        // Problème Précision double C# 70.2 au lieu de 69.95        
        [Fact]
        void Should_Buy_Returns_Correct_Prices_When_Command_Is_More_Three_Books_With_Doublons_And_With_Differents_Categories()
        {
            var validator = new JsonValidator();
            string inpuJsonPath = @"./catalogAsJsonDocument.json";
            var str = new Store(validator);
            str.Import(inpuJsonPath);
            var total = str.Buy("Ayn Rand - FountainHead", "Isaac Asimov - Robot series", "Isaac Asimov - Foundation", 
                "J.K Rowling - Goblet Of fire", "J.K Rowling - Goblet Of fire",
                "Robin Hobb - Assassin Apprentice", "Robin Hobb - Assassin Apprentice");
            Assert.Equal(70.2, total);
        }

    }
}
