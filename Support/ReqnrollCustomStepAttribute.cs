using Reqnroll;
using Reqnroll.Bindings;

namespace PlaywrightReqnRollCSharp.Support;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class PrepareAttribute(string expression) : StepDefinitionBaseAttribute(expression, [StepDefinitionType.Given]) { }
