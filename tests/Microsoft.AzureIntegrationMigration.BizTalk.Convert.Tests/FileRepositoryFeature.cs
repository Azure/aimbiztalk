using System;
using System.IO;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.BizTalk.Convert.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xbehave;
using Xunit;

#pragma warning disable CA1303 // Do not pass literals as localized parameters
namespace Microsoft.AzureIntegrationMigration.BizTalk.Convert.Tests
{
    /// <summary>
    /// Tests for the <see cref="FileRepository"/> class.
    /// </summary>
    public class FileRepositoryFeature
    {
        #region CreateDirectory Scenarios

        /// <summary>
        /// Scenario tests that the create directory succeeds.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="path">The path.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void CreateDirectoryWithSuccess(FileRepository repository, string path, Exception e)
        {
            "Given a repository"
                .x(() => repository = new FileRepository());

            "And a path"
                .x(() => path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

            "When creating the path"
                .x(() => e = Record.Exception(() => repository.CreateDirectory(path)))
                .Teardown(() =>
                {
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path);
                    }
                });

            "Then the creation should succeed"
                .x(() => e.Should().BeNull());

            "And the path should exist"
                .x(() =>
                {
                    Directory.Exists(path).Should().BeTrue();
                });
        }

        #endregion

        #region DoesDirectoryExist Scenarios

        /// <summary>
        /// Scenario tests that the directory exists succeeds.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="path">The path.</param>
        /// <param name="exists">The path exists or not.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DoesDirectoryExistWithSuccess(FileRepository repository, string path, bool exists, Exception e)
        {
            "Given a repository"
                .x(() => repository = new FileRepository());

            "And a path"
                .x(() => path = Path.GetTempPath());

            "When checking if path exists"
                .x(() => e = Record.Exception(() => exists = repository.DoesDirectoryExist(path)));

            "Then the call should succeed"
                .x(() => e.Should().BeNull());

            "And the path should exist"
                .x(() => exists.Should().BeTrue());
        }

        #endregion

        #region DoesFileExist Scenarios

        /// <summary>
        /// Scenario tests that the file exists succeeds.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="path">The path.</param>
        /// <param name="exists">The path exists or not.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void DoesFileExistWithSuccess(FileRepository repository, string path, bool exists, Exception e)
        {
            "Given a repository"
                .x(() => repository = new FileRepository());

            "And a file"
                .x(() =>
                {
                    path = Path.GetTempFileName();
                    using var stream = File.Create(path);
                    var guid = Guid.NewGuid().ToByteArray();
                    stream.Write(guid, 0, guid.Length);
                    stream.Close();
                })
                .Teardown(() =>
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                });

            "When checking if path exists"
                .x(() => e = Record.Exception(() => exists = repository.DoesFileExist(path)));

            "Then the call should succeed"
                .x(() => e.Should().BeNull());

            "And the file should exist"
                .x(() => exists.Should().BeTrue());
        }

        #endregion

        #region DoesFileExist Scenarios

        /// <summary>
        /// Scenario tests that when an xml file is written it succeeds.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="path">The path.</param>
        /// <param name="content">The xml content.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void WriteXmlFileSuccess(FileRepository repository, string path, string content, Exception e)
        {
            "Given a repository"
                .x(() => repository = new FileRepository());

            "And a file"
                .x(() =>
                {
                    path = Path.GetTempFileName();
                    content = "<somexml></somexml>";
                })
                .Teardown(() =>
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                });

            "When writing the xml file"
                .x(() => e = Record.Exception(() => repository.WriteXmlFile(path, content)));

            "Then the call should succeed"
                .x(() => e.Should().BeNull());

            "And the file should exist"
                .x(() => File.Exists(path));
        }

        #endregion

        #region Write Json File Scenarios

        /// <summary>
        /// Scenario tests when a Json file write succeeds.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="path">The path.</param>
        /// <param name="content">The xml  content.</param>
        /// <param name="e">The thrown exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void WriteJsonFileWithSuccess(FileRepository repository, string path, dynamic jsonObject, Exception e)
        {
            var testKey1 = "testKey1";
            var testKey2 = "testKey2";
            var testContent1 = "testData1";
            var testContent2 = "testData2";

            "Given a repository"
                .x(() => repository = new FileRepository());

            "And a file"
                .x(() =>
                {
                    path = Path.GetTempFileName();
                    jsonObject = new JObject();
                    jsonObject.Add(testKey1, testContent1);
                    jsonObject.Add(testKey2, testContent2);
                })
                .Teardown(() =>
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                });

            "When checking if path exists"
                .x(() => e = Record.Exception(() => repository.WriteJsonFile(path, (JObject)jsonObject)));

            "Then the call should succeed"
                .x(() => e.Should().BeNull());

            "And the file should exist"
                .x(() => File.Exists(path));

            "And the file content should be as expected"
                .x(() =>
                {
                    using (var file = File.OpenText(path))
                    using (var reader = new JsonTextReader(file))
                    {
                        var testObject = (JObject)JToken.ReadFrom(reader);

                        var testData1 = testObject.SelectToken(testKey1);
                        testData1.Should().NotBeNull();
                        testData1.Value<string>().Should().Be(testContent1);

                        var testData2 = testObject.SelectToken(testKey2);
                        testData2.Should().NotBeNull();
                        testData2.Value<string>().Should().Be(testContent2);
                    }
                });
        }

        #endregion
    }
}
#pragma warning restore CA1303 // Do not pass literals as localized parameters
