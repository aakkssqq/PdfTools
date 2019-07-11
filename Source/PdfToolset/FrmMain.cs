using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PdfToolset
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        private List<string> lstPDFFiles = new List<string>();

        private void AppendFile(string path, string filename)
        {
            if (lstPDFFiles.Contains(path))
            {
                return;
            }
            lstPDFFiles.Add(path);
            this.lstFiles.Items.Add(filename);
            
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            var dr = this.folderBrowserDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                var folder = this.folderBrowserDialog1.SelectedPath;
                if (!string.IsNullOrWhiteSpace(folder))
                {
                    var dir = new System.IO.DirectoryInfo(folder);
                    foreach (var item in dir.EnumerateFiles())
                    {
                        if (item.Extension == ".pdf")
                        {
                            AppendFile(item.FullName, item.Name);
                        }
                    }
                }
            }
            
        }

        private void btnChooseFile_Click(object sender, EventArgs e)
        {
            var dr = this.openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                var file = new System.IO.FileInfo(this.openFileDialog1.FileName);
                AppendFile(file.FullName, file.Name);
            }
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            if (lstPDFFiles.Count == 0)
            {
                return;
            }
            foreach (var item in lstPDFFiles)
            {
                var fi = new FileInfo(item);
                removePagesFromPdf(item, System.IO.Path.Combine(fi.DirectoryName, string.Format("{0}-trim.pdf", Path.GetFileNameWithoutExtension(item))));
            }
        }

        //public static void ExtractPagesFromPdf(string inputFile, string outputFile, int start, int end)
        //{
        //    PdfReader inputPdf = new PdfReader(inputFile);
        //    PdfDocument docIn = new PdfDocument(inputPdf);

        //    PdfWriter outputWriter = new PdfWriter(outputFile);
        //    PdfDocument docOut = new PdfDocument(outputWriter);

        //    // retrieve the total number of pages
        //    int pageCount = docIn.GetNumberOfPages();

        //    if (end < start || end > pageCount)
        //    {
        //        end = pageCount;
        //    }

        //    var merge = new PdfMerger(docOut);

        //    merge.Merge(docIn, start, end);

        //    merge.Close();
        //}

        public void removePagesFromPdf(String sourceFile, String destinationFile)
        {
            try
            {
                //Used to pull individual pages from our source
                PdfReader reader = new PdfReader(sourceFile);
                if (reader.NumberOfPages < 2)
                {
                    Log(string.Format(" 文件 {0}页数=1，不删除", sourceFile));
                    return;
                }
                //Create our destination file
                using (FileStream fs = new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (Document doc = new Document())
                    {
                        using (PdfWriter w = PdfWriter.GetInstance(doc, fs))
                        {
                            //Open the desitination for writing
                            doc.Open();
                            for (int i = 1; i < reader.NumberOfPages; i++)
                            {
                                //Add a new blank page to destination document
                                doc.NewPage();
                                //Extract the given page from our reader and add it directly to the destination PDF

                                PdfContentByte cb = w.DirectContent;
                                PdfImportedPage pageImport = w.GetImportedPage(reader, i);
                                int rot = reader.GetPageRotation(i);
                                //if (rot == 90)
                                //    cb.AddTemplate(pageImport, 0, -1.0F, 1.0F, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                                //else if (rot == 270)
                                //    cb.AddTemplate(pageImport, 0, -1.0F, 1.0F, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                                //else
                                //    cb.AddTemplate(pageImport, 1.0F, 0, 0, 1.0F, 0, 0);
                                switch (rot)
                                {
                                    case 0:
                                        cb.AddTemplate(pageImport, 1f, 0, 0, 1f, 0, 0);
                                        break;

                                    case 90:
                                        cb.AddTemplate(pageImport, 0, -1f, 1f, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                                        break;

                                    case 180:
                                        cb.AddTemplate(pageImport, -1f, 0, 0, -1f, reader.GetPageSizeWithRotation(i).Width, reader.GetPageSizeWithRotation(i).Height);
                                        break;

                                    case 270:
                                        cb.AddTemplate(pageImport, 0, 1f, -1f, 0, reader.GetPageSizeWithRotation(i).Width, 0);
                                        break;

                                    default:
                                        throw new InvalidOperationException(string.Format("Unexpected page rotation: [{0}].", rot));
                                }
                                //w.DirectContent.AddTemplate(w.GetImportedPage(r, i), 0, 0);
                            }

                            //Close our document
                            doc.Close();
                            Log(string.Format(" 文件 {0} 删除成功", sourceFile));
                        }
                    }
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Log("发生未知错误：" + ex.ToString());
            }
            
        }

        private void Log(string msg)
        {
            txtLog.AppendText(string.Format(" {0}\r\n", msg));
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            this.lstPDFFiles.Clear();
            this.lstFiles.Items.Clear();
            this.txtLog.Text = "";
        }
    }
}
