using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace HelloWord
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // In real application I store template on a DB table configurable by user
            // eg. {model="Survey", fields={ {name="id", type="IdType"}, {name="stars", type="IntType"}, {name="commentary", type="StringType"} } }

            // create ObjectType from template 
            ObjectType survey = new ObjectType(c =>
            {
                c.Name("Survey");
                c.Field<JsonResolvers>(r => r.GetProperty(default)).Type<IdType>().Name("id");
                c.Field<JsonResolvers>(r => r.GetProperty(default)).Type<IntType>().Name("stars");
                c.Field<JsonResolvers>(r => r.GetProperty(default)).Type<StringType>().Name("commentary");
            });

            // create InputObjectType from template 
            InputObjectType surveyInput = new InputObjectType(c =>
            {
                c.Name("SurveyInput");
                c.Field("id").Type<NonNullType<IdType>>();
                c.Field("stars").Type<NonNullType<IntType>>();
                c.Field("commentary").Type<NonNullType<StringType>>();
            });


            ObjectType query = new ObjectType(c =>
            {
                c.Name("Query");
                c.Field<JsonResolvers>(r => r.GetDocumentById(default)).Type(survey).Argument("id", a => a.Type<NonNullType<IdType>>()).Name("survey");
                c.Field<JsonResolvers>(r => r.GetDocuments()).Type(new ListType(survey)).Name("surveys");

            });


            ObjectType mutation = new ObjectType(c =>
            {
                c.Name("Mutation");
/* ERROR
                c.Field<JsonResolvers>(r => r.Upsert(default))
                    .Type(survey)
                    .Argument("survey", d => d.Type(surveyInput))
                    .Name("upsertSurvey"); 
*/
            });

            ISchema schema = SchemaBuilder.New()
                    .AddQueryType(query)
                    .AddMutationType(mutation)
                    .AddType(survey)
                    .AddType(surveyInput)
                    .Create();

            IQueryExecutor executor = schema.MakeExecutable();

            Console.WriteLine(schema.ToString());
            Console.WriteLine("-------------------------------------------------------------");

            /* 
            var request = QueryRequestBuilder.New()
                .SetQuery("mutation { upsertSurvey(servey: { id: 100, stars: 3, commentary: \"good\" }) { stars commentary } }")
                .Create();

            IExecutionResult result = await executor.ExecuteAsync(request);
            Console.WriteLine(result.ToJson());
            Console.WriteLine("-------------------------------------------------------------");
            */

            var request = QueryRequestBuilder.New()
                            .SetQuery("{ survey(id: 2) { id stars commentary } surveys { id stars commentary } }")
                            .Create();

            var result = await executor.ExecuteAsync(request);
            Console.WriteLine(result.ToJson());
        }
    }

    class JsonResolvers
    {
        private class Item
        {
            public int Id { get; set; }
            public string Json { get; set; }
        }
        private List<Item> data;
        public JsonResolvers()
        {
            data = new List<Item>();
            data.Add(new Item { Id = 1, Json = "{\"id\":1,\"stars\":5,\"commentary\":\"beautiful\"}" });
            data.Add(new Item { Id = 2, Json = "{\"id\":2,\"stars\":4,\"commentary\":\"it works!\"}" });
        }

        public IEnumerable<JsonDocument> GetDocuments()
        {
            var results = data.Select(e => JsonDocument.Parse(e.Json));
            return results;
        }
        public JsonDocument GetDocumentById(int id)
        {
            var item = data.FirstOrDefault(d => d.Id == id);
            var document = JsonDocument.Parse(item.Json);
            return document;
        }

        public object GetProperty([Parent] IResolverContext parent)
        {
            var name = parent.Field.Name.Value;
            var type = parent.Field.Type;
            var document = parent.Source.First() as JsonDocument;
            if (type is IdType)
            {
                return document.RootElement.GetProperty(name).GetInt32();
            }
            if (type is IntType)
            {
                return document.RootElement.GetProperty(name).GetInt32();
            }
            if (type is StringType)
            {
                return document.RootElement.GetProperty(name).GetString();
            }
            return null;
        }

        public JsonDocument Upsert(object input)
        {
            // store object in json format in data storage
            return null;
        }
    }
}

