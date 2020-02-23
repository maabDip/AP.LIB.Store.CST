using Newtonsoft.Json.Linq;

namespace AP.LIB.Common.CST
{
    public interface IJsonValidator
    {
        JObject IsJsonValid(string json);
    }
}
