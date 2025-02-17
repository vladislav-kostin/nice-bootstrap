using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Linq;
using System;

namespace VK.Bootstrap
{
    public static class BootstrapManager
    {
        private static List<BootstrapObjectData> _bootstrapObjects = new();
        private static GameObject _bootstrapRootObject;
        public static event Action OnCompleted;

        [RuntimeInitializeOnLoadMethod]
        static void Bootstrap()
        {
            Addressables.InitializeAsync().WaitForCompletion();
            if (BootstrapAddressExists())
            {
                CreateBootstrapRootObject();
                GenerateBootstrapObjectData();
                SortObjectsByDependencies();
                InstantiatePrefabs();
                ReorderSpawnedTransforms(); 
                OnCompleted?.Invoke();
            }
        }
        
        private static bool BootstrapAddressExists()
        {
            if (!string.IsNullOrEmpty(BootstrapSettings.Settings.BootstrapFolderAddress))
            {
                return true;
            }
            else
            {
                Debug.Log("Bootstrap address is empty. Skipping bootstrap.");
                return false;
            }
        }
        
        private static void CreateBootstrapRootObject()
        {
            _bootstrapRootObject = new GameObject(BootstrapSettings.Settings.BootstrapFolderAddress.Split('/').Last());
            if (BootstrapSettings.Settings.DontDestroyOnLoad)
            {
                GameObject.DontDestroyOnLoad(_bootstrapRootObject);
            }
        }

        static void GenerateBootstrapObjectData()
        {
            _bootstrapObjects.Clear();
            var assetsWithLocations = AddressableUtility.LoadAssetsWithLocationsAtPath<GameObject>(BootstrapSettings.Settings.BootstrapFolderAddress);

            // Create directory objects first
            var allFolderPaths = assetsWithLocations
                .Select(asset => GetHierarchyPath(asset.Location.ToString()))
                .Where(path => !string.IsNullOrEmpty(path))
                .Distinct()
                .OrderBy(path => path) // Sort folder paths alphabetically
                .ToList();

            foreach (var folderPath in allFolderPaths)
            {
                string[] folders = folderPath.Split('/');
                string currentPath = "";
                DirectoryObjectData parentData = null;

                foreach (var folder in folders)
                {
                    currentPath = string.IsNullOrEmpty(currentPath) ? folder : $"{currentPath}/{folder}";

                    if (!_bootstrapObjects.OfType<DirectoryObjectData>().Any(d => d.HierarchyPath == currentPath))
                    {
                        var folderObject = new GameObject(folder);
                        var folderObjectTransform = folderObject.transform;
                        folderObjectTransform.parent = parentData?.Transform ?? _bootstrapRootObject.transform;

                        var directoryData = new DirectoryObjectData
                        {
                            HierarchyPath = currentPath,
                            Object = folderObject,
                            Transform = folderObjectTransform,
                            ParentDirectoryObjectData = parentData
                        };
                        _bootstrapObjects.Add(directoryData);

                        parentData = directoryData;
                    }
                    else
                    {
                        parentData = _bootstrapObjects.OfType<DirectoryObjectData>().First(d => d.HierarchyPath == currentPath);
                    }
                }
            }

            // Create prefab objects
            foreach (var asset in assetsWithLocations)
            {
                var hierarchyPath = GetHierarchyPath(asset.Location.ToString());

                var prefabObjectData = new PrefabObjectData
                {
                    PrefabWithLocation = asset,
                    HierarchyPath = hierarchyPath,
                    MainScriptType = asset.Asset.GetComponent<MonoBehaviour>()?.GetType()
                };
                GatherOrderAttributes(prefabObjectData);
                _bootstrapObjects.Add(prefabObjectData);
            }
        }

        static string GetHierarchyPath(string location)
        {
            var hierarchyPath = location.Substring(BootstrapSettings.Settings.BootstrapFolderAddress.Length + 1);

            // Remove the filename, leaving only the folder structure (if any)
            int lastSlashIndex = hierarchyPath.LastIndexOf('/');
            return lastSlashIndex != -1 ? hierarchyPath.Substring(0, lastSlashIndex) : "";
        }

        static void GatherOrderAttributes(BootstrapObjectData bootstrapObjectData)
        {
            if (bootstrapObjectData.MainScriptType != null)
            {
                var orderAttr = (BootstrapOrder)Attribute.GetCustomAttribute(bootstrapObjectData.MainScriptType, typeof(BootstrapOrder));
                bootstrapObjectData.Order = orderAttr?.Order ?? 0;

                var beforeAttrs = bootstrapObjectData.MainScriptType.GetCustomAttributes(typeof(BootstrapBefore), true).Cast<BootstrapBefore>();
                var afterAttrs = bootstrapObjectData.MainScriptType.GetCustomAttributes(typeof(BootstrapAfter), true).Cast<BootstrapAfter>();

                foreach (var attr in beforeAttrs)
                {
                    bootstrapObjectData.BeforeTypes.Add(attr.Type);
                }

                foreach (var attr in afterAttrs)
                {
                    bootstrapObjectData.AfterTypes.Add(attr.Type);
                }
            }
        }

        static void SortObjectsByDependencies()
        {
            _bootstrapObjects = _bootstrapObjects.OrderBy(data => data.Order).ThenBy(data => data, new DependencyComparer(_bootstrapObjects)).ToList();
        }

        static void InstantiatePrefabs()
        {
            foreach (var data in _bootstrapObjects.OfType<PrefabObjectData>())
            {
                var instantiatedPrefab = GameObject.Instantiate(data.PrefabWithLocation.Asset, null);
                instantiatedPrefab.name = data.PrefabWithLocation.Asset.name;
                data.Object = instantiatedPrefab;
                data.Transform = instantiatedPrefab.transform;

                var parentDirectory = _bootstrapObjects.OfType<DirectoryObjectData>()
                    .FirstOrDefault(d => d.HierarchyPath == data.HierarchyPath);

                data.Transform.parent = parentDirectory?.Transform ?? _bootstrapRootObject.transform;
            }
        }

        // Reorder objects to match folder and file hierarchy
        static void ReorderSpawnedTransforms()
        {
            // Group objects by their parent
            var groupedObjects = _bootstrapObjects
                .GroupBy(data => data.Transform.parent)
                .ToList();

            foreach (var group in groupedObjects)
            {
                // Separate folders and files within the same parent
                var folders = group
                    .Where(data => data is DirectoryObjectData) // Folders
                    .OrderBy(data => data.Transform.name) // Sort folders alphabetically
                    .ToList();

                var files = group
                    .Where(data => data is PrefabObjectData) // Files
                    .OrderBy(data => data.Transform.name) // Sort files alphabetically
                    .ToList();

                // Combine folders and files, with folders first
                var sortedObjects = folders.Concat(files).ToList();

                // Apply the new order
                for (int i = 0; i < sortedObjects.Count; i++)
                {
                    sortedObjects[i].Transform.SetSiblingIndex(i);
                }
            }
        }

        public abstract class BootstrapObjectData
        {
            public GameObject Object;
            public Transform Transform;
            public string HierarchyPath;
            public int Order;
            public HashSet<Type> BeforeTypes = new HashSet<Type>();
            public HashSet<Type> AfterTypes = new HashSet<Type>();
            public Type MainScriptType;
        }

        public class PrefabObjectData : BootstrapObjectData
        {
            public AssetWithLocation<GameObject> PrefabWithLocation;
        }

        public class DirectoryObjectData : BootstrapObjectData
        {
            public DirectoryObjectData ParentDirectoryObjectData;
        }

        private class DependencyComparer : IComparer<BootstrapObjectData>
        {
            private readonly List<BootstrapObjectData> _bootstrapObjectData;
            private readonly Dictionary<Type, List<Type>> _dependencyGraph;
            private readonly HashSet<Type> _visited;
            private readonly HashSet<Type> _stack;
            private readonly List<Type> _sortedTypes;

            public DependencyComparer(List<BootstrapObjectData> bootstrapObjectData)
            {
                _bootstrapObjectData = bootstrapObjectData;
                _dependencyGraph = new Dictionary<Type, List<Type>>();
                _visited = new HashSet<Type>();
                _stack = new HashSet<Type>();
                _sortedTypes = new List<Type>();

                BuildDependencyGraph();
                TopologicalSort();
            }

            public int Compare(BootstrapObjectData a, BootstrapObjectData b)
            {
                if (a.MainScriptType == null && b.MainScriptType == null) return 0;
                if (a.MainScriptType == null) return -1;
                if (b.MainScriptType == null) return 1;

                if (a.MainScriptType == b.MainScriptType) return 0;

                int aIndex = _sortedTypes.IndexOf(a.MainScriptType);
                int bIndex = _sortedTypes.IndexOf(b.MainScriptType);

                if (aIndex < bIndex) return -1;
                if (aIndex > bIndex) return 1;

                return 0;
            }

            private void BuildDependencyGraph()
            {
                foreach (var data in _bootstrapObjectData)
                {
                    var type = data.MainScriptType;
                    if (type == null) continue;

                    if (!_dependencyGraph.ContainsKey(type))
                        _dependencyGraph[type] = new List<Type>();

                    foreach (var beforeType in data.BeforeTypes)
                    {
                        if (!_dependencyGraph.ContainsKey(beforeType))
                            _dependencyGraph[beforeType] = new List<Type>();

                        _dependencyGraph[beforeType].Add(type);
                    }

                    foreach (var afterType in data.AfterTypes)
                    {
                        if (!_dependencyGraph.ContainsKey(type))
                            _dependencyGraph[type] = new List<Type>();

                        _dependencyGraph[type].Add(afterType);
                    }
                }
            }

            private void TopologicalSort()
            {
                foreach (var type in _dependencyGraph.Keys)
                {
                    if (!_visited.Contains(type))
                        if (!DepthFirstSearch(type))
                        {
                            Debug.LogError("Circular dependency detected.");
                            return;
                        }
                }
            }

            private bool DepthFirstSearch(Type type)
            {
                _visited.Add(type);
                _stack.Add(type);

                if (!_dependencyGraph.TryGetValue(type, out var dependencies))
                {
                    _sortedTypes.Add(type);
                    return true;
                }

                foreach (var dependentType in dependencies)
                {
                    if (!_dependencyGraph.ContainsKey(dependentType))
                    {
                        Debug.LogError($"Type {dependentType} specified in dependency attributes is not present in the graph.");
                        continue;
                    }

                    if (!_visited.Contains(dependentType))
                    {
                        if (!DepthFirstSearch(dependentType))
                            return false;
                    }
                    else if (_stack.Contains(dependentType))
                    {
                        return false; // Circular dependency detected
                    }
                }

                _stack.Remove(type);
                _sortedTypes.Add(type);

                return true;
            }
        }
    }
}