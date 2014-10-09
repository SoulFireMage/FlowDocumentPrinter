using System;
//using System.Drawing;
using System.Drawing.Printing;
using System.Printing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace FlowPrinter
{
    public class PrintFunctions
    {
        #region "Properties"
        private static PrintFunctions _printFunctionSingleton {get; set; }
        // Mutual Exclusion, required to check for thread duplication
        private static readonly Mutex _printMutex = new Mutex();

        private FlowDocument _Document {get; set; }
        private PrintDialog _PrintDialog {get; set; }
        private Margins _Margin = new Margins(20, 20, 20, 20);
        private DocumentPaginator _Paginator {get; set; } 
        private PrintPaginatorWrapper _PaginatorWrapper {get; set; } 
        private double _headPadding {get; set; } 
        private double _footPadding {get; set; }
        #endregion


#region "Get Print Function Instance and Overloads"
        private void New()
        {
            PrintDialog _PrintDialog = new PrintDialog();
         }


        public static PrintFunctions GetInstance()
        {
            _printMutex.WaitOne();
            try
            {
                if (_printFunctionSingleton == null)
                {
                    _printFunctionSingleton = new PrintFunctions();
                }
            }
            finally
            {
                _printMutex.ReleaseMutex();
            }
            return _printFunctionSingleton;
        }

    
    public void SetupPage(FlowDocument document, Margins margin)
        {
            SetupPage(document, margin, 1.0, Brushes.Black);
        }

    public void SetupPage(FlowDocument document, Margins margin, double border, Brush brush)
        {
            _headPadding = 40;
            _footPadding = 20;
            SetupPage(document, margin, border, brush, _headPadding, _footPadding);
    }

    public void SetupPage(FlowDocument document, Margins margin, double border, Brush brush, double heading, double footing)
        {
            _Paginator = GetPaginatedDocument(document);
            _PaginatorWrapper = new PrintPaginatorWrapper(_Paginator, margin, border, brush, heading, footing);
            GetInstance();
     }

    public void SetupPage(PrintObject PrintObject)
        {
            _Paginator = GetPaginatedDocument(PrintObject.Document);
            _PaginatorWrapper = new PrintPaginatorWrapper(_Paginator, PrintObject.Margin, PrintObject.Border, PrintObject.Brush, PrintObject.HeadMargin, PrintObject.FootMargin);
            GetInstance();
       }
 
    #endregion

    #region "Print or Preview"
    public void PrintDocument(FlowDocument document, Margins margin, double border, Brush brush)
        {
            _PrintDialog = new PrintDialog();

            if (_PrintDialog.ShowDialog() == true)
            {
                SetupPage(document, margin, border, brush);
                _PrintDialog.PrintDocument(_PaginatorWrapper, "New File.pdf");

        }
    }

        #region "GetPaginator"
        //Turns a Flowdocument into a paginated fixed document. Overall width and height worked out here
        //we talk to DocumentPaginatorWrapper next. Needs a new name!
        private DocumentPaginator GetPaginatedDocument(FlowDocument document)
        {

            double height = _PrintDialog.PrintTicket.PageMediaSize.Height.Value;
            double width = _PrintDialog.PrintTicket.PageMediaSize.Width.Value;

            if (_PrintDialog.PrintTicket.PageOrientation == PageOrientation.Landscape)
            {
                height = _PrintDialog.PrintTicket.PageMediaSize.Width.Value;
                width = _PrintDialog.PrintTicket.PageMediaSize.Height.Value;
            }

            FlowDocument sourceDocument = (FlowDocument) XamlReader.Parse(XamlWriter.Save(document));
                      //Find out why i did this?
            //FlowDocument copy = sourceDocument;
            FlowDocument flowDocumentCopy = sourceDocument;

            flowDocumentCopy.PagePadding = new Thickness(0, _headPadding, 0, _footPadding);
            flowDocumentCopy.ColumnWidth = width;

            DocumentPaginator paginator = ((IDocumentPaginatorSource)flowDocumentCopy).DocumentPaginator;

            paginator.PageSize = new Size(width, height);

            return paginator;

        }

        #endregion

        public void PrintDocument(FlowDocument flowDocument)
        {
            PrintDocument(flowDocument, _Margin, 1, Brushes.Black);
        }

        public void PrintDocument(FlowDocument flowDocument, string header)
        {
            PrintDocument(flowDocument, _Margin, 1, Brushes.Black);
        }
 
    }



    public struct PrintObject
    {
        public FlowDocument Document { get; set; }
        public double HeadMargin { get; set; }
        public double FootMargin { get; set; }
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public Brush Brush { get; set; }
        public System.Drawing.Font Font { get; set; }
        public double borderThickness { get; set; }
        public Margins Margin { get; set; }
        public double Border { get; set; }
    }
}
#endregion