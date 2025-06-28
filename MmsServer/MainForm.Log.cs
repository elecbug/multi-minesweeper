namespace MmsServer
{
    public partial class MainForm : Form
    {
        private void AppendLog(params string[] messages)
        {
            string timestamp = $"{DateTime.Now:yy.MM.dd. HH:mm:ss}";

            lock (_logBox)
            {
                foreach (string msg in messages)
                {
                    _logBox.AppendText($"{timestamp} | {msg}\n");
                }
                _logBox.AppendText("\n");

                _logBox.SelectionStart = _logBox.TextLength;
                _logBox.ScrollToCaret();
            }
        }

        private void SaveLog()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Log File|*.log";
                dialog.FileName = "server.log";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(dialog.FileName, _logBox.Text);

                    AppendLog($"Log saved successfully to {dialog.FileName}.");
                }
            }
        }
    }
}
