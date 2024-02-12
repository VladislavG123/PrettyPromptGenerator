using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class PromptValidator
{
    public static void ValidatePrompt(Prompt prompt)
    {
        if (string.IsNullOrEmpty(prompt.Instruction))
            throw new ArgumentException("Instruction cannot be empty");

        if (prompt.InputKeys == null || prompt.InputKeys.Count == 0)
            throw new ArgumentException("InputKeys cannot be empty");

        if (string.IsNullOrEmpty(prompt.OutputKey))
            throw new ArgumentException("OutputKey cannot be empty");

        if (prompt.Examples != null)
        {
            for (var no = 0; no < prompt.Examples.Count; no++)
            {
                var example = prompt.Examples[no];
                foreach (var inpKey in prompt.InputKeys)
                {
                    if (!example.ExampleDict.ContainsKey(inpKey))
                    {
                        throw new ArgumentException(
                            $"Example {no + 1} does not have the variable {inpKey} in the definition");
                    }
                }

                if (!example.ExampleDict.ContainsKey(prompt.OutputKey))
                {
                    throw new ArgumentException(
                        $"Example {no + 1} does not have the variable {prompt.OutputKey} in the definition");
                }

                if (prompt.OutputType.ToLower() == "json")
                {
                    JsonHandler.ValidateJson(example.ExampleDict[prompt.OutputKey], no, prompt.OutputKey);
                }
            }
        }
    }
}

public class JsonHandler
{
    public static void ValidateJson(object outputValue, int exampleNo, string outputKey)
    {
        if (outputValue is string outputValueStr)
        {
            try
            {
                JsonConvert.DeserializeObject<JToken>(outputValueStr);
            }
            catch (JsonException e)
            {
                throw new ArgumentException(
                    $"{outputKey} in example {exampleNo + 1} is not in valid json format: {e.Message}");
            }
        }
    }

    public static string FormatValue(object value, string outputType)
    {
        var valueStr = JsonConvert.SerializeObject(value);
        return outputType.ToLower() == "json" ? valueStr.Replace("{", "{{").Replace("}", "}}") : valueStr;
    }
}

public class Prompt
{
    public string Instruction { get; set; }
    public List<Example> Examples { get; set; } = new();
    public List<string> InputKeys { get; set; }
    public string OutputKey { get; set; }
    public string OutputType { get; set; } = "json";
    public string Language { get; set; } = "english";

    private string FormatExample(Example example)
    {
        var exampleStr = "";
        foreach (var kvp in example.ExampleDict)
        {
            var value = JsonHandler.FormatValue(kvp.Value, OutputType);
            exampleStr += $"\n{kvp.Key}: {value}";
        }

        return exampleStr;
    }

    public override string ToString()
    {
        var promptStr = Instruction + "\n";

        if (Examples != null)
        {
            foreach (var example in Examples)
            {
                promptStr += FormatExample(example) + "\n";
            }
        }

        if (InputKeys != null)
        {
            foreach (var key in InputKeys)
            {
                promptStr += $"\n{key}: {{{key}}}";
            }
        }

        if (OutputKey != null)
        {
            promptStr += $"\n{OutputKey}: \n";
        }

        return promptStr;
    }

    public string GetExampleStr(int exampleNo)
    {
        if (exampleNo >= Examples.Count)
        {
            throw new ArgumentException($"Example number {exampleNo} is out of range");
        }

        return FormatExample(Examples[exampleNo]);
    }
    
    public PromptValue Format(Dictionary<string, string> kwargs)
    {
        PromptValidator.ValidatePrompt(this);

        var inputKeysSet = new HashSet<string>(InputKeys);
        var kwargsKeysSet = new HashSet<string>(kwargs.Keys);
        if (!inputKeysSet.SetEquals(kwargsKeysSet))
        {
            throw new ArgumentException(
                $"Input variables {string.Join(", ", InputKeys)} do not match with the given parameters {string.Join(", ", kwargsKeysSet)}");
        }

        var promptBuilder = new StringBuilder(ToString());

        foreach (var (key, value) in kwargs)
        {
            promptBuilder.Replace($"{{{key}}}", value);
        }

        promptBuilder.Replace("{{", "{").Replace("}}", "}");
        
        return new PromptValue { PromptStr = promptBuilder.ToString() };
    }
}

public class BaseMessage
{
}

public class HumanMessage : BaseMessage
{
    public string Content { get; set; }
}

public class BasePromptValue
{
    public List<BaseMessage> ToMessages()
    {
        return new List<BaseMessage> { new HumanMessage { Content = ToString() } };
    }

    public override string ToString()
    {
        return PromptStr;
    }

    public string PromptStr { get; set; }
}

public class Example
{
    public Dictionary<string, object> ExampleDict { get; set; }
}

public class PromptValue : BasePromptValue
{
    public override string ToString()
    {
        return PromptStr;
    }
}