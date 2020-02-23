using AP.LIB.Common.CST.Exceptions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace AP.LIB.Common.CST
{
    public class JsonValidator : IJsonValidator
    {
        private readonly JsonSchema _jsonSchema;

        public JsonValidator()
        {
            _jsonSchema = JsonSchema.Parse(StoreDataConstant.schemaJsonCatalog);

        }

        /// <summary>
        /// Check if json document store catalog price and categories is valid
        /// </summary>
        /// <param name="json"> json document of catalog and categories</param>
        /// <returns></returns>
        public JObject IsJsonValid(string json)
        {            
            JObject catalogAsObject;
            
            catalogAsObject = JObject.Parse(File.ReadAllText(json));

            if(catalogAsObject != null)
            {
                bool valid = catalogAsObject.IsValid(_jsonSchema, out IList<string> messages);

                if (messages != null && messages.Any() && !valid)
                {
                    throw new InvalidJsonSchemaException(messages.ToString());
                }
            }
            return catalogAsObject;
        }
    }
}
