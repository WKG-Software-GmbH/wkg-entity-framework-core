﻿using Wkg.Versioning;

namespace Wkg.EntityFrameworkCore;

/// <summary>
/// Provides version information for the Wkg.EntityFrameworkCore framework.
/// </summary>
public class WkgEntityFrameworkCore : DeploymentVersionInfo
{
    private const string CI_DEPLOYMENT__VERSION_PREFIX = "0.0.0";
    private const string CI_DEPLOYMENT__VERSION_SUFFIX = "CI-INJECTED";
    private const string CI_DEPLOYMENT__DATETIME_UTC = "1970-01-01 00:00:00";

    private WkgEntityFrameworkCore() : base
    (
        CI_DEPLOYMENT__VERSION_PREFIX,
        CI_DEPLOYMENT__VERSION_SUFFIX,
        CI_DEPLOYMENT__DATETIME_UTC
    ) => Pass();

    /// <summary>
    /// Provides version information for the Wkg.EntityFrameworkCore framework.
    /// </summary>
    public static WkgEntityFrameworkCore VersionInfo { get; } = new WkgEntityFrameworkCore();
}
