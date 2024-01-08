using System.Text.Json;
using System.Text.Json.Nodes;
using AutoBogus;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NServiceBus.Configuration.AdvancedExtensibility;
using NServiceBus.MessageInterfaces.MessageMapper.Reflection;
using NServiceBus.Serialization;
using NServiceBus.Settings;
using SerializerCompatTest.Messages;

var newtonsoftSerializer = CreateNewtonsoftMessageSerializer();
// some settings might be required to achieve equal output
var systemJsonSerializer = CreateSystemJsonMessageSerializer(new JsonSerializerOptions { IncludeFields = true, WriteIndented = true });

var messages = new object[]
{
    CreateMessage<PocoMessage>(),
    CreateMessage<ClassWithFields>(),
    CreateMessage<IInterfaceMessage>()
};

foreach (var message in messages)
{
    var newtonsoftSerializedMessage = SerializeMessage(newtonsoftSerializer, message);
    var systemJsonSerializedMessage = SerializeMessage(systemJsonSerializer, message);

    Console.WriteLine($"Message {message} compatibility (System.Text.Json): {JsonNode.DeepEquals(JsonNode.Parse(newtonsoftSerializedMessage),  JsonNode.Parse(systemJsonSerializedMessage))}");
    Console.WriteLine($"Message {message} compatibility (System.Text.Json): {JToken.DeepEquals(JsonConvert.DeserializeObject<JObject>(newtonsoftSerializedMessage), JsonConvert.DeserializeObject<JObject>(systemJsonSerializedMessage))}");
}

string SerializeMessage(IMessageSerializer serializer, object message)
{
    using var stream = new MemoryStream();
    serializer.Serialize(message, stream);
    stream.Position = 0;
    using var reader = new StreamReader(stream);
    return reader.ReadToEnd();
}

T CreateMessage<T>() where T : class
{
    AutoFaker<T> faker = new AutoFaker<T>();
    return faker.Generate();
}

IMessageSerializer CreateNewtonsoftMessageSerializer()
{
    var newtonsoftJsonSerializer = new NewtonsoftJsonSerializer();
    var newtonsoftSettings = new SerializationExtensions<NewtonsoftJsonSerializer>(new SettingsHolder(), new SettingsHolder());
    return newtonsoftJsonSerializer.Configure(newtonsoftSettings.GetSettings())(new MessageMapper());
}

IMessageSerializer CreateSystemJsonMessageSerializer(JsonSerializerOptions jsonOptions)
{
    var systemJsonSerializer = new SystemJsonSerializer();
    var systemJsonSettings = new SerializationExtensions<SystemJsonSerializer>(new SettingsHolder(), new SettingsHolder());
    systemJsonSettings.Options(jsonOptions);
    return systemJsonSerializer.Configure(systemJsonSettings.GetSettings())(new MessageMapper());
}
