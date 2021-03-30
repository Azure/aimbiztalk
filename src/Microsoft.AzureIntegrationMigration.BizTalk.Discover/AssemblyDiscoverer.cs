// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

//-----------------------------------------------------------------------
// <copyright file="AssemblyDiscoverer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved. THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// </copyright>
// <summary>Discoverer that discovers the contents of BizTalk assemblies.</summary>
//-----------------------------------------------------------------------

#region Using Directives

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AzureIntegrationMigration.ApplicationModel;
using Microsoft.AzureIntegrationMigration.ApplicationModel.Source;
using Microsoft.AzureIntegrationMigration.BizTalk.Discover.Resources;
using Microsoft.AzureIntegrationMigration.BizTalk.Types;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities;
using Microsoft.AzureIntegrationMigration.BizTalk.Types.Enumerations;
using Microsoft.AzureIntegrationMigration.Runner.Model;
using Microsoft.Extensions.Logging;
using Mono.Cecil;

#endregion

namespace Microsoft.AzureIntegrationMigration.BizTalk.Discover
{
    /// <summary>
    /// Discoverer that discovers the contents of BizTalk assemblies.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

    public class AssemblyDiscoverer : BizTalkDiscovererBase
    {
        /// <summary>
        /// Defines a logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Defines the name of this discoverer.
        /// </summary>
        public const string DiscovererName = ModelConstants.ApplicationPrefix + "assemblydiscoverer";

        /// <summary>
        /// The schema attribute type name.
        /// </summary>
        private const string SchemaAttributeType = "Microsoft.XLANGs.BaseTypes.SchemaAttribute";

        /// <summary>
        /// The promoted property attribute type name.
        /// </summary>
        private const string PropertyAttributeType = "Microsoft.XLANGs.BaseTypes.PropertyAttribute";

        /// <summary>
        /// The attribute for a distinguished field.
        /// </summary>
        private const string DistinguishedFieldAttributeType = "Microsoft.XLANGs.BaseTypes.DistinguishedFieldAttribute";

        /// <summary>
        /// The body XPath attribute type name.
        /// </summary>
        private const string BodyXPathAttributeType = "Microsoft.XLANGs.BaseTypes.BodyXPathAttribute";

        /// <summary>
        /// The attribute for a schema.
        /// </summary>
        private const string SchemaBaseType = "Microsoft.XLANGs.BaseTypes.SchemaBase";

        /// <summary>
        /// Creates a new instance of the <see cref="AssemblyDiscoverer" class./>
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        /// <param name="logger">The logger.</param>
        public AssemblyDiscoverer(IApplicationModel model, MigrationContext context, ILogger logger)
            :base(DiscovererName, model, context, logger)
        {
            // Validate and set the members
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Performs the internal work of the discoverer.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        protected override void DiscoverInternal(AzureIntegrationServicesModel model, MigrationContext context)
        {
            // Null check.
            _ = (AzureIntegrationServicesModel)model ?? throw new ArgumentNullException(nameof(model));
            _ = context ?? throw new ArgumentNullException(nameof(context));

            DiscoverAssemblyContents(model, context);
        }

        /// <summary>
        /// Discovers the assemblies that have been unpacked.
        /// </summary>
        /// <param name="model">The application model.</param>
        /// <param name="context">The context that gathers migration information.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This is by design")]
        private void DiscoverAssemblyContents(AzureIntegrationServicesModel model, MigrationContext context)
        {
            // Iterate through the applications and discover the contents of the assemblies.
            var group = model.GetSourceModel<ParsedBizTalkApplicationGroup>();

            if (group?.Applications == null)
            {
                _logger.LogDebug(TraceMessages.SkippingDiscoveryAsTheSourceModelIsMissing, nameof(AssemblyDiscoverer));
            }
            else
            { 
                foreach (var application in group.Applications)
                {
                    _logger.LogDebug(TraceMessages.DiscoveringAssemblyContentsInResourceContainer, application.ResourceContainerKey);

                    // Find assemblies associated with this application
                    var assemblies = from msiResourceContainer in model.MigrationSource.ResourceContainers
                                     from cabResourceContainer in msiResourceContainer.ResourceContainers
                                     from assemblyResourceContainers in cabResourceContainer.ResourceContainers
                                     from assembly in application.Application.Assemblies
                                     where assembly.ResourceContainerKey == assemblyResourceContainers.Key &&                                        
                                        assemblyResourceContainers.Type == ModelConstants.ResourceContainerAssembly
                                     select assemblyResourceContainers;

                    // Iterate through the assemblies discovered for each application.
                    foreach (var assembly in assemblies)
                    {
                        try
                        { 
                            _logger.LogDebug(TraceMessages.DiscoveringTheResourcesInAssemblyCointainer, assembly.Name);

                            // Check to ensure the assembly is a managed (.Net) assembly
                            if (IsManagedAssembly(assembly.ContainerLocation))
                            {
                                using (var assemblyDefinition = AssemblyDefinition.ReadAssembly(assembly.ContainerLocation))
                                {
                                    // Loads the components that are packaged into a BizTalk assembly, adding them to the assembly container.
                                    LoadSchemas(assemblyDefinition, application, assembly);
                                    LoadTransforms(assemblyDefinition, application, assembly);
                                    LoadReceivePipelines(assemblyDefinition, application, assembly);
                                    LoadSendPipelines(assemblyDefinition, application, assembly);
                                    LoadOrchestrations(assemblyDefinition, application, assembly);
                                }
                            }
                            else
                            {
                                // Log and skip
                                _logger.LogInformation(InformationMessages.UnmanagedAssemblyDiscovered, assembly.Name);
                            }
                        }
                        catch (Exception ex)
                        {
                            var message = string.Format(CultureInfo.CurrentCulture, ErrorMessages.UnableToLoadAssembly, assembly.Name, ex.Message);
                            context.Errors.Add(new ErrorMessage(message));
                            _logger.LogError(message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates schema enitites for any schemas found in the given <see cref="AssemblyDefinition"/> and adds them to the given <see cref="ParsedBizTalkApplication"/>.
        /// </summary>
        /// <param name="assemblyDefinition"><see cref="AssemblyDefinition"/> in which to look for schemas.</param>
        /// <param name="application"><see cref="ParsedBizTalkApplication"/> to add schema entities to.</param>
        /// <param name="container">The resource container for the definitions.</param>
        private void LoadSchemas(AssemblyDefinition assemblyDefinition, ParsedBizTalkApplication application, ResourceContainer container)
        {
            // Check if we have any schema attributes in the types defined in the assembly
            var schemaTypes = assemblyDefinition.MainModule.Types.Where(ty => ty.BaseType != null && ty.BaseType.FullName == SchemaBaseType).ToList();

            _logger.LogDebug(TraceMessages.DiscoveringTheSchemasInResourceContainer, container.Key);

            foreach (var schemaType in schemaTypes)
            {
                // Get schema content
                var schemaContent = schemaType.Fields.Where(f => f.Name == "_strSchema").Single().Constant.ToString();

                // Add definition to resource container
                var resourceName = schemaType.Name;
                var resourceDefinitionKey = string.Concat(container.Key, ":", resourceName);
                container.ResourceDefinitions.Add(new ResourceDefinition() { Key = resourceDefinitionKey, Name = resourceName, Type = ModelConstants.ResourceDefinitionSchema, ResourceContent = schemaContent });

                // Add schema object to model
                var schema = new Schema(container.Key, resourceDefinitionKey)
                {
                    Name = schemaType.Name,
                    FullName = schemaType.FullName,
                    ModuleName = assemblyDefinition.FullName,
                    Namespace = schemaType.Namespace,
                    SchemaType = HasCustomAttribute(schemaType, SchemaAttributeType) ? BizTalkSchemaType.Document : BizTalkSchemaType.Unknown,
                    IsEnvelope = HasCustomAttribute(schemaType, BodyXPathAttributeType) ? true : false,
                    ResourceKey = resourceDefinitionKey + ":schema",
                    BodyXPath = GetCustomAttributeConstructorValue(schemaType, BodyXPathAttributeType, 0), 
                    XmlNamespace = GetCustomAttributeConstructorValue(schemaType, SchemaAttributeType, 0),
                    RootNodeName = GetCustomAttributeConstructorValue(schemaType, SchemaAttributeType, 1)
                };

                // Property schemas are recognised by this annotation in the XML.
                if (schemaContent.Contains("<b:schemaInfo schema_type=\"property\" xmlns:b=\"http://schemas.microsoft.com/BizTalk/2003\" />"))
                {
                    schema.SchemaType = BizTalkSchemaType.Property;
                }

                application.Application.Schemas.Add(schema);

                _logger.LogDebug(TraceMessages.DiscoveredTheSchema, schema.FullName);

                // Get the promoted properties and distinguished fields for the schema.
                schema.PromotedProperties.AddRange(ExtractPromotedProperties(schemaType));
                ExtractDistinguishedFields(schemaType, schema);

                // If the schema defines a root node and namespace then create a message definition.
                if (!string.IsNullOrEmpty(GetCustomAttributeConstructorValue(schemaType, SchemaAttributeType, 1)))
                {
                    // Create the message definition.
                    var messageDefinition = CreateMessageDefinition(schemaType.FullName, schemaType.Name, schemaType, schema.ResourceKey);

                    // Add the message definitions to the parent schema.
                    schema.MessageDefinitions.Add(messageDefinition);
                }

                // Multiple root nodes in a schema manifest themselves as nested types.  Create a separate schema object for each of these.
                if (schemaType.HasNestedTypes)
                {
                    var nestedTypes = schemaType.NestedTypes.Where(ty => ty.BaseType != null && ty.BaseType.FullName == SchemaBaseType).ToList();
                    foreach (var nestedType in nestedTypes)
                    {
                        var messageName = $"{schemaType.Name}.{nestedType.Name}";

                        // Create the message definition.
                        var messageDefinition = CreateMessageDefinition(nestedType.FullName, messageName, nestedType, schema.ResourceKey);

                        // Multi-root schemas, dont have the custom attribute set at the root class, its on the nested types.
                        // Check to see if it should be set for the nested type.
                        if (schema.SchemaType != BizTalkSchemaType.Document && HasCustomAttribute(nestedType, SchemaAttributeType))
                        {
                            schema.SchemaType = BizTalkSchemaType.Document;
                        }

                        // Add the message definitions to the parent schema.
                        schema.MessageDefinitions.Add(messageDefinition);
                    }
                    schema.XmlNamespace = GetCustomAttributeConstructorValue(nestedTypes.First(), SchemaAttributeType, 0);
                }
            }
        }

        /// <summary>
        /// Creates a message definition.
        /// </summary>
        /// <param name="messageFullName">The fully qualified type name to assign to the message definition.</param>
        /// <param name="messageLocalName">The local class name to assign to the message definition.</param>
        /// <param name="messageType">The message type to create the message definition from.</param>
        /// <param name="parentResourceKey">The resource key of the message definitions parent.</param>
        /// <returns>A message definition.</returns>
        private MessageDefinition CreateMessageDefinition(string messageFullName, string messageLocalName, TypeDefinition messageType, string parentResourceKey)
        {
            var messageDefinition = new MessageDefinition(
                GetCustomAttributeConstructorValue(messageType, SchemaAttributeType, 1),
                GetCustomAttributeConstructorValue(messageType, SchemaAttributeType, 0),
                messageFullName,
                messageLocalName,
                parentResourceKey + ":" + GetCustomAttributeConstructorValue(messageType, SchemaAttributeType, 1));

            // Add the promoted properties.
            messageDefinition.PromotedProperties.AddRange(ExtractPromotedProperties(messageType));

            return messageDefinition;
        }

        /// <summary>
        /// Extracts the promoted properties from the type.
        /// </summary>
        /// <param name="schemaType">The <see cref="TypeDefinition"/> of the schema.</param>
        /// <returns>The promoted properties extracted.</returns>
        private IList<PromotedProperty> ExtractPromotedProperties(TypeDefinition schemaType)
        {
            var promotedProperties = new List<PromotedProperty>();

            // Check for promoted properties.
            if (HasCustomAttribute(schemaType, PropertyAttributeType))
            {
                // Promoted properties in BizTalk manifest themselves as attributes on the schema type.
                var promotedPropertyAttributes = schemaType.CustomAttributes.Where(attr => attr.AttributeType.FullName == PropertyAttributeType).ToList();

                // There can be multiple promoted properties, so extract each in turn and add to the list.
                foreach (var promotedPropertyAttribute in promotedPropertyAttributes)
                {
                    var property = new PromotedProperty()
                    {
                        XPath = promotedPropertyAttribute.Properties.Where(p => p.Name == "XPath").Single().Argument.Value.ToString(),
                        PropertyType = promotedPropertyAttribute.ConstructorArguments[0].Value.ToString()
                    };
                    promotedProperties.Add(property);

                    _logger.LogDebug(TraceMessages.DiscoveredThePromotedProperty, property.XPath);
                }
            }

            return promotedProperties;
        }

        /// <summary>
        /// Gets the distinguished fields associated with the schema from the type definition.
        /// </summary>
        /// <param name="schemaType">The <see cref="TypeDefinition"/> of the schema.</param>
        /// <param name="schema">The <see cref="Schema"/> model being built from the type.</param>
        private void ExtractDistinguishedFields(TypeDefinition schemaType, Schema schema)
        {
            // Check for distinguished fields.
            if (HasCustomAttribute(schemaType, DistinguishedFieldAttributeType))
            {
                // Distinguished fields in BizTalk manifest themselves as attributes on the schema type.
                var distinguishedFieldAttributes = schemaType.CustomAttributes.Where(attr => attr.AttributeType.FullName == DistinguishedFieldAttributeType).ToList();

                // There can be multiple distinguished fields, so extract each in turn and add to the list.
                foreach (var distinguishedFieldAttribute in distinguishedFieldAttributes)
                {
                    var distinguishedField = new DistinguishedField()
                    {
                        XPath = distinguishedFieldAttribute.Properties.Where(p => p.Name == "XPath").Single().Argument.Value.ToString(),
                        DistinguishedFieldName = distinguishedFieldAttribute.ConstructorArguments[1].Value.ToString()
                    };
                    schema.DistinguishedFields.Add(distinguishedField);

                    _logger.LogDebug(TraceMessages.DiscoveredTheDistinguisedField, distinguishedField.XPath);
                }
            }
        }

        /// <summary>
        /// Gets the constructor argument with the given index for the custom attribute with the given name in the given type.
        /// </summary>
        /// <param name="type"><see cref="TypeDefinition"/> containing the custom attribute.</param>
        /// <param name="attributeTypeName">The full type name of the custom attribute</param>
        /// <param name="constructorArgumentIndex">The index of the constructor argument to get.</param>
        /// <returns>The value of the constructor attribute.</returns>
        private static string GetCustomAttributeConstructorValue(TypeDefinition type, string attributeTypeName, int constructorArgumentIndex)
        {
            string value = null;
            var attributeType = type.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.FullName == attributeTypeName);
            if (attributeType != null && attributeType.ConstructorArguments.Count >= constructorArgumentIndex + 1)
            {
                value = attributeType.ConstructorArguments[constructorArgumentIndex].Value.ToString();
            }

            return value;
        }

        /// <summary>
        /// Determines whether the custom attribute exists.
        /// </summary>
        /// <param name="type"><see cref="TypeDefinition"/> containing the custom attribute.</param>
        /// <param name="attributeTypeName">The full type name of the custom attribute</param>
        /// <returns>A value indicating whether the attribute exists.</returns>
        private static bool HasCustomAttribute(TypeDefinition type, string attributeTypeName)
        {
            var attributeType = type.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.FullName == attributeTypeName);
            return (attributeType != null);
        }

        /// <summary>
        /// Creates transform enitites for any transforms found in the given <see cref="AssemblyDefinition"/> and adds them to the given <see cref="ParsedBizTalkApplication"/>.
        /// </summary>
        /// <param name="assembly"><see cref="AssemblyDefinition"/> in which to look for transforms.</param>
        /// <param name="application"><see cref="ParsedBizTalkApplication"/> to add transform entities to.</param>
        /// <param name="container">The resource container for the definitions.</param>
        private void LoadTransforms(AssemblyDefinition assembly, ParsedBizTalkApplication application, ResourceContainer container)
        {
            // Check if we have any types that inherit from TransformBase in the types defined in the assembly
            var transformTypes = assembly.MainModule.Types.Where(ty => ty.BaseType != null && ty.BaseType.FullName == "Microsoft.XLANGs.BaseTypes.TransformBase").ToList();

            foreach (var transformType in transformTypes)
            {
                // Get transform content
                var transformContent = transformType.Fields.Where(f => f.Name == "_strMap").Single().Constant.ToString();

                // Add definition to resource container
                var resourceName = transformType.Name;
                var resourceKey = string.Concat(container.Name, ":", resourceName);
                container.ResourceDefinitions.Add(new ResourceDefinition() { Key = resourceKey, Name = resourceName, Type = ModelConstants.ResourceDefinitionMap, ResourceContent = transformContent });

                // Add transform to model
                var transform = new Transform(container.Key, resourceKey)
                {
                    FullName = transformType.FullName,
                    ModuleName = assembly.FullName,
                    Name = transformType.Name,
                    Namespace = transformType.Namespace
                };

                // Add schemas
                var i = 0;
                bool sourceDone, targetDone;
                sourceDone = targetDone = false;
                for (;;)
                {
                    // Check source schema
                    if (!sourceDone)
                    {
                        var sourceSchemaName = string.Format(CultureInfo.InvariantCulture, "_strSrcSchemasList{0}", i);
                        var sourceSchema = GetFieldConstantValue(transformType, sourceSchemaName);
                        if (!string.IsNullOrWhiteSpace(sourceSchema))
                        {
                            transform.SourceSchemaTypeNames.Add(sourceSchema);
                        }
                        else
                        {
                            sourceDone = true;
                        }
                    }

                    // Check target schema
                    if (!targetDone)
                    {
                        var targetSchemaName = string.Format(CultureInfo.InvariantCulture, "_strTrgSchemasList{0}", i);
                        var targetSchema = GetFieldConstantValue(transformType, targetSchemaName);
                        if (!string.IsNullOrWhiteSpace(targetSchema))
                        {
                            transform.TargetSchemaTypeNames.Add(targetSchema);
                        }
                        else
                        {
                            targetDone = true;
                        }
                    }

                    // Done?
                    if (sourceDone && targetDone)
                    {
                        break;
                    }
                    else
                    {
                        i++;
                    }
                }

                application.Application.Transforms.Add(transform);

                _logger.LogDebug(TraceMessages.DiscoveredTheTransform, resourceKey);
            }
        }

        /// <summary>
        /// Gets a constant string value from the field with the given name defined in the given <see cref="TypeDefinition"/>.
        /// </summary>
        /// <param name="type"><see cref="TypeDefinition"/> containing the field.</param>
        /// <param name="fieldName">Name of the field to obtain a constant value for.</param>
        /// <returns>Field constant value, if defined, otherwise null.</returns>
        private static string GetFieldConstantValue(TypeDefinition type, string fieldName)
        {
            if (type == null || string.IsNullOrWhiteSpace(fieldName))
            {
                return null;
            }

            var field = type.Fields.FirstOrDefault(f => f.Name == fieldName);
            if (field == null || field.Constant == null)
            {
                return null;
            }

            return field.Constant.ToString();
        }

        /// <summary>
        /// Creates pipeline entries for any pipelines found in the given <see cref="AssemblyDefinition"/> and adds them to the given <see cref="ParsedBizTalkApplication"/>.
        /// </summary>
        /// <param name="assembly"><see cref="AssemblyDefinition"/> in which to look for pipelines.</param>
        /// <param name="application"><see cref="ParsedBizTalkApplication"/> to add pipelines entities to.</param>
        /// <param name="container">The resource container for the definitions.</param>
        private void LoadReceivePipelines(AssemblyDefinition assembly, ParsedBizTalkApplication application, ResourceContainer container)
        {
            // Check if we have any types that inherit from ReceivePipeline in the types defined in the assembly
            var pipelineTypes = assembly.MainModule.Types.Where(ty => ty.BaseType != null && ty.BaseType.FullName == "Microsoft.BizTalk.PipelineOM.ReceivePipeline").ToList();

            foreach (var pipelineType in pipelineTypes)
            {
                // Get pipeline content
                var pipelineContent = pipelineType.Fields.Where(f => f.Name == "_strPipeline").Single().Constant.ToString();

                // Add definition to resource container
                var resourceName = pipelineType.Name;
                var resourceKey = string.Concat(container.Name, ":", resourceName);
                container.ResourceDefinitions.Add(new ResourceDefinition() { Key = resourceKey, Name = resourceName, Type = ModelConstants.ResourceDefinitionReceivePipeline, ResourceContent = pipelineContent });

                // Add receive pipeline to model
                var pipeline = new Pipeline(container.Key, resourceKey)
                {
                    FullName = pipelineType.FullName,
                    ModuleName = assembly.FullName,
                    Name = pipelineType.Name,
                    Namespace = pipelineType.Namespace,
                    Direction = PipelineDirection.Receive
                };

                application.Application.Pipelines.Add(pipeline);

                _logger.LogDebug(TraceMessages.DiscoveredTheReceivePipeline, resourceKey);
            }
        }

        /// <summary>
        /// Creates pipeline entries for any pipelines found in the given <see cref="AssemblyDefinition"/> and adds them to the given <see cref="ParsedBizTalkApplication"/>.
        /// </summary>
        /// <param name="assembly"><see cref="AssemblyDefinition"/> in which to look for pipelines.</param>
        /// <param name="application"><see cref="ParsedBizTalkApplication"/> to add pipelines entities to.</param>
        /// <param name="container">The resource container for the definitions.</param>
        private void LoadSendPipelines(AssemblyDefinition assembly, ParsedBizTalkApplication application, ResourceContainer container)
        {
            // Check if we have any types that inherit from Pipeline in the types defined in the assembly
            var pipelineTypes = assembly.MainModule.Types.Where(ty => ty.BaseType != null && ty.BaseType.FullName == "Microsoft.BizTalk.PipelineOM.SendPipeline").ToList();

            foreach (var pipelineType in pipelineTypes)
            {
                // Get pipeline content
                var pipelineContent = pipelineType.Fields.Where(f => f.Name == "_strPipeline").Single().Constant.ToString();

                // Add definition to resource container
                var resourceName = pipelineType.Name;
                var resourceKey = string.Concat(container.Name, ":", resourceName);
                container.ResourceDefinitions.Add(new ResourceDefinition() { Key = resourceKey, Name = resourceName, Type = ModelConstants.ResourceDefinitionSendPipeline, ResourceContent = pipelineContent });

                // And send pipeline to model
                var pipeline = new Pipeline(container.Key, resourceKey)
                {
                    FullName = pipelineType.FullName,
                    ModuleName = assembly.FullName,
                    Name = pipelineType.Name,
                    Namespace = pipelineType.Namespace,
                    Direction = PipelineDirection.Send
                };

                application.Application.Pipelines.Add(pipeline);

                _logger.LogDebug(TraceMessages.DiscoveredTheSendPipeline, resourceKey);
            }
        }

        /// <summary>
        /// Creates orchestration entries for any orchestrations found in the given <see cref="AssemblyDefinition"/> and adds them to the given <see cref="ParsedBizTalkApplication"/>.
        /// </summary>
        /// <param name="assembly"><see cref="AssemblyDefinition"/> in which to look for orchestrations.</param>
        /// <param name="application"><see cref="ParsedBizTalkApplication"/> to add orchestration entities to.</param>
        /// <param name="container">The resource container for the definitions.</param>
        private void LoadOrchestrations(AssemblyDefinition assembly, ParsedBizTalkApplication application, ResourceContainer container)
        {
            // Check if we have any types that inherit from BTXService in the types defined in the assembly
            var orchTypes = assembly.MainModule.Types.Where(ty => ty.BaseType != null && ty.BaseType.FullName == "Microsoft.BizTalk.XLANGs.BTXEngine.BTXService").ToList();

            foreach (var orchType in orchTypes)
            {
                // Get orchestration content
                var orchContent = orchType.Fields.Where(f => f.Name == "_symODXML").Single().Constant.ToString().Replace("\n", string.Empty);

                // Add definition to resource container
                var resourceName = orchType.Name;
                var resourceKey = string.Concat(container.Name, ":", resourceName);
                container.ResourceDefinitions.Add(new ResourceDefinition() { Key = resourceKey, Name = resourceName, Type = ModelConstants.ResourceDefinitionOrchestration, ResourceContent = orchContent });

                // Add orchestration to model
                var orch = new Orchestration(container.Key, resourceKey)
                {
                    FullName = orchType.FullName,
                    ModuleName = assembly.FullName,
                    Name = orchType.Name,
                    Namespace = orchType.Namespace
                };

                application.Application.Orchestrations.Add(orch);

                _logger.LogDebug(TraceMessages.DiscoveredTheOrchestration, resourceKey);
            }
        }

        /// <summary>
        /// Tests to see whether the assembly is a .Net (managed) assembly.
        /// .Net assemblies are a Portable Executable (PE).
        /// See https://docs.microsoft.com/en-us/dotnet/standard/assembly/file-format for more information on the format.
        /// </summary>
        /// <param name="fileName">The file name of the assembly.</param>
        /// <returns>A value indicating whether the assembly is managed (true) or not (false).</returns>
        private static bool IsManagedAssembly(string fileName)
        {
            using (Stream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fs))
            {
                // Check for a stream with fewer bytes than the headers we need.
                if (fs.Length < 64)
                {
                    return false;
                }

                // See https://www.red-gate.com/simple-talk/blogs/anatomy-of-a-net-assembly-pe-headers/ for more information on the headers
                // Read the pointer to the PE signature at 0x3C
                fs.Position = 0x3C;
                var peSignaturePointer = reader.ReadUInt32();
                if (peSignaturePointer == 0)
                {
                    peSignaturePointer = 0x80;
                }

                // Check that the file stream has sufficient capacity to hold the header information.
                if (peSignaturePointer > fs.Length - 256)
                {
                    return false;
                }

                // Read and verify the PE signature
                fs.Position = peSignaturePointer;
                var peSignature = reader.ReadUInt32();
                if (peSignature != 0x00004550)
                {
                    return false;
                }

                // Advance over the remainder of the header
                fs.Position += 20;

                // Read the PE standard
                // 0x010b for x86 and AnyCPU
                // 0x20b for x64
                const ushort PE32 = 0x10b;
                const ushort PE32Plus = 0x20b;
                var peFormat = reader.ReadUInt16();
                if (peFormat != PE32 && peFormat != PE32Plus)
                {
                    return false;
                }

                // Read the RVA field which contains the size of the CLR header
                var dataDirectoryStart = (ushort)(peSignaturePointer + (peFormat == PE32 ? 232 : 248));
                fs.Position = dataDirectoryStart;

                var cliHeaderRva = reader.ReadUInt32();
                if (cliHeaderRva == 0)
                {
                    return false;
                }

                return true;
            }
        }
    }
}
