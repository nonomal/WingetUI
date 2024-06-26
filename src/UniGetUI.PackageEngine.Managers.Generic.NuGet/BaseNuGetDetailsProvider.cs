﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using UniGetUI.Core.IconEngine;
using UniGetUI.Core.Logging;
using UniGetUI.Core.Tools;
using UniGetUI.PackageEngine.Classes.Manager.BaseProviders;
using UniGetUI.PackageEngine.Classes.Manager.ManagerHelpers;
using UniGetUI.PackageEngine.ManagerClasses.Manager;
using UniGetUI.PackageEngine.Managers.Generic.NuGet.Internal;
using UniGetUI.PackageEngine.PackageClasses;
using static UniGetUI.PackageEngine.Classes.Manager.ManagerHelpers.ManagerSource;

namespace UniGetUI.PackageEngine.Managers.PowerShellManager
{
    internal class BaseNuGetDetailsProvider : BasePackageDetailsProvider<PackageManager>
    {
        public BaseNuGetDetailsProvider(BaseNuGet manager) : base(manager) { }

        protected override async Task<PackageDetails> GetPackageDetails_Unsafe(Package package)
        {
            PackageDetails details = new(package);
            try
            {
                details.ManifestUrl = PackageManifestLoader.GetPackageManifestUrl(package);
                string? PackageManifestContents = await PackageManifestLoader.GetPackageManifestContent(package);
                if (PackageManifestContents == null)
                {
                    Logger.Warn($"No manifest content could be loaded for package {package.Id} on manager {package.Manager.Name}, returning empty PackageDetails");
                    return details;
                }

                // details.InstallerUrl = new Uri($"https://globalcdn.nuget.org/packages/{package.Id}.{package.Version}.nupkg");
                details.InstallerUrl = PackageManifestLoader.GetPackageNuGetPackageUrl(package);
                details.InstallerType = CoreTools.Translate("NuPkg (zipped manifest)");
                details.InstallerSize = await CoreTools.GetFileSizeAsync(details.InstallerUrl);

                foreach (Match match in Regex.Matches(PackageManifestContents, @"<name>[^<>]+<\/name>"))
                {
                    details.Author = match.Value.Replace("<name>", "").Replace("</name>", "");
                    details.Publisher = match.Value.Replace("<name>", "").Replace("</name>", "");
                    break;
                }

                foreach (Match match in Regex.Matches(PackageManifestContents, @"<d:Description>[^<>]+<\/d:Description>"))
                {
                    details.Description = match.Value.Replace("<d:Description>", "").Replace("</d:Description>", "");
                    break;
                }

                foreach (Match match in Regex.Matches(PackageManifestContents, @"<updated>[^<>]+<\/updated>"))
                {
                    details.UpdateDate = match.Value.Replace("<updated>", "").Replace("</updated>", "");
                    break;
                }

                foreach (Match match in Regex.Matches(PackageManifestContents, @"<d:ProjectUrl>[^<>]+<\/d:ProjectUrl>"))
                {
                    details.HomepageUrl = new Uri(match.Value.Replace("<d:ProjectUrl>", "").Replace("</d:ProjectUrl>", ""));
                    break;
                }

                foreach (Match match in Regex.Matches(PackageManifestContents, @"<d:LicenseUrl>[^<>]+<\/d:LicenseUrl>"))
                {
                    details.LicenseUrl = new Uri(match.Value.Replace("<d:LicenseUrl>", "").Replace("</d:LicenseUrl>", ""));
                    break;
                }

                foreach (Match match in Regex.Matches(PackageManifestContents, @"<d:PackageHash>[^<>]+<\/d:PackageHash>"))
                {
                    details.InstallerHash = match.Value.Replace("<d:PackageHash>", "").Replace("</d:PackageHash>", "");
                    break;
                }

                foreach (Match match in Regex.Matches(PackageManifestContents, @"<d:ReleaseNotes>[^<>]+<\/d:ReleaseNotes>"))
                {
                    details.ReleaseNotes = match.Value.Replace("<d:ReleaseNotes>", "").Replace("</d:ReleaseNotes>", "");
                    break;
                }

                foreach (Match match in Regex.Matches(PackageManifestContents, @"<d:LicenseNames>[^<>]+<\/d:LicenseNames>"))
                {
                    details.License = match.Value.Replace("<d:LicenseNames>", "").Replace("</d:LicenseNames>", "");
                    break;
                }

                return details;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return details;
            }
        }

        protected override async Task<CacheableIcon?> GetPackageIcon_Unsafe(Package package)
        {
            var PackageManifestContent = await PackageManifestLoader.GetPackageManifestContent(package);
            if (PackageManifestContent == null)
            {
                Logger.Warn($"No manifest content could be loaded for package {package.Id} on manager {package.Manager.Name}");
                return null;
            }

            var possibleIconUrl = Regex.Match(PackageManifestContent, "<(?:d\\:)?IconUrl>(.*)<(?:\\/d:)?IconUrl>");

            if (!possibleIconUrl.Success)
            {
                Logger.Warn($"No Icon URL could be parsed on the manifest Url={PackageManifestLoader.GetPackageManifestUrl(package).ToString()}");
                return null;
            }

            Logger.Debug($"A native icon with Url={possibleIconUrl.Groups[1].Value} was found");
            return new CacheableIcon(new Uri(possibleIconUrl.Groups[1].Value), package.Version);
        }

        protected override Task<Uri[]> GetPackageScreenshots_Unsafe(Package package)
        {
            throw new NotImplementedException();
        }

        protected override async Task<string[]> GetPackageVersions_Unsafe(Package package)
        {
            Uri SearchUrl = new Uri($"{package.Source.Url}/FindPackagesById()?id='{package.Id}'");
            Logger.Debug($"Begin package version search with url={SearchUrl} on manager {Manager.Name}"); ;
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.All
            };

            using (HttpClient client = new HttpClient(handler))
            {
                var response = await client.GetAsync(SearchUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Logger.Warn($"Failed to fetch api at Url={SearchUrl} with status code {response.StatusCode} to load versions");
                    return [];
                }

                string SearchResults = await response.Content.ReadAsStringAsync();
                MatchCollection matches = Regex.Matches(SearchResults, "Version='([^<>']+)'");

                List<string> results = new List<string>();
                HashSet<string> alreadyProcessed = new HashSet<string>();

                foreach (Match match in matches)
                    if(!alreadyProcessed.Contains(match.Groups[1].Value) && match.Success)
                    {
                        results.Add(match.Groups[1].Value);
                        alreadyProcessed.Add(match.Groups[1].Value);
                    }

                results.Reverse();
                return results.ToArray();

            }
        }
    }
}
