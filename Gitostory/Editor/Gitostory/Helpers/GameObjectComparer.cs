using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System;

namespace GitostorySpace
{
    public enum GitostoryPrefabComparisonMismatchType
    {
        ValueMismatch,
        ComponentMismatch,
        HierarchyMismatch
    }

    public class ComparisonResult
    {
        public GitostoryPrefabComparisonMismatchType MismatchType;
        public string GameObjectPath;
        public string ComponentName;
        public string PropertyName;
        public string ValueA;
        public string ValueB;
    }

    public class GameObjectComparer
    {
        private StringBuilder comparisonSummary = new StringBuilder();
  
        public List<ComparisonResult> ComparePrefabs(GameObject prefabA, GameObject prefabB)
        {
            comparisonSummary.Clear();

            if (prefabA == null || prefabB == null)
            {
                Debug.LogError("One or both of the prefabs are null.");
                return null;
            }

            return ParseComparisonResult(CompareGameObject(prefabA, prefabB, prefabA.name));
        }

        public List<ComparisonResult> ParseComparisonResult(string comparisonSummary)
        {
            List<ComparisonResult> results = new List<ComparisonResult>();

            var lines = comparisonSummary.Split('\n');
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;

                if (line.StartsWith("VALUEMISMATCH"))
                {
                    var parts = line.Split(new string[] { "**" }, StringSplitOptions.None);
                    if (parts.Length >= 5)
                    {
                        results.Add(new ComparisonResult
                        {
                            ComponentName = parts[1],
                            PropertyName = parts[2],
                            ValueA = parts[3],
                            ValueB = parts[4],
                            MismatchType = GitostoryPrefabComparisonMismatchType.ValueMismatch
                        }); ;
                    }
                }
                else if (line.StartsWith("MISSINGCOMPONENT"))
                {
                    var parts = line.Split(new string[] { "**" }, StringSplitOptions.None);
                    var isMissingInA = parts[2] == "A";
                    results.Add(new ComparisonResult
                    {
                        ComponentName = parts[1],
                        MismatchType = GitostoryPrefabComparisonMismatchType.ComponentMismatch,
                        ValueA = isMissingInA ? "Missing" : "Present", // Refactor here to a bool not a string...
                        ValueB = isMissingInA ? "Present" : "Missing"
                    }); 
                }
                else if (line.StartsWith("HIERARCHYMISMATCH"))
                {
                    var parts = line.Split(new string[] { "**" }, StringSplitOptions.None);
                    var isMissingInA = parts[2] == "A";
                    results.Add(new ComparisonResult
                    {
                        ComponentName = parts[1],
                        MismatchType = GitostoryPrefabComparisonMismatchType.HierarchyMismatch,
                        ValueA = isMissingInA ? "Missing" : "Present", // Refactor here to a bool not a string...
                        ValueB = isMissingInA ? "Present" : "Missing"
                    });
                }
            }

            return results;
        }


        private string CompareGameObject(GameObject objA, GameObject objB, string path)
        {
            RecordDifference(path, objA, objB, "GameObject");

            var componentsA = objA.GetComponents<Component>();
            var componentsB = objB.GetComponents<Component>();

            if (componentsA.Length != componentsB.Length)
            {
                comparisonSummary.AppendLine($"Component count mismatch in {path}: {componentsA.Length} vs {componentsB.Length}");
            }

            for (int i = 0; i < Mathf.Max(componentsA.Length, componentsB.Length); i++)
            {
                var componentPath = $"{path}/Component[{i}]";

                if (i >= componentsA.Length || componentsA[i] == null)
                {
                    comparisonSummary.AppendLine($"MISSINGCOMPONENT**{componentsA[i].GetType().Name}**A");
                    continue;
                }

                if (i >= componentsB.Length || componentsB[i] == null)
                {
                    comparisonSummary.AppendLine($"MISSINGCOMPONENT**{componentsA[i].GetType().Name}**B");
                    continue;
                }

                if (componentsA[i].GetType() != componentsB[i].GetType())
                {
                    comparisonSummary.AppendLine($"{componentPath} - Component type mismatch: {componentsA[i].GetType().Name} vs {componentsB[i].GetType().Name}");
                    continue;
                }

                CompareComponentsGeneric(componentsA[i], componentsB[i], componentPath);
            }

            // Recursive comparison for child GameObjects
            for (int i = 0; i < Mathf.Max(objA.transform.childCount, objB.transform.childCount); i++)
            {
                var childPath = $"{path}/Child[{i}]";

                GameObject childA = i < objA.transform.childCount ? objA.transform.GetChild(i).gameObject : null;
                GameObject childB = i < objB.transform.childCount ? objB.transform.GetChild(i).gameObject : null;

                if (childA == null || childB == null)
                {
                    comparisonSummary.AppendLine($"HIERARCHYMISMATCH**{childPath} **" + (childA == null ? "A":"B"));
                    continue;
                }

                CompareGameObject(childA, childB, childPath);
            }

           return comparisonSummary.ToString();
        }

        private void CompareComponentsGeneric(Component compA, Component compB, string path)
        {
            if (compA.GetType() != compB.GetType())
            {
                comparisonSummary.AppendLine($"COMPONENTHIERARCHYMISMATCH**{compA.GetType()}**{compB.GetType()}");
                return;
            }

            var soA = new SerializedObject(compA);
            var soB = new SerializedObject(compB);
            var propertyA = soA.GetIterator();
            var propertyB = soB.GetIterator();

            bool enterChildren = true;
            while (propertyA.NextVisible(enterChildren) && propertyB.NextVisible(enterChildren))
            {
                // Check if the properties are equal
                if (SerializedProperty.DataEquals(propertyA, propertyB))
                {
                    enterChildren = false;
                    continue;
                }

                // Property mismatch found
                string propertyName = propertyA.displayName;
                string propertyPath = propertyA.propertyPath;
                string valueA = GetPropertyValueAsString(propertyA);
                string valueB = GetPropertyValueAsString(propertyB);
                comparisonSummary.AppendLine($"VALUEMISMATCH**{compA.GetType()}**{propertyName}**{valueA}**{valueB}");
                enterChildren = false;
            }
        }

        private static string GetPropertyValueAsString(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue.ToString();
                case SerializedPropertyType.Boolean:
                    return property.boolValue.ToString();
                case SerializedPropertyType.Float:
                    return property.floatValue.ToString();
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Color:
                    return property.colorValue.ToString();
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue ? property.objectReferenceValue.name : "null";
                case SerializedPropertyType.LayerMask:
                    return LayerMask.LayerToName(property.intValue);
                case SerializedPropertyType.Enum:
                    return property.enumNames[property.enumValueIndex];
                case SerializedPropertyType.Vector2:
                    return property.vector2Value.ToString();
                case SerializedPropertyType.Vector3:
                    return property.vector3Value.ToString();
                case SerializedPropertyType.Vector4:
                    return property.vector4Value.ToString();
                case SerializedPropertyType.Rect:
                    return property.rectValue.ToString();
                case SerializedPropertyType.ArraySize:
                    return property.arraySize.ToString();
                case SerializedPropertyType.Character:
                    return ((char)property.intValue).ToString();
                case SerializedPropertyType.AnimationCurve:
                    return property.animationCurveValue.ToString();
                case SerializedPropertyType.Bounds:
                    return property.boundsValue.ToString();
                case SerializedPropertyType.Gradient:
                    return "Gradient (not directly supported)";
                case SerializedPropertyType.Quaternion:
                    return property.quaternionValue.ToString();
                // for newer Unity versions
                case SerializedPropertyType.ExposedReference:
                    return property.exposedReferenceValue ? property.exposedReferenceValue.ToString() : "null";
                case SerializedPropertyType.FixedBufferSize:
                    return property.fixedBufferSize.ToString();
                case SerializedPropertyType.Vector2Int:
                    return property.vector2IntValue.ToString();
                case SerializedPropertyType.Vector3Int:
                    return property.vector3IntValue.ToString();
                case SerializedPropertyType.RectInt:
                    return property.rectIntValue.ToString();
                case SerializedPropertyType.BoundsInt:
                    return property.boundsIntValue.ToString();
                // You can add more cases here if you need to handle more types and i welcome any pull requests if its generic
                default:
                    return GetListOrArrayPropertyValueAsString(property);

            }
        }

        private static string GetListOrArrayPropertyValueAsString(SerializedProperty property)
        {
            if (!property.isArray)
            {
                return "";
            }

            var result = new StringBuilder();
            result.Append("[");
            for (int i = 0; i < property.arraySize; i++)
            {
                var elementProperty = property.GetArrayElementAtIndex(i);
                string elementValue = GetPropertyValueAsString(elementProperty);
                result.Append(elementValue);
                if (i < property.arraySize - 1)
                {
                    result.Append(", ");
                }
            }
            result.Append("]");
            return result.ToString();
        }




        private void RecordDifference(string path, System.Object objA, System.Object objB, string objectType)
        {
            if (objA == null || objB == null)
            {
                var isAMissing = objA == null;
                var isBMissing = objB == null;

                if (!isAMissing && isBMissing)
                {
                    comparisonSummary.AppendLine($"{path} - {objectType} is missing in Prefab B");
                }
                else if (isAMissing && !isBMissing)
                {
                    comparisonSummary.AppendLine($"{path} - {objectType} is missing in Prefab A");
                }

            }
        }
    }
}