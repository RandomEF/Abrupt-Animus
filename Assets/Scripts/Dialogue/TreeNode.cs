using System.Collections.Generic;
using UnityEngine;

public class TreeNode
{
    public Dictionary<string, string> value = new Dictionary<string, string>(); // Dictionary for values
    private List<TreeNode> children = new List<TreeNode>(); // List of children
    public void AddChild(TreeNode child)
    { // Add a child
        children.Add(child);
    }
    public List<TreeNode> GetChildren()
    { // Get every child
        return children;
    }
    public void AddType(string typeValue)
    { // Add a type to the node
        Debug.Log($"Adding type '{typeValue}'");
        value.Add("type", typeValue); // Add the type
    }
    public string GetNodeType()
    { // Get the node's type
        return value["type"];
    }
}
