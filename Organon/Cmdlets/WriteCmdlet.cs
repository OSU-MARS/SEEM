using System.IO;
using System.Management.Automation;

namespace Osu.Cof.Ferm.Cmdlets
{
    public class WriteCmdlet : Cmdlet
    {
        private bool openedExistingFile;

        [Parameter]
        public SwitchParameter Append;

        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string CsvFile;

        public WriteCmdlet()
        {
            this.Append = false;
            this.openedExistingFile = false;
        }

        protected StreamWriter GetWriter()
        {
            FileMode fileMode = this.Append ? FileMode.Append : FileMode.Create;
            if (fileMode == FileMode.Append)
            {
                this.openedExistingFile = File.Exists(this.CsvFile);
            }
            return new StreamWriter(new FileStream(this.CsvFile, fileMode, FileAccess.Write, FileShare.Read));
        }

        protected bool ShouldWriteHeader()
        {
            return this.openedExistingFile == false;
        }
    }
}
