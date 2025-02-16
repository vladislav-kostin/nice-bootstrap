using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Collections.Generic;

namespace VK.Bootstrap
{
    public static class AddressableUtility
    {
        public static List<IResourceLocation> GetLocationsAtPath<T>(string path) where T : Object
        {
            if (!path.EndsWith("/"))
            {
                path += "/";
            }

            var addressableLocations = new List<IResourceLocation>();
            foreach (var locator in Addressables.ResourceLocators)
            {
                foreach (var key in locator.Keys)
                {
                    if (locator.Locate(key, typeof(T), out var resourceLocations))
                    {
                        foreach (var location in resourceLocations)
                        {
                            if (location.PrimaryKey.StartsWith(path))
                            {
                                addressableLocations.Add(location);
                            }
                        }
                    }

                }
            }

            return addressableLocations;
        }

        public static List<T> LoadAssetsAtPath<T>(string path) where T : Object
        {
            var locations = GetLocationsAtPath<T>(path);
            var assets = new List<T>();
            foreach (var location in locations)
            {
                Addressables.LoadAssetAsync<T>(location).Completed += op =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        assets.Add(op.Result);
                    }
                };
            }

            return assets;
        }

        public static List<AssetWithLocation<T>> LoadAssetsWithLocationsAtPath<T>(string path) where T : Object
        {
            var locations = GetLocationsAtPath<T>(path);
            var assetsWithLocations = new List<AssetWithLocation<T>>();
            foreach (var location in locations)
            {
                var assetWithLocation = new AssetWithLocation<T>
                {
                    Asset = null,
                    Location = location
                };
                assetWithLocation.Asset = Addressables.LoadAssetAsync<T>(location).WaitForCompletion();
                // {
                //     if (op.Status == AsyncOperationStatus.Succeeded)
                //     {
                //         assetWithLocation.Asset = op.Result;
                //     }
                // };
                // Addressables.LoadAssetAsync<T>(location).Completed += op =>
                // {
                //     if (op.Status == AsyncOperationStatus.Succeeded)
                //     {
                //         assetWithLocation.Asset = op.Result;
                //     }
                // };
                if (assetWithLocation.Asset != null)
                {
                    assetsWithLocations.Add(assetWithLocation);
                }
                else
                {
                    Debug.LogError($"Failed to load asset at location: #{location}");
                }
            }

            return assetsWithLocations;
        }
    }

    public struct AssetWithLocation<T>
    {
        public T Asset;
        public IResourceLocation Location;
    }
}