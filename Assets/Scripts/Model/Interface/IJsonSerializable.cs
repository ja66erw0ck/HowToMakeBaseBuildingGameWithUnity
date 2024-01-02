using Newtonsoft.Json.Linq;

namespace Model.Interface
{
    public interface IJsonSerializable
    {
        void FromJson(JToken token);
        JToken ToJson();
    }
}
