using OFXParser.Entities;

namespace OFXParser.Test
{
    [TestClass]
    public class ParserTransactionNameTest
    {
        private string ofxFile = string.Empty;

        [TestInitialize]
        public void Initialize()
        {
            ofxFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".ofx");
        }

        [TestCleanup]
        public void Cleanup()
        {
            File.Delete(ofxFile);
            File.Delete(ofxFile + ".xml");
        }

        [TestMethod]
        public void GenerateExtract_TransactionWithName_ReadsNameAndKeepsDescription()
        {
            WriteOfxFile("<NAME>Padaria  A & B", "<MEMO>Compra no debito");

            Extract extract = Parser.GenerateExtract(ofxFile);

            Assert.AreEqual("Padaria A & B", extract.Transactions[0].Name);
            Assert.AreEqual("Compra no debito", extract.Transactions[0].Description);
        }

        [TestMethod]
        public void GenerateExtract_TransactionWithoutName_KeepsNameNull()
        {
            WriteOfxFile("<MEMO>Compra no debito");

            Extract extract = Parser.GenerateExtract(ofxFile);

            Assert.IsNull(extract.Transactions[0].Name);
        }

        [TestMethod]
        public void GenerateExtract_EmptyName_KeepsNameNullAndReadsNextField()
        {
            WriteOfxFile("<NAME>", "<MEMO>Compra no debito");

            Extract extract = Parser.GenerateExtract(ofxFile);

            Assert.IsNull(extract.Transactions[0].Name);
            Assert.AreEqual("Compra no debito", extract.Transactions[0].Description);
        }

        [TestMethod]
        public void GenerateExtract_NameOutsideTransaction_IsIgnored()
        {
            File.WriteAllLines(ofxFile, new[]
            {
                "<OFX>",
                "<NAME>Fora da transacao",
                "<STMTTRN>",
                "<TRNTYPE>DEBIT",
                "</STMTTRN>",
                "</OFX>"
            });

            Extract extract = Parser.GenerateExtract(ofxFile);

            Assert.IsNull(extract.Transactions[0].Name);
        }

        private void WriteOfxFile(params string[] transactionLines)
        {
            List<string> lines = new List<string> { "OFXHEADER:100", "<OFX>", "<STMTTRN>", "<TRNTYPE>DEBIT" };
            lines.AddRange(transactionLines);
            lines.Add("</STMTTRN>");
            lines.Add("</OFX>");

            File.WriteAllLines(ofxFile, lines);
        }
    }
}
