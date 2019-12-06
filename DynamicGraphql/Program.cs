using System;
using System.Threading.Tasks;
using HelloWorldCode;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Language;
using HotChocolate.Types;

namespace HelloWord
{
    class Program
    {
        static void Main(string[] args)
        {
            ExecuteGraphQL().Wait();
        }

        static async Task ExecuteGraphQL()
        {
            ObjectType survey = new ObjectType(c =>
            {
                c.Name("Survey");
                c.Field<JsonQL>(r => r.GetElement(default)).Type<IdType>().Name("id");
                c.Field<JsonQL>(r => r.GetElement(default)).Type<IntType>().Name("stars");
                c.Field<JsonQL>(r => r.GetElement(default)).Type<StringType>().Name("commentary");
            });

            ObjectType query = new ObjectType(c =>
            {
                c.Name("Query");
                c.Field<JsonQL>(r => r.GetById(default)).Type(survey).Argument("id", a => a.Type<NonNullType<IdType>>()).Name("survey");
                c.Field<JsonQL>(r => r.GetAll()).Type(new ListType(survey)).Name("surveys");
            });

            InputObjectType surveyInput = new InputObjectType(c =>
            {
                c.Name("SurveyInput");
                c.Field("id").Type<NonNullType<IdType>>();
                c.Field("stars").Type<NonNullType<IntType>>();
                c.Field("commentary").Type<NonNullType<StringType>>();
            });

            ObjectType mutation = new ObjectType(c =>
            {
                c.Name("Mutation");
/*  ERROR          c.Field<JsonQL>(r => r.Upsert(default))
                    .Type(survey)
                    .Argument("survey", d => d.Type(surveyInput))
                    .Name("upsertSurvey"); */
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
}

