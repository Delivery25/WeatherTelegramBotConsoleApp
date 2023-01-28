using Newtonsoft.Json;
using System.Web;
using WeatherConsoleApp;
using WeatherTelegramBotConsoleApp;

var httpClient = new HttpClient();

//основные данные для запроса
const string addressTelegramApi = "https://api.telegram.org/";
const string token = "СЮДА НУЖНО ПОДСТАВИТЬ ТОКЕН БОТА";

var idOffset = 0;

while (true)
{
    //запрос на получения сообщения от пользователя
    var response = await httpClient.GetAsync(addressTelegramApi + token + "/getUpdates?&offset=" + idOffset);
    
    //обработка ошибки
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine("Произошла внутренняя ошибка приложения: не удалось сделать запрос к боту");
        continue;
    }

    var jsonResult = response.Content.ReadAsStringAsync().Result;
    var modelResponseTelegram = JsonConvert.DeserializeObject<ResponseTelegram>(jsonResult);

    if (modelResponseTelegram.Result.Length == 0)
        continue;

    idOffset = modelResponseTelegram.Result[^1].UpdateId + 1;
    foreach (var model in modelResponseTelegram.Result)
    {
        if (model.Message == null)
            continue;

        var message = model.Message.Text;
        var chatId = model.Message.Chat.Id;
        var textMessage = string.Empty;

        if (message == @"/start")
        {
            textMessage = $"Добро пожаловать, {model.Message.From.FirstName}!"
                + "\nЯ с удовольствие буду информировать Вас о погоде."
                + "\nВаш преданный помощник, WeatherBot";
            response = await httpClient.GetAsync(addressTelegramApi + token + "/sendMessage?chat_id=" + chatId + "&text=" + textMessage);
            //обработка ошибки
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Произошла внутренняя ошибка приложения: не удалось отправить сообщение в чат {chatId}");
                continue;
            }
        }
        if (message == @"/weather")
        {
            textMessage = "Для какого города Вы хотите узнать погоду?";
            response = await httpClient.GetAsync(addressTelegramApi + token + "/sendMessage?chat_id=" + chatId + "&text=" + textMessage);
            //обработка ошибки
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Произошла внутренняя ошибка приложения: не удалось отправить сообщение в чат {chatId}");
                continue;
            }
        }
        if (!string.IsNullOrWhiteSpace(message) && message != @"/start" && message != @"/weather")
        {
            //получение данных о погоде
            const string apiKey = "СЮДА НУЖНО ПОДСТАВИТЬ КЛЮЧ";
            const string units = "metric";
            const string language = "ru";

            var uri = new Uri($"https://api.openweathermap.org/data/2.5/weather?q={HttpUtility.UrlEncode(message)}&appid={apiKey}&units={units}&lang={language}");
            response = await httpClient.GetAsync(uri);

            //обработка ошибки
            if (!response.IsSuccessStatusCode)
            {
                textMessage = $"Проверьте название города \"{message}\", если оно написано верно, то повторите попытку позднее";
                response = await httpClient.GetAsync(addressTelegramApi + token + "/sendMessage?chat_id=" + chatId + "&text=" + textMessage);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Произошла внутренняя ошибка приложения: не удалось отправить сообщение в чат {chatId}");
                }
                continue;
            }

            jsonResult = await response.Content.ReadAsStringAsync();
            var modelResponseWeather = JsonConvert.DeserializeObject<WeatherModel>(jsonResult);

            //отправка сообщения пользователю с погодой
            textMessage = $"Город {modelResponseWeather.Name}"
                          + $"\nТемпература: {Math.Round(modelResponseWeather.Main.Temp):+#;-#;0}°"
                          + $"\nНа улице {modelResponseWeather.Weather[0].Description}"
                          + $"\nОщущается как: {Math.Round(modelResponseWeather.Main.FeelsLike):+#;-#;0}°"
                          + $"\nВетер: {Math.Round(modelResponseWeather.Wind.Speed, 1)} м/с, {GetDirection(modelResponseWeather.Wind.Deg)}"
                          + $"\nВлажность: {modelResponseWeather.Main.Humidity}%"
                          + $"\nДавление: {modelResponseWeather.Main.Pressure} мм рт.ст.";

            response = await httpClient.GetAsync(addressTelegramApi + token + "/sendMessage?chat_id=" + chatId + "&text=" + textMessage);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Произошла внутренняя ошибка приложения: не удалось отправить сообщение в чат {chatId}");
                return;
            }
        }
    }
}

string GetDirection(int deg) =>
    deg switch
    {
        >= 0 and < 23 or >= 338 and <= 360 => "C",
        >= 23 and < 68 => "СВ",
        >= 68 and < 113 => "В",
        >= 113 and < 158 => "ЮВ",
        >= 158 and < 203 => "Ю",
        >= 203 and < 248 => "ЮЗ",
        >= 248 and < 292 => "З",
        >= 292 and < 338 => "CЗ",
        _ => "",
    };
