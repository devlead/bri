using System.Text;

namespace BRI.Tests;

public static class Constants
{
    public static class Request
    {
        public static class Method
        {
            public const string
                Post = "POST",
                Get = "GET";
        }
    }

    public static class ACR
    {
        public const string
            ContainerService = "bri.azurecr.io",
            Repo = "bicep/modules/bri";

        public static class Digest
        {
            public const string
                Tag = "sha256:b5bd971dc2dacc73937e5b9cb731e02c857845aa00b7a8124ee0afdd24fd621e",
                Blob = "sha256:91d9146823b1bbca091af39418cce0b98e7b031e63039666362d147b80f8ef52",
                BlobOptionIfExists = "sha256:ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad";

        }

        public static class Request
        {
            public static class Uri
            {
                public static class Oauth2
                {
                    public const string
                        Exchange = $"https://{ContainerService}/oauth2/exchange",
                        Token = $"https://{ContainerService}/oauth2/token";
                }

                public const string
                    Catalog = $"https://{ContainerService}/acr/v1/_catalog",
                    Tag = $"https://{ContainerService}/acr/v1/{Repo}/_tags?orderby=timedesc&n=1",
                    Manifest = $"https://{ContainerService}/v2/{Repo}/manifests/{Digest.Tag}",
                    Blob = $"https://{ContainerService}/v2/{Repo}/blobs/{Digest.Blob}",
                    BlobOptionIfExists = $"https://{ContainerService}/v2/{Repo}/blobs/{Digest.BlobOptionIfExists}";
            }
        }

        public static class Response
        {
            public static class Json
            {
                public const string
                    Oauth2Token =
                            $$"""
                            {
                                "access_token": "DUMMY_ACCESS_TOKEN"
                            }
                            """,
                    Oauth2Exchange =
                            $$"""
                            {
                                "refresh_token": "DUMMY_REFRESH_TOKEN"
                            }
                            """,
                    Catalog =
                            $$"""
                            {
                                "repositories": ["bicep/modules/bri"]
                            }
                            """,
                    Manifests =
                            $$"""
                            {
                                "schemaVersion": 2,
                                "config": {
                                "mediaType": "application/vnd.ms.bicep.module.config.v1+json",
                                "digest": "sha256:e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
                                "size": 0,
                                "annotations": {}
                                },
                                "layers": [
                                {
                                    "mediaType": "application/vnd.ms.bicep.module.layer.v1+json",
                                    "digest": "sha256:91d9146823b1bbca091af39418cce0b98e7b031e63039666362d147b80f8ef52",
                                    "size": 2389,
                                    "annotations": {}
                                },
                                {
                                    "mediaType": "application/vnd.ms.bicep.module.layer.v1+json",
                                    "digest": "sha256:ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad",
                                    "size": 3001,
                                    "annotations": {}
                                }
                                ]
                            }
                            """;
                public static class Repo
                {
                    public const string
                        Tag =
                                $$"""
                                {
                                    "name": "2.0.0.0",
                                    "digest": "sha256:b5bd971dc2dacc73937e5b9cb731e02c857845aa00b7a8124ee0afdd24fd621e",
                                    "createdTime": "2023-01-18T12:24:27.6928322Z",
                                    "lastUpdateTime": "2023-01-18T12:24:27.6928322Z",
                                    "signed": false,
                                    "changeableAttributes": {
                                        "deleteEnabled": true,
                                        "writeEnabled": true,
                                        "readEnabled": true,
                                        "listEnabled": true
                                    }
                                }
                                """,
                        Tags =
                                $$"""
                                {
                                    "registry": "{{ContainerService}}",
                                    "imageName": "{{ACR.Repo}}",
                                    "tags": [
                                {{Tag}}
                                    ]
                                }
                                """;
                    public static string Module { get; } = GetResourceString("bri.json");
                    public static string ModuleOptionIfExists { get; } = GetResourceString("bri_optionifexists.json");
                }
            }
        }
    }
    public static class MediaType
    {
        public const string
            Json = "application/json",
            BicepModuleLayer = "application/vnd.ms.bicep.module.layer.v1+json";
    }

    private static string GetResourceString(string filename, Encoding? encoding = null)
    {
        using var stream = GetResourceStream(filename);
        using var reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static Stream GetResourceStream(string filename)
    {
        var resourceStream = typeof(BriServiceProviderFixture)
                                .Assembly
                                .GetManifestResourceStream($"BRI.Tests.Resources.{filename}");

        return resourceStream
            ?? throw new Exception($"Failed to get stream for {filename}.");
    }
}
