// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources() => new List<IdentityResource>{
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
        };

        public static IEnumerable<ApiResource> GetApis() => new List<ApiResource>{
            new ApiResource("api1", "My API")
        };

        public static IEnumerable<Client> GetClients() => new List<Client>{
            new Client{
                ClientId = "client",
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedScopes = { "api1" }
            },
            new Client{
                ClientId = "ro.client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedScopes = { "api1" }
            }
        };

        public static List<TestUser> GetUsers() => new List<TestUser>{
            new TestUser{
                SubjectId = "1",
                Username = "alice",
                Password = "123456"
            },
            new TestUser{
                SubjectId = "2",
                Username = "bob",
                Password = "1qaz2wsx"
            }
        };
    }
}