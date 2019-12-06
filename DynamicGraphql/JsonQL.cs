using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace HelloWorldCode
{
    public class JsonQL
    {
        private SingletonDataContainer data = SingletonDataContainer.Instance;

        public IEnumerable<JsonDocument> GetAll()
        {
            IEnumerable<JsonDocument> results = data.GetAll().Select(e => JsonDocument.Parse(e.Json));
            return results;
        }
        public JsonDocument GetById(int id)
        {
            JsonItem item = data.GetById(id);
            var document = JsonDocument.Parse(item.Json);
            return document;
        }

        public object GetElement([Parent] IResolverContext parent)
        {
            var name = parent.Field.Name.Value;
            IOutputType tipo = parent.Field.Type;
            JsonDocument document = parent.Source.First() as JsonDocument;
            object result = null;
            if (tipo is IdType)
            {
                var intRes = document.RootElement.GetProperty(name).GetInt32();
                result = intRes;
            }
            if (tipo is IntType)
            {
                var intRes = document.RootElement.GetProperty(name).GetInt32();
                result = intRes;
            }
            if (tipo is StringType)
            {
                var stringRes = document.RootElement.GetProperty(name).GetString();
                result = stringRes;
            }
            return result;
        }

        public JsonDocument Upsert(object input)
        {
            // store object in json format in data storage
            return null;
        }
    }

    public class JsonItem
    {
        public int Id { get; set; }
        public string Json { get; set; }
    }

    public class SingletonDataContainer
    {
        private List<JsonItem> items = new List<JsonItem>();

        private SingletonDataContainer()
        {
            items.Add(new JsonItem { Id = 1, Json = "{\"id\":1,\"stars\":5,\"commentary\":\"beautiful\"}" });
            items.Add(new JsonItem { Id = 2, Json = "{\"id\":2,\"stars\":4,\"commentary\":\"it works!\"}" });
        }
        public IEnumerable<JsonItem> GetAll()
        {
            return items;
        }

        public JsonItem GetById(int id)
        {
            return items.FirstOrDefault(i => i.Id == id);
        }

        public void Upsert(JsonItem item)
        {
            if (item.Id == 0)
            {
                items.Add(item);
            }
            else
            {
                var index = items.FindIndex(i => i.Id == item.Id);
                items[index].Json = item.Json;
            }
        }

        private static readonly SingletonDataContainer instance = new SingletonDataContainer();

        public static SingletonDataContainer Instance => instance;
    }
}
