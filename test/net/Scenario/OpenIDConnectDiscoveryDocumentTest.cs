//-----------------------------------------------------------------------
// <copyright file="OpenIDConnectDiscoveryDocumentTest.cs" company="Microsoft">Copyright 2012 Microsoft Corporation</copyright>
// <license>
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </license>

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.WindowsAzure.MediaServices.Client.Tests
{
    [TestClass]
    public class OpenIDConnectDiscoveryDocumentTest
    {
        private const string googleOpenConectDiscoveryUri = "https://accounts.google.com/.well-known/openid-configuration";
        private const string adOpenConectDiscoveryUri = "https://login.windows.net/common/.well-known/openid-configuration";

        [TestMethod]
        public void FetchGooleJWKKeysAndUseIdentityExtensions()
        {

            GetAndVerifyJsonWebKeys(googleOpenConectDiscoveryUri);


        }
        [TestMethod]
        public void FetchMicrosoftJWKKeysAndUseIdentityExtensions()
        {
            GetAndVerifyJsonWebKeys(adOpenConectDiscoveryUri);

        }

        private static void GetAndVerifyJsonWebKeys(string uri)
        {
            JsonWebKey key = new JsonWebKey();
            OpenIdConnectConfiguration config;
            System.Threading.CancellationTokenSource src = new System.Threading.CancellationTokenSource();
            config = OpenIdConnectConfigurationRetriever.GetAsync(uri, src.Token).Result;
            JsonWebKeySet keyset = config.JsonWebKeySet;
            Assert.IsNotNull(keyset);
            Assert.IsNotNull(keyset.GetSigningKeys());
        }
    }
}