using System;
using System.Configuration;
using System.Linq;
using UMT.IServices;

namespace UMT.Services
{
    public class ApplicationSettingsService : IApplicationSettingsService
    {
        public int ComplexOpearationDbBatchSize => GetSafeIntValue("ComplexOpearationDbBatchSize");

        public string ConnectionStringTemplate => GetSafeStringValue("ConnectionStringTemplate");

        public int DefaultDbBatchSize => GetSafeIntValue("DefaultDbBatchSize");

        public string DefaultEmailRecipientAddress => GetSafeStringValue("DefaultEmailRecipientAddress");

        public int DefaultLogsViewPageSize => GetSafeIntValue("DefaultLogsViewPageSize");

        public int DefaultSearchSourcePageSize => GetSafeIntValue("DefaultSearchSourcePageSize");

        public int DefaultSearchTaskPageSize => GetSafeIntValue("DefaultSearchTaskPageSize");

        public string DefaultSharedDriveReportLocation => GetSafeStringValue("DefaultSharedDriveReportLocation");

        public int DefaultSharePointPageSize => GetSafeIntValue("DefaultSharePointPageSize");

        public string EmailFromAddress => GetSafeStringValue("EmailFromAddress");

        public string[] GmpExcludedLibraryNames => GetSafeStringValue("GmpExcludedLibraryNames")
            .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

        public string[] OneSpExcludedLibraryNames => GetSafeStringValue("OneSpExcludedLibraryNames")
            .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

        public string[] GmpToOneArchiveExludedFields => GetSafeStringValue("GmpToOneArchiveExludedFields")
            .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

        public string GmpToQdMigrationConfigsLocation => GetSafeStringValue("GmpToQdMigrationConfigsLocation");

        public string GoogleClientId => GetSafeStringValue("GoogleClientId");

        public string GoogleClientSecret => GetSafeStringValue("GoogleClientSecret");

        public string HubStoreLoginName => GetSafeStringValue("HubStoreLoginName");

        public string HubStorePassword => GetSafeStringValue("HubStorePassword");

        public int MaxRunningTasksCount => GetSafeIntValue("MaxRunningTasksCount");
        public string NewDbsLocation => GetSafeStringValue("NewDbsLocation");

        public string[] ProjectLibraryIgnoredPermissionGroups => GetSafeStringValue("ProjectLibraryIgnoredPermissionGroups")
            .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

        public int QualityDocsMinimumRemainingApiCalsPercentage => GetSafeIntValue("QualityDocsMinimumRemainingApiCalsPercentage");

        public int QualityDocsServerWorkTimeInMilisecondsPerDocument => GetSafeIntValue("QualityDocsServerWorkTimeInMilisecondsPerDocument");

        public int QualityDocsServerWorkTimeInMilisecondsPerDocumentVersion => GetSafeIntValue("QualityDocsServerWorkTimeInMilisecondsPerDocumentVersion");

        public int QualityDocsServerWorkTimeInMilisecondsPerRendition => GetSafeIntValue("QualityDocsServerWorkTimeInMilisecondsPerRendition");

        public bool SendEmailAfterAllMigrations => GetSafeBooleanValue("SendEmailAfterAllMigrations");

        public bool SendEmailAfterEachMigration => GetSafeBooleanValue("SendEmailAfterEachMigration");

        public string SmtpServer => GetSafeStringValue("SmtpServer");

        public string UapAuditTrailFileName => GetSafeStringValue("UapAuditTrailFileName");

        public string UapClientId => GetSafeStringValue("UapClientId");

        public string UapGroupsManagerLoginName => GetSafeStringValue("UapGroupsManagerLoginName");

        public string UapGroupsManagerPassword => GetSafeStringValue("UapGroupsManagerPassword");

        public string UapLoginName => GetSafeStringValue("UapLoginName");

        public string UapMetadataFileName => GetSafeStringValue("UapMetadataFileName");
        public string[] UapMigrationsTechniciansLogins => GetSafeStringValue("UapMigrationsTechniciansLogins")
            .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

        public string UapPassword => GetSafeStringValue("UapPassword");
        public string UapTenant => GetSafeStringValue("UapTenant");

        private void EnsureKey(string key)
        {
            if (!ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                throw new Exception("Could not find '" + key + "' property in the config file.");
            }
        }

        private bool GetSafeBooleanValue(string key)
        {
            EnsureKey(key);
            return Convert.ToBoolean(ConfigurationManager.AppSettings[key]);
        }

        private int GetSafeIntValue(string key)
        {
            EnsureKey(key);
            return Convert.ToInt32(ConfigurationManager.AppSettings[key]);
        }

        private string GetSafeStringValue(string key)
        {
            EnsureKey(key);
            return ConfigurationManager.AppSettings[key];
        }
    }
}
