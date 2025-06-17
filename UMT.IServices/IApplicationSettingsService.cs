namespace UMT.IServices
{
    public interface IApplicationSettingsService
    {
        int ComplexOpearationDbBatchSize { get; }

        string ConnectionStringTemplate { get; }

        int DefaultDbBatchSize { get; }

        string DefaultEmailRecipientAddress { get; }

        int DefaultLogsViewPageSize { get; }

        int DefaultSearchSourcePageSize { get; }

        int DefaultSearchTaskPageSize { get; }

        string DefaultSharedDriveReportLocation { get; }

        int DefaultSharePointPageSize { get; }

        string EmailFromAddress { get; }

        string[] OneSpExcludedLibraryNames { get; }

        string[] GmpExcludedLibraryNames { get; }

        string[] GmpToOneArchiveExludedFields { get; }

        string GmpToQdMigrationConfigsLocation { get; }

        string GoogleClientId { get; }

        string GoogleClientSecret { get; }

        string HubStoreLoginName { get; }

        string HubStorePassword { get; }

        int MaxRunningTasksCount { get; }
        string NewDbsLocation { get; }

        string[] ProjectLibraryIgnoredPermissionGroups { get; }

        int QualityDocsMinimumRemainingApiCalsPercentage { get; }

        int QualityDocsServerWorkTimeInMilisecondsPerDocument { get; }

        int QualityDocsServerWorkTimeInMilisecondsPerDocumentVersion { get; }

        int QualityDocsServerWorkTimeInMilisecondsPerRendition { get; }

        bool SendEmailAfterAllMigrations { get; }

        bool SendEmailAfterEachMigration { get; }

        string SmtpServer { get; }

        string UapAuditTrailFileName { get; }
        string UapClientId { get; }

        string UapGroupsManagerLoginName { get; }

        string UapGroupsManagerPassword { get; }

        string UapLoginName { get; }

        string UapMetadataFileName { get; }
        string[] UapMigrationsTechniciansLogins { get; }

        string UapPassword { get; }
        string UapTenant { get; }
    }
}
