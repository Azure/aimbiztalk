//-----------------------------------------------------------------------
// <copyright file="MsiDiscovererFeature.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Tests for discovering the MSI files.</summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Discover.Repositories;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Moq;
using Xbehave;
using Xunit;

namespace Microsoft.AzureIntegrationMigration.BizTalk.Discover.Tests
{
#pragma warning disable CA1303 // Do not pass literals as localized parameters
    /// <summary>
    /// Tests for discovering the MSI files.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "This is handled by xBehave.net and the background attribute.")]
    public class MsiDiscovererFeature
    {
        /// <summary>
        /// Defines a mocked file repository.
        /// </summary>
        private Mock<IFileRepository> _mockFileRespository;

        /// <summary>
        /// Builds a migration context.
        /// </summary>
        /// <returns>A migration context.</returns>
        private static MigrationContext BuildContext()
        {
            return new MigrationContext
            {
                WorkingFolder = @"c:\temp\workingfolder"
            };
        }

        #region Before Each Scenario

        /// <summary>
        /// Sets up state before each scenario.
        /// </summary>
        [Background]
        public void Setup()
        {
            "Given a new mock file repository"
                .x(() => _mockFileRespository = new Mock<IFileRepository>());
        }

        #endregion

        #region Constructor Scenarios

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null file logger is passed.
        /// </summary>
        /// <param name="discoverer">The discoverer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The discoverer exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullLogger(MsiDiscoverer discoverer, ILogger logger, IFileRepository fileRepository, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a discoverer"
                .x(() => discoverer.Should().BeNull());

            "And null logger"
                .x(() => logger.Should().BeNull());

            "And a file repository"
                .x(() => fileRepository = _mockFileRespository.Object);

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = BuildContext());

            "When constructing with a null file logger"
                .x(() => e = Record.Exception(() => new MsiDiscoverer(fileRepository, model, context, logger)));

            "Then the discoverer constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("logger"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null file repository is passed.
        /// </summary>
        /// <param name="discoverer">The discoverer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The discoverer exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullFileRepository(MsiDiscoverer discoverer, ILogger logger, IFileRepository fileRepository, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a discoverer"
                .x(() => discoverer.Should().BeNull());

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = BuildContext());

            "When constructing with a null file repository"
                .x(() => e = Record.Exception(() => new MsiDiscoverer(fileRepository, model, context, logger)));

            "Then the discoverer constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("fileRepository"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null model is passed.
        /// </summary>
        /// <param name="discoverer">The discoverer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The discoverer exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullModel(MsiDiscoverer discoverer, ILogger logger, IFileRepository fileRepository, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a discoverer"
                .x(() => discoverer.Should().BeNull());

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);
            
            "And a file repository"
                .x(() => fileRepository = _mockFileRespository.Object);

            "And a context"
                .x(() => context = BuildContext());

            "When constructing with a null model"
                .x(() => e = Record.Exception(() => new MsiDiscoverer(fileRepository, model, context, logger)));

            "Then the discoverer constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("model"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a model is passed of a type it isn't expecting.
        /// </summary>
        /// <param name="discoverer">The discoverer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The discoverer exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithModelWithUnexpectedType(MsiDiscoverer discoverer, ILogger logger, IFileRepository fileRepository, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a discoverer"
                .x(() => discoverer.Should().BeNull());

            "And a logger"
               .x(() => logger = new Mock<ILogger>().Object);
            
            "And a file repository"
                .x(() => fileRepository = _mockFileRespository.Object);

            "And a model"
                .x(() => model = new Mock<IApplicationModel>().Object);

            "And a context"
                .x(() => context = BuildContext());

            "When constructing with a null model"
                .x(() => e = Record.Exception(() => new MsiDiscoverer(fileRepository, model, context, logger)));

            "Then the discoverer constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<InvalidCastException>());
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null context is passed.
        /// </summary>
        /// <param name="discoverer">The discoverer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The discoverer exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullMigrationContext(MsiDiscoverer discoverer, ILogger logger, IFileRepository fileRepository, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a discoverer"
                .x(() => discoverer.Should().BeNull());

            "And a logger"
               .x(() => logger = new Mock<ILogger>().Object);
            
            "And a file repository"
                .x(() => fileRepository = _mockFileRespository.Object);

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "When constructing with a null context"
                .x(() => e = Record.Exception(() => new MsiDiscoverer(fileRepository, model, context, logger)));

            "Then the discoverer constructor should throw an exception"
                .x(() => e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentNullException>().Which.ParamName.Should().Be("context"));
        }

        /// <summary>
        /// Scenario tests that the object construction throws an exception when a null working directory is passed.
        /// </summary>
        /// <param name="discoverer">The discoverer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The discoverer exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithNullWorkingDirectory(MsiDiscoverer discoverer, ILogger logger, IFileRepository fileRepository, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a discoverer"
                .x(() => discoverer.Should().BeNull());

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);
            
            "And a file repository"
                .x(() => fileRepository = _mockFileRespository.Object);

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a migration context with null working directory"
                .x(() => context = new MigrationContext { WorkingFolder = null });

            "When constructing"
                .x(() => e = Record.Exception(() => new MsiDiscoverer(fileRepository, model, context, logger)));

            "Then the discoverer constructor should throw an exception"
                .x(() =>
                {
                    e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentException>().Which.ParamName.Should().Be("context");
                    e.Should().NotBeNull().And.Subject.Should().BeOfType<ArgumentException>().Which.Message.Should().Contain("WorkingDirectory");
                });
        }

        /// <summary>
        /// Scenario tests that the object is constructed when a valid file repository and migration context is passed.
        /// </summary>
        /// <param name="discoverer">The discoverer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The discoverer exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ConstructWithSuccess(MsiDiscoverer discoverer, ILogger logger, IFileRepository fileRepository, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a discoverer"
               .x(() => discoverer.Should().BeNull());

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);
            
            "And a file repository"
                .x(() => fileRepository = _mockFileRespository.Object);

            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = new MigrationContext { WorkingFolder = @"c:\temp\workingdirectory" });

            "When constructing"
                .x(() => e = Record.Exception(() => new MsiDiscoverer(fileRepository, model, context, logger)));

            "Then the discoverer constructor should succeed"
                .x(() => e.Should().BeNull());
        }

        #endregion

        #region Boostrap Scenarios

        /// <summary>
        /// Scenario to test when an MSI is discovered then the output folders are initialised.
        /// </summary>
        /// <param name="discoverer">The discoverer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="model">The application model.</param>
        /// <param name="artefactFolder">The artefact folder.</param>
        /// <param name="workingFolder">The working folder.</param>
        /// <param name="context">The migration context.</param>        
        /// <param name="e">The discoverer exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void OutputFolderInitialisedSuccessfully(MsiDiscoverer discoverer, ILogger logger, IFileRepository fileRepository, IApplicationModel model, string artefactFolder, string workingFolder, MigrationContext context, Exception e)
        {
            "Given a file repository"
                .x(() =>
                {
                    workingFolder = @"c:\temp\artefactfolder";

                    _mockFileRespository.Setup(f => f.CreateArtefactFolder(
                        It.IsAny<string>(), 
                        It.IsAny<string>()))
                    .Callback((string w, string p) => artefactFolder = Path.Combine(w, p))
                    .Returns((string w, string p) => new DirectoryInfo(Path.Combine(w, p)));

                    fileRepository = _mockFileRespository.Object; 
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);
            
            "And a model"
                .x(() => model = new AzureIntegrationServicesModel());

            "And a context"
                .x(() => context = BuildContext());

            "And a new discoverer"
                .x(() => discoverer = new MsiDiscoverer(fileRepository, model, context, logger));

            "When discovering"
                .x(() => e = Record.Exception(() => discoverer.Discover()));

            "Then the discover should succeed without error"
                .x(() => e.Should().BeNull());

            "And the output folders should be initialised"
                .x(() =>
                {
                    // verify the working folder has been set on the context
                    context.WorkingFolder.Should().NotBeNullOrEmpty().And.Subject.Should().BeOfType<string>().Which.Should().Be(artefactFolder);

                    // verify the folder has been created.
                    _mockFileRespository.Verify(f => f.CreateArtefactFolder(
                        It.Is<string>(p => p == @"c:\temp\workingfolder"),
                        It.Is<string>(p => p.StartsWith("unpack", StringComparison.InvariantCulture))),
                        Times.Once);
                });
        }

        #endregion

        #region Unpack MSI Scenarios

        /// <summary>
        /// Scenario to test when an attempt to discover an MSI which does not exist in the file repository.
        /// then an error is recorded.
        /// </summary>
        /// <param name="discoverer">The discoverer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The discoverer exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void MsiFilesCantUnpackAsFileDoesNotExist(MsiDiscoverer discoverer, ILogger logger, IFileRepository fileRepository, IApplicationModel model, MigrationContext context, Exception e)
        {
            "Given a context"
                .x(() =>
                {
                    context = new MigrationContext
                    {
                        WorkingFolder = @"c:\test-working-folder"
                    };
                });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);
            
            "And a model with a missing MSI"
                .x(() =>
                {
                    var azureModel = new AzureIntegrationServicesModel();
                    azureModel.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Name = "MsiIsMissing", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = "MsiIsMissing.msi" });
                    model = azureModel;
                });

            "And a file repository not containing the MSI"
                .x(() =>
                {
                    var msiNameToCheck = ((AzureIntegrationServicesModel)model).MigrationSource.ResourceContainers[0].ContainerLocation;

                    _mockFileRespository.Setup(f => f.CheckFileExists(msiNameToCheck)).Returns(false);

                    _mockFileRespository.Setup(f => f.CreateArtefactFolder(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                    .Returns(new DirectoryInfo(@"c:\temp\anyfolder"));

                    fileRepository = _mockFileRespository.Object;                    
                });

            "And a new discoverer"
                .x(() => discoverer = new MsiDiscoverer(fileRepository, model, context, logger));

            "When discovering"
                .x(() => e = Record.Exception(() => discoverer.Discover()));

            "Then the discover should succeed without error"
                .x(() => e.Should().BeNull());

            "And there should be an error recorded"
                .x(() =>
                {
                    context.Errors.Should().NotBeNullOrEmpty().And.HaveCount(1);
                    context.Errors.First().Message.Should().Contain(((AzureIntegrationServicesModel)model).MigrationSource.ResourceContainers[0].ContainerLocation);
                    context.Errors.First().Message.Should().Contain("not found");
                });
        }

        /// <summary>
        /// Scenario to test successful unpacking of the MSI files.
        /// </summary>
        /// <param name="discoverer">The discoverer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>        
        /// <param name="artefactFolderCreateParameter">The folder to create the artefacts folder in.</param>
        /// <param name="extractMsiParameter1">The msi to extract.</param>
        /// <param name="extractMsiParameter2">The folder to extract the msi to.</param>
        /// <param name="extractCabFilesParameter1">The cab file to extract.</param>
        /// <param name="extractCabFilesParameter2">The folder where the cab files to extract are.</param>
        /// <param name="e">The discoverer exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void MsiFilesUnpackedSuccessfully(MsiDiscoverer discoverer, ILogger logger, IFileRepository fileRepository, IApplicationModel model, MigrationContext context, string artefactFolderCreateParameter, string extractMsiParameter1, string extractMsiParameter2, string extractCabFilesParameter1, string extractCabFilesParameter2, Exception e)
        {
            var unpackFolder = @"c:\temp\unpackfolder";
            var artefactIdentifier = "TestArtefactIdentifier";
            var artefactFolder = Path.Combine(unpackFolder, artefactIdentifier);
            var msiFileName = "ValidMsi.msi";
            var cabFileName = "ITEM~0.CAB";
            var applicationWorkingFolder = Path.Combine(artefactFolder, Path.GetFileNameWithoutExtension(msiFileName));
            var msiFolder = @"c:\temp\msifolder";
            var arbitaryFileName = "arbitaryFileName.txt";

            "Given a context"
               .x(() =>
               {
                   context = new MigrationContext
                   {
                       WorkingFolder = unpackFolder
                   };
               });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);
            
            "And a model with a valid MSI"
                .x(() =>
                {
                    var azureModel = new AzureIntegrationServicesModel();
                    azureModel.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Name = Path.GetFileNameWithoutExtension(msiFileName), Type = ModelConstants.ResourceContainerMsi, ContainerLocation = Path.Combine(msiFolder, msiFileName) });
                    model = azureModel;
                });

            "And a file repository containing the MSI"
                .x(() =>
                {
                    var msiNameToCheck = ((AzureIntegrationServicesModel)model).MigrationSource.ResourceContainers[0].ContainerLocation;

                    _mockFileRespository.Setup(f => f.CheckFileExists(msiNameToCheck)).Returns(true);

                    _mockFileRespository.Setup(f => f.CreateArtefactFolder(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                    .Returns(new DirectoryInfo(Path.Combine(unpackFolder, artefactIdentifier)));

                    _mockFileRespository.Setup(f => f.CreateApplicationWorkingFolder(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                    .Returns(new DirectoryInfo(Path.Combine(artefactFolder, Path.GetFileNameWithoutExtension(msiFileName))))
                    .Callback<string, string>(
                        (p1, p2) =>
                        {
                            artefactFolderCreateParameter = p1;
                        });

                    _mockFileRespository.Setup(f => f.ExtractFilesInMsi(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                    .Callback<string, string>(
                        (p1, p2) =>
                        {
                            extractMsiParameter1 = p1;
                            extractMsiParameter2 = p2;
                        })
                    .Returns(new string[] { cabFileName, arbitaryFileName });

                    _mockFileRespository.Setup(f => f.ExtractFilesInCab(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                    .Callback<string, string>(
                        (p1, p2) =>
                        {
                            extractCabFilesParameter1 = p1;
                            extractCabFilesParameter2 = p2;
                        })
                    .Returns(new string[] { "BindingInfo.xml", "Lib.dll", "ArbitraryWebFile.html" });

                    _mockFileRespository.Setup(f => f.GetCabFiles(It.IsAny<string>())).Returns(new string[] { Path.Combine(applicationWorkingFolder, cabFileName) });

                    fileRepository = _mockFileRespository.Object;
                });

            "And a new discoverer"
                .x(() => discoverer = new MsiDiscoverer(fileRepository, model, context, logger));

            "When discovering"
                .x(() => e = Record.Exception(() => discoverer.Discover()));

            "Then the discover should succeed without error"
                .x(() => e.Should().BeNull());

            "And there should be no error recorded"
                .x(() =>
                {
                    var app = ((AzureIntegrationServicesModel)model).GetSourceModel<ParsedBizTalkApplicationGroup>().Applications[0];
                    context.Errors.Should().BeNullOrEmpty();
                });

            "And there should be a new application on the context"
                .x(() =>
                {
                    var app = ((AzureIntegrationServicesModel)model).GetSourceModel<ParsedBizTalkApplicationGroup>().Applications[0];
                    app.WorkingFolder = applicationWorkingFolder;
                });

            "And there should be 1 resource in the msi container"
                .x(() =>
                {
                    var aisModel = (AzureIntegrationServicesModel)model;
                    aisModel.MigrationSource.ResourceContainers.Should().HaveCount(1);
                    aisModel.MigrationSource.ResourceContainers[0].Type.Should().Be(ModelConstants.ResourceContainerMsi);
                    aisModel.MigrationSource.ResourceContainers[0].ResourceDefinitions.Should().HaveCount(1);
                    aisModel.MigrationSource.ResourceContainers[0].ResourceDefinitions[0].Name.Should().Be(Path.GetFileNameWithoutExtension(arbitaryFileName));
                });

            "And the artefact folder was created"
                .x(() =>
                {
                    var path = Path.Combine(artefactFolder, Path.GetFileNameWithoutExtension(msiFileName));
                    artefactFolderCreateParameter.Should().BeEquivalentTo(path);                
                });


            "And the files were extracted from the MSI"
                .x(() =>
                {
                    extractMsiParameter1.Should().StartWith(msiFolder);
                    extractMsiParameter1.Should().EndWith(msiFileName);
                    extractMsiParameter2.Should().BeEquivalentTo(applicationWorkingFolder);
                });

            "And the Cab files were extracted"
                .x(() =>
                {
                    var cabFile = Path.Combine(applicationWorkingFolder, cabFileName);
                    var cabPath = Path.Combine(applicationWorkingFolder, Path.GetFileNameWithoutExtension(cabFileName));
                    extractCabFilesParameter1.Should().BeEquivalentTo(cabFile);
                    extractCabFilesParameter2.Should().BeEquivalentTo(cabPath);
                });
        }

        #endregion

        #region Discover Binding File Scenarios

        /// <summary>
        /// Scenario to test when an exception occurs, when accessing the Binding File, the error is recorded.
        /// </summary>
        /// <param name="discoverer">The discoverer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The discoverer exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ErrorOpeningBindingFile(MsiDiscoverer discoverer, ILogger logger, IFileRepository fileRepository, IApplicationModel model, MigrationContext context, Exception e)
        {
            var cabFile = @"c:\temp\applicationworkingfolder\app1\testcab.cab";
            var bindingFileName = @"c:\temp\bindingfiles\BindingInfo.xml";

            "Given a context"
               .x(() =>
               {
                   context = new MigrationContext
                   {
                       WorkingFolder = @"c:\test-working-folder"
                   };
               });

            "And a logger"
               .x(() => logger = new Mock<ILogger>().Object);
            
            "And a model with a valid MSI"
                .x(() =>
                {
                    var azureModel = new AzureIntegrationServicesModel();
                    azureModel.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Name = "MsiIsValid.msi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = "MsiIsValid.msi" });
                    model = azureModel;
                });

            "And a file repository which throws an exception when accessing the Binding file"
                .x(() =>
                {
                    var msiNameToCheck = ((AzureIntegrationServicesModel)model).MigrationSource.ResourceContainers[0].ContainerLocation;

                    _mockFileRespository.Setup(f => f.CheckFileExists(msiNameToCheck)).Returns(true);

                    _mockFileRespository.Setup(f => f.CreateArtefactFolder(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                    .Returns(new DirectoryInfo(@"c:\temp\anyfolder"));

                    _mockFileRespository.Setup(f => f.CreateApplicationWorkingFolder(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                    .Returns(new DirectoryInfo(@"c:\temp\anyfolder2"));

                    _mockFileRespository.Setup(f => f.GetCabFiles(
                        It.IsAny<string>()))
                    .Returns(new string[] { cabFile });

                    _mockFileRespository.Setup(f => f.ExtractFilesInCab(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                    .Returns(new string[] { bindingFileName });

                    _mockFileRespository.Setup(f => f.ReadBindingFile(
                        It.IsAny<string>()))
                    .Throws<IOException>();

                    fileRepository = _mockFileRespository.Object;
                });

            "And a new discoverer"
                .x(() => discoverer = new MsiDiscoverer(fileRepository, model, context, logger));

            "When discovering"
                .x(() => e = Record.Exception(() => discoverer.Discover()));

            "Then the discover should succeed without error"
                .x(() => e.Should().BeNull());

            "And there should be an error recorded"
                .x(() =>
                {
                    context.Errors.Should().NotBeNullOrEmpty().And.HaveCount(1);
                    context.Errors.First().Message.Should().Contain("error occurred trying to process the files");
                });
        }

        /// <summary>
        /// Scenario to test when an attempt to discover an MSI which does have a binding file.        
        /// </summary>
        /// <param name="discoverer">The discoverer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="readBindingFileParameter1">The full path to the binding file when reading the contents.</param>
        /// <param name="e">The discoverer exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void BindingFileDoesExist(MsiDiscoverer discoverer, ILogger logger, IFileRepository fileRepository, IApplicationModel model, MigrationContext context, string readBindingFileParameter1, Exception e)
        {
            var bindingFileName = @"c:\temp\bindingfiles\BindingInfo.xml";
            var applicationWorkingFolder = @"c:\temp\applicationworkingfolder";
            var cabFile = @"c:\temp\applicationworkingfolder\app1\testcab.cab";
            var bindingFileContents = "binding file contents";

            "Given a context"
               .x(() =>
               {
                   context = new MigrationContext
                   {
                       WorkingFolder = @"c:\test-working-folder"
                   };
               });

            "And a logger"
               .x(() => logger = new Mock<ILogger>().Object);
            
            "And a model with a valid MSI"
                .x(() =>
                {
                    var azureModel = new AzureIntegrationServicesModel();
                    azureModel.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Key = "MsiIsValid.msi.Key", Name = "MsiIsValid.msi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = "MsiIsValid.msi" });
                    model = azureModel;
                });

            "And a file repository with a binding file"
                .x(() =>
                {
                    var msiNameToCheck = ((AzureIntegrationServicesModel)model).MigrationSource.ResourceContainers[0].ContainerLocation;

                    _mockFileRespository.Setup(f => f.CheckFileExists(msiNameToCheck)).Returns(true);

                    _mockFileRespository.Setup(f => f.CreateArtefactFolder(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                    .Returns(new DirectoryInfo(@"c:\temp\anyfolder"));

                    _mockFileRespository.Setup(f => f.CreateApplicationWorkingFolder(
                       It.IsAny<string>(),
                       It.IsAny<string>()))
                    .Returns(new DirectoryInfo(applicationWorkingFolder));

                    _mockFileRespository.Setup(f => f.GetCabFiles(
                        It.IsAny<string>()))
                    .Returns(new string[] { cabFile });

                    _mockFileRespository.Setup(f => f.ExtractFilesInCab(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                    .Returns(new string[] { bindingFileName });

                    _mockFileRespository.Setup(f => f.ReadBindingFile(
                        It.IsAny<string>()))
                    .Callback<string>(
                        (p1) =>
                        {
                            readBindingFileParameter1 = p1;                            
                        })
                    .Returns(bindingFileContents);

                    fileRepository = _mockFileRespository.Object;
                });

            "And a new discoverer"
                .x(() => discoverer = new MsiDiscoverer(fileRepository, model, context, logger));

            "When discovering"
                .x(() => e = Record.Exception(() => discoverer.Discover()));

            "Then the discover should succeed without error"
                .x(() => e.Should().BeNull());

            "And there should be no errors recorded"
                .x(() =>
                {
                    var app = ((AzureIntegrationServicesModel)model).GetSourceModel<ParsedBizTalkApplicationGroup>().Applications[0];
                    context.Errors.Should().HaveCount(0);
                });

            "And the binding file contents were added to the model"
                .x(() =>
                {
                    readBindingFileParameter1.Should().BeEquivalentTo(bindingFileName);
                    var app = ((AzureIntegrationServicesModel)model).GetSourceModel<ParsedBizTalkApplicationGroup>().Applications[0];
                    app.Application.Bindings.Should().NotBeNull();

                    // Get resource definition content
                    var binding = from msiResourceContainer in ((AzureIntegrationServicesModel)model).MigrationSource.ResourceContainers
                                  from cabResourceContainer in msiResourceContainer.ResourceContainers
                                  from resourceDefinition in cabResourceContainer.ResourceDefinitions
                                  where msiResourceContainer.Key == app.ResourceContainerKey &&
                                    app.Application.Bindings.ResourceContainerKey == cabResourceContainer.Key &&
                                    app.Application.Bindings.ResourceDefinitionKey == resourceDefinition.Key &&
                                    resourceDefinition.Type == ModelConstants.ResourceDefinitionBindings
                                  select resourceDefinition;

                    binding.Should().HaveCount(1);
                    binding.Single().ResourceContent.Should().BeEquivalentTo(bindingFileContents);
                });
        }

        #endregion

        #region ADF File Scenarios

        /// <summary>
        /// Scenario to test when an exception occurs, when accessing the ADF file, the error is recorded.
        /// </summary>
        /// <param name="discoverer">The discoverer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The discoverer exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void ErrorOpeningAdfFile(MsiDiscoverer discoverer, ILogger logger, IFileRepository fileRepository, IApplicationModel model, MigrationContext context, Exception e)
        {
            var adfFileName = @"c:\temp\adffile\ApplicationDefinition.adf";

            "Given a context"
               .x(() =>
               {
                   context = new MigrationContext
                   {
                       WorkingFolder = @"c:\test-working-folder"
                   };
               });

            "And a logger"
               .x(() => logger = new Mock<ILogger>().Object);
            
            "And a model with a valid MSI"
                .x(() =>
                {
                    var azureModel = new AzureIntegrationServicesModel();
                    azureModel.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Key = "MsiIsValid.msi", Name = "MsiIsValid.msi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = "MsiIsValid.msi" });
                    model = azureModel;
                });

            "And a file repository which throws an exception when accessing the ADF file"
                .x(() =>
                {
                    var msiNameToCheck = ((AzureIntegrationServicesModel)model).MigrationSource.ResourceContainers[0].ContainerLocation;

                    _mockFileRespository.Setup(f => f.CheckFileExists(msiNameToCheck)).Returns(true);

                    _mockFileRespository.Setup(f => f.CreateArtefactFolder(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                    .Returns(new DirectoryInfo(@"c:\temp\anyfolder"));

                    _mockFileRespository.Setup(f => f.CreateApplicationWorkingFolder(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                    .Returns(new DirectoryInfo(@"c:\temp\anyfolder2"));

                    _mockFileRespository.Setup(f => f.ExtractFilesInMsi(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                    .Returns(new string[] { adfFileName });

                    _mockFileRespository.Setup(f => f.ReadAdfFile(
                        It.IsAny<string>()))
                    .Throws<IOException>();

                    fileRepository = _mockFileRespository.Object;
                });

            "And a new discoverer"
                .x(() => discoverer = new MsiDiscoverer(fileRepository, model, context, logger));

            "When discovering"
                .x(() => e = Record.Exception(() => discoverer.Discover()));

            "Then the discover should succeed without error"
                .x(() => e.Should().BeNull());

            "And there should be an error recorded"
                .x(() =>
                {
                    context.Errors.Should().NotBeNullOrEmpty().And.HaveCount(1);
                    context.Errors.First().Message.Should().Contain("error occurred trying to process the files");
                });
        }

        /// <summary>
        /// Scenario to test when an attempt to discover an MSI which does have an ADF file.        
        /// </summary>
        /// <param name="discoverer">The discoverer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="readAdfParameter1">The name of the adf file.</param>
        /// <param name="e">The discoverer exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void AdfDoesExist(MsiDiscoverer discoverer, ILogger logger, IFileRepository fileRepository, IApplicationModel model, MigrationContext context, string readAdfParameter1, Exception e)
        {
            var applicationWorkingFolder = @"c:\temp\applicationworkingfolder";
            var adfFileName = @"c:\temp\adffile\ApplicationDefinition.adf";
            var adfContents = "ADF file contents";

            "Given a context"
               .x(() =>
               {
                   context = new MigrationContext
                   {
                       WorkingFolder = @"c:\test-working-folder"
                   };
               });

            "And a logger"
                .x(() => logger = new Mock<ILogger>().Object);
            
            "And a model with a valid MSI"
                .x(() =>
                {
                    var azureModel = new AzureIntegrationServicesModel();
                    azureModel.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Key = "MsiIsValid.Key", Name = "MsiIsValid.msi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = "MsiIsValid.msi" });
                    model = azureModel;
                });

            "And a file repository with an ADF file"
                .x(() =>
                {
                    var msiNameToCheck = ((AzureIntegrationServicesModel)model).MigrationSource.ResourceContainers[0].ContainerLocation;

                    _mockFileRespository.Setup(f => f.CheckFileExists(msiNameToCheck)).Returns(true);

                    _mockFileRespository.Setup(f => f.CreateArtefactFolder(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                    .Returns(new DirectoryInfo(@"c:\temp\anyfolder"));

                    _mockFileRespository.Setup(f => f.CreateApplicationWorkingFolder(
                       It.IsAny<string>(),
                       It.IsAny<string>()))
                   .Returns(new DirectoryInfo(applicationWorkingFolder));

                    _mockFileRespository.Setup(f => f.ExtractFilesInMsi(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                    .Returns(new string[] { adfFileName });

                    _mockFileRespository.Setup(f => f.ReadBindingFile(
                        It.IsAny<string>()))
                    .Returns("binding file contents");

                    _mockFileRespository.Setup(f => f.ReadAdfFile(
                        It.IsAny<string>()))
                    .Callback<string>(
                        (p1) =>
                        {
                            readAdfParameter1 = p1;
                        })
                    .Returns(adfContents);

                    fileRepository = _mockFileRespository.Object;
                });

            "And a new discoverer"
                .x(() => discoverer = new MsiDiscoverer(fileRepository, model, context, logger));

            "When discovering"
                .x(() => e = Record.Exception(() => discoverer.Discover()));

            "Then the discover should succeed without error"
                .x(() => e.Should().BeNull());

            "And there should be no errors recorded"
                .x(() =>
                {
                    var app = ((AzureIntegrationServicesModel)model).GetSourceModel<ParsedBizTalkApplicationGroup>().Applications[0];
                    context.Errors.Should().HaveCount(0);
                });

            "And the ADF file contents were added to the context"
                .x(() =>
                {
                    readAdfParameter1.Should().BeEquivalentTo(adfFileName);
                    var app = ((AzureIntegrationServicesModel)model).GetSourceModel<ParsedBizTalkApplicationGroup>().Applications[0];
                    app.Application.ApplicationDefinition.Should().NotBeNull();

                    var adf = from resourceContainer in ((AzureIntegrationServicesModel)model).MigrationSource.ResourceContainers
                              from resourceDefinition in resourceContainer.ResourceDefinitions
                              where resourceContainer.Key == app.ResourceContainerKey &&
                                app.Application.ApplicationDefinition.ResourceContainerKey == resourceContainer.Key &&
                                app.Application.ApplicationDefinition.ResourceDefinitionKey == resourceDefinition.Key &&
                                resourceDefinition.Type == ModelConstants.ResourceDefinitionApplicationDefinition
                              select resourceDefinition;

                    adf.Single().ResourceContent.Should().BeEquivalentTo(adfContents);
                });
        }
        #endregion

        #region Discover BizTalk Assemblies Scenarios

        /// <summary>
        /// Scenario to test when an attempt to discover an MSI which has BizTalk assemblies.
        /// </summary>
        /// <param name="discoverer">The discoverer.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="fileRepository">The file repository.</param>
        /// <param name="model">The application model.</param>
        /// <param name="context">The migration context.</param>
        /// <param name="e">The discoverer exception, if any.</param>
        [Scenario]
        [Trait(TestConstants.TraitCategory, TestConstants.CategoryUnitTest)]
        public void BizTalkAssembliesFound(MsiDiscoverer discoverer, ILogger logger, IFileRepository fileRepository, IApplicationModel model, MigrationContext context, Exception e)
        {
            var applicationWorkingFolder = @"c:\temp\applicationworkingfolder";
            var bizTalkAssemblies = new string[] { "Assembly1.dll", "Assembly2.dll" };
            var cabFile = @"c:\temp\applicationworkingfolder\app1\testcab.cab";

            "Given a context"
               .x(() =>
               {
                   context = new MigrationContext
                   {
                       WorkingFolder = @"c:\test-working-folder"
                   };
               });

            "And a logger"
               .x(() => logger = new Mock<ILogger>().Object);
            
            "And a model with a valid MSI"
                .x(() =>
                {
                    var azureModel = new AzureIntegrationServicesModel();
                    azureModel.MigrationSource.ResourceContainers.Add(new ResourceContainer() { Name = "MsiIsValid.msi", Type = ModelConstants.ResourceContainerMsi, ContainerLocation = "MsiIsValid.msi" });
                    model = azureModel;
                });

            "And a file repository BizTalk Assemblies"
                .x(() =>
                {
                    var msiNameToCheck = ((AzureIntegrationServicesModel)model).MigrationSource.ResourceContainers[0].ContainerLocation;

                    _mockFileRespository.Setup(f => f.CheckFileExists(msiNameToCheck)).Returns(true);

                    _mockFileRespository.Setup(f => f.CreateArtefactFolder(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                    .Returns(new DirectoryInfo(@"c:\temp\anyfolder"));

                    _mockFileRespository.Setup(f => f.CreateApplicationWorkingFolder(
                       It.IsAny<string>(),
                       It.IsAny<string>()))
                    .Returns(new DirectoryInfo(applicationWorkingFolder));

                    _mockFileRespository.Setup(f => f.GetCabFiles(
                        It.IsAny<string>()))
                    .Returns(new string[] { cabFile });

                    _mockFileRespository.Setup(f => f.ExtractFilesInCab(
                        It.IsAny<string>(),
                        It.IsAny<string>()))
                    .Returns(bizTalkAssemblies);

                    _mockFileRespository.Setup(f => f.ReadBindingFile(
                        It.IsAny<string>()))
                    .Returns("binding file contents");

                    _mockFileRespository.Setup(f => f.ReadAdfFile(
                        It.IsAny<string>()))                    
                    .Returns("ADF File contents");

                    fileRepository = _mockFileRespository.Object;
                });

            "And a new discoverer"
                .x(() => discoverer = new MsiDiscoverer(fileRepository, model, context, logger));

            "When discovering"
                .x(() => e = Record.Exception(() => discoverer.Discover()));

            "Then the discover should succeed without error"
                .x(() => e.Should().BeNull());

            "And there should be no errors recorded"
                .x(() =>
                {
                    var app = ((AzureIntegrationServicesModel)model).GetSourceModel<ParsedBizTalkApplicationGroup>().Applications[0];
                    context.Errors.Should().HaveCount(0);
                });

            "And the BizTalk Assemblies were added to the context"
                .x(() =>
                {
                    var app = ((AzureIntegrationServicesModel)model).GetSourceModel<ParsedBizTalkApplicationGroup>().Applications[0];
                    app.Application.Assemblies.Should().HaveCount(bizTalkAssemblies.Length);
                });
        }
        #endregion

    }
#pragma warning restore CA1303 // Do not pass literals as localized parameters
}
