using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PaintDemo.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class Main : Page
{
    public Main()
    {
        this.InitializeComponent();
    }
    private Brush currentBrush = new SolidColorBrush(Colors.Black);
    private bool isDrawing = false;
    private bool isErasing = false;
    private Point startPoint;
    private List<UIElement> elementsToRemove = new();
    private int selectedIndex = 0;
    private Ellipse currentEllipse;
    private Rectangle currentRectangle;
    private Polygon currentPolygon;
    private enum DrawingMode
    {
        Circle,
        Rectangle,
        Triangle
    }
    private DrawingMode currentDrawingMode = DrawingMode.Rectangle;
    public List<double> BrushThickness =
    [
        4,8,18,20,31,42,54,66,78,80,94,108,116,148,172
    ];

    private void pencilButton_Tapped(object sender, TappedRoutedEventArgs e)
    {
        //var clickedButton = sender as ToggleButton;

        brushButton.IsChecked = false;
        eraserButton.IsChecked = false;
        colorPickerButton.IsChecked = false;
        shapesButton.IsChecked = false;
        pencilButton.IsChecked = true;
    }

    private void brushButton_Tapped(object sender, TappedRoutedEventArgs e)
    {
        pencilButton.IsChecked = false;
        eraserButton.IsChecked = false;
        colorPickerButton.IsChecked = false;
        shapesButton.IsChecked = false;
        brushButton.IsChecked = true;
    }

    private void eraserButton_Tapped(object sender, TappedRoutedEventArgs e)
    {
        isErasing = !isErasing;
        currentBrush = new SolidColorBrush(Colors.White);

        brushButton.IsChecked = false;
        pencilButton.IsChecked = false;
        colorPickerButton.IsChecked = false;
        shapesButton.IsChecked = false;
        eraserButton.IsChecked = true;
    }

    private void clearButton_Tapped(object sender, TappedRoutedEventArgs e)
    {
        canvas.Children.Clear();
    }

    private void colorPickerButton_Tapped(object sender, TappedRoutedEventArgs e)
    {
        brushButton.IsChecked = false;
        pencilButton.IsChecked = false;
        eraserButton.IsChecked = false;
        shapesButton.IsChecked = false;
        colorPickerButton.IsChecked = true;
    }

    private void colorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
    {
        currentBrush = new SolidColorBrush(args.NewColor);
        currentColor.Background = new SolidColorBrush(args.NewColor);
    }

    private void comboBoxBrushThickness_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        selectedIndex = comboBoxBrushThickness.SelectedIndex;
    }

    private void shapesButton_Tapped(object sender, TappedRoutedEventArgs e)
    {
        brushButton.IsChecked = false;
        pencilButton.IsChecked = false;
        eraserButton.IsChecked = false;
        colorPickerButton.IsChecked = false;
        shapesButton.IsChecked = true;
    }

    private void comboBoxShapes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        currentDrawingMode = (sender as ComboBox).SelectedIndex switch
        {
            0 => DrawingMode.Circle,
            1 => DrawingMode.Rectangle,
            2 => DrawingMode.Triangle,
            _ => DrawingMode.Circle
        };
    }

    private async void canvas_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (e.Pointer.PointerDeviceType is Microsoft.UI.Input.PointerDeviceType.Mouse)
        {
            if (colorPickerButton.IsChecked.Value)
            {
                var pointerPosition = e.GetCurrentPoint(canvas);
                int x = (int)pointerPosition.Position.X;
                int y = (int)pointerPosition.Position.Y;

                RenderTargetBitmap renderTargetBitmap = new();
                await renderTargetBitmap.RenderAsync(canvas);

                var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
                var pixelData = pixelBuffer.ToArray();

                int pixelIndex = (y * renderTargetBitmap.PixelWidth + x) * 4;

                byte[] pixelColor = new byte[4];
                Array.Copy(pixelData, pixelIndex, pixelColor, 0, 4);

                Color color = Color.FromArgb(pixelColor[3], pixelColor[2], pixelColor[1], pixelColor[0]);

                currentColor.Background = currentBrush = new SolidColorBrush(color);
                return;
            }
            isDrawing = true;
            startPoint = e.GetCurrentPoint(canvas).Position;

            if (shapesButton.IsChecked.Value)
            {
                switch (currentDrawingMode)
                {
                    case DrawingMode.Circle:
                        currentEllipse = new Ellipse
                        {
                            Width = 0,
                            Height = 0,
                            Stroke = currentBrush,
                            StrokeThickness = BrushThickness[selectedIndex]
                        };
                        canvas.Children.Add(currentEllipse);
                        Canvas.SetLeft(currentEllipse, startPoint.X);
                        Canvas.SetTop(currentEllipse, startPoint.Y);
                        break;
                    case DrawingMode.Rectangle:
                        currentRectangle = new Rectangle
                        {
                            Width = 0,
                            Height = 0,
                            Stroke = currentBrush,
                            StrokeThickness = BrushThickness[selectedIndex]
                        };
                        canvas.Children.Add(currentRectangle);
                        Canvas.SetLeft(currentRectangle, startPoint.X);
                        Canvas.SetTop(currentRectangle, startPoint.Y);
                        break;
                    case DrawingMode.Triangle:
                        currentPolygon = new Polygon
                        {
                            Stroke = currentBrush,
                            StrokeThickness = BrushThickness[selectedIndex]
                        };
                        currentPolygon.Points.Add(startPoint);
                        currentPolygon.Points.Add(startPoint);
                        currentPolygon.Points.Add(startPoint);

                        canvas.Children.Add(currentPolygon);
                        break;
                }
            }
        }
    }

    private void canvas_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!isDrawing)
            return;
        if (shapesButton.IsChecked.Value)
        {
            var currentPoint = e.GetCurrentPoint(canvas).Position;
            switch (currentDrawingMode)
            {
                case DrawingMode.Circle:
                    if (currentEllipse is null)
                        return;
                    double newWidth = Math.Abs(currentPoint.X - startPoint.X) * 2d;
                    double newHeight = Math.Abs(currentPoint.Y - startPoint.Y) * 2d;

                    currentEllipse.Width = newWidth;
                    currentEllipse.Height = newHeight;

                    double left = Math.Min(startPoint.X, currentPoint.X);
                    double top = Math.Min(startPoint.Y, currentPoint.Y);
                    Canvas.SetLeft(currentEllipse, left);
                    Canvas.SetTop(currentEllipse, top);
                    break;
                case DrawingMode.Rectangle:
                    if (currentRectangle is null)
                        return;
                    newWidth = Math.Abs(currentPoint.X - startPoint.X);
                    newHeight = Math.Abs(currentPoint.Y - startPoint.Y);

                    currentRectangle.Width = newWidth;
                    currentRectangle.Height = newHeight;

                    left = Math.Min(startPoint.X, currentPoint.X);
                    top = Math.Min(startPoint.Y, currentPoint.Y);
                    Canvas.SetLeft(currentRectangle, left);
                    Canvas.SetTop(currentRectangle, top);
                    break;
                case DrawingMode.Triangle:
                    if (currentPolygon is null)
                        return;
                    double centerX = (startPoint.X + currentPoint.X) / 2d;
                    double centerY = (startPoint.Y + currentPoint.Y) / 2d;
                    double sideLength = Math.Min(Math.Abs(currentPoint.X - startPoint.X), Math.Abs(currentPoint.Y - startPoint.Y));


                    Point vertex1 = new Point(centerX, centerY - (sideLength / 2));
                    Point vertex2 = new Point(centerX - (sideLength / 2), centerY + (sideLength / 2));
                    Point vertex3 = new Point(centerX + (sideLength / 2), centerY + (sideLength / 2));

                    currentPolygon.Points.Clear();
                    currentPolygon.Points.Add(vertex1);
                    currentPolygon.Points.Add(vertex2);
                    currentPolygon.Points.Add(vertex3);
                    currentPolygon.Points.Add(vertex1);
                    break;
            }
        }
        else if (brushButton.IsChecked.Value || eraserButton.IsChecked.Value)
        {
            Line line = new Line
            {
                X1 = startPoint.X,
                Y1 = startPoint.Y,
                X2 = e.GetCurrentPoint(canvas).Position.X,
                Y2 = e.GetCurrentPoint(canvas).Position.Y,
                Stroke = currentBrush,
                StrokeThickness = BrushThickness[selectedIndex],
                StrokeEndLineCap = PenLineCap.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round
            };
            Line lineBlur = new Line
            {
                X1 = startPoint.X,
                Y1 = startPoint.Y,
                X2 = e.GetCurrentPoint(canvas).Position.X,
                Y2 = e.GetCurrentPoint(canvas).Position.Y,
                Stroke = currentBrush,
                StrokeThickness = BrushThickness[selectedIndex] + 10,
                StrokeEndLineCap = PenLineCap.Square,
                StrokeStartLineCap = PenLineCap.Square,
                StrokeLineJoin = PenLineJoin.Miter,
                Opacity = 0.2
            };
            canvas.Children.Add(lineBlur);
            canvas.Children.Add(line);
            startPoint = e.GetCurrentPoint(canvas).Position;
        }
        else
        {
            Line line = new Line
            {
                X1 = startPoint.X,
                Y1 = startPoint.Y,
                X2 = e.GetCurrentPoint(canvas).Position.X,
                Y2 = e.GetCurrentPoint(canvas).Position.Y,
                Stroke = currentBrush,
                StrokeThickness = BrushThickness[selectedIndex],
            };
            canvas.Children.Add(line);
            startPoint = e.GetCurrentPoint(canvas).Position;
        }
    }

    private void canvas_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        isDrawing = false;
        isErasing = false;
    }
}
