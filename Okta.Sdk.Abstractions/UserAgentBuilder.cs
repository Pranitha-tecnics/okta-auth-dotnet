// <copyright file="UserAgentBuilder.cs" company="Okta, Inc">
// Copyright (c) 2018 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Okta.Sdk.Abstractions
{
    /// <summary>
    /// A User-Agent string generator that uses reflection to detect the current assembly version.
    /// </summary>
    public sealed class UserAgentBuilder
    {
        // Lazy ensures this only runs once and is cached.
        /// <summary>
        /// Defines the _cachedUserAgent.
        /// </summary>
        private readonly Lazy<string> _cachedUserAgent;

        /// <summary>
        /// Defines the _oktaSdkUserAgentName.
        /// </summary>
        private string _oktaSdkUserAgentName = string.Empty;

        /// <summary>
        /// Defines the _sdkVersion.
        /// </summary>
        private Version _sdkVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAgentBuilder"/> class.
        /// </summary>
        public UserAgentBuilder()
        {
            _cachedUserAgent = new Lazy<string>(this.Generate);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAgentBuilder"/> class.
        /// </summary>
        /// <param name="sdkUserAgentName">The user agent name. i.e: okta-sdk-dotnet.</param>
        /// <param name="sdkVersion">The sdk version.</param>
        public UserAgentBuilder(string sdkUserAgentName, Version sdkVersion)
        {
            _cachedUserAgent = new Lazy<string>(Generate);
            _oktaSdkUserAgentName = sdkUserAgentName;
            _sdkVersion = sdkVersion;
        }

        /// <summary>
        /// Constructs a User-Agent string.
        /// </summary>
        /// <returns>A User-Agent string with the default tokens, and any additional tokens.</returns>
        public string GetUserAgent() => _cachedUserAgent.Value;

        /// <summary>
        /// The Generate.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        private string Generate()
        {
            //var sdkToken = $"{_oktaSdkUserAgentName}/{GetSdkVersion()}";

            var machineName = $"machineName/{Environment.MachineName}";

            var machineIP = $"machineIP/{this.GetLocalIPAddress()}";
            var runtimeToken = $"runtime/{UserAgentHelper.GetRuntimeVersion()}";

            var operatingSystemToken = $"os/{Sanitize(RuntimeInformation.OSDescription)}";

            return string.Join(
                " ",
                machineName,
                machineIP,
                operatingSystemToken,
                runtimeToken)
            .Trim();
        }

        /// <summary>
        /// The GetLocalIPAddress.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        //private string GetSdkVersion()
        //{
        //    return $"{_sdkVersion.Major}.{_sdkVersion.Minor}.{_sdkVersion.Build}";
        //}

        /// <summary>
        /// Defines the IllegalCharacters.
        /// </summary>
        private static readonly char[] IllegalCharacters = new char[] { '/', ':', ';' };

        /// <summary>
        /// The Sanitize.
        /// </summary>
        /// <param name="input">The input<see cref="string"/>.</param>
        /// <returns>The <see cref="string"/>.</returns>
        private static string Sanitize(string input)
            => IllegalCharacters.Aggregate(input, (current, bad) => current.Replace(bad, '-'));
    }
}
