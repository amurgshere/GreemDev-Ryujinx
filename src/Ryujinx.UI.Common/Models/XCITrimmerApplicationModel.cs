using Ryujinx.Common.Logging;
using Ryujinx.Common.Utilities;
using Ryujinx.UI.App.Common;

namespace Ryujinx.UI.Common.Models
{
    public record XCITrimmerApplicationModel(string Name, string Path, bool Trimmable, bool Untrimmable, long PotentialSavingsB, long CurrentSavingsB)
    {
        public static XCITrimmerApplicationModel FromApplicationData(ApplicationData applicationData, XCIFileTrimmerLog logger)
        {
            var trimmer = new XCIFileTrimmer(applicationData.Path, logger);

            return new XCITrimmerApplicationModel(
                applicationData.Name,
                applicationData.Path,
                trimmer.CanBeTrimmed,
                trimmer.CanBeUntrimmed,
                trimmer.DiskSpaceSavingsB,
                trimmer.DiskSpaceSavedB
            );
        }
    }
}
