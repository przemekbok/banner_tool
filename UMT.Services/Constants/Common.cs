namespace UMT.Services.Constants
{
    internal static class Common
    {
        internal const string CamlQueryView = "<View Scope=\"All\"> " +
                        "<RowLimit>{0}</RowLimit>" +
                    "</View>";
        internal const string RecPointAdditionalInfoCamlQuery = "<View>" +
                                                                "<ViewFields><FieldRef Name='ContainerUrl' />" +
                                                                "<FieldRef Name='TriggerMode' />" +
                                                                "<FieldRef Name='DispositionMode' />" +
                                                                "<FieldRef Name='SuspendDeletion' />" +
                                                                "<FieldRef Name='RetentionPeriodOffset' />" +
                                                                "</ViewFields>"
                                                                + "</View>";

        internal const string UapStorsPattern = @"/stors/(.+?)/";
        internal const string UapFolderPattern = @"/folders/(.+?)";

        internal const string RecPointDummyGroupName = "Migration Read Only Dummy Group";
        internal const string RecPointDummyGroupDescription = "This group was created to migrate RecPoint to a new environment.";
        internal const string AppName = "UltimateMigrationTool";
    }
}
