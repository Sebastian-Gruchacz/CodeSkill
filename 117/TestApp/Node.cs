namespace CodeSkill17.TestApp
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class Node
    {
        public string Name;
        [JsonIgnore]
        public Node Parent;
        public Dictionary<string, Node> Children;
        public int? Value;
    }
}