namespace MmsServer
{
    public partial class MainForm : Form
    {
        private void CopyPublicKey()
        {
            byte[] pubKey = _rsa.ExportRSAPublicKey();
            string pubKeyStr = Convert.ToBase64String(pubKey);

            Clipboard.SetText(pubKeyStr);
            AppendLog("Public key copied to clipboard.");
        }

        private void LoadKey()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Key File|*.key";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string fileName = dialog.FileName;

                    string keyContent = File.ReadAllText(fileName);
                    string[] keyParts = keyContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                    byte[] pubKey = Convert.FromBase64String(keyParts[0]);
                    byte[] privKey = Convert.FromBase64String(keyParts[1]);

                    _rsa.ImportRSAPublicKey(pubKey, out _);
                    _rsa.ImportRSAPrivateKey(privKey, out _);

                    AppendLog($"Key loaded successfully from {dialog.FileName}.");
                }
            }
        }

        private void SaveKey()
        {
            byte[] pubKey = _rsa.ExportRSAPublicKey();
            byte[] privKey = _rsa.ExportRSAPrivateKey();

            string pubKeyStr = Convert.ToBase64String(pubKey);
            string privKeyStr = Convert.ToBase64String(privKey);

            string keyStr = $"{pubKeyStr}\n{privKeyStr}";

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Key File|*.key";
                dialog.FileName = "new.key";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(dialog.FileName, keyStr);
                    File.WriteAllText(dialog.FileName + ".pub", pubKeyStr);

                    AppendLog($"Key saved successfully to {dialog.FileName}.",
                        $"Public key saved successfully to {dialog.FileName}.pub.");
                }
            }
        }
    }
}
