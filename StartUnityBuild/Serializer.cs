using System.Text;
using Newtonsoft.Json;
using NLog;

namespace StartUnityBuild
{
    public static class Serializer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Encoding Encoding = new UTF8Encoding(false);

        public static void SaveStateJson(object dataObject, string filename)
        {
            var jsonString = JsonConvert.SerializeObject(dataObject);
            try
            {
                File.WriteAllText(filename, jsonString, Encoding);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
        }

        public static T? LoadStateJson<T>(string filename) where T : new()
        {
            if (!File.Exists(filename))
            {
                return new T();
            }
            var jsonString = File.ReadAllText(filename, Encoding);
            return JsonConvert.DeserializeObject<T>(jsonString);
        }
    }
}
