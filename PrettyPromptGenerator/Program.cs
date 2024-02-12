var prompt = new Prompt
{
    Instruction =
        "Given user's query and current datetime, complete the following task and output answer {format} date format.\n" +
        "Generate a date query in the format \"{format}\" from a user request.\n",
    Examples = new List<Example>
    {
        new()
        {
            ExampleDict = new Dictionary<string, object>
            {
                { "current datetime", "20240209 12:00" },
                { "query", "today at 9" },
                { "output", "20240209 21:00" },
            }
        },
        new()
        {
            ExampleDict = new Dictionary<string, object>
            {
                { "current datetime", "20240209 12:00" },
                { "query", "tomorrow at 12" },
                { "output", "20240210 12:00" },
            }
        },
        new()
        {
            ExampleDict = new Dictionary<string, object>
            {
                { "current datetime", "20240209 12:00" },
                { "query", "12 of february at 12" },
                { "output", "20240212 12:00" },
            }
        },
        new()
        {
            ExampleDict = new Dictionary<string, object>
            {
                { "current datetime", "20240209 12:00" },
                { "query", "2024-02-14T12:00:00+06:00" },
                { "output", "20240214 12:00" },
            }
        },
    },
    InputKeys = new List<string> { "current datetime", "query" },
    OutputKey = "output",
    OutputType = "string",
    Language = "english"
};

var promptString = prompt
    .Format(
        new Dictionary<string, string>
        {
            { "query", "2" },
            { "current datetime", "1" },
        })
    .PromptStr;

Console.WriteLine(promptString);