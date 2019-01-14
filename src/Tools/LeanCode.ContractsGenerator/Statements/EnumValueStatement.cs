namespace LeanCode.ContractsGenerator.Statements
{
    class EnumValueStatement : IStatement
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
