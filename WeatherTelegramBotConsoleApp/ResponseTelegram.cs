using Newtonsoft.Json;

namespace WeatherTelegramBotConsoleApp
{
    public class ResponseTelegram
    {
        public bool Ok { get; set; }
        public Result[] Result { get; set; }
    }

    public class Result
    {
        [JsonProperty("update_id")]
        public int UpdateId { get; set; }
        public Message Message { get; set; }
    }

    public class Message
    {
        [JsonProperty("message_id")]
        public int MessageId { get; set; }
        public From From { get; set; }
        public Chat Chat { get; set; }
        public int Date { get; set; }
        public string Text { get; set; }
        public Entity[] Entities { get; set; }
    }

    public class From
    {
        public int Id { get; set; }

        [JsonProperty("is_bot")]
        public bool IsBot { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        public string Username { get; set; }

        [JsonProperty("language_code")]
        public string LanguageCode { get; set; }
    }

    public class Chat
    {
        public int Id { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        public string Username { get; set; }
        public string Type { get; set; }
    }

    public class Entity
    {
        public int Offset { get; set; }
        public int Length { get; set; }
        public string Type { get; set; }
    }
}
