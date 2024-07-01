﻿using UniGetUI.Core.Logging;

namespace UniGetUI.PackageEngine.PackageClasses
{
    /// <summary>
    /// Holds the details of a Package.
    /// </summary>
    public class PackageDetails
    {
        /// <summary>
        /// The package to which this details instance corresponds
        /// </summary>
        public Package Package { get; }
        
        /// <summary>
        /// Wether this PackageDetails instance has valid data or not.
        /// To load valid data, make use of the `Load()` method
        /// </summary>
        public bool IsPopulated { get; private set; } = false;

        /// <summary>
        /// The description of the package
        /// </summary>
        public string? Description { get; set; } = null;

        /// <summary>
        /// The publisher of the package. The one(s) in charge of maintaining the package published on the package manager.
        /// </summary>
        public string? Publisher { get; set; } = null;

        /// <summary>
        /// The author of the package. Who has created the package. Usually the developer of the package.
        /// </summary>
        public string? Author { get; set; } = null;

        /// <summary>
        /// A link to the homepage of the package
        /// </summary>
        public Uri? HomepageUrl { get; set; } = null;

        /// <summary>
        /// The license name (not the URL) of the package
        /// </summary>
        public string? License { get; set; } = null;

        /// <summary>
        /// A URL pointing to the license of the package.
        /// </summary>
        public Uri? LicenseUrl { get; set; } = null;

        /// <summary>
        /// A URL pointing to the installer of the package
        /// </summary>
        public Uri? InstallerUrl { get; set; } = null;

        /// <summary>
        /// A string representing the hash of the installer.
        /// </summary>
        public string? InstallerHash { get; set; } = null;

        /// <summary>
        /// A string representing the type of the installer (.zip, .exe, .msi, .appx, tarball, etc.)
        /// </summary>
        public string? InstallerType { get; set; } = null;

        /// <summary>
        /// The size, in **MEGABYTES**, of the installer
        /// </summary>
        public double InstallerSize { get; set; } = 0;

        /// <summary>
        /// A URL pointing to the Manifest File of the package
        /// </summary>
        public Uri? ManifestUrl { get; set; } = null;

        /// <summary>
        /// The update date (aka the publication date for the latest version) of the package
        /// </summary>
        public string? UpdateDate { get; set; } = null;

        /// <summary>
        /// The release notes (not the URL) for the package.
        /// </summary>
        public string? ReleaseNotes { get; set; } = null;

        /// <summary>
        /// A URL to the package release notes.
        /// </summary>
        public Uri? ReleaseNotesUrl { get; set; } = null;

        /// <summary>
        /// A list of tags that (in theory) represent the package
        /// </summary>
        public string[] Tags { get; set; } = [];

        public PackageDetails(Package package)
        {
            Package = package;
        }

        /// <summary>
        /// Loads the available package details. May override existing data.
        /// If the load succeeds, `IsPopulated` will be set to True.
        /// </summary>
        /// <returns>An asynchronous task that can be awaited</returns>
        public async Task Load()
        {
            try {
                await Package.Manager.GetPackageDetails(this);
                IsPopulated = true;
            } 
            catch (Exception ex)
            {
                Logger.Error($"PackageDetails.Load failed for package {Package.Name}");
                Logger.Error(ex);
            }
        }
    }

}
