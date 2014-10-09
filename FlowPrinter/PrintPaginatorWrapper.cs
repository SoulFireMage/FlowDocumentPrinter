using System;
using System.Drawing.Printing;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
//using System.Windows.Media;
//TODO:Parameterise Heading, Footing
//TODO:re add overloads taken out during C# conversion
//TODO:Fix The HardCoded Fonts
public class PrintPaginatorWrapper : DocumentPaginator
{
    #region "constructors and properties"
    public static System.Windows.Size PPageSize;
    private readonly Margins _margin;
    private readonly DocumentPaginator _paginator;
    private readonly System.Windows.Media.Brush _borderBrush;
    private readonly double _thickness;
    private readonly double _headerSize;
    private readonly double _footerSize;
 
    private PrintPaginatorWrapper p;

    public PrintPaginatorWrapper(DocumentPaginator paginator, Margins margin, double borderThickness, System.Windows.Media.Brush borderBrush, double headerSize, double footerSize) : this()
    {
        //One way to provide a few options. Set them all here.
        _headerSize = headerSize;
        _footerSize = footerSize;
        _thickness = borderThickness;
        _borderBrush = borderBrush;
        PPageSize = paginator.PageSize;
        _margin = margin;
        _paginator = paginator;
        DocumentPage t = _paginator.GetPage(1);
        _paginator.PageSize = new System.Windows.Size(PPageSize.Width - (margin.Left + margin.Right), PPageSize.Height - (margin.Top + margin.Bottom));

    }

    //One way to provide a few options. Set them all here.
    public PrintPaginatorWrapper(DocumentPaginator paginator, Margins margin, double borderThickness, System.Windows.Media.Brush borderBrush, double headerSize) : this(paginator, margin, borderThickness, borderBrush, 0, 0)
    {
    }

    //all i need to do now is directly influence the page itself with the headfootsize!
    public PrintPaginatorWrapper(DocumentPaginator paginator, Margins margin, double borderThickness, System.Windows.Media.Brush borderBrush) : this(paginator, margin, borderThickness, borderBrush, 0)
    {
        //remember to swap this!
    }

    public PrintPaginatorWrapper(DocumentPaginator paginator, Margins margin) : this(paginator, margin,   null   )
    {
    }

 
    protected PrintPaginatorWrapper()
    {
    }

    public PrintPaginatorWrapper(DocumentPaginator paginator, Margins margin, PrintPaginatorWrapper p) : this(paginator, margin)
    {
        this.p = p;
    }
    #endregion


    private Rect Move(Rect rect)
    {
        return rect.IsEmpty ? rect : new Rect(rect.Left + (_margin.Left), rect.Top + (_margin.Top + _margin.Bottom + ((_headerSize + _footerSize) / 2)), rect.Width, rect.Height);
    }

    //TODO Page numbering class.
    //TODO Expose methods
    //TODO Seperate visual builder class?'3 elements need defining. Heading, Footer, Page Number style.


    private DrawingVisual LayoutHeadingandNumbering(int pageNumber)
    {
        return LayoutHeadingAndNumber(pageNumber, "Heading Demo ", 22, System.Windows.Media.Brushes.Blue);

    }


    private DrawingVisual LayoutHeadingAndNumber(int pageNumber, string Heading, int fontSize, System.Windows.Media.Brush brush)
    {
        DrawingVisual title = new DrawingVisual();
        using (DrawingContext titleContext = title.RenderOpen())
        {

            TextPreformats text3 = GetPageNumbers(pageNumber);
            //TODO this must be parameterised, HARD CODED DEMO ONLY
           
            TextPreformats headingDemo = new TextPreformats(Heading, fontSize, brush);
            TextPreformats footerTitle = new TextPreformats(System.DateTime.Today, 14, System.Windows.Media.Brushes.Green);

            Rect contentBox = _paginator.GetPage(pageNumber).ContentBox;

            PlaceHeadingFooting(headingDemo, contentBox, Positions.Topleft, titleContext);
            if (text3.textHeight < _footerSize + 2)
                PlaceHeadingFooting(text3, contentBox, Positions.BottomRight, titleContext);

            PlaceHeadingFooting(text3, contentBox, Positions.Topright, titleContext);
            if (text3.textHeight < _footerSize + 2)
                PlaceHeadingFooting(footerTitle, contentBox, Positions.Bottomleft, titleContext);

        }

        return title;
    }

    private object PlaceHeadingFooting(TextPreformats text, Rect contentBox, Positions position, DrawingContext context)
    {

        System.Windows.Point topLeftPoint = new System.Windows.Point();
        System.Windows.Point topRightPoint  = new System.Windows.Point();
        System.Windows.Point bottomLeftPoint  = new System.Windows.Point();
        System.Windows.Point bottomRightPoint  = new System.Windows.Point();

        CreateCornerPoints(contentBox, text, ref topLeftPoint, ref topRightPoint, ref bottomLeftPoint, ref bottomRightPoint);

        FormattedText fText = text.P_formattedText;

        switch (position)
        {
            case Positions.Topleft:
                context.DrawText(fText, topLeftPoint);
                break;
            case Positions.Topright:
                context.DrawText(fText, topRightPoint);
                break;
            case Positions.Bottomleft:
                context.DrawText(fText, bottomLeftPoint);
                break;
            case Positions.BottomRight:
                context.DrawText(fText, bottomRightPoint);
                break;
        }

        return context;
    }

    private void CreateCornerPoints(Rect contentBox, TextPreformats text,
                                    ref System.Windows.Point topLeftPoint, 
                                    ref System.Windows.Point topRightPoint, 
                                    ref System.Windows.Point bottomLeftPoint, 
                                    ref System.Windows.Point bottomRightPoint)
    {
        double headingTopY = _thickness * 5;

        double headingBottomY = PPageSize.Height - (_margin.Bottom * 2) - (_thickness * 2) - text.textHeight;
        //contentBox.Bottom + (m_FooterSize / 4) - m_Thickness
        double headingLeftX = contentBox.Left + (_margin.Right / 2) + (_margin.Left / 2);
        double headingRightX = contentBox.Right - text.textWidth - (_margin.Right - _thickness);

        topLeftPoint = new System.Windows.Point
        {
            X = headingLeftX,
            Y = headingTopY
        };
        topRightPoint = new System.Windows.Point
        {
            X = headingRightX,
            Y = headingTopY
        };

        bottomLeftPoint = new System.Windows.Point
        {
            X = headingLeftX,
            Y = headingBottomY
        };
        bottomRightPoint = new System.Windows.Point
        {
            X = headingRightX,
            Y = headingBottomY
        };
    }

    public enum Positions : int
    {
        Topleft,
        Topright,
        Bottomleft,
        BottomRight
    }

    private TextPreformats GetPageNumbers(int pageNumber)
    {
        TextPreformats text3 = new TextPreformats("Page " + (pageNumber + 1), System.Windows.Media.Brushes.Red);
        return text3;
    }

    public override DocumentPage GetPage(int pageNumber)
    {
        //Does too many jobs. Should be asking for the containers to print, something else should build containers and decide how margins should be applied?

        DocumentPage page = _paginator.GetPage(pageNumber);
        ContainerVisual mainPageContent = new ContainerVisual();
        DrawingVisual headFootandBorder = new DrawingVisual();
        //Makes border, header and footer
        GetHeaderFooter(page, headFootandBorder);

        mainPageContent.Children.Add(headFootandBorder);
        // Scale down page and center
        mainPageContent.Children.Add(ReScaleMainContent(page));
        mainPageContent.Children.Add(LayoutHeadingandNumbering(pageNumber));

        //Whole page including borders moved!
        mainPageContent.Transform = new TranslateTransform((_margin.Left), (_margin.Top));

        return new DocumentPage(mainPageContent, PPageSize, Move(page.BleedBox), Move(page.ContentBox));
    }



    private void GetHeaderFooter(DocumentPage page, DrawingVisual backGround)
    {
        System.Windows.Media.Pen pen = new System.Windows.Media.Pen(_borderBrush, _thickness);
        using (DrawingContext backgroundContext = backGround.RenderOpen())
        {
            //header & footer dept.
            GetRectangles(page, backgroundContext, pen);
            backgroundContext.DrawRectangle(null, pen, page.ContentBox);

        }
    }

    private DrawingContext GetRectangles(DocumentPage page, DrawingContext backgroundContext, System.Windows.Media.Pen pen)
    {
        double boxLeft = page.ContentBox.Left;
        double boxTop = page.ContentBox.Top;
        double boxRight = page.ContentBox.Right;
        double boxBottom = page.ContentBox.Bottom;

        System.Windows.Media.Pen mainBoxPen = new System.Windows.Media.Pen(_borderBrush, _thickness * 2);

        //Big rectangle
        System.Windows.Point mainBoxTopLeft = new System.Windows.Point
        {
            X = boxLeft,
            Y = boxTop - _headerSize
        };
        System.Windows.Point mainBoxBottomRight = new System.Windows.Point
        {
            X = boxRight,
            Y = boxBottom + _footerSize
        };
        System.Windows.Rect mainRect = new Rect(mainBoxTopLeft, mainBoxBottomRight);

        backgroundContext.DrawRectangle(null, mainBoxPen, mainRect);
        //header
        drawPageline(boxLeft, boxRight, boxTop, pen, backgroundContext);
        //footer
        drawPageline(boxLeft, boxRight, boxBottom, pen, backgroundContext);

        return backgroundContext;
    }

    private void drawPageline(double leftX, double rightX, double Y, System.Windows.Media.Pen pen, DrawingContext context)
    {
        System.Windows.Point lineLeftStart = new System.Windows.Point(leftX, Y);
        System.Windows.Point lineRightFinish = new System.Windows.Point(rightX, Y);
        context.DrawLine(pen, lineLeftStart, lineRightFinish);
    }

    #region "Rescaling Containers"
    /// <summary>
    /// Even with ridiculous borders and margins, the inner content is centered. 
    /// This does that-even when the main outside lines are placed on the page wrong!
    /// </summary>
    /// <param name="page">The Rescaled Content Page.</param>
    /// <returns></returns>

    private ContainerVisual ReScaleMainContent(DocumentPage page)
    {

        ContainerVisual smallerPage = new ContainerVisual();

        double currentWidth = page.ContentBox.Width;
        double currentHeight = page.ContentBox.Height;
        double adjWidth = page.ContentBox.Width - (_margin.Left + _margin.Right + (_thickness * 2));
        double adjHeight = page.ContentBox.Height - (_margin.Top + _margin.Bottom + (_thickness * 2));


        double centerX = currentWidth / 2;
        double centerY = (currentHeight / 2) + (_headerSize + _margin.Top);

        double ScaleX = adjWidth / currentWidth;
        double scaleY = adjHeight / currentHeight;

        smallerPage.Children.Add(page.Visual);
        smallerPage.Transform = new ScaleTransform(ScaleX, scaleY, centerX, centerY);

        return smallerPage;
    }



    #endregion

    #region "Properties"
    public override bool IsPageCountValid
    {
        get { return _paginator.IsPageCountValid; }
    }

    public override int PageCount
    {
        get { return _paginator.PageCount; }
    }

    public override System.Windows.Size PageSize
    {
        get { return _paginator.PageSize; }
        set { _paginator.PageSize = value; }
    }

    public override IDocumentPaginatorSource Source
    {
        get { return _paginator.Source; }
    }

    
    #endregion
}

public class TextPreformats
{
    private DateTime today;
    private int v;
    private System.Windows.Media.Brush green;

    private Typeface _typeFace { get; set; }
    private int _fontSize { get; set; }
    private System.Windows.Media.Brush _brush { get; set; }
    private string _text { get; set; }
    public double textHeight { get; set; }
    public double textWidth { get; set; }
    public FormattedText P_formattedText { get; set; }

    public TextPreformats(string text, Typeface type, int fontSize, System.Windows.Media.Brush brush)
    {
        
        if (type != null)
            _typeFace = type;
        if (brush != null)
            _brush = brush;
        if (fontSize != 0)
            _fontSize = fontSize;
        if  (text != null)
            _text = text;

        PreformatText();
    }

    public TextPreformats(string text, int fontsize, System.Windows.Media.Brush brush)
    {
        _text = text;
        _fontSize = fontsize;
        _brush = brush;
        PreformatText();
    }

    public TextPreformats(string text, System.Windows.Media.Brush brush)
    {
        _text = text;
        _brush = brush;
        PreformatText();
    }

    public TextPreformats(string text, int fontsize)
    {
        _text = text;
        _fontSize = fontsize;
        PreformatText();
    }

    public TextPreformats(string text, Typeface type)
    {
        _typeFace = type;
        _text = text;
        PreformatText();
    }

    public TextPreformats(string text)
    {
        _text = text;
        PreformatText();
    }

    public TextPreformats(DateTime today, int v, System.Windows.Media.Brush green)
    {
        this.today = today;
        this.v = v;
        this.green = green;
    }

    private void PreformatText()
    {
        System.Windows.Media.Brush Brush = _brush;
        if (_typeFace == null)
            _typeFace = new Typeface("Arial");
        if (_fontSize <= 0)
            _fontSize = 10;

        FormattedText formattedText = new FormattedText(_text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, _typeFace, _fontSize, _brush);
        textHeight = formattedText.Height;
        textWidth = formattedText.Width;
        P_formattedText = formattedText;
        return;
    }

}

 
