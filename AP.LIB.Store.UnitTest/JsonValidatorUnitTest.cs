using AP.LIB.Common.CST;
using AP.LIB.Common.CST.Exceptions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace AP.LIB.Store.UnitTest
{
    public class JsonValidatorUnitTest
    {
        [Fact]
        void Should_IsValid_Succeed_When_Json_File_Path_Exists()
        {
            var expectedJsonObj = JObject.Parse(StoreDataConstant.jsonCatalog);
            string inpuJsonPath = @"./catalogAsJsonDocument.json";
            var validator = new JsonValidator();
            var result = validator.IsJsonValid(inpuJsonPath);
            Assert.NotNull(result);
            Assert.Equal(expectedJsonObj, result);
        }

        [Fact]
        void Should_IsValid_Throws_InvalidJsonSchemaException_When_Json_File_Path_Not_Valid()
        {
            string inpuJsonPath = @"./InvalidJsonCatalog.json";
            var validator = new JsonValidator();
            Assert.Throws<InvalidJsonSchemaException>(() => validator.IsJsonValid(inpuJsonPath));
        }
    }
}
