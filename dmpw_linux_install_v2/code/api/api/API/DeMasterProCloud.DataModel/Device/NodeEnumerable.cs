using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DeMasterProCloud.DataModel.Device
{
    public class Node
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "parentId")]
        [JsonIgnore]
        public int? ParentId { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string BuildingName { get; set; }
        [JsonProperty(PropertyName = "devices")]
        public List<SimpleData> Devices { get; set; }
        [JsonProperty(PropertyName = "children")]
        public List<Node> Children { get; set; }
    }

    public static class NodeEnumerable
    {
        public static IList<Node> BuildTree(this IEnumerable<Node> source, int level = 0)
        {
            var groups = source.GroupBy(i => i.ParentId);

            if (groups.FirstOrDefault() == null)
                return null;

            var roots = source.Where(m => m.ParentId == null).ToList();


            if (roots.Count > 0)
            {
                var dict = groups.Where(g => g.Key.HasValue).ToDictionary(g => g.Key.Value, g => g.ToList());
                for (int i = 0; i < roots.Count; i++)
                {
                    if (level > 0)
                    {
                        if (i == level)
                            break;
                    }

                    AddChildren(roots[i], dict);
                }
            }

            return roots;
        }

        private static void AddChildren(Node node, IDictionary<int, List<Node>> source)
        {
            if (source.ContainsKey(node.Id))
            {
                node.Children = source[node.Id];
                foreach (var item in node.Children)
                    AddChildren(item, source);
            }
            else
            {
                //node.Children = new List<Node>();
                node.Children = null;
            }
        }
    }

    public class SimpleData{
        public int Id { get; set; }
        
        public string Name { get; set; }

        public string DeviceAddress { get; set; }

        public int ActiveTZId { get; set; }

        public bool IsAssigned { get; set; }
    }
}
