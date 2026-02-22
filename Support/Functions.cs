using Microsoft.Playwright;
using PlaywrightReqnRollCSharp.Steps;
using Reqnroll;
using System.Collections.Concurrent;
using System.Xml.Linq;

namespace PlaywrightReqnRollCSharp.Support;

public class Functions
{
    private static readonly ConcurrentDictionary<string, string> _cachedScripts = new();

    public static string GetSqlScript(string fileName)
    {
        return _cachedScripts.GetOrAdd(fileName, name =>
        {
            var script = File.ReadAllText($"{Hooks.RuntimeDirectory}/SqlScripts/{name}.sql");
            return script;
        });
    }

    public static Dictionary<string, List<string>> ConvertReqnRollDatatableToDictionary(Reqnroll.DataTable dataTable)
    {
        var data = new Dictionary<string, List<string>>();
        foreach (string header in dataTable.Header)
        {
            var values = dataTable.Rows.Select(row => (object)row[header]).ToList();
            var updatedvalues = new List<string>();
            foreach (string value in values)
            {
                updatedvalues.Add(GenerateRandomConversion(value));
            }
            data.Add(header, updatedvalues);
        }
        return data;
    }

    public static Dictionary<string, string> ConvertReqnRollVeriticalDatatableToDictionary(Reqnroll.DataTable dataTable)
    {
        var data = new Dictionary<string, string>();
        foreach (var nameValuePair in dataTable.Rows)
        {
            var nvArray = nameValuePair.Values;
            var propertyName = nvArray.First().Trim();
            var propertyValue = GenerateRandomConversion(nvArray.Last().Trim());
            data.Add(propertyName, propertyValue.ToString());
        }
        return data;
    }

    public static Dictionary<string, string> ConvertReqnRollDatatableFirstRowToDictionary(Reqnroll.DataTable dataTable)
    {
        var data = new Dictionary<string, string>();
        if (dataTable.Rows.Count > 0)
        {
            foreach (string header in dataTable.Header)
            {
                var value = dataTable.Rows[0][header];
                data.Add(header, GenerateRandomConversion(value));
            }
        }
        return data;
    }

    public static string GenerateRandomConversion(string value)
    {
        var randomtext = value.Trim().Split(',');
        string uVal = value;
        foreach(var rt in randomtext)
        {
            switch (rt)
            {
                case string s when s.Contains("GetTodaysDateTime"):
                    var todayDate = DateTime.Now;
                    int plusIndex = s.LastIndexOf('+');
                    int minusIndex = s.LastIndexOf('-');
                    int operatorIndex = Math.Max(plusIndex, minusIndex);
                    if (operatorIndex == -1)
                    {
                        uVal = uVal.Replace("GetTodaysDateTime", $"{todayDate:MM/dd/yyyy}&{todayDate:HH:mm}");
                    }
                    else
                    {
                        int daysOffset = int.Parse(s[operatorIndex..]);
                        uVal = uVal.Replace(s, $"{todayDate.AddDays(daysOffset):MM/dd/yyyy}&{todayDate:HH:mm}");
                    }
                    break;
                case string s when s.Contains("GetTodaysDate"):
                    todayDate = DateTime.Today;
                    plusIndex = s.LastIndexOf('+');
                    minusIndex = s.LastIndexOf('-');
                    operatorIndex = Math.Max(plusIndex, minusIndex);
                    if (operatorIndex == -1)
                    {
                        uVal = uVal.Replace("GetTodaysDate", todayDate.ToString("MM/dd/yyyy"));
                    }
                    else
                    {
                        int daysOffset = int.Parse(s[operatorIndex..]);
                        uVal = uVal.Replace(s, todayDate.AddDays(daysOffset).ToString("MM/dd/yyyy"));
                    }
                    break;
            }
        }
        
        return uVal.Trim();
    }

    public static DateTime GenerateRandomDOB(int minAge, int maxAge)
    {
        if (minAge > maxAge)
        {
            throw new ArgumentException("minAge cannot be greater than maxAge.");
        }

        DateTime today = DateTime.Today;
        DateTime latestDOB = today.AddYears(-minAge);
        DateTime earliestDOB = today.AddYears(-(maxAge + 1)).AddDays(1);

        int totalDays = (int)(latestDOB - earliestDOB).TotalDays;
        Random random = new();
        int randomDays = random.Next(0, totalDays);
        DateTime randomDOB = earliestDOB.AddDays(randomDays);

        return randomDOB;
    }

    public static readonly object SyncLock = new();
    public static int GetRandomNumber(int min, int max)
    {
        var random = new Random();
        lock (SyncLock)
        {
            return random.Next(min, max);
        }
    }

    public async static Task<System.Data.DataTable> GetTableUsingTableId(ILocator tableId)
    {
        var th = await tableId.Locator("th").AllAsync();
        var tr = await tableId.Locator("tbody").Locator("tr").AllAsync();
        var dt = new System.Data.DataTable();

        if (th.Count == 0)
        {
            for (var i = 0; i < 12; i++)
            {
                dt.Columns.Add("Column" + i);
            }
        }

        var colNum = 1;
        foreach (var header in th)
        {
            var colName = await header.TextContentAsync();
            if (dt.Columns.Contains(colName))
            {
                colName = colName + colNum;
                colNum++;
            }
            dt.Columns.Add(colName);
        }

        foreach (var row in tr)
        {
            var dataRow = dt.NewRow();
            var i = 0;
            foreach (var td in await row.Locator("td").AllAsync())
            {
                dataRow[i] = await td.TextContentAsync();
                i++;
            }
            dt.Rows.Add(dataRow);
        }

        return dt;
    }
}
