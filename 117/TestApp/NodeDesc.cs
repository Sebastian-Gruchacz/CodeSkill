namespace CodeSkill17.TestApp
{
    public struct NodeDesc
    {
        public string Name;
        public string Parent;
        public int? Value;

        public NodeDesc(string name, string parent, int? value)
        {
            this.Name = name;
            this.Parent = parent;
            this.Value = value;
        }
    }
}