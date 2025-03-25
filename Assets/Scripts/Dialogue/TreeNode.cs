using System.Collections.Generic;

public class TreeNode
{
    public Dictionary<string, string> value = new Dictionary<string, string>();
    private List<TreeNode> children = new List<TreeNode>();
    public void AddChild(TreeNode child)
    {
        children.Add(child);
    }
    public List<TreeNode> GetChildren()
    {
        return children;
    }
    public void AddType(string typeValue)
    {
        value.Add("type", typeValue);
    }
    public string GetNodeType()
    {
        return value["type"];
    }
}
