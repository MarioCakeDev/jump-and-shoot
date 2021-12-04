using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Array = Godot.Collections.Array;

namespace JumpAndShoot.scripts
{
    public static class NodeExtensions
    {
        public static T GetComponent<T>(this Node node, string? nodeName = null, SearchOrder searchOrder = SearchOrder.BreadthFirst) where T : Node
        {
            T? component = searchOrder switch
            {
                SearchOrder.DepthFirst => GetComponentDepthFirst<T>(node, nodeName),
                SearchOrder.BreadthFirst => GetComponentBreadthFirst<T>(node, nodeName),
                _ => throw new ArgumentOutOfRangeException(nameof(searchOrder), $"Unexpected search order {searchOrder}.")
            };

            if (component is null)
            {
                throw new Exception($"Component of type {typeof(T)} has not been found.");
            }

            return component;
        }

        private static T? GetComponentBreadthFirst<T>(Node node, string? nodeName) where T : Node
        {
            IEnumerable<object> children = node.GetChildren().OfType<object>().ToList();
            var hasAny = true;

            while (hasAny)
            {
                hasAny = false;
                IEnumerable<object> moreChildren = System.Array.Empty<object>();
                foreach (object child in children)
                {
                    switch (child)
                    {
                        case T component when nodeName is null || component.Name == nodeName:
                            return component;
                        case Node childNode:
                        {
                            Array array = childNode.GetChildren();
                            hasAny |= array.Count != 0;
                            moreChildren = moreChildren.Concat(array.OfType<object>());
                            break;
                        }
                    }
                }

                children = moreChildren;
            }

            return null;
        }

        private static T? GetComponentDepthFirst<T>(Node node, string? nodeName) where T : Node
        {
            foreach (object child in node.GetChildren())
            {
                switch (child)
                {
                    case T component when nodeName is null || component.Name == nodeName:
                        return component;
                    case Node childNode:
                    {
                        var candidate = GetComponentDepthFirst<T>(childNode, nodeName);
                        if (candidate is not null)
                        {
                            return candidate;
                        }

                        break;
                    }
                }
            }

            return null;
        }
    }
}