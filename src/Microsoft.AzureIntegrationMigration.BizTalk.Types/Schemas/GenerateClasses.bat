xsd.exe ./ApplicationDefinition.xsd /classes /outputdir:../Entities/ApplicationDefinitions /namespace:Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.ApplicationDefinitions
xsd.exe ./BindingInfo.xsd /classes /outputdir:../Entities/Bindings /namespace:Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Bindings
REM xsd.exe ./FilterExpression.xsd /classes /outputdir:../Entities/Filters /namespace:Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Filters
xsd.exe ./MetaModel.xsd /classes /outputdir:../Entities/Orchestrations /namespace:Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Orchestrations
xsd.exe ./Document.xsd /classes /outputdir:../Entities/Pipelines /namespace:Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines
xsd.exe ./Root.xsd /classes /outputdir:../Entities /namespace:Microsoft.AzureIntegrationMigration.BizTalk.Types.Entities.Pipelines
