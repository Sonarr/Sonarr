using System;
using System.Collections.Generic;
using NzbDrone.Core.Authentication;

namespace Sonarr.Http.Authentication
{
    public static class OidcGroupMapping
    {
        /// <summary>The group-to-tier mapping used for backward-compatible resolution.</summary>
        public static readonly Dictionary<string, string> GroupToTier = new(StringComparer.OrdinalIgnoreCase)
        {
            ["pirates"] = "T1",
            ["homies"] = "T2",
            ["family"] = "T3",
            ["maintenance"] = "T4",
            ["authentik Admins"] = "T5",
        };

        /// <summary>The tier-to-role mapping used for backward-compatible resolution.</summary>
        public static readonly Dictionary<string, SonarrRole> TierToRole = new(StringComparer.OrdinalIgnoreCase)
        {
            ["T2"] = SonarrRole.ReadOnly,
            ["T3"] = SonarrRole.AddSearch,
            ["T4"] = SonarrRole.Manage,
            ["T5"] = SonarrRole.Admin,
        };

        /// <summary>Resolves a user's group membership to a SonarrRole using the configured mappings.</summary>
        /// <param name="groups">The user's OIDC groups from the token.</param>
        /// <param name="mappings">Semicolon-separated key=value pairs (e.g., "homies=ReadOnly;family=AddSearch").</param>
        /// <returns>The highest role among all matching groups, or null if no match found.</returns>
        public static SonarrRole? Resolve(IEnumerable<string> groups, string mappings)
        {
            // Empty or whitespace-only mappings → deny-all (fail closed)
            if (string.IsNullOrWhiteSpace(mappings))
            {
                return null;
            }

            var groupToRole = new Dictionary<string, SonarrRole>(StringComparer.OrdinalIgnoreCase);

            foreach (var pair in mappings.Split(';'))
            {
                // Trim each pair and skip empty entries
                var trimmed = pair.Trim();
                if (trimmed.Length == 0)
                {
                    continue;
                }

                // Parse key=value — ignore malformed pairs
                var eqIndex = trimmed.IndexOf('=');
                if (eqIndex < 1 || eqIndex >= trimmed.Length - 1)
                {
                    // Malformed: no '=' or empty side → ignore this pair
                    continue;
                }

                var groupKey = trimmed.Substring(0, eqIndex).Trim();
                var roleValue = trimmed.Substring(eqIndex + 1).Trim();

                if (string.IsNullOrWhiteSpace(groupKey) || string.IsNullOrWhiteSpace(roleValue))
                {
                    // Empty side → ignore this pair
                    continue;
                }

                // Normalize role to lowercase for comparison and store the enum value
                var normalizedRole = roleValue.Trim().ToLowerInvariant();

                // Check against valid SonarrRole enum values (case-insensitive)
                switch (normalizedRole)
                {
                    case "readonly":
                        groupToRole[groupKey] = SonarrRole.ReadOnly;
                        break;
                    case "addsearch":
                        groupToRole[groupKey] = SonarrRole.AddSearch;
                        break;
                    case "manage":
                        groupToRole[groupKey] = SonarrRole.Manage;
                        break;
                    case "admin":
                        groupToRole[groupKey] = SonarrRole.Admin;
                        break;
                    default:
                        // Unknown/unrecognized role → ignore this pair
                        continue;
                }
            }

            // No valid mappings parsed → deny-all
            if (groupToRole.Count == 0)
            {
                return null;
            }

            var bestRole = (SonarrRole?)null;

            foreach (var group in groups ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(group))
                {
                    continue;
                }

                // Case-insensitive lookup of the group
                if (!groupToRole.TryGetValue(group, out var role))
                {
                    continue;
                }

                if (bestRole == null || role > bestRole.Value)
                {
                    bestRole = role;
                }
            }

            return bestRole ?? null; // no match found → deny-all
        }

        /// <summary>
        /// Resolves a user's group membership to a SonarrRole using a static tier-based mapping.
        /// This is retained for backward compatibility with existing callers that may not pass mappings.
        /// </summary>
        public static SonarrRole? Resolve(IEnumerable<string> groups)
        {
            if (groups == null)
            {
                return null;
            }

            var groupToTier = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            groupToTier.Add("pirates", "T1");
            groupToTier.Add("homies", "T2");
            groupToTier.Add("family", "T3");
            groupToTier.Add("maintenance", "T4");
            groupToTier.Add("authentik Admins", "T5");

            var tierToRole = new Dictionary<string, SonarrRole>(StringComparer.OrdinalIgnoreCase);
            tierToRole.Add("T2", SonarrRole.ReadOnly);
            tierToRole.Add("T3", SonarrRole.AddSearch);
            tierToRole.Add("T4", SonarrRole.Manage);
            tierToRole.Add("T5", SonarrRole.Admin);

            var best = (SonarrRole?)null;

            foreach (var group in groups)
            {
                if (string.IsNullOrWhiteSpace(group))
                {
                    continue;
                }

                if (!groupToTier.TryGetValue(group, out var tier))
                {
                    continue;
                }

                if (!tierToRole.TryGetValue(tier, out var role))
                {
                    continue;
                }

                if (best == null || role > best.Value)
                {
                    best = role;
                }
            }

            return best;
        }
    }
}
