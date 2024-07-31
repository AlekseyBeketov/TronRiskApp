using System.Text.Json;
public class TransactionInfo
{
    public bool riskToken { get; set; }
    public bool zeroTransfer { get; set; }
    public bool riskAddress { get; set; }
    public bool sameTailAttach { get; set; }
    public bool riskTransaction { get; set; }
}
public class TronRisk
{
    public static async Task Main(string[] args)
    {
        String apiKey = "9ecfcd53-414c-4b55-aeaf-1d6750d8d71b";
        //String transcationHash = "853793d552635f533aa982b92b35b00e63a1c1add062c099da2450a15119bcb2";

        Console.WriteLine("Введите хеш проверочной транзакции: ");
        string transcationHash = Console.ReadLine();
        String endpointFirst = $"https://apilist.tronscanapi.com/api/security/transaction/data?hashes={transcationHash}";
        String endpointSecond = $"https://apilist.tronscanapi.com/api/transaction-info?hash={transcationHash}";

        try
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("TRON-PRO-API-KEY", apiKey);

                // Первый запрос
                HttpResponseMessage response = await client.GetAsync(endpointFirst);
                response.EnsureSuccessStatusCode();
                string responseBodyFirst = await response.Content.ReadAsStringAsync();

                // Второй запрос
                response = await client.GetAsync(endpointSecond);
                response.EnsureSuccessStatusCode();
                string responseBodySecond = await response.Content.ReadAsStringAsync();

                // Парсим JSON-строку
                var jsonDocument = JsonDocument.Parse(responseBodyFirst);
                var rootElement = jsonDocument.RootElement;

                var transactionElement = rootElement.EnumerateObject().First();

                // Парсим JSON-строку второго запроса
                var jsonDocumentSecond = JsonDocument.Parse(responseBodySecond);
                var rootElementSecond = jsonDocumentSecond.RootElement;
                if (rootElementSecond.TryGetProperty("contractData", out var contractDataElement) &&
                contractDataElement.TryGetProperty("tokenInfo", out var tokenInfoElement) &&
                tokenInfoElement.TryGetProperty("tokenLevel", out var tokenLevelElement))
                {
                    var tokenLevel = rootElementSecond.GetProperty("contractData").GetProperty("tokenInfo").GetProperty("tokenLevel").GetString();

                    // Десериализуем объект в класс TransactionInfo
                    var transactionInfo = JsonSerializer.Deserialize<TransactionInfo>(transactionElement.Value.GetRawText());

                    Console.WriteLine($"\nДанные об уровне риска транзакции {transcationHash}:");

                    if (transactionInfo.riskToken)
                        Console.WriteLine("Введенный токен действительно является токеном риска");
                    else
                        Console.WriteLine("Введенный не является токеном риска");

                    if (transactionInfo.riskTransaction)
                        Console.WriteLine("Введенная транзакция является рискованной");
                    else
                        Console.WriteLine("Введенная транзакция не является рискованной");

                    Console.WriteLine("Дополнительная информация:");

                    switch (Convert.ToInt32(tokenLevel))
                    {
                        case 0:
                            Console.WriteLine("Уровень токена: 0 (неизвестный)");
                            break;
                        case 1:
                            Console.WriteLine("Уровень токена: 1 (нейтральный)");
                            break;
                        case 2:
                            Console.WriteLine("Уровень токена: 2 (нормальный)");
                            break;
                        case 3:
                            Console.WriteLine("Уровень токена: 3 (подозрительный)");
                            break;
                        case 4:
                            Console.WriteLine("Уровень токена: 4 (небезопасный)");
                            break;
                    }

                    if (transactionInfo.zeroTransfer)
                        Console.WriteLine("Сумма перевода равна 0");
                    else
                        Console.WriteLine("Сумма перевода не равна 0");

                    if (transactionInfo.riskAddress)
                        Console.WriteLine("Адрес является рисковым");
                    else
                        Console.WriteLine("Адрес не является рисковым");

                    if (transactionInfo.sameTailAttach)
                        Console.WriteLine("Обнаружена атака с адреса, обладающего конечными символами, что и у адреса пользователя");
                    else
                        Console.WriteLine("Не обнаружена атака с адреса, обладающего конечными символами, что и у адреса пользователя");

                    // Console.WriteLine(responseBodyFirst);
                    // Console.WriteLine(responseBodySecond);
                    // Console.WriteLine(Convert.ToInt32(tokenLevel));
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}